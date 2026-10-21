using ArchUnitNET.Fluent;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PolsanLeaveManager.ArchitectureTests;

internal static class Rules
{
    public static void ShouldHold(this IArchRule rule, ArchUnitNET.Domain.Architecture architecture)
    {
        var failures = rule.Evaluate(architecture).Where(e => !e.Passed).Select(e => e.Description).ToArray();
        Assert.True(failures.Length == 0, string.Join(Environment.NewLine, failures));
    }

    public static string AssemblyPattern(string assemblyName) => $@"^{System.Text.RegularExpressions.Regex.Escape(assemblyName)}(,|$)";

    public const string EfCore = @"^Microsoft\.EntityFrameworkCore(\..*)?$";
    public const string AspNet = @"^Microsoft\.AspNetCore(\..*)?$";
    public const string Http = @"^System\.Net\.Http(\..*)?$";
    public const string AnyInternal = @"^PolsanLeaveManager\.[^.]+\.Internal(\..*)?$";
    public const string DomainNamespace = @"^PolsanLeaveManager\.[^.]+\.Internal\.Domain(\..*)?$";

    /// <summary>Matches every module inner assembly (not the Contracts ones).</summary>
    public static string AnyInnerAssembly =>
        $@"^PolsanLeaveManager\.({string.Join("|", ModuleCatalog.Modules)})(,|$)";

    /// <summary>Types in the assembly must not depend on types in any of the forbidden namespaces.</summary>
    public static IArchRule AssemblyDoesNotDependOnNamespaces(string assemblyName, params string[] forbiddenNamespaceRegexes) =>
        Types().That().ResideInAssemblyMatching(Rules.AssemblyPattern(assemblyName))
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(string.Join("|", forbiddenNamespaceRegexes))
            .WithoutRequiringPositiveResults();

    /// <summary>Rule 1: a module may use only its own Internal namespace (others only via Contracts).</summary>
    public static IArchRule ModuleDoesNotUseForeignInternal(string module, string assemblyName) =>
        Types().That().ResideInAssemblyMatching(Rules.AssemblyPattern(assemblyName))
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching($@"^PolsanLeaveManager\.(?!{System.Text.RegularExpressions.Regex.Escape(module)}\.)[^.]+\.Internal(\..*)?$")
            .WithoutRequiringPositiveResults();

    /// <summary>Rule 2 (assembly level): a Contracts assembly must not use any type of a module inner assembly.</summary>
    public static IArchRule DoesNotDependOnInnerAssemblies(string assemblyName) =>
        Types().That().ResideInAssemblyMatching(Rules.AssemblyPattern(assemblyName))
            .Should().NotDependOnAnyTypesThat().ResideInAssemblyMatching(AnyInnerAssembly)
            .WithoutRequiringPositiveResults();

    /// <summary>Rule 4: types in the selected domain namespaces must not use EF Core, ASP.NET Core or HTTP.</summary>
    public static IArchRule DomainTypesAvoidInfrastructure(string domainNamespaceRegex) =>
        Types().That().ResideInNamespaceMatching(domainNamespaceRegex)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching($"{EfCore}|{AspNet}|{Http}")
            .WithoutRequiringPositiveResults();
}
