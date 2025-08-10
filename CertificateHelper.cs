using System.Security.Cryptography.X509Certificates;
using Titanium.Web.Proxy;

namespace ProxyLauncher
{
    public static class CertificateHelper
    {
        // 检查并确保根证书已存在且安装到“受信任的根证书颁发机构”
        // 返回 true 表示已就绪；false 表示用户拒绝或安装失败
        public static bool EnsureTrustedRootCertificate(ProxyServer proxyServer)
        {
            try
            {
                // 1) 确保证书已生成（磁盘存在）
                proxyServer.CertificateManager.EnsureRootCertificate();

                // 2) 读取当前根证书（优先使用 X509Certificate2 的 API）
                X509Certificate2? rootCert = null;
                try
                {
                    // 部分版本提供该方法
                    rootCert = proxyServer.CertificateManager.LoadRootCertificate();
                }
                catch
                {
                    // 若无此方法则回退
                    var cert = proxyServer.CertificateManager.LoadRootCertificate();
                    if (cert is X509Certificate2 c2)
                        rootCert = c2;
                    else if (cert != null)
                        rootCert = new X509Certificate2(cert);
                }

                if (rootCert == null)
                {
                    MessageBox.Show(
                        LanguageManager.GetText("CertCreateOrLoadFailed"),
                        LanguageManager.GetText("CriticalError"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

                // 3) 检查 LocalMachine\Root 是否已安装同一根证书（根据拇指指纹）
                if (IsCertInStore(StoreName.Root, StoreLocation.LocalMachine, rootCert.Thumbprint))
                    return true; // 已信任且可用

                // 4) 未安装则提示用户是否自动安装
                var dr = MessageBox.Show(
                    LanguageManager.GetText("CertNotInstalledPrompt"),
                    LanguageManager.GetText("Prompt"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr == DialogResult.No)
                    return false;

                // 5) 安装到 LocalMachine\My 和 LocalMachine\Root
                InstallToStore(StoreName.My, StoreLocation.LocalMachine, rootCert);
                InstallToStore(StoreName.Root, StoreLocation.LocalMachine, rootCert);

                // 6) 再次校验
                if (!IsCertInStore(StoreName.Root, StoreLocation.LocalMachine, rootCert.Thumbprint))
                {
                    MessageBox.Show(
                        LanguageManager.GetText("CertInstallFailed"),
                        LanguageManager.GetText("CriticalError"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.GetText("CertInstallException") + ex.Message,
                    LanguageManager.GetText("CriticalError"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool IsCertInStore(StoreName storeName, StoreLocation location, string thumbprint)
        {
            using (var store = new X509Store(storeName, location))
            {
                store.Open(OpenFlags.ReadOnly);
                var found = store.Certificates
                    .OfType<X509Certificate2>()
                    .Any(c => string.Equals(
                        CleanThumbprint(c.Thumbprint),
                        CleanThumbprint(thumbprint),
                        StringComparison.OrdinalIgnoreCase));
                store.Close();
                return found;
            }
        }

        private static void InstallToStore(StoreName storeName, StoreLocation location, X509Certificate2 cert)
        {
            using (var store = new X509Store(storeName, location))
            {
                store.Open(OpenFlags.ReadWrite);
                // 避免重复添加
                bool exists = store.Certificates
                    .OfType<X509Certificate2>()
                    .Any(c => string.Equals(
                        CleanThumbprint(c.Thumbprint),
                        CleanThumbprint(cert.Thumbprint),
                        StringComparison.OrdinalIgnoreCase));
                if (!exists)
                {
                    store.Add(cert);
                }
                store.Close();
            }
        }

        private static string CleanThumbprint(string? t)
            => (t ?? "").Replace(" ", "").Trim();
    }
}
