using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary
{
    public static class DeviceConstant
    {
        public const int MaxTipCount = 96;
        public const int MaxWellCount = 17;
        public const int SampleCountPerChannel = 4;

        public static int StripInterval = 1020;//1433

        public static int BaseTipType = 2;

        public static int MpBaseTipType = 0;

        public static int PcrIntervalX = 135;//350
        public static int PcrIntervalY = 5900;
        public static int TubeIntervalX = 1433;//20mm
        public static string ModuleConfigFileName = "ModuleConfig.json";

        public static int GetStep(double millimeter, bool isZ = true)
        {
            if (isZ)
            {
                return (int)(millimeter * 3200 / 6.35);
            }
            else
            {
                return (int)(millimeter * 3200 / (14.22 * Math.PI));
            }

        }
        public static int GetHandleZStep(double millimeter)
        {
            return (int)(millimeter * 3200 / 12.0);

        }

        public static int GetXHandleZStep(double millimeter)
        {
            return (int)(millimeter * 3200 / 1.0);

        }

        public static int GetHandleYStep(double millimeter)
        {
            return (int)(millimeter * 3200 / 2.0);

        }

        public static int GetLiqHaZStep(double millimeter)
        {
            return (int)(millimeter * 3200 / 38.1);
        }

        public static int GetLiqHaXStep(double millimeter)
        {
            return (int)(millimeter * 3200 / 59.3);
        }

        public static double ParseAppConfToDbl(string section, double def)
        {
            var str = ConfigurationManager.AppSettings[section];
            double value = 0d;
            if (!double.TryParse(str, out value))
            {
                value = def;
            }
            return value;
        }

        public static List<int> PraseAppConfToList(string section)
        {
            var list = new List<int>();
            var str = ConfigurationManager.AppSettings[section];
            if (!string.IsNullOrEmpty(str))
            {
                var strArray = str.Split(new char[] { ',' });
                foreach (var s in strArray)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        var value = 0;
                        if (int.TryParse(s, out value))
                        {
                            list.Add(value);
                        }
                    }
                }
            }
            return list;
        }

        public static List<string> PraseAppConfToStrList(string section)
        {
            var list = new List<string>();
            var str = ConfigurationManager.AppSettings[section];
            if (!string.IsNullOrEmpty(str))
            {
                var strArray = str.Split(new char[] { ',' });
                foreach (var s in strArray)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        list.Add(s);
                    }
                }
            }
            return list;
        }

        public static bool SaveKey(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            return true;
        }

        public static bool PraseAppConfToBool(string section, bool def)
        {
            var str = ConfigurationManager.AppSettings[section];
            var val = def;
            bool val2;
            if (Boolean.TryParse(str, out val2))
            {
                val = val2;
            }
            return val;
        }

        public static int PraseAppConfToInt(string section, int def)
        {
            var str = ConfigurationManager.AppSettings[section];
            var value = 0;
            if (!int.TryParse(str, out value))
            {
                value = def;
            }
            return value;
        }
    }
}
