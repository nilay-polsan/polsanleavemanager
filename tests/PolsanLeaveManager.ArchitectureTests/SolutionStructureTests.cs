namespace PolsanLeaveManager.ArchitectureTests;

public class SolutionStructureTests
{
    [Fact]
    public void Solution_HasSevenModulesWithContractsAndWeb()
    {
        var root = AppContext.BaseDirectory;
        while (root is not null && !File.Exists(Path.Combine(root, "PolsanLeaveManager.slnx")))
        {
            root = Path.GetDirectoryName(root);
        }

        Assert.NotNull(root);
        var projects = Directory.GetFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension)
            .Order()
            .ToArray();

        var expected = ModuleCatalog.InnerAssemblyNames
            .Concat(ModuleCatalog.ContractsAssemblyNames)
            .Append("PolsanLeaveManager.Web")
            .Order()
            .ToArray();

        Assert.Equal(expected, projects);
    }
}
