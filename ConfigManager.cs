using Newtonsoft.Json;

namespace ProxyLauncher
{
    public class ProxyConfig
    {
        public int proxyPort { get; set; } = 8899;
        public string redirectHost { get; set; } = "127.0.0.1";
        public int redirectPort { get; set; } = 5000;

        // 确保targetDomains有默认值，防止null引用
        public List<string> targetDomains { get; set; } = new List<string>
        {
            "gryphline.com",
            "hg-cdn.com",
            "hypergryph.com"
        };
    }
    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        /// <summary>
        /// 加载配置文件，如果文件不存在或损坏则创建默认配置
        /// </summary>
        public static ProxyConfig LoadConfig()
        {
            try
            {
                // 运行前检查，没有json文件则生成一个
                if (!File.Exists(ConfigPath))
                {
                    LogManager.WriteLine("配置文件不存在，创建默认配置文件。");
                    return CreateAndSaveDefault();
                }

                string json = File.ReadAllText(ConfigPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    LogManager.WriteLine("配置文件为空，创建并使用默认配置。");
                    return CreateAndSaveDefault();
                }

                var config = JsonConvert.DeserializeObject<ProxyConfig>(json);
                if (config == null)
                {
                    LogManager.WriteLine("配置文件反序列化失败，创建并使用默认配置。");
                    return CreateAndSaveDefault();
                }

                // 验证配置的完整性
                ValidateAndFixConfig(config);
                LogManager.WriteLine("配置文件加载成功。");
                return config;
            }
            catch (Exception ex)
            {
                // 当JSON文件损坏或格式错误时，向用户弹出明确提示
                LogManager.WriteLine($"加载 config.json 失败: {ex.Message}。文件可能已损坏。将创建并使用默认配置。");
                MessageBox.Show(
                    $"加载配置文件 'config.json' 失败，文件可能已损坏或格式不正确。\n\n错误详情: {ex.Message}\n\n程序将使用默认设置，并已生成一份新的默认配置文件。",
                    "配置加载错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // 返回一份默认配置，并覆盖掉损坏的文件
                return CreateAndSaveDefault();
            }
        }

        /// <summary>
        /// 验证并修复配置的完整性
        /// </summary>
        private static void ValidateAndFixConfig(ProxyConfig config)
        {
            bool needsSave = false;

            // 确保基本配置有效
            if (config.proxyPort <= 0 || config.proxyPort > 65535)
            {
                config.proxyPort = 8899;
                needsSave = true;
            }

            if (config.redirectPort <= 0 || config.redirectPort > 65535)
            {
                config.redirectPort = 5000;
                needsSave = true;
            }

            if (string.IsNullOrWhiteSpace(config.redirectHost))
            {
                config.redirectHost = "127.0.0.1";
                needsSave = true;
            }

            // 确保目标域名列表不为空
            if (config.targetDomains == null || config.targetDomains.Count == 0)
            {
                config.targetDomains = new List<string>
                {
                    "gryphline.com",
                    "hg-cdn.com",
                    "hypergryph.com"
                };
                needsSave = true;
            }

            // 如果配置被修复，保存文件
            if (needsSave)
            {
                SaveConfig(config);
                LogManager.WriteLine("配置文件已自动修复并保存。");
            }
        }

        /// <summary>
        /// 创建并保存默认配置
        /// </summary>
        private static ProxyConfig CreateAndSaveDefault()
        {
            var defaultConfig = new ProxyConfig
            {
                proxyPort = 8899,
                redirectHost = "127.0.0.1", 
                redirectPort = 5000,
                targetDomains = new List<string>
                {
                    "gryphline.com",
                    "hg-cdn.com",
                    "hypergryph.com"
                }
            };
            
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public static void SaveConfig(ProxyConfig config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
                LogManager.WriteLine("配置已保存到: " + ConfigPath);
            }
            catch (Exception ex)
            {
                LogManager.WriteLine($"保存配置失败: {ex.Message}");
                MessageBox.Show(
                    $"保存配置文件失败: {ex.Message}",
                    "保存错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        public static string GetConfigPath()
        {
            return ConfigPath;
        }

        /// <summary>
        /// 检查配置文件是否存在
        /// </summary>
        public static bool ConfigExists()
        {
            return File.Exists(ConfigPath);
        }
    }
}