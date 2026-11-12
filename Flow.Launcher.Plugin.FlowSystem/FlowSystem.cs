using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowSystem
{
    public class FlowSystem : IPlugin, IPluginI18n
    {
        private IPublicAPI _api;
        private PluginStrings _strings;
        private SystemInfo _systemInfo;
        private SystemInfoText _displayText;

        public void Init(PluginInitContext context)
        {
            _api = context.API;
            _strings = new PluginStrings(context.API, CultureInfo.CurrentUICulture);
            _systemInfo = SystemInfoReader.Read();
            UpdateLocalizedText();
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
            UpdateLocalizedText();
        }

        private void UpdateLocalizedText()
        {
            _displayText = SystemInfoFormatter.Format(_systemInfo, _strings);
        }

        public List<Result> Query(Query query)
        {
            string search = query.Search.Trim();
            if (search.Length > 0 && "copy".StartsWith(search, StringComparison.OrdinalIgnoreCase))
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = _strings.Copy,
                        SubTitle = _strings.CopyDescription,
                        Glyph = new GlyphInfo("/Resources/#Segoe Fluent Icons", "\uE8C8"),
                        AddSelectedCount = false,
                        Action = _ =>
                        {
                            _api.CopyToClipboard(
                                SystemInfoFormatter.FormatSummary(_displayText, _strings),
                                directCopy: false, showDefaultNotification: true);
                            return true;
                        }
                    }
                };
            }

            if (search.Length > 0 && "reload".StartsWith(search, StringComparison.OrdinalIgnoreCase))
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = _strings.Reload,
                        SubTitle = _strings.ReloadDescription,
                        Glyph = new GlyphInfo("/Resources/#Segoe Fluent Icons", "\uE72C"),
                        AddSelectedCount = false,
                        AsyncAction = async _ =>
                        {
                            _systemInfo = await Task.Run(SystemInfoReader.Read);
                            UpdateLocalizedText();
                            _api.ChangeQuery(query.ActionKeyword == "*" ? string.Empty : query.ActionKeyword, true);
                            _api.ShowMsg("FlowSystem", _strings.ReloadSuccess, "icon.png");
                            return false;
                        }
                    }
                };
            }

            SystemInfoText text = _displayText;
            var results = new List<Result>
            {
                new Result
                {
                    Title = text.CpuName,
                    SubTitle = text.CpuDescription,
                    Glyph = new GlyphInfo(
                        "/Resources/#Segoe Fluent Icons",
                        "\uE950"
                    )
                }
            };

            foreach (string gpuName in text.GpuNames)
            {
                results.Add(new Result
                {
                    Title = gpuName,
                    SubTitle = text.GpuDescription,
                    Glyph = new GlyphInfo(
                        "/Resources/#Segoe Fluent Icons",
                        "\uE7F4"
                    )
                });
            }

            results.Add(new Result
            {
                Title = text.RamText,
                SubTitle = text.RamDescription,
                Glyph = new GlyphInfo(
                    "/Resources/#Segoe Fluent Icons",
                    "\uE964"
                )
            });

            foreach (NetworkAdapterText adapter in text.NetworkAdapters)
            {
                results.Add(new Result
                {
                    Title = adapter.Name,
                    SubTitle = adapter.Description,
                    Glyph = new GlyphInfo(
                        "/Resources/#Segoe Fluent Icons",
                        adapter.Glyph
                    )
                });
            }

            foreach (DiskText disk in text.Disks)
            {
                results.Add(new Result
                {
                    Title = disk.Name,
                    SubTitle = disk.Description,
                    Glyph = new GlyphInfo(
                        "/Resources/#Segoe Fluent Icons",
                        "\uEDA2"
                    )
                });
            }

            results.Add(new Result
            {
                Title = text.OsName,
                SubTitle = text.OsDescription,
                Glyph = new GlyphInfo(
                    "/Resources/#Segoe Fluent Icons",
                    "\uE977"
                )
            });

            results.Add(new Result
            {
                Title = text.Architecture,
                SubTitle = text.ArchitectureDescription,
                Glyph = new GlyphInfo(
                    "/Resources/#Segoe Fluent Icons",
                    "\uE770"
                )
            });

            results.Add(new Result
            {
                Title = text.SecureBootText,
                SubTitle = text.DeviceSecurityDescription,
                Glyph = new GlyphInfo(
                    "/Resources/#Segoe Fluent Icons",
                    "\uEA18"
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
