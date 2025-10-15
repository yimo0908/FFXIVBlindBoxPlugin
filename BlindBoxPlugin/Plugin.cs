using BlindBoxPlugin.Windows;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BlindBoxPlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] private static ICommandManager CommandManager { get; set; } = null!;
        [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; set; } = null!;

        public string Name => "Blind Box";
        private const string CommandName = "/blindbox";

        public Configuration Configuration { get; init; }

        private readonly WindowSystem _windowSystem = new("BlindBox");
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            MainWindow = new MainWindow(this, DataManager); // 传递 DataManager
            ConfigWindow = new ConfigWindow(this);

            _windowSystem.AddWindow(MainWindow);
            _windowSystem.AddWindow(ConfigWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "打开盲盒信息界面。"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        }

        public void Dispose()
        {
            _windowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();

            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            if (args == "config")
            {
                ConfigWindow.Toggle();
            }
            else
            {
                MainWindow.Toggle();
            }
        }

        private void DrawUI() => _windowSystem.Draw();
        private void ToggleConfigUI() => ConfigWindow.Toggle();
        private void ToggleMainUI() => MainWindow.Toggle();
    }
}
