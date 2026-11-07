using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.FlowSystem
{
    internal static class SystemInfoReader
    {
        public static string ReadOsArchitecture()
        {
            return RuntimeInformation.OSArchitecture.ToString();
        }

        public static string ReadCpuName(PluginStrings strings)
        {
            if (Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0",
                    "ProcessorNameString",
                    null) is string cpuName)
            {
                return cpuName.Trim();
            }

            return strings.CpuUnknown;
        }

        public static List<string> ReadGpuNames(PluginStrings strings)
        {
            var gpuNames = new List<string>();

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name FROM Win32_VideoController");

                using var videoControllers = searcher.Get();

                foreach (ManagementObject gpu in videoControllers)
                {
                    using (gpu)
                    {
                        if (gpu["Name"] is string gpuName
                            && !string.IsNullOrWhiteSpace(gpuName))
                        {
                            gpuNames.Add(gpuName.Trim());
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                gpuNames.Add(strings.GpuQueryFailed);
            }

            if (gpuNames.Count == 0)
            {
                gpuNames.Add(strings.GpuUnknown);
            }

            return gpuNames;
        }

        public static string ReadRamText(PluginStrings strings)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Capacity FROM Win32_PhysicalMemory");

                using var modules = searcher.Get();
                ulong totalBytes = 0;

                foreach (ManagementObject module in modules)
                {
                    using (module)
                    {
                        if (module["Capacity"] is ulong capacity)
                        {
                            totalBytes += capacity;
                        }
                    }
                }

                if (totalBytes > 0)
                {
                    double totalGiB = totalBytes / (1024d * 1024 * 1024);
                    return totalGiB.ToString("0.##", strings.Culture) + " GB";
                }
            }
            catch (ManagementException)
            {
                return strings.RamQueryFailed;
            }

            return strings.RamUnknown;
        }
    }
}
