using ArchUnitNET.Fluent;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PolsanLeaveManager.ArchitectureTests;

public class ForbiddenDependenciesTests
{
    private static ArchUnitNET.Domain.Architecture Arch => ModuleCatalog.Architecture.Value;

    public static TheoryData<string> Modules => [.. ModuleCatalog.Modules];

    [Fact]
    public void ModuleSet_IsNotEmpty()
    {
        Assert.NotEmpty(ModuleCatalog.Modules);
        Assert.Equal(ModuleCatalog.Modules.Length * 2, ModuleCatalog.ModuleAssemblies.Count);
        foreach (var name in ModuleCatalog.ContractsAssemblyNames.Concat(ModuleCatalog.InnerAssemblyNames))
        {
            Assert.NotEmpty(Types().That().ResideInAssemblyMatching(Rules.AssemblyPattern(name)).GetObjects(Arch));
        }
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void ForbiddenDependency1_ModuleUsesOnlyOwnInternal(string module)
    {
        Rules.ModuleDoesNotUseForeignInternal(module, $"PolsanLeaveManager.{module}").ShouldHold(Arch);
        Rules.ModuleDoesNotUseForeignInternal(module, $"PolsanLeaveManager.{module}.Contracts").ShouldHold(Arch);
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void ForbiddenDependency2_ContractsDoNotDependOnInternalEfOrAspNet(string module)
    {
        Rules.AssemblyDoesNotDependOnNamespaces(
            $"PolsanLeaveManager.{module}.Contracts", Rules.AnyInternal, Rules.EfCore, Rules.AspNet).ShouldHold(Arch);
        Rules.DoesNotDependOnInnerAssemblies($"PolsanLeaveManager.{module}.Contracts").ShouldHold(Arch);
    }

    [Fact]
    public void ForbiddenDependency3_WebDoesNotUseEfCoreDbContextOrInternal()
    {
        Rules.AssemblyDoesNotDependOnNamespaces("PolsanLeaveManager.Web", Rules.EfCore, Rules.AnyInternal).ShouldHold(Arch);
        Types().That().ResideInAssemblyMatching(Rules.AssemblyPattern("PolsanLeaveManager.Web"))
            .Should().NotDependOnAnyTypesThat().HaveNameEndingWith("DbContext")
            .WithoutRequiringPositiveResults().ShouldHold(Arch);
    }

    [Fact]
    public void ForbiddenDependency4_DomainTypesDoNotDependOnEfAspNetOrHttp()
    {
        Rules.DomainTypesAvoidInfrastructure(Rules.DomainNamespace).ShouldHold(Arch);
    }

    [Fact]
    public void Rules_DetectViolations_NegativeControl()
    {
        var http = Rules.AssemblyDoesNotDependOnNamespaces("PolsanLeaveManager.ArchitectureTests", Rules.Http);
        Assert.False(http.HasNoViolations(Arch));

        var foreign = Types().That().ResideInNamespaceMatching(@"^PolsanLeaveManager\.ArchitectureTests\.Fixtures$")
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(@"^PolsanLeaveManager\.ArchitectureTests\.Fixtures\.OtherInternal$");
        Assert.False(foreign.HasNoViolations(Arch));
    }

    [Fact]
    public void Rule1_Selector_FlagsForeignInternalButNotOwn()
    {
        var failures = Rules.ModuleDoesNotUseForeignInternal("Personnel", "PolsanLeaveManager.ArchitectureTests")
            .Evaluate(Arch).Where(e => !e.Passed).Select(e => e.Description).ToArray();

        Assert.Contains(failures, f => f.Contains("ForeignInternalUser"));
        Assert.DoesNotContain(failures, f => f.Contains("OwnInternalUser"));
    }

    [Fact]
    public void Rule2_AssemblyRule_FlagsInnerAssemblyUse()
    {
        Assert.False(Rules.DoesNotDependOnInnerAssemblies("PolsanLeaveManager.ArchitectureTests").HasNoViolations(Arch));
    }

    [Fact]
    public void Rule4_Selector_MatchesDomainNamespacesAndRuleFlagsHttp()
    {
        Assert.Matches(Rules.DomainNamespace, "PolsanLeaveManager.Personnel.Internal.Domain.Entities");
        Assert.DoesNotMatch(Rules.DomainNamespace, "PolsanLeaveManager.Personnel.Internal.Application");
        Assert.False(Rules.DomainTypesAvoidInfrastructure(@"^PolsanLeaveManager\.ArchitectureTests\.FixtureDomain$").HasNoViolations(Arch));
    }
}
