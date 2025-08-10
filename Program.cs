using System.Diagnostics;
using System.Security.Principal;

namespace ProxyLauncher
{
    static class Program
    {
        public static ProxyConfig? Config;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // 检查是否以管理员身份运行
            if (!IsRunAsAdministrator())
            {
                RestartAsAdministrator();
                return;
            }

            // 初始化配置和语言
            Config = ConfigManager.LoadConfig();
            LanguageManager.Initialize();
            
            Application.Run(new MainForm());
        }

        // 检查是否以管理员身份运行
        private static bool IsRunAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // 重新以管理员身份启动程序
        private static void RestartAsAdministrator()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas" // 请求管理员权限
                };
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"需要管理员权限才能运行此程序。\n错误: {ex.Message}",
                    "权限错误", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
    public static class Launcher
    {
        /// <summary>
        /// 启动一个新进程，并为其设置代理环境变量。
        /// </summary>
        /// <param name="exePath">可执行文件路径</param>
        /// <param name="proxyAddress">代理地址，例如 "http://127.0.0.1:8899"</param>
        /// <returns>启动的进程对象，如果启动失败则返回null</returns>
        public static Process? StartProcessWithProxy(string exePath, string proxyAddress)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = false // 如果需要，可以设置为 true 来隐藏目标程序窗口
                };

                // 为新进程设置代理环境变量，这是最常用且兼容性较好的方式
                startInfo.EnvironmentVariables["HTTP_PROXY"] = proxyAddress;
                startInfo.EnvironmentVariables["HTTPS_PROXY"] = proxyAddress;
                startInfo.EnvironmentVariables["ALL_PROXY"] = proxyAddress;

                LogManager.WriteLine(LanguageManager.GetText("PreparingToStartProcess") + exePath);
                LogManager.WriteLine(LanguageManager.GetText("ProxyAddress") + proxyAddress);

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    LogManager.WriteLine(LanguageManager.GetText("ProcessStartedSuccessfully") + process.Id);
                }
                else
                {
                    LogManager.WriteLine(LanguageManager.GetText("ProcessStartFailedNull"));
                }

                return process; // 可能为null
            }
            catch (Exception ex)
            {
                LogManager.WriteLine(LanguageManager.GetText("ProcessStartCriticalError") + ex.Message);
                // 将异常向上抛出，让调用者（如UI线程）知道启动失败并处理
                throw;
            }
        }
    }
}