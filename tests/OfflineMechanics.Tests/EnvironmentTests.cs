using System;
using System.IO;
using System.Linq;
using WotR.Testing.Offline;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>Stage evidence only: these prove the environment, not that any feat works.</summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class EnvironmentTests
    {
        private readonly ACHomebrewGame fixture;

        public EnvironmentTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [Fact]
        public void GameAndModAssembliesComeFromTheDeclaredInputsAndThisBuild()
        {
            fixture.RequireGame();
            var runtime = Path.GetFullPath(OfflineRuntime.RuntimeDirectory);
            Assert.StartsWith(runtime, typeof(Kingmaker.Game).Assembly.Location, StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith(runtime, typeof(UnityEngine.Object).Assembly.Location, StringComparison.OrdinalIgnoreCase);

            var mod = typeof(ACHomebrew.Main).Assembly;
            Assert.StartsWith(runtime, mod.Location, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(OfflineRuntime.FileSha256(OfflineRuntime.Inputs.ModAssembly), OfflineRuntime.FileSha256(mod.Location));
            fixture.Observations["modAssembly"] = new { source = OfflineRuntime.Inputs.ModAssembly, sha256 = OfflineRuntime.FileSha256(mod.Location), version = mod.GetName().Version.ToString() };
            fixture.Observations["gameVersion"] = OfflineRuntime.Inputs.Identity.GameVersion;
        }

        [Fact]
        public void NoGameOrUnityProcessOrPlayerRuntimeIsInvolved()
        {
            fixture.RequireGame();
            Assert.Empty(WotrGameFixture.LoadedForbiddenModules());
            Assert.Empty(fixture.ForbiddenProcessesStartedDuringRun());
            Assert.DoesNotContain("Wrath", System.Diagnostics.Process.GetCurrentProcess().ProcessName, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("Assembly-CSharp.dll")]
        [InlineData("Assembly-CSharp-firstpass.dll")]
        [InlineData("Owlcat.Runtime.Core.dll")]
        public void RewrittenGameAssembliesKeepEveryMethodBody(string assembly)
        {
            fixture.RequireGame();
            var compared = IlComparison.CompareMethodBodies(
                Path.Combine(OfflineRuntime.Inputs.Managed, assembly),
                Path.Combine(OfflineRuntime.RuntimeDirectory, assembly));
            Assert.True(compared > 0);
            fixture.Observations[$"ilBodiesCompared:{assembly}"] = compared;
        }

        [Fact]
        public void ModInitializesThroughItsRealEntryPointAndBlueprintCacheHook()
        {
            fixture.RequireGame();
            Assert.All(fixture.Session.Stages, stage => Assert.True(stage.Passed, stage.Stage));
            Assert.Contains(fixture.Session.Stages, stage => stage.Stage == "mod-verification");
            Assert.Contains(fixture.Session.ModLog, line => line.Contains("ACHomebrew: registry initialized."));
            Assert.DoesNotContain(fixture.Session.ModLog, line => line.Contains(" failed - "));
        }
    }
}

