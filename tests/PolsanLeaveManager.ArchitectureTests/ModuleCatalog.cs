using System.Reflection;
using ArchUnitNET.Loader;
using Arch = ArchUnitNET.Domain.Architecture;

namespace PolsanLeaveManager.ArchitectureTests;

internal static class ModuleCatalog
{
    public static readonly string[] Modules =
        ["Personnel", "LeaveRequests", "LeaveBalance", "Calendar", "MedicalLeave", "Notifications", "Audit"];

    public static readonly Lazy<Arch> Architecture = new(Load);

    public static IReadOnlyList<string> InnerAssemblyNames => [.. Modules.Select(m => $"PolsanLeaveManager.{m}")];

    public static IReadOnlyList<string> ContractsAssemblyNames => [.. Modules.Select(m => $"PolsanLeaveManager.{m}.Contracts")];

    public static IReadOnlyList<Assembly> ModuleAssemblies =>
        [.. InnerAssemblyNames.Concat(ContractsAssemblyNames).Select(Assembly.Load)];

    private static Arch Load() =>
        new ArchLoader()
            .LoadAssemblies(
                [.. ModuleAssemblies, Assembly.Load("PolsanLeaveManager.Web"), typeof(ModuleCatalog).Assembly])
            .Build();
}
