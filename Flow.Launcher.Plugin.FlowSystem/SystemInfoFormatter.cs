using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Flow.Launcher.Plugin.FlowSystem
{
    internal static class SystemInfoFormatter
    {
        public static SystemInfoText Format(SystemInfo info, PluginStrings strings)
        {
            return new SystemInfoText(
                info.Processor.Name ?? strings.CpuUnknown,
                FormatProcessor(info.Processor, strings),
                FormatGraphics(info.Graphics, strings),
                strings.GraphicsCard,
                FormatMemory(info.Memory, strings),
                strings.InstalledMemory,
                FormatNetwork(info.Network, strings),
                FormatStorage(info.Storage, strings),
                info.OperatingSystem.Name ?? (info.OperatingSystem.NameQueryFailed
                    ? strings.OsQueryFailed : strings.OsUnknown),
                FormatOperatingSystem(info.OperatingSystem, strings),
                info.OperatingSystem.Architecture,
                strings.SystemArchitecture);
        }

        private static string FormatProcessor(ProcessorInfo processor, PluginStrings strings)
        {
            var parts = new List<string> { strings.Processor };
            if (processor.Cores is ulong cores && cores > 0)
            {
                parts.Add(cores.ToString(strings.Culture) + " "
                    + (cores == 1 ? strings.Core : strings.Cores));
            }

            if (processor.Threads is ulong threads && threads > 0)
            {
                parts.Add(threads.ToString(strings.Culture) + " "
                    + (threads == 1 ? strings.Thread : strings.Threads));
            }

            return string.Join(" · ", parts);
        }

        private static IReadOnlyList<string> FormatGraphics(GraphicsInfo graphics, PluginStrings strings)
        {
            var names = new List<string>(graphics.Names);
            if (graphics.QueryFailed)
            {
                names.Add(strings.GpuQueryFailed);
            }

            if (names.Count == 0)
            {
                names.Add(strings.GpuUnknown);
            }

            return names;
        }

        private static string FormatMemory(MemoryInfo memory, PluginStrings strings)
        {
            if (memory.QueryFailed)
            {
                return strings.RamQueryFailed;
            }

            if (memory.TotalBytes == 0)
            {
                return strings.RamUnknown;
            }

            double totalGiB = memory.TotalBytes / (1024d * 1024 * 1024);
            string text = totalGiB.ToString("0.##", strings.Culture) + " GB";
            return memory.Types.Count > 0
                ? text + " " + string.Join("/", memory.Types)
                : text;
        }

        private static IReadOnlyList<NetworkAdapterText> FormatNetwork(NetworkInfo network, PluginStrings strings)
        {
            var adapters = new List<NetworkAdapterText>();
            foreach (NetworkAdapterInfo adapter in network.Adapters)
            {
                (string type, string glyph) = adapter.InterfaceType switch
                {
                    NetworkInterfaceType.Wireless80211 => (strings.WirelessLan, "\uE701"),
                    NetworkInterfaceType.Ethernet or NetworkInterfaceType.FastEthernetT
                        or NetworkInterfaceType.FastEthernetFx or NetworkInterfaceType.GigabitEthernet
                        => (strings.Ethernet, "\uE839"),
                    _ => (null, "\uEDA3")
                };

                string description = type == null
                    ? strings.NetworkAdapter
                    : strings.NetworkAdapter + " · " + type;
                adapters.Add(new NetworkAdapterText(adapter.Name, description, glyph));
            }

            if (network.QueryFailed)
            {
                adapters.Add(new NetworkAdapterText(strings.NetworkQueryFailed, strings.NetworkAdapter, "\uEDA3"));
            }

            if (adapters.Count == 0)
            {
                adapters.Add(new NetworkAdapterText(strings.NetworkUnknown, strings.NetworkAdapter, "\uEDA3"));
            }

            return adapters;
        }

        private static IReadOnlyList<DiskText> FormatStorage(StorageInfo storage, PluginStrings strings)
        {
            var disks = new List<DiskText>();
            foreach (DiskInfo disk in storage.Disks)
            {
                if (disk.BusType == 7 || (disk.MediaType != 3 && disk.MediaType != 4))
                {
                    continue;
                }

                var parts = new List<string> { disk.MediaType == 4 ? "SSD" : "HDD" };
                string connection = disk.BusType switch
                {
                    1 => "SCSI",
                    3 => "ATA",
                    8 => "RAID",
                    10 => "SAS",
                    11 => "SATA",
                    17 => "NVMe",
                    _ => null
                };

                if (connection != null)
                {
                    parts.Add(connection);
                }

                if (disk.Size > 0)
                {
                    parts.Add(FormatDiskSize(disk.Size, strings));
                }

                string name = string.IsNullOrWhiteSpace(disk.Name) ? strings.StorageDevice : disk.Name;
                disks.Add(new DiskText(name, string.Join(" · ", parts)));
            }

            if (storage.QueryFailed)
            {
                disks.Add(new DiskText(strings.StorageQueryFailed, strings.StorageDevice));
            }

            return disks;
        }

        private static string FormatDiskSize(ulong bytes, PluginStrings strings)
        {
            if (bytes >= 1_000_000_000_000UL)
            {
                return (bytes / 1_000_000_000_000d).ToString("0.##", strings.Culture) + " TB";
            }

            if (bytes >= 1_000_000_000UL)
            {
                return (bytes / 1_000_000_000d).ToString("0.##", strings.Culture) + " GB";
            }

            return (bytes / 1_000_000d).ToString("0.##", strings.Culture) + " MB";
        }

        private static string FormatOperatingSystem(OperatingSystemInfo os, PluginStrings strings)
        {
            var parts = new List<string> { strings.OperatingSystem };
            if (!string.IsNullOrWhiteSpace(os.DisplayVersion))
            {
                parts.Add(os.DisplayVersion);
            }

            if (!string.IsNullOrWhiteSpace(os.Build))
            {
                parts.Add(strings.Build + " " + os.Build);
            }

            return string.Join(" · ", parts);
        }
    }

    internal sealed record SystemInfoText(
        string CpuName,
        string CpuDescription,
        IReadOnlyList<string> GpuNames,
        string GpuDescription,
        string RamText,
        string RamDescription,
        IReadOnlyList<NetworkAdapterText> NetworkAdapters,
        IReadOnlyList<DiskText> Disks,
        string OsName,
        string OsDescription,
        string Architecture,
        string ArchitectureDescription);

    internal sealed record DiskText(string Name, string Description);

    internal sealed record NetworkAdapterText(string Name, string Description, string Glyph);
}
