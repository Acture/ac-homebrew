using System;
using System.IO;
using HarmonyLib;
using UnityModManagerNet;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class InstallationIdentityTests
    {
        private readonly ACHomebrewGame fixture;
        public InstallationIdentityTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ActiveLegacyInstallationIsRejectedBeforeApplyingNewPatches(bool enabled, bool started)
        {
            fixture.RequireGame();
            var current = (UnityModManager.ModEntry)AccessTools.Field(typeof(Main), "Entry").GetValue(null);
            Assert.Equal("ACHomebrew", current.Info.Id);
            var legacy = new UnityModManager.ModEntry(new UnityModManager.ModInfo
            {
                Id = "AttributeFeats", Version = "0.2.0", ManagerVersion = "0.27.11",
            }, Path.Combine(Path.GetTempPath(), "ACHomebrew-legacy-conflict-" + Guid.NewGuid().ToString("N")));
            legacy.Enabled = enabled;
            AccessTools.Field(typeof(UnityModManager.ModEntry), "mStarted").SetValue(legacy, started);
            UnityModManager.modEntries.Add(legacy);
            try { Assert.False(Main.Load(current)); }
            finally { UnityModManager.modEntries.Remove(legacy); }
        }
    }
}
