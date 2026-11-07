using System.Globalization;

namespace Flow.Launcher.Plugin.FlowSystem
{
    internal sealed class PluginStrings
    {
        private readonly IPublicAPI _api;

        public PluginStrings(IPublicAPI api, CultureInfo culture)
        {
            _api = api;
            Culture = culture;
        }

        public CultureInfo Culture { get; set; }

        public string PluginDescription => GetString("plugin_description");
        public string Processor => GetString("processor");
        public string GraphicsCard => GetString("graphics_card");
        public string InstalledMemory => GetString("installed_memory");
        public string SystemArchitecture => GetString("system_architecture");
        public string CpuUnknown => GetString("cpu_unknown");
        public string GpuUnknown => GetString("gpu_unknown");
        public string GpuQueryFailed => GetString("gpu_query_failed");
        public string RamUnknown => GetString("ram_unknown");
        public string RamQueryFailed => GetString("ram_query_failed");

        private string GetString(string key)
        {
            return _api.GetTranslation("flowlauncher_plugin_flowsystem_" + key);
        }
    }
}
