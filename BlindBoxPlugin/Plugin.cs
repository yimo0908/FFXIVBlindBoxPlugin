using BlindBoxPlugin.Windows;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] private static ICommandManager CommandManager { get; set; } = null!;
        [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
        [PluginService] internal static IChatGui ChatGUI { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;

        /// <summary>物品 ExcelSheet 的统一访问点，避免散布的 GetExcelSheet 调用。</summary>
        internal static ExcelSheet<Item> ItemSheet => DataManager.GetExcelSheet<Item>();

        public string Name => "Blind Box";
        private const string CommandName = "/blindbox";

        public Configuration Configuration { get; init; }

        private readonly WindowSystem windowSystem = new("BlindBox");
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            MainWindow = new MainWindow(this);
            ConfigWindow = new ConfigWindow();

            windowSystem.AddWindow(MainWindow);
            windowSystem.AddWindow(ConfigWindow);

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
            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUI;
            PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUI;

            windowSystem.RemoveAllWindows();

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

        private void DrawUI() => windowSystem.Draw();
        public void ToggleConfigUI() => ConfigWindow.Toggle();
        private void ToggleMainUI() => MainWindow.Toggle();
    }
}
