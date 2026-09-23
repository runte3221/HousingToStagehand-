using System;
using System.IO;
using System.Numerics;
using System.Threading;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Stagehand.Api;
using WinForms = System.Windows.Forms;

namespace HoToSta;

public sealed class MainWindow : Window, IDisposable
{
    private readonly Plugin _plugin;
    private readonly IPluginLog _log;
    private readonly FurnitureModelResolver _resolver;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IClientState _clientState;
    private readonly IDataManager _dataManager;
    private readonly IObjectTable _objectTable;
    private readonly StagehandIpcClient _ipcClient;

    private string _layoutPathInput = string.Empty;
    private LayoutFile? _loadedLayout;
    private string? _loadError;

    private string _stageNameInput = string.Empty;
    private string _stageDescriptionInput = string.Empty;
    private string _stagesFolderInput = string.Empty;

    private string? _saveMessage;
    private string? _saveError;
    private ConversionResult? _lastResult;

    private volatile string? _pendingLayoutPath;
    private volatile string? _pendingStagesFolderPath;

    public MainWindow(
        Plugin plugin,
        IPluginLog log,
        FurnitureModelResolver resolver,
        IDalamudPluginInterface pluginInterface,
        IClientState clientState,
        IDataManager dataManager,
        IObjectTable objectTable,
        StagehandIpcClient ipcClient)
        : base("HoToSta (/hotosta or /h2s)###hotosta_main", ImGuiWindowFlags.None)
    {
        _plugin = plugin;
        _log = log;
        _resolver = resolver;
        _pluginInterface = pluginInterface;
        _clientState = clientState;
        _dataManager = dataManager;
        _objectTable = objectTable;
        _ipcClient = ipcClient;

        Size = new Vector2(560, 680);
        SizeCondition = ImGuiCond.FirstUseEver;

        _layoutPathInput = plugin.Configuration.LastLayoutPath;
        _stagesFolderInput = string.IsNullOrWhiteSpace(plugin.Configuration.TargetStagesFolder)
            ? StagehandExporter.GetDefaultStagesDirectory()
            : plugin.Configuration.TargetStagesFolder;
    }

    public override void Draw()
    {
        if (_pendingLayoutPath is { } newLayoutPath)
        {
            _pendingLayoutPath = null;
            _layoutPathInput = newLayoutPath;
            LoadLayout();
        }

        if (_pendingStagesFolderPath is { } newStagesFolder)
        {
            _pendingStagesFolderPath = null;
            _stagesFolderInput = newStagesFolder;
            _plugin.Configuration.TargetStagesFolder = newStagesFolder;
            _plugin.SaveConfiguration();
        }

        DrawLayoutSection();

        if (_loadedLayout is not null)
        {
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            DrawOptionsSection();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            DrawStagehandExportSection();
        }
    }

    private void DrawLayoutSection()
    {
        ImGui.TextUnformatted("Layout File (MakePlace JSON)");

        ImGui.SetNextItemWidth(-90f);
        ImGui.InputText("###layout_path", ref _layoutPathInput, 512);
        ImGui.SameLine();
        if (ImGui.Button("Browse...###browse_layout"))
        {
            BrowseForLayoutFile();
        }

        if (ImGui.Button("Load Layout", new Vector2(140, 0)))
        {
            LoadLayout();
        }

        if (_loadError is not null)
        {
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), _loadError);
        }

        if (_loadedLayout is { } layout)
        {
            ImGui.Spacing();
            ImGui.TextUnformatted($"House size: {(string.IsNullOrEmpty(layout.HouseSize) ? "Unknown" : layout.HouseSize)}");
            var interiorTotal = layout.GetTotalInteriorFurnitureCount();
            var interiorDirect = layout.InteriorFurniture.Count;
            var interiorAttach = interiorTotal - interiorDirect;
            if (interiorAttach > 0)
            {
                ImGui.TextUnformatted($"Interior furniture: {interiorTotal} ({interiorDirect} + {interiorAttach} attachments)");
            }
            else
            {
                ImGui.TextUnformatted($"Interior furniture: {interiorTotal}");
            }

            var exteriorTotal = layout.GetTotalExteriorFurnitureCount();
            var exteriorDirect = layout.ExteriorFurniture.Count;
            var exteriorAttach = exteriorTotal - exteriorDirect;
            if (exteriorAttach > 0)
            {
                ImGui.TextUnformatted($"Exterior furniture: {exteriorTotal} ({exteriorDirect} + {exteriorAttach} attachments)");
            }
            else
            {
                ImGui.TextUnformatted($"Exterior furniture: {exteriorTotal}");
            }

            ImGui.TextUnformatted($"Fixtures (walls/floors, skipped): {layout.InteriorFixture.Count + layout.ExteriorFixture.Count}");

            var currentSize = HouseSizeDetector.GetCurrentIndoorHouseSize(_clientState, _dataManager);
            if (currentSize is not null
                && !string.IsNullOrEmpty(layout.HouseSize)
                && !string.Equals(currentSize, layout.HouseSize, StringComparison.OrdinalIgnoreCase))
            {
                ImGui.Spacing();
                ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f), $"Notice: Layout is {layout.HouseSize}, but you are currently in a {currentSize} interior.");
            }
        }
    }

    private void DrawOptionsSection()
    {
        ImGui.TextUnformatted("Conversion Options");

        var includeInterior = _plugin.Configuration.IncludeInterior;
        if (ImGui.Checkbox("Include interior furniture", ref includeInterior))
        {
            _plugin.Configuration.IncludeInterior = includeInterior;
            _plugin.SaveConfiguration();
        }

        var includeExterior = _plugin.Configuration.IncludeExterior;
        if (ImGui.Checkbox("Include exterior furniture", ref includeExterior))
        {
            _plugin.Configuration.IncludeExterior = includeExterior;
            _plugin.SaveConfiguration();
        }

        var applyDye = _plugin.Configuration.ApplyDyeColors;
        if (ImGui.Checkbox("Apply dye colors (Stain)", ref applyDye))
        {
            _plugin.Configuration.ApplyDyeColors = applyDye;
            _plugin.SaveConfiguration();
        }

        var anchorToPlayer = _plugin.Configuration.AnchorToPlayerPosition;
        if (ImGui.Checkbox("Anchor layout to current player position", ref anchorToPlayer))
        {
            _plugin.Configuration.AnchorToPlayerPosition = anchorToPlayer;
            _plugin.SaveConfiguration();
        }
    }

    private void DrawStagehandExportSection()
    {
        ImGui.TextUnformatted("Stagehand Stage Export & Spawn");

        ImGui.TextUnformatted("Stages Folder:");
        ImGui.SetNextItemWidth(-90f);
        ImGui.InputText("###stages_folder", ref _stagesFolderInput, 512);
        ImGui.SameLine();
        if (ImGui.Button("Browse...###browse_folder"))
        {
            BrowseForStagesFolder();
        }

        ImGui.TextUnformatted("Stage Name:");
        ImGui.InputText("###stage_name", ref _stageNameInput, 128);

        ImGui.TextUnformatted("Description:");
        ImGui.InputText("###stage_desc", ref _stageDescriptionInput, 256);

        ImGui.Spacing();

        // 1. Export as JSON file
        if (ImGui.Button("Save as Stagehand Stage (.json)", new Vector2(240, 32)))
        {
            SaveStageToFile();
        }

        ImGui.SameLine();

        // 2. Direct spawn via IPC
        var availability = _ipcClient.CheckAvailability();
        var isStagehandAvailable = availability == StagehandApiAvailability.Available;

        if (!isStagehandAvailable)
        {
            ImGui.BeginDisabled();
        }

        if (!_ipcClient.IsSpawned)
        {
            if (ImGui.Button("Spawn via Stagehand (IPC)", new Vector2(240, 32)))
            {
                SpawnStageViaIpc();
            }
        }
        else
        {
            if (ImGui.Button("Clear Spawned Stage (IPC)", new Vector2(240, 32)))
            {
                _ipcClient.ClearStage();
            }
        }

        if (!isStagehandAvailable)
        {
            ImGui.EndDisabled();
            ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), $"Stagehand IPC unavailable ({availability}). Enable Stagehand to use direct spawn.");
        }

        if (_saveMessage is not null)
        {
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(0.4f, 1f, 0.4f, 1f), _saveMessage);
        }

        if (_saveError is not null)
        {
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), _saveError);
        }

        if (_lastResult is { } result && result.SkippedUnknownItemCount > 0)
        {
            ImGui.Spacing();
            ImGui.TextColored(
                new Vector4(1f, 0.8f, 0.3f, 1f),
                $"Skipped {result.SkippedUnknownItemCount} item(s) without matching model data.");

            if (ImGui.TreeNode("Show skipped items###skipped_items"))
            {
                foreach (var name in result.SkippedItemNames)
                {
                    ImGui.BulletText(name);
                }
                ImGui.TreePop();
            }
        }
    }

    private void LoadLayout()
    {
        _loadError = null;
        _saveMessage = null;
        _saveError = null;
        _lastResult = null;

        if (string.IsNullOrWhiteSpace(_layoutPathInput))
        {
            _loadError = "Please specify a layout file path.";
            return;
        }

        try
        {
            _loadedLayout = LayoutParser.LoadFromFile(_layoutPathInput);
            _plugin.Configuration.LastLayoutPath = _layoutPathInput;
            _plugin.SaveConfiguration();

            if (string.IsNullOrWhiteSpace(_stageNameInput))
            {
                var baseName = Path.GetFileNameWithoutExtension(_layoutPathInput);
                _stageNameInput = string.IsNullOrWhiteSpace(baseName) ? "ImportedStage" : baseName;
            }
        }
        catch (Exception ex)
        {
            _loadedLayout = null;
            _loadError = $"Failed to load layout: {ex.Message}";
        }
    }

    private ConversionResult? RunConversion()
    {
        if (_loadedLayout is null)
            return null;

        var localPlayer = _objectTable.Length > 0 ? _objectTable[0] : null;
        var options = new ConversionOptions
        {
            IncludeInterior = _plugin.Configuration.IncludeInterior,
            IncludeExterior = _plugin.Configuration.IncludeExterior,
            ApplyDyeColors = _plugin.Configuration.ApplyDyeColors,
            PositionOffset = _plugin.Configuration.AnchorToPlayerPosition && localPlayer is not null
                ? localPlayer.Position
                : Vector3.Zero,
        };

        var stageName = string.IsNullOrWhiteSpace(_stageNameInput) ? "ImportedStage" : _stageNameInput.Trim();
        var territoryId = (int)_clientState.TerritoryType;

        var result = LayoutToStagehandConverter.Convert(
            _loadedLayout,
            _resolver,
            options,
            stageName,
            _stageDescriptionInput,
            territoryId);

        _lastResult = result;
        return result;
    }

    private void SaveStageToFile()
    {
        _saveMessage = null;
        _saveError = null;

        var result = RunConversion();
        if (result is null)
            return;

        var stageName = string.IsNullOrWhiteSpace(_stageNameInput) ? "ImportedStage" : _stageNameInput.Trim();
        var exportResult = StagehandExporter.ExportToFile(result.Stage, _stagesFolderInput, stageName);

        if (exportResult.Success)
        {
            _saveMessage = $"{exportResult.Message} ({result.PlacedCount} objects placed)";
        }
        else
        {
            _saveError = exportResult.Message;
        }
    }

    private void SpawnStageViaIpc()
    {
        _saveMessage = null;
        _saveError = null;

        var result = RunConversion();
        if (result is null)
            return;

        var translation = Vector3.Zero;
        var rotation = Quaternion.Identity;

        var success = _ipcClient.SpawnStage(result.Stage, translation, rotation);
        if (success)
        {
            _saveMessage = $"Spawned {result.PlacedCount} objects directly into Stagehand!";
        }
        else
        {
            _saveError = "Failed to spawn stage via Stagehand IPC. Make sure Stagehand is loaded.";
        }
    }

    private void BrowseForLayoutFile()
    {
        var thread = new Thread(() =>
        {
            using var dialog = new WinForms.OpenFileDialog
            {
                Title = "Select MakePlace Layout JSON",
                Filter = "MakePlace Layout Files (*.json)|*.json|All Files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false,
            };

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                _pendingLayoutPath = dialog.FileName;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    private void BrowseForStagesFolder()
    {
        var thread = new Thread(() =>
        {
            using var dialog = new WinForms.FolderBrowserDialog
            {
                Description = "Select Stagehand Stages Folder",
                UseDescriptionForTitle = true,
                SelectedPath = string.IsNullOrWhiteSpace(_stagesFolderInput)
                    ? StagehandExporter.GetDefaultStagesDirectory()
                    : _stagesFolderInput,
            };

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                _pendingStagesFolderPath = dialog.SelectedPath;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
