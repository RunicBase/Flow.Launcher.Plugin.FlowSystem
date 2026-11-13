FlowSystem
==================

A plugin for [Flow Launcher](https://github.com/Flow-Launcher/Flow.Launcher) that provides quick access to Windows system and hardware information.

## ✨ Features

- CPU name, core count, and thread count
- Graphics card names
- Installed RAM capacity, memory type (such as DDR4 or DDR5), module count, and configured speed in MT/s
- Physical network adapters with Wi-Fi or Ethernet labels
- SSDs and HDDs with connection type and capacity (excluding USB drives)
- Windows version, build number, and system architecture
- Secure Boot status
- Copy the complete system overview to the clipboard as localized plain text

## ⌨️ Usage

    sys

The device security entry shows the Secure Boot state reported by Windows in the
registry without requesting administrator privileges. If the value is missing or
cannot be read, the status is shown as unavailable rather than disabled.

To refresh the cached hardware and system information, enter:

    sys reload

Press Enter to reload the data. FlowSystem then shows the updated system
overview.

To copy the cached hardware and system information, enter:

    sys copy

Press Enter to copy the overview to the clipboard. Each component gets its own
line, including separate lines for multiple graphics cards, network adapters, and
drives. Labels follow the language selected in Flow Launcher.

## 🌍 Languages

The plugin follows the language selected in Flow Launcher and supports German,
English, Dutch, Spanish, and French, including language changes while Flow Launcher
is running.
Other languages fall back to English.

## 📋 Planned

- add support for more languages
- show device name
- add battery information for supported devices
- display GPU memory information (dedicated and shared VRAM)

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
