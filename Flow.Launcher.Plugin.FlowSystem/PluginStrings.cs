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
        public string Reload => GetString("reload");
        public string ReloadDescription => GetString("reload_description");
        public string ReloadSuccess => GetString("reload_success");
        public string Copy => GetString("copy");
        public string CopyDescription => GetString("copy_description");
        public string Processor => GetString("processor");
        public string Core => GetString("core");
        public string Cores => GetString("cores");
        public string Thread => GetString("thread");
        public string Threads => GetString("threads");
        public string GraphicsCard => GetString("graphics_card");
        public string InstalledMemory => GetString("installed_memory");
        public string MemoryModule => GetString("memory_module");
        public string MemoryModules => GetString("memory_modules");
        public string NetworkAdapter => GetString("network_adapter");
        public string WirelessLan => GetString("wireless_lan");
        public string Ethernet => GetString("ethernet");
        public string NetworkUnknown => GetString("network_unknown");
        public string NetworkQueryFailed => GetString("network_query_failed");
        public string StorageDevice => GetString("storage_device");
        public string StorageQueryFailed => GetString("storage_query_failed");
        public string OperatingSystem => GetString("operating_system");
        public string Build => GetString("build");
        public string SystemArchitecture => GetString("system_architecture");
        public string DeviceSecurity => GetString("device_security");
        public string SecureBootEnabled => GetString("secure_boot_enabled");
        public string SecureBootDisabled => GetString("secure_boot_disabled");
        public string SecureBootUnreadable => GetString("secure_boot_unreadable");
        public string TpmNotDetected => GetString("tpm_not_detected");
        public string TpmVersionUnavailable => GetString("tpm_version_unavailable");
        public string OsUnknown => GetString("os_unknown");
        public string OsQueryFailed => GetString("os_query_failed");
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
