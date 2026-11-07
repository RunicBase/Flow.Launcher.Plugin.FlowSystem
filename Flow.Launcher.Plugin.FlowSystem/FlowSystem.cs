using System.Collections.Generic;
using System.Globalization;

namespace Flow.Launcher.Plugin.FlowSystem
{
    public class FlowSystem : IPlugin, IPluginI18n
    {
        private PluginStrings _strings;
        private string _osArchitecture;
        private string _cpuName;
        private string _ramText;
        private List<string> _gpuNames = new List<string>();

        public void Init(PluginInitContext context)
        {
            _strings = new PluginStrings(context.API, CultureInfo.CurrentUICulture);
            LoadSystemInfo();
        }

        public string GetTranslatedPluginTitle()
        {
            return "FlowSystem";
        }

        public string GetTranslatedPluginDescription()
        {
            return _strings.PluginDescription;
        }

        public void OnCultureInfoChanged(CultureInfo newCulture)
        {
            _strings.Culture = newCulture;
            LoadSystemInfo();
        }

        private void LoadSystemInfo()
        {
            _osArchitecture = SystemInfoReader.ReadOsArchitecture();
            _cpuName = SystemInfoReader.ReadCpuName(_strings);
            _gpuNames = SystemInfoReader.ReadGpuNames(_strings);
            _ramText = SystemInfoReader.ReadRamText(_strings);
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>
            {
                new Result
                {
                    Title = _cpuName ?? _strings.CpuUnknown,
                    SubTitle = _strings.Processor,
                    Glyph = new GlyphInfo(
                        "/Resources/#Segoe Fluent Icons",
                        "\uE950"
                    )
                }
            };

            foreach (string gpuName in _gpuNames)
            {
                results.Add(new Result
                {
                    Title = gpuName,
                    SubTitle = _strings.GraphicsCard,
                    Glyph = new GlyphInfo(
                        "/Resources/#Segoe Fluent Icons",
                        "\uE7F4"
                    )
                });
            }

            results.Add(new Result
            {
                Title = _ramText ?? _strings.RamUnknown,
                SubTitle = _strings.InstalledMemory,
                Glyph = new GlyphInfo(
                    "/Resources/#Segoe Fluent Icons",
                    "\uE964"
                )
            });

            results.Add(new Result
            {
                Title = _osArchitecture,
                SubTitle = _strings.SystemArchitecture,
                Glyph = new GlyphInfo(
                    "/Resources/#Segoe Fluent Icons",
                    "\uE770"
                )
            });

            for (int i = 0; i < results.Count; i++)
            {
                results[i].Score = results.Count - i;
                results[i].AddSelectedCount = false;
            }

            return results;
        }
    }
}
