using System;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Stagehand.Api;
using Stagehand.Definitions;

namespace HousingToStagehand;

public sealed class StagehandIpcClient : IDisposable
{
    private const string TemporaryStageId = "HousingToStagehand_LivePreview";

    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IPluginLog _log;
    private readonly IStagehandApi _api;

    public bool IsSpawned { get; private set; }

    public StagehandIpcClient(IDalamudPluginInterface pluginInterface, IPluginLog log)
    {
        _pluginInterface = pluginInterface;
        _log = log;
        _api = StagehandApi.CreateIpcClient(pluginInterface);
    }

    public StagehandApiAvailability CheckAvailability()
    {
        return _api.CheckApiAvailability();
    }

    /// <summary>
    /// Spawns or updates the stage definition directly as a temporary Stage in Stagehand.
    /// </summary>
    public bool SpawnStage(StageDefinition stage, Vector3 translation, Quaternion rotation, float uniformScale = 1.0f)
    {
        var availability = CheckAvailability();
        if (availability != StagehandApiAvailability.Available)
        {
            _log.Warning($"Stagehand API is not available: {availability}");
            return false;
        }

        try
        {
            var defString = stage.ToDefinitionString();
            var success = _api.TryCreateOrUpdateTemporaryStageWithTransform(
                defString,
                TemporaryStageId,
                translation,
                rotation,
                uniformScale,
                debugName: stage.Info.Name);

            if (success)
            {
                _api.TrySetTemporaryStageVisible(TemporaryStageId, true);
                IsSpawned = true;
                _log.Information($"Spawned temporary stage \"{TemporaryStageId}\" in Stagehand.");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to spawn temporary stage in Stagehand.");
            return false;
        }
    }

    /// <summary>
    /// Clears and removes the temporary Stage from Stagehand.
    /// </summary>
    public bool ClearStage()
    {
        var availability = CheckAvailability();
        if (availability != StagehandApiAvailability.Available)
            return false;

        try
        {
            var destroyed = _api.TryDestroyTemporaryStage(TemporaryStageId);
            IsSpawned = false;
            return destroyed;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to clear temporary stage from Stagehand.");
            return false;
        }
    }

    public void Dispose()
    {
        if (IsSpawned)
        {
            ClearStage();
        }

        if (_api is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
