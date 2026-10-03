using BepInEx.Configuration;
using MSU.Config;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.IO;

namespace FortunesFromTheScrapyard
{
    public sealed class FFTSConfigureFieldAttribute : RiskOfOptionsConfigureFieldAttribute
    {
        private readonly float? minimum;
        private readonly float? maximum;

        public bool restartRequired { get; set; }

        public FFTSConfigureFieldAttribute(string fileIdentifier) : base(fileIdentifier)
        {
        }

        public FFTSConfigureFieldAttribute(string fileIdentifier, float minimum, float maximum) : base(fileIdentifier)
        {
            if (float.IsNaN(minimum) || float.IsInfinity(minimum) ||
                float.IsNaN(maximum) || float.IsInfinity(maximum) || minimum >= maximum)
                throw new ArgumentException("Option bounds must be finite and the minimum must be less than the maximum.");

            this.minimum = minimum;
            this.maximum = maximum;
        }

        protected override void OnConfigured(ConfigFile configFile, object value)
        {
            BaseOption option;
            if (value is bool)
            {
                option = new CheckBoxOption(GetConfigEntry<bool>(), restartRequired);
            }
            else
            {
                if (!minimum.HasValue || !maximum.HasValue)
                    throw new InvalidOperationException($"Numeric option {attachedMemberInfo.Name} requires slider bounds.");

                switch (value)
                {
                    case float defaultValue:
                        ConfigEntry<float> floatEntry = GetConfigEntry<float>();
                        option = new SliderOption(floatEntry, new SliderConfig
                        {
                            min = Math.Min(minimum.Value, Math.Min(defaultValue, floatEntry.Value)),
                            max = Math.Max(maximum.Value, Math.Max(defaultValue, floatEntry.Value)),
                            FormatString = "{0:0.#####}",
                            restartRequired = restartRequired
                        });
                        break;
                    case int defaultValue:
                        ConfigEntry<int> intEntry = GetConfigEntry<int>();
                        option = new IntSliderOption(intEntry, new IntSliderConfig
                        {
                            min = Math.Min(checked((int)minimum.Value), Math.Min(defaultValue, intEntry.Value)),
                            max = Math.Max(checked((int)maximum.Value), Math.Max(defaultValue, intEntry.Value)),
                            restartRequired = restartRequired
                        });
                        break;
                    default:
                        throw new NotSupportedException($"Option type {value.GetType().Name} is not supported.");
                }
            }

            bool separateEntry = ConfigSystem.ShouldCreateSeparateRiskOfOptionsEntry(configFile);
            string fileName = Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
            string category = fileName.StartsWith(FFTSConfig.PREFIX, StringComparison.Ordinal)
                ? fileName.Substring(FFTSConfig.PREFIX.Length)
                : fileName;
            string guid = separateEntry ? modGUID + "." + fileName : modGUID;
            string name = separateEntry ? modName + " - " + category : modName;
            ModSettingsManager.AddOption(option, guid, name);
        }
    }
}
