namespace PolsanLeaveManager.ArchitectureTests.Fixtures
{
    /// <summary>Deliberate violator: proves the rules can fail (negative control).</summary>
    public sealed class ViolatingFixture
    {
        public static HttpClient Client => new();

        public static object Peek() => new OtherInternal.Secret();
    }
}

namespace PolsanLeaveManager.ArchitectureTests.Fixtures.OtherInternal
{
    public sealed class Secret;
}

namespace PolsanLeaveManager.Foreign.Internal
{
    public sealed class Secret;
}

namespace PolsanLeaveManager.ArchitectureTests.Fixtures
{
    public sealed class ForeignInternalUser
    {
        public static object Peek() => new PolsanLeaveManager.Foreign.Internal.Secret();
    }

    public sealed class OwnInternalUser
    {
        public static object Peek() => new PolsanLeaveManager.Personnel.Internal.ModuleMarker();
    }
}

namespace PolsanLeaveManager.ArchitectureTests.FixtureDomain
{
    public sealed class DomainViolator
    {
        public static HttpClient Client => new();
    }
}
