using System;
using System.Linq;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>One offline game session with ACHomebrew loaded, shared by all tests in the collection.</summary>
    public sealed class ACHomebrewGame : WotrGameFixture
    {
        // The mod swallows per-family failures and reports them in its log, so success is read from the log.
        protected override void VerifyModInitialized(GameSession session)
        {
            if (!session.ModLog.Any(line => line.Contains("ACHomebrew: registry initialized.")))
                throw new InvalidOperationException("The mod did not report a successful registry initialization:\n" + string.Join("\n", session.ModLog));
        }
    }

    [CollectionDefinition(Name)]
    public sealed class ACHomebrewGameCollection : ICollectionFixture<ACHomebrewGame>
    {
        public const string Name = "ACHomebrew offline game";
    }
}
