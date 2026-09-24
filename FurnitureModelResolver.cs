using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace HoToSta;

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

        var altLocation = indoors ? "outdoor" : "indoor";
        var altPrefix = indoors ? "gar" : "fun";

        string[] candidates =
        [
            // Same location candidates
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b0_m{model}.mdl",
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b0_m{model}a.mdl",
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b0_m{model}b.mdl",
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b1_m{model}.mdl",
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_a0_m{model}.mdl",
            $"bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_m{model}.mdl",

            // Cross-location fallbacks (some indoor furnishings use outdoor asset folders, e.g. Chilled Red, Starlight Dodo)
            $"bgcommon/hou/{altLocation}/general/{model}/bgparts/{altPrefix}_b0_m{model}.mdl",
            $"bgcommon/hou/{altLocation}/general/{model}/bgparts/{altPrefix}_b0_m{model}a.mdl",
            $"bgcommon/hou/{altLocation}/general/{model}/bgparts/{altPrefix}_b0_m{model}b.mdl",
            $"bgcommon/hou/{altLocation}/general/{model}/bgparts/{altPrefix}_b1_m{model}.mdl",
            $"bgcommon/hou/{altLocation}/general/{model}/bgparts/{altPrefix}_a0_m{model}.mdl",
            $"bgcommon/hou/{altLocation}/general/{model}/bgparts/{altPrefix}_m{model}.mdl",
        ];

        foreach (var candidate in candidates)
        {
            if (_dataManager.FileExists(candidate))
            {
                mdlPath = candidate;
                return true;
            }
        }

        // Return false if no model exists in game assets
        return false;
    }
}
