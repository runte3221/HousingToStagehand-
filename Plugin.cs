using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace HoToSta;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "HoToSta";

    private const string CommandName = "/hotosta";
    private const string ShortCommandName = "/h2s";

    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ICommandManager _commandManager;
    private readonly IPluginLog _log;

    private readonly WindowSystem _windowSystem = new("HoToSta");
    private readonly MainWindow _mainWindow;
    private readonly StagehandIpcClient _ipcClient;

    public Configuration Configuration { get; }
    internal FurnitureModelResolver FurnitureResolver { get; }

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IPluginLog log,
        IDataManager dataManager,
        IClientState clientState,
        IObjectTable objectTable)
    {
        _pluginInterface = pluginInterface;
        _commandManager = commandManager;
        _log = log;

        Configuration = _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        FurnitureResolver = new FurnitureModelResolver(dataManager);
        _ipcClient = new StagehandIpcClient(_pluginInterface, _log);

        _log.Information(
            "HoToSta loaded housing sheets: {Indoor} indoor, {Outdoor} outdoor entries",
            FurnitureResolver.IndoorEntryCount,
            FurnitureResolver.OutdoorEntryCount);

        _mainWindow = new MainWindow(
            this,
            _log,
            FurnitureResolver,
            _pluginInterface,
            clientState,
            dataManager,
            objectTable,
            _ipcClient);

        _windowSystem.AddWindow(_mainWindow);

        _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        _pluginInterface.UiBuilder.OpenMainUi += ToggleMainWindow;

        _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the HoToSta window.",
        });

        _commandManager.AddHandler(ShortCommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the HoToSta window.",
            ShowInHelp = false,
        });
    }

    private void OnCommand(string command, string args) => ToggleMainWindow();

    private void ToggleMainWindow() => _mainWindow.IsOpen = !_mainWindow.IsOpen;

    public void SaveConfiguration() => _pluginInterface.SavePluginConfig(Configuration);

    public void Dispose()
    {
        _commandManager.RemoveHandler(CommandName);
        _commandManager.RemoveHandler(ShortCommandName);

        _pluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        _pluginInterface.UiBuilder.OpenMainUi -= ToggleMainWindow;

        _windowSystem.RemoveAllWindows();
        _mainWindow.Dispose();
        _ipcClient.Dispose();
    }
}
