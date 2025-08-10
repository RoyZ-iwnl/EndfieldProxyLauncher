using System.Globalization;
using Newtonsoft.Json;

namespace ProxyLauncher
{
    public static class LanguageManager
    {
        private static Dictionary<string, string>? _currentLanguage;
        private static string _currentLanguageCode = "en-US"; // 默认英文

        public static void Initialize()
        {
            try
            {
                string systemLang = CultureInfo.CurrentUICulture.Name;
                if (systemLang.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                    _currentLanguageCode = "zh-CN";
                else
                    _currentLanguageCode = "en-US";
            }
            catch
            {
                _currentLanguageCode = "en-US";
            }
            LoadLanguage(_currentLanguageCode);
        }

        public static void LoadLanguage(string languageCode)
        {
            try
            {
                _currentLanguageCode = languageCode;
                string langDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");
                Directory.CreateDirectory(langDir);
                string langFile = Path.Combine(langDir, $"{languageCode}.json");

                if (!File.Exists(langFile))
                {
                    CreateDefaultLanguageFiles(langDir);
                }

                if (File.Exists(langFile))
                {
                    string json = File.ReadAllText(langFile);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    // 回退保证：任何异常或反序列化失败，使用内置中文
                    _currentLanguage = dict ?? (languageCode == "zh-CN" ? GetDefaultChineseTexts() : GetDefaultEnglishTexts());
                }
                else
                {
                    _currentLanguage = languageCode == "zh-CN" ? GetDefaultChineseTexts() : GetDefaultEnglishTexts();
                }
            }
            catch
            {
                // 强化回退：确保不返回键名
                _currentLanguage = _currentLanguageCode == "zh-CN" ? GetDefaultChineseTexts() : GetDefaultEnglishTexts();
            }
        }

        public static string GetText(string key)
        {
            // 强化：双重回退，避免返回键名
            if (_currentLanguage != null && _currentLanguage.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                return value;

            // 如果当前语言字典缺词，回退到另一种内置语言尝试
            var fallback = _currentLanguageCode == "zh-CN" ? GetDefaultChineseTexts() : GetDefaultEnglishTexts();
            if (fallback.TryGetValue(key, out var fb) && !string.IsNullOrEmpty(fb))
                return fb;

            // 最后兜底：依然返回键名（调试可见）
            return key;
        }

        public static string[] GetAvailableLanguages() => new[] { "zh-CN", "en-US" };

        public static string GetCurrentLanguage() => _currentLanguageCode;

        private static void CreateDefaultLanguageFiles(string langDir)
        {
            try
            {
                var zh = JsonConvert.SerializeObject(GetDefaultChineseTexts(), Formatting.Indented);
                File.WriteAllText(Path.Combine(langDir, "zh-CN.json"), zh);
                var en = JsonConvert.SerializeObject(GetDefaultEnglishTexts(), Formatting.Indented);
                File.WriteAllText(Path.Combine(langDir, "en-US.json"), en);
            }
            catch
            {
                // 忽略文件创建错误，运行时仍使用内置字典
            }
        }

        private static Dictionary<string, string> GetDefaultChineseTexts()
        {
            return new Dictionary<string, string>
            {
                {"AppTitle", "Endfield 代理启动器"},
                {"ConfigInfo", "配置信息"},
                {"EditConfig", "编辑配置"},
                {"TargetProgram", "目标程序"},
                {"ProgramPath", "程序路径:"},
                {"ControlPanel", "控制面板"},
                {"Browse", "浏览..."},
                {"StartProxy", "启动代理"},
                {"StopProxy", "停止代理"},
                {"Status", "状态"},
                {"Ready", "就绪"},
                {"Running", "运行中"},
                {"Stopped", "已停止"},
                {"Language", "语言:"},
                {"ShowLog", "显示日志"},
                {"Error", "错误"},
                {"Warning", "警告"},
                {"Information", "信息"},
                {"Prompt", "提示"},
                {"ProcessStarted", "目标进程已启动"},
                {"ProcessExited", "目标程序已退出"},
                {"LogWindow", "日志窗口"},
                {"Clear", "清空"},
                {"SaveLog", "保存日志"},
                {"LoadingConfig", "配置信息加载中..."},
                {"RedirectTarget", "重定向目标"},
                {"ProxyPort", "代理端口"},
                {"MonitoredDomains", "监控域名"},
                {"ModifyConfigPrompt", "请在修改并保存配置文件后，点击'是'重新加载配置。\n点击'否'将取消本次加载。"},
                {"ConfigReloaded", "配置已成功重新加载！"},
                {"OpenConfigFailed", "打开配置文件失败："},
                {"ExecutableFileFilter", "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*"},
                {"TextFileFilter", "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*"},
                {"NoTargetProgramPrompt", "未选择目标程序，是否仅启动代理？"},
                {"StartingProxyService", "开始启动代理服务..."},
                {"ProxyServiceStarted", "代理服务已启动，端口: "},
                {"TargetProcessStarted", "目标进程已启动: "},
                {"RunningStatus", "运行中 - 代理端口: "},
                {"StartupFailed", "启动失败: "},
                {"StoppingService", "正在停止服务..."},
                {"ProxyServiceStopped", "代理服务已停止"},
                {"TargetProcessTerminated", "目标进程已终止"},
                {"StopServiceError", "停止服务时出错: "},
                {"StartingTitaniumProxy", "正在启动 Titanium-Web-Proxy 代理引擎..."},
                {"TitaniumProxyStarted", "Titanium 代理已在端口 "},
                {"SetAsSystemProxy", " 上成功启动，并已设置为系统代理。"},
                {"TitaniumProxyStartFailed", "启动 Titanium 代理失败: "},
                {"ProxyStartFailed", "启动代理失败: "},
                {"CriticalError", "严重错误"},
                {"StoppingTitaniumProxy", "正在停止 Titanium 代理引擎..."},
                {"TitaniumProxyStopped", "Titanium 代理已停止。"},
                {"InterceptBlacklistUrl", "[拦截] 命中黑名单URL，已拦截: "},
                {"CapturedTargetRequest", ">[捕获] 目标请求: "},
                {"InjectedCookie", ">[修改] 已注入Cookie: "},
                {"RedirectedRequest", ">[重定向] 请求已转发至: "},
                {"PassThroughNonTargetRequest", "[放行] 非目标请求: "},
                {"PreparingToStartProcess", "准备启动进程并设置代理: "},
                {"ProxyAddress", "代理地址: "},
                {"ProcessStartedSuccessfully", "进程已成功启动，PID: "},
                {"ProcessStartFailedNull", "启动进程失败，返回的 process 对象为空。"},
                {"ProcessStartCriticalError", "启动进程时发生严重错误: "},
                {"LogSavedTo", "日志已保存到: "},
                {"SaveLogFailed", "保存日志失败: "},
                {"CertNotInstalledPrompt", "未检测到受信任的拦截根证书，是否现在安装？\n需要管理员权限，安装到“受信任的根证书颁发机构”和“个人”存储。"},
                {"CertInstallFailed", "根证书安装失败或未生效，请以管理员身份运行并重试。"},
                {"CertInstallException", "证书安装过程发生异常："},
                {"CertCreateOrLoadFailed", "无法创建或加载代理根证书。"}
            };
        }

        private static Dictionary<string, string> GetDefaultEnglishTexts()
        {
            return new Dictionary<string, string>
            {
                {"AppTitle", "Endfield Proxy Launcher"},
                {"ConfigInfo", "Configuration Info"},
                {"EditConfig", "Edit Config"},
                {"TargetProgram", "Target Program"},
                {"ProgramPath", "Program Path:"},
                {"ControlPanel", "Control Panel"},
                {"Browse", "Browse..."},
                {"StartProxy", "Start Proxy"},
                {"StopProxy", "Stop Proxy"},
                {"Status", "Status"},
                {"Ready", "Ready"},
                {"Running", "Running"},
                {"Stopped", "Stopped"},
                {"Language", "Language:"},
                {"ShowLog", "Show Log"},
                {"Error", "Error"},
                {"Warning", "Warning"},
                {"Information", "Information"},
                {"Prompt", "Prompt"},
                {"ProcessStarted", "Target process started"},
                {"ProcessExited", "Target program exited"},
                {"LogWindow", "Log Window"},
                {"Clear", "Clear"},
                {"SaveLog", "Save Log"},
                {"LoadingConfig", "Loading configuration..."},
                {"RedirectTarget", "Redirect Target"},
                {"ProxyPort", "Proxy Port"},
                {"MonitoredDomains", "Monitored Domains"},
                {"ModifyConfigPrompt", "Please modify and save the config file, then click 'Yes' to reload.\nClick 'No' to cancel this operation."},
                {"ConfigReloaded", "Configuration reloaded successfully!"},
                {"OpenConfigFailed", "Failed to open config file: "},
                {"ExecutableFileFilter", "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*"},
                {"TextFileFilter", "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"},
                {"NoTargetProgramPrompt", "No target program selected. Start proxy only?"},
                {"StartingProxyService", "Starting proxy service..."},
                {"ProxyServiceStarted", "Proxy service started on port: "},
                {"TargetProcessStarted", "Target process started: "},
                {"RunningStatus", "Running - Proxy Port: "},
                {"StartupFailed", "Startup failed: "},
                {"StoppingService", "Stopping service..."},
                {"ProxyServiceStopped", "Proxy service stopped"},
                {"TargetProcessTerminated", "Target process terminated"},
                {"StopServiceError", "Error stopping service: "},
                {"StartingTitaniumProxy", "Starting Titanium-Web-Proxy engine..."},
                {"TitaniumProxyStarted", "Titanium proxy started successfully on port "},
                {"SetAsSystemProxy", " and set as system proxy."},
                {"TitaniumProxyStartFailed", "Failed to start Titanium proxy: "},
                {"ProxyStartFailed", "Failed to start proxy: "},
                {"CriticalError", "Critical Error"},
                {"StoppingTitaniumProxy", "Stopping Titanium proxy engine..."},
                {"TitaniumProxyStopped", "Titanium proxy stopped."},
                {"InterceptBlacklistUrl", "[INTERCEPT] Blacklisted URL blocked: "},
                {"CapturedTargetRequest", ">[CAPTURE] Target request: "},
                {"InjectedCookie", ">[MODIFY] Cookie injected: "},
                {"RedirectedRequest", ">[REDIRECT] Request forwarded to: "},
                {"PassThroughNonTargetRequest", "[PASS] Non-target request: "},
                {"PreparingToStartProcess", "Preparing to start process with proxy: "},
                {"ProxyAddress", "Proxy address: "},
                {"ProcessStartedSuccessfully", "Process started successfully, PID: "},
                {"ProcessStartFailedNull", "Process start failed, returned null object."},
                {"ProcessStartCriticalError", "Critical error starting process: "},
                {"LogSavedTo", "Log saved to: "},
                {"SaveLogFailed", "Failed to save log: "},
                {"CertNotInstalledPrompt", "No trusted interception root certificate detected. Install now?\nRequires admin rights to install in 'Trusted Root Certification Authorities' and 'Personal' stores."},
                {"CertInstallFailed", "Root certificate installation failed or not effective. Please run as administrator and retry."},
                {"CertInstallException", "An exception occurred during certificate installation: "},
                {"CertCreateOrLoadFailed", "Failed to create or load proxy root certificate."}
            };
        }
    }
}
