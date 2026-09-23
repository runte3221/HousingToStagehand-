using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace HousingToStagehand;

/// <summary>
/// Resolves housing item IDs to their corresponding .mdl asset paths for Stagehand BgObjects.
/// </summary>
public sealed class FurnitureModelResolver
{
    private readonly IDataManager _dataManager;
    private readonly Dictionary<uint, uint> _indoorModelKeyByItemId = new();
    private readonly Dictionary<uint, uint> _outdoorModelKeyByItemId = new();

    public int IndoorEntryCount => _indoorModelKeyByItemId.Count;
    public int OutdoorEntryCount => _outdoorModelKeyByItemId.Count;

    public FurnitureModelResolver(IDataManager dataManager)
    {
        _dataManager = dataManager;

        var indoorSheet = dataManager.GetExcelSheet<HousingFurniture>();
        if (indoorSheet is not null)
        {
            foreach (var row in indoorSheet)
            {
                var item = row.Item.ValueNullable;
                if (item is null)
                    continue;

                _indoorModelKeyByItemId[item.Value.RowId] = row.ModelKey;
            }
        }

        var outdoorSheet = dataManager.GetExcelSheet<HousingYardObject>();
        if (outdoorSheet is not null)
        {
            foreach (var row in outdoorSheet)
            {
                var item = row.Item.ValueNullable;
                if (item is null)
                    continue;

                _outdoorModelKeyByItemId[item.Value.RowId] = row.ModelKey;
            }
        }
    }

    /// <summary>
    /// Attempts to resolve the .mdl game path for a given housing item ID.
    /// </summary>
    public bool TryResolveMdlPath(uint itemId, bool indoors, out string mdlPath)
    {
        mdlPath = string.Empty;
        var table = indoors ? _indoorModelKeyByItemId : _outdoorModelKeyByItemId;

        if (!table.TryGetValue(itemId, out var modelKey))
            return false;

        var model = modelKey.ToString("0000");
        var location = indoors ? "indoor" : "outdoor";
        var prefix = indoors ? "fun" : "gar";

        // Primary convention: bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b0_m{model}.mdl
        var primaryCandidate = $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b0_m{model}.mdl";
        if (_dataManager.FileExists(primaryCandidate))
        {
            mdlPath = primaryCandidate;
            return true;
        }

        // Secondary fallback candidates
        string[] fallbacks =
        [
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b1_m{model}.mdl",
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_a0_m{model}.mdl",
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_m{model}.mdl",
        ];

        foreach (var candidate in fallbacks)
        {
            if (_dataManager.FileExists(candidate))
            {
                mdlPath = candidate;
                return true;
            }
        }

        // If file existence check fails, return primary candidate anyway (useful in case Lumina vfs differs)
        mdlPath = primaryCandidate;
        return true;
    }
}
