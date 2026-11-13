using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.FlowSystem
{
    internal static class SystemInfoReader
    {
        private const uint TbsTpmNotFound = 0x8028400F;
        private const uint TpmVersion12 = 1;
        private const uint TpmVersion20 = 2;

        public static SystemInfo Read()
        {
            return new SystemInfo(ReadOperatingSystem(), ReadProcessor(), ReadGraphics(),
                ReadMemory(), ReadNetwork(), ReadStorage(), ReadSecureBoot(), ReadTpm());
        }

        private static TpmStatus ReadTpm()
        {
            try
            {
                uint result = Tbsi_GetDeviceInfo((uint)Marshal.SizeOf<TpmDeviceInfo>(), out TpmDeviceInfo info);
                return (result, info.TpmVersion) switch
                {
                    (0, TpmVersion12) => TpmStatus.Version12,
                    (0, TpmVersion20) => TpmStatus.Version20,
                    (TbsTpmNotFound, _) => TpmStatus.NotDetected,
                    _ => TpmStatus.Unavailable
                };
            }
            catch (System.DllNotFoundException)
            {
                return TpmStatus.Unavailable;
            }
            catch (System.EntryPointNotFoundException)
            {
                return TpmStatus.Unavailable;
            }
        }

        [DllImport("tbs.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern uint Tbsi_GetDeviceInfo(uint size, out TpmDeviceInfo info);

        [StructLayout(LayoutKind.Sequential)]
        private struct TpmDeviceInfo
        {
            public uint StructVersion;
            public uint TpmVersion;
            public uint TpmInterfaceType;
            public uint TpmImplementationRevision;
        }

        private static bool? ReadSecureBoot()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
                return key?.GetValue("UEFISecureBootEnabled") switch
                {
                    1 => true,
                    0 => false,
                    _ => null
                };
            }
            catch (System.Security.SecurityException)
            {
                return null;
            }
            catch (System.UnauthorizedAccessException)
            {
                return null;
            }
            catch (System.IO.IOException)
            {
                return null;
            }
        }

        private static OperatingSystemInfo ReadOperatingSystem()
        {
            string name = null;
            bool nameQueryFailed = false;
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Caption FROM Win32_OperatingSystem");
                using var operatingSystems = searcher.Get();

                foreach (ManagementObject operatingSystem in operatingSystems)
                {
                    using (operatingSystem)
                    {
                        if (operatingSystem["Caption"] is string osName
                            && !string.IsNullOrWhiteSpace(osName))
                        {
                            name = osName.Trim();
                            break;
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                nameQueryFailed = true;
            }
            catch (System.UnauthorizedAccessException)
            {
                nameQueryFailed = true;
            }

            var (displayVersion, build) = ReadOsVersion();
            return new OperatingSystemInfo(name, displayVersion, build,
                RuntimeInformation.OSArchitecture.ToString(), nameQueryFailed);
        }

        private static (string DisplayVersion, string Build) ReadOsVersion()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string displayVersion = (key?.GetValue("DisplayVersion") as string)?.Trim();
                string build = (key?.GetValue("CurrentBuildNumber") as string)?.Trim();

                if (!string.IsNullOrWhiteSpace(build)
                    && key.GetValue("UBR") is int revision && revision >= 0)
                {
                    build += "." + revision.ToString(CultureInfo.InvariantCulture);
                }

                return (displayVersion, build);
            }
            catch (System.Security.SecurityException)
            {
                return (null, null);
            }
            catch (System.UnauthorizedAccessException)
            {
                return (null, null);
            }
        }

        private static string ReadCpuName()
        {
            if (Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0",
                    "ProcessorNameString",
                    null) is string cpuName && !string.IsNullOrWhiteSpace(cpuName))
            {
                return cpuName.Trim();
            }

            return null;
        }

        private static ProcessorInfo ReadProcessor()
        {
            string name = ReadCpuName();
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");
                using var processors = searcher.Get();
                ulong totalCores = 0;
                ulong totalThreads = 0;
                bool coresKnown = true;
                bool threadsKnown = true;

                foreach (ManagementObject processor in processors)
                {
                    using (processor)
                    {
                        if (processor["NumberOfCores"] is uint cores && cores > 0)
                        {
                            totalCores += cores;
                        }
                        else
                        {
                            coresKnown = false;
                        }

                        if (processor["NumberOfLogicalProcessors"] is uint threads && threads > 0)
                        {
                            totalThreads += threads;
                        }
                        else
                        {
                            threadsKnown = false;
                        }
                    }
                }

                return new ProcessorInfo(name,
                    coresKnown && totalCores > 0 ? totalCores : null,
                    threadsKnown && totalThreads > 0 ? totalThreads : null);
            }
            catch (ManagementException)
            {
                return new ProcessorInfo(name, null, null);
            }
            catch (System.UnauthorizedAccessException)
            {
                return new ProcessorInfo(name, null, null);
            }
        }

        private static GraphicsInfo ReadGraphics()
        {
            var names = new List<string>();
            bool queryFailed = false;
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
                            names.Add(gpuName.Trim());
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                queryFailed = true;
            }
            catch (System.UnauthorizedAccessException)
            {
                queryFailed = true;
            }

            return new GraphicsInfo(names, queryFailed);
        }

        private static MemoryInfo ReadMemory()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Capacity, SMBIOSMemoryType, ConfiguredClockSpeed FROM Win32_PhysicalMemory");
                using var modules = searcher.Get();
                ulong totalBytes = 0;
                int moduleCount = 0;
                var types = new SortedSet<string>();
                var configuredSpeeds = new SortedSet<uint>();

                foreach (ManagementObject module in modules)
                {
                    using (module)
                    {
                        moduleCount++;
                        if (module["Capacity"] is ulong capacity && capacity > 0)
                        {
                            totalBytes += capacity;
                            if (module["ConfiguredClockSpeed"] is uint speed && speed > 0)
                            {
                                configuredSpeeds.Add(speed);
                            }

                            if (module["SMBIOSMemoryType"] is uint memoryType)
                            {
                                string type = GetRamType(memoryType);
                                if (!string.IsNullOrEmpty(type))
                                {
                                    types.Add(type);
                                }
                            }
                        }
                    }
                }

                return new MemoryInfo(totalBytes, moduleCount, new List<string>(types),
                    new List<uint>(configuredSpeeds), false);
            }
            catch (ManagementException)
            {
                return new MemoryInfo(0, 0, System.Array.Empty<string>(), System.Array.Empty<uint>(), true);
            }
            catch (System.UnauthorizedAccessException)
            {
                return new MemoryInfo(0, 0, System.Array.Empty<string>(), System.Array.Empty<uint>(), true);
            }
        }

        private static NetworkInfo ReadNetwork()
        {
            var adapterInfos = new List<NetworkAdapterInfo>();
            bool queryFailed = false;
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\StandardCimv2",
                    "SELECT InterfaceDescription, InterfaceType FROM MSFT_NetAdapter WHERE HardwareInterface = TRUE");
                using var adapters = searcher.Get();

                foreach (ManagementObject adapter in adapters)
                {
                    using (adapter)
                    {
                        if (adapter["InterfaceDescription"] is string name
                            && !string.IsNullOrWhiteSpace(name))
                        {
                            var type = adapter["InterfaceType"] is uint interfaceType
                                ? (NetworkInterfaceType)interfaceType
                                : NetworkInterfaceType.Unknown;
                            adapterInfos.Add(new NetworkAdapterInfo(name.Trim(), type));
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                queryFailed = true;
            }
            catch (System.UnauthorizedAccessException)
            {
                queryFailed = true;
            }
            catch (COMException)
            {
                queryFailed = true;
            }

            adapterInfos.Sort((left, right) => System.StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name));
            return new NetworkInfo(adapterInfos, queryFailed);
        }

        private static StorageInfo ReadStorage()
        {
            var disks = new List<DiskInfo>();
            bool queryFailed = false;
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\Microsoft\Windows\Storage",
                    "SELECT FriendlyName, MediaType, BusType, Size FROM MSFT_PhysicalDisk "
                    + "WHERE BusType <> 7 AND (MediaType = 3 OR MediaType = 4)");
                using var physicalDisks = searcher.Get();

                foreach (ManagementObject disk in physicalDisks)
                {
                    using (disk)
                    {
                        if (disk["MediaType"] is ushort mediaType
                            && disk["BusType"] is ushort busType)
                        {
                            string name = (disk["FriendlyName"] as string)?.Trim();
                            ulong size = disk["Size"] is ulong diskSize ? diskSize : 0;
                            disks.Add(new DiskInfo(name, mediaType, busType, size));
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                queryFailed = true;
            }
            catch (System.UnauthorizedAccessException)
            {
                queryFailed = true;
            }
            catch (COMException)
            {
                queryFailed = true;
            }

            disks.Sort((left, right) => System.StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name));
            return new StorageInfo(disks, queryFailed);
        }

        private static string GetRamType(uint memoryType)
        {
            return memoryType switch
            {
                18 => "DDR",
                19 => "DDR2",
                20 => "DDR2 FB-DIMM",
                24 => "DDR3",
                26 => "DDR4",
                27 => "LPDDR",
                28 => "LPDDR2",
                29 => "LPDDR3",
                30 => "LPDDR4",
                34 => "DDR5",
                35 => "LPDDR5",
                _ => ""
            };
        }
    }
}
