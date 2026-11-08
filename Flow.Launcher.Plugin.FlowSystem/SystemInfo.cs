using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Flow.Launcher.Plugin.FlowSystem
{
    internal sealed record SystemInfo(
        OperatingSystemInfo OperatingSystem,
        ProcessorInfo Processor,
        GraphicsInfo Graphics,
        MemoryInfo Memory,
        NetworkInfo Network,
        StorageInfo Storage);

    internal sealed record OperatingSystemInfo(
        string Name,
        string DisplayVersion,
        string Build,
        string Architecture,
        bool NameQueryFailed);

    internal sealed record ProcessorInfo(string Name, ulong? Cores, ulong? Threads);

    internal sealed record GraphicsInfo(IReadOnlyList<string> Names, bool QueryFailed);

    internal sealed record MemoryInfo(
        ulong TotalBytes,
        int ModuleCount,
        IReadOnlyList<string> Types,
        IReadOnlyList<uint> ConfiguredSpeeds,
        bool QueryFailed);

    internal sealed record StorageInfo(IReadOnlyList<DiskInfo> Disks, bool QueryFailed);

    internal sealed record NetworkInfo(IReadOnlyList<NetworkAdapterInfo> Adapters, bool QueryFailed);

    internal sealed record NetworkAdapterInfo(string Name, NetworkInterfaceType InterfaceType);

    internal sealed record DiskInfo(string Name, ushort MediaType, ushort BusType, ulong Size);
}
