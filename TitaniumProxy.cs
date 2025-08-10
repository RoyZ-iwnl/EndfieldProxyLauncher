using System.Net;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace ProxyLauncher
{
    public class TitaniumProxy
    {
        private readonly ProxyServer _proxyServer;
        private readonly ProxyConfig _config;
        private ExplicitProxyEndPoint? _explicitEndPoint;

        public TitaniumProxy(ProxyConfig config)
        {
            _config = config;
            _proxyServer = new ProxyServer(false); // false表示我们手动管理系统代理
            _proxyServer.CertificateManager.EnsureRootCertificate();
        }

        public void Start()
        {
            try
            {
                LogManager.WriteLine(LanguageManager.GetText("StartingTitaniumProxy"));
                _proxyServer.BeforeRequest += OnRequest;

                // 创建代理端点，并将第三个参数设置为 true 来启用 SSL/TLS 解密
                _explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, _config.proxyPort, true);
                _proxyServer.AddEndPoint(_explicitEndPoint);

                // 先启动代理服务器
                _proxyServer.Start();

                // 设置为系统代理，这是捕获流量的必要步骤
                _proxyServer.SetAsSystemHttpProxy(_explicitEndPoint);
                _proxyServer.SetAsSystemHttpsProxy(_explicitEndPoint);

                LogManager.WriteLine(LanguageManager.GetText("TitaniumProxyStarted") + _config.proxyPort + LanguageManager.GetText("SetAsSystemProxy"));
            }
            catch (Exception ex)
            {
                LogManager.WriteLine(LanguageManager.GetText("TitaniumProxyStartFailed") + ex.Message);
                _proxyServer.DisableSystemHttpProxy();
                _proxyServer.DisableSystemHttpsProxy();
                MessageBox.Show(
                    LanguageManager.GetText("ProxyStartFailed") + ex.ToString(),
                    LanguageManager.GetText("CriticalError"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
        }

        public ProxyServer GetProxyServer()
        {
            return _proxyServer;
        }

        public void Stop()
        {
            LogManager.WriteLine(LanguageManager.GetText("StoppingTitaniumProxy"));
            if (_proxyServer.ProxyRunning)
            {
                _proxyServer.BeforeRequest -= OnRequest;
                // 停止时取消系统代理设置
                _proxyServer.DisableSystemHttpProxy();
                _proxyServer.DisableSystemHttpsProxy();
                _proxyServer.Stop();
            }
            LogManager.WriteLine(LanguageManager.GetText("TitaniumProxyStopped"));
        }

        private Task OnRequest(object sender, SessionEventArgs e)
        {
            string originalUrl = e.HttpClient.Request.Url;
            string originalHost = e.HttpClient.Request.RequestUri.Host;

            // 1. 显式拦截黑名单中的URL，直接阻止循环
            if (originalUrl.Contains("gateBulletin") || originalUrl.Contains("gameBulletin") || originalUrl.Contains("batch_event"))
            {
                LogManager.WriteLine(LanguageManager.GetText("InterceptBlacklistUrl") + originalUrl);
                e.TerminateSession();
                return Task.CompletedTask;
            }

            // 2. 检查请求是否为目标请求 (URL包含"meta" 或 host是目标域名)
            bool isTargetRequest = originalUrl.Contains("meta") ||
                _config.targetDomains.Any(domain => originalHost.Contains(domain, StringComparison.OrdinalIgnoreCase));

            if (isTargetRequest)
            {
                // 对于目标请求，首先放行CONNECT方法，让HTTPS隧道建立
                if (e.HttpClient.Request.Method.Equals("CONNECT", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.CompletedTask;
                }

                LogManager.WriteLine(LanguageManager.GetText("CapturedTargetRequest") + originalUrl);

                // 将原始信息注入Cookie
                string existingCookie = string.Empty;
                var cookieHeader = e.HttpClient.Request.Headers.GetFirstHeader("Cookie");
                if (cookieHeader != null)
                {
                    existingCookie = cookieHeader.Value;
                }

                string injectionCookie = $"OriginalHost={originalHost};OriginalUrl={originalUrl}";
                string newCookie = injectionCookie;
                if (!string.IsNullOrEmpty(existingCookie))
                {
                    newCookie = existingCookie.TrimEnd(';') + ";" + injectionCookie;
                }

                e.HttpClient.Request.Headers.RemoveHeader("Cookie");
                e.HttpClient.Request.Headers.AddHeader("Cookie", newCookie);
                LogManager.WriteLine(LanguageManager.GetText("InjectedCookie") + injectionCookie);

                // 3. 修改请求，使其指向本地服务器
                var newUriBuilder = new UriBuilder(originalUrl)
                {
                    Scheme = "http",                      // 强制使用HTTP协议
                    Host = _config.redirectHost,          // 重定向到本地
                    Port = _config.redirectPort           // 使用配置的端口
                };

                e.HttpClient.Request.RequestUri = newUriBuilder.Uri;

                // 更新Host头以匹配新的目标
                e.HttpClient.Request.Headers.RemoveHeader("Host");
                e.HttpClient.Request.Headers.AddHeader("Host", $"{_config.redirectHost}:{_config.redirectPort}");
                LogManager.WriteLine(LanguageManager.GetText("RedirectedRequest") + e.HttpClient.Request.RequestUri);

                return Task.CompletedTask;
            }

            // 对于所有其他不相关的请求，我们不再拦截，而是直接放行。
            LogManager.WriteLine(LanguageManager.GetText("PassThroughNonTargetRequest") + originalUrl);
            return Task.CompletedTask;
        }
    }
}