using CommLiby;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.Configuration;

namespace CommonLib
{
    public static class ConfigHelper
    {
        /// <summary>
        /// 添加配置文件
        /// </summary>
        /// <param name="name">配置项名称</param>
        /// <param name="value">配置项值</param>
        public static void AddConfig(string name, string value)
        {
            try
            {
                Configuration config = null;
                if (Common.IsWebSite())
                    config = WebConfigurationManager.OpenWebConfiguration(null);
                else
                    config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                var settings = config.AppSettings.Settings;
                if (settings[name] == null)
                {
                    settings.Add(name, value);
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name); //重新加载新的配置文件 
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 修改配置文件
        /// </summary>
        /// <param name="cname">The cname.</param>
        /// <param name="cvalue">The cvalue.</param>
        /// <param name="createKeyAuto"></param>
        public static bool UpdateConfig(string cname, string cvalue, bool createKeyAuto = false)
        {

            try
            {
                Configuration config = null;
                if (Common.IsWebSite())
                    config = WebConfigurationManager.OpenWebConfiguration(null);
                else
                    config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                var settings = config.AppSettings.Settings;
                if (settings[cname] != null)
                {
                    settings[cname].Value = cvalue;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name); //重新加载新的配置文件 
                }
                else
                {
                    if (createKeyAuto)
                        AddConfig(cname, cvalue);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        /// <summary>
        /// 返回配置文件中
        /// </summary>
        /// <param name="key">传入的信息</param>
        /// <returns></returns>
        public static string GetConfigString(string key)
        {
            try
            {
                string objModel = null;
                if (Common.IsWebSite())
                    objModel = WebConfigurationManager.AppSettings[key];
                else
                    objModel = ConfigurationManager.AppSettings[key];

                if (objModel == null || objModel == "-")
                    return "";
                return objModel;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 获取用户配置信息 如果没有词配置项，则自动创建并赋默认值
        /// </summary>
        /// <param name="key">配置项关键字</param>
        /// <param name="defaultValue">配置项默认值</param>
        /// <param name="createKeyAuto">自动创建配置项</param>
        /// <returns></returns>
        public static string GetConfigString(string key, string defaultValue, bool createKeyAuto = false)
        {
            string val = GetConfigString(key);

            if (!string.IsNullOrEmpty(val)) return val;

            if (createKeyAuto)
                AddConfig(key, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 修改配置文件
        /// </summary>
        /// <param name="cname">The cname.</param>
        /// <param name="cvalue">The cvalue.</param>
        /// <param name="createKeyAuto"></param>
        public static bool UpdateConfig(string cname, object cvalue, bool createKeyAuto = false)
        {
            string svalue = "";
            if (cvalue is bool)
            {
                svalue = (bool)cvalue ? "true" : "false";
            }
            if (cvalue is int)
            {
                svalue = cvalue.ToString();
            }
            return UpdateConfig(cname, svalue, createKeyAuto);
        }


        /// <summary>
        /// 得到AppSettings中的配置int信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="createKeyAuto"></param>
        /// <returns></returns>
        public static int GetConfigInt(string key, int defaultValue = 0, bool createKeyAuto = false)
        {
            int result = defaultValue;
            string cfgVal = GetConfigString(key).Trim();
            if (string.IsNullOrEmpty(cfgVal))
            {
                if (createKeyAuto)
                    AddConfig(key, defaultValue.ToString());
            }
            else
            {
                int.TryParse(cfgVal, out result);
            }

            return result;
        }

        /// <summary>
        /// 得到AppSettings中的配置Decimal信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="createKeyAuto"></param>
        /// <returns></returns>
        public static decimal GetConfigDecimal(string key, decimal defaultValue = 0, bool createKeyAuto = false)
        {
            decimal result = defaultValue;
            string cfgVal = GetConfigString(key).Trim();
            if (string.IsNullOrEmpty(cfgVal))
            {
                if (createKeyAuto)
                    AddConfig(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                decimal.TryParse(cfgVal, out result);
            }

            return result;
        }

        /// <summary>
        /// 得到AppSettings中的配置uint信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="createKeyAuto"></param>
        /// <returns></returns>
        public static uint GetConfigUint(string key, uint defaultValue = 0, bool createKeyAuto = false)
        {
            uint result = defaultValue;
            string cfgVal = GetConfigString(key).Trim();
            if (string.IsNullOrEmpty(cfgVal))
            {
                if (createKeyAuto)
                    AddConfig(key, defaultValue.ToString());
            }
            else
            {
                uint.TryParse(cfgVal, out result);
            }

            return result;
        }
        /// <summary>
        /// 得到AppSettings中的配置ushort信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="createKeyAuto"></param>
        /// <returns></returns>
        public static ushort GetConfigUshort(string key, ushort defaultValue = 0, bool createKeyAuto = false)
        {
            ushort result = defaultValue;
            string cfgVal = GetConfigString(key).Trim();
            if (string.IsNullOrEmpty(cfgVal))
            {
                if (createKeyAuto)
                    AddConfig(key, defaultValue.ToString());
            }
            else
            {
                ushort.TryParse(cfgVal, out result);
            }

            return result;
        }

        /// <summary>
        /// 获取用户配置信息 如果没有词配置项，则自动创建并赋默认值
        /// </summary>
        /// <param name="key">配置项关键字</param>
        /// <param name="defaultValue">配置项默认值</param>
        ///<param name="createKeyAuto">自动创建配置项</param>
        /// <returns></returns>
        public static bool GetConfigBool(string key, bool defaultValue = false, bool createKeyAuto = false)
        {
            bool result = defaultValue;
            string cfgVal = GetConfigString(key).Trim();
            if (string.IsNullOrEmpty(cfgVal))
            {
                if (createKeyAuto)
                    AddConfig(key, defaultValue ? "1" : "0");
            }
            else
            {
                int i = 0;
                if (int.TryParse(cfgVal, out i))
                {
                    result = i > 0;
                }
                else
                {
                    result = cfgVal.ToLower() == "true";
                }
            }
            return result;
        }
    }
}
