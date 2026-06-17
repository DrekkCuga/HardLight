#nullable enable
using Content.Shared.CCVar;
using Content.Shared.HL.CCVar;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.UnitTesting;

namespace Content.IntegrationTests;

// Partial class containing cvar logic
public static partial class PoolManager
{
    private static readonly (string cvar, string value)[] TestCvars =
    {
        // @formatter:off
        (CCVars.DatabaseSynchronous.Name,     "true"),
        (CCVars.DatabaseSqliteDelay.Name,     "0"),
        (CCVars.HolidaysEnabled.Name,         "false"),
        (CCVars.GameMap.Name,                 TestMap),
        (CCVars.AdminLogsQueueSendDelay.Name, "0"),
        (CVars.NetPVS.Name,                   "false"),
        (CCVars.NPCMaxUpdates.Name,           "999999"),
        (CVars.ThreadParallelCount.Name,      "1"),
        (CCVars.GameRoleTimers.Name,          "false"),
        (CCVars.GameRoleWhitelist.Name,       "false"),
        (CCVars.GridFill.Name,                "false"),
        (CCVars.PreloadGrids.Name,            "false"),
        (CCVars.ArrivalsShuttles.Name,        "false"),
        (CCVars.EmergencyShuttleEnabled.Name, "false"),
        (CCVars.ProcgenPreload.Name,          "false"),
        (CCVars.WorldgenEnabled.Name,         "false"),
        (CCVars.GatewayGeneratorEnabled.Name, "false"),
        (CVars.ReplayClientRecordingEnabled.Name, "false"),
        (CVars.ReplayServerRecordingEnabled.Name, "false"),
        (CCVars.GameDummyTicker.Name, "true"),
        (CCVars.GameLobbyEnabled.Name, "false"),
        (CCVars.ConfigPresetDevelopment.Name, "false"),
        (CCVars.AdminLogsEnabled.Name, "false"),
        (CCVars.AutosaveEnabled.Name, "false"),
        (CVars.NetBufferSize.Name, "0"),
        (CCVars.InteractionRateLimitCount.Name, "9999999"),
        (CCVars.InteractionRateLimitPeriod.Name, "0.1"),
        (CCVars.MovementMobPushing.Name, "false"),
        (CCVars.GameLobbyDefaultPreset.Name, "nftest"), // Frontier: Adventure takes ages, default to nftest (no need to test events we will not run, e.g. meteor swarm)
        (CCVars.StaticStorageUI.Name, "true"), // Frontier: causes storage test failures
        (CCVars.StorageLimit.Name, "1"),// Frontier: test failures with multiple storage enabled
        (CCVars.AutoVoteEnabled.Name,         "false"), // HL: Stop auto-vote from starting a round mid-test
        (HLCCVars.RoundPersistenceEnabled.Name,      "false"), // HL: Stop things persisting between rounds for testing
        (CCVars.EventsEnabled.Name, "false"), // HL: We don't want random events messing with tests
        (HLCCVars.AutoSpawnColComm.Name, "false"), // HL: colcomm spawning fucks all the tests that look at ent counts and I spent way too long figuring out how to turn it off
        (CCVars.VoteTimerRestart.Name, "120"),
        (CCVars.VoteTimerPreset.Name, "120"),
        (CCVars.VoteTimerMap.Name, "120"),
        (CCVars.VoteTimerAlone.Name, "120"),
        (CCVars.RoundRestartTime.Name, "1200")
    };

    public static async Task SetupCVars(RobustIntegrationTest.IntegrationInstance instance, PoolSettings settings)
    {
        var cfg = instance.ResolveDependency<IConfigurationManager>();
        await instance.WaitPost(() =>
        {
            if (cfg.IsCVarRegistered(CCVars.GameDummyTicker.Name))
                cfg.SetCVar(CCVars.GameDummyTicker, settings.UseDummyTicker);

            if (cfg.IsCVarRegistered(CCVars.GameLobbyEnabled.Name))
                cfg.SetCVar(CCVars.GameLobbyEnabled, settings.InLobby);

            if (cfg.IsCVarRegistered(CVars.NetInterp.Name))
                cfg.SetCVar(CVars.NetInterp, settings.DisableInterpolate);

            if (cfg.IsCVarRegistered(CCVars.GameMap.Name))
                cfg.SetCVar(CCVars.GameMap, settings.Map);

            if (cfg.IsCVarRegistered(CCVars.AdminLogsEnabled.Name))
                cfg.SetCVar(CCVars.AdminLogsEnabled, settings.AdminLogsEnabled);

            if (cfg.IsCVarRegistered(CVars.NetInterp.Name))
                cfg.SetCVar(CVars.NetInterp, !settings.DisableInterpolate);

            if (cfg.IsCVarRegistered(CCVars.GameLobbyDefaultPreset.Name) && !string.IsNullOrEmpty(settings.GameLobbyDefaultPreset)) // Frontier
                cfg.SetCVar(CCVars.GameLobbyDefaultPreset, settings.GameLobbyDefaultPreset); // Frontier
        });
    }

    private static void SetDefaultCVars(RobustIntegrationTest.IntegrationOptions options)
    {
        foreach (var (cvar, value) in TestCvars)
        {
            options.CVarOverrides[cvar] = value;
        }
    }
}
