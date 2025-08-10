using System.Diagnostics;


namespace ProxyLauncher
{
    public partial class MainForm : Form
    {
        private TitaniumProxy? _proxy;
        private Process? targetProcess;
        private ProxyConfig config;
        private LogWindow logWindow;


        private Panel? panelTop;
        private Panel? panelMain;
        private Panel? panelBottom;
        private GroupBox? groupConfig;
        private GroupBox? groupTarget;
        private GroupBox? groupControl;
        private Label? lblTitle;
        private Label? lblLang;
        private Label? lblConfigInfo;
        private Button? btnEditConfig;
        private Label? lblProgramPath;
        private TextBox? txtTargetExe;
        private Button? btnBrowse;
        private Button? btnStart;
        private Button? btnStop;
        private Label? lblStatus;
        private ComboBox? cmbLanguage;
        private CheckBox? chkShowLog;
        private ProgressBar? progressBar;
        private Label? lblCopyright;

        public MainForm()
        {
            config = ConfigManager.LoadConfig();
            InitializeComponent();

            // 先创建日志窗口，避免 UpdateLanguageTexts 时 logWindow 为空
            logWindow = new LogWindow();

            logWindow.WindowHiddenByUser += LogWindow_WindowHiddenByUser;

            // 根据LanguageManager的初始状态设置UI下拉
            string currentLang = LanguageManager.GetCurrentLanguage();
            if (cmbLanguage != null)
                cmbLanguage.SelectedIndex = currentLang == "zh-CN" ? 0 : 1;

            // 统一刷新文本，确保不会显示英文键名
            UpdateLanguageTexts();
            UpdateConfigDisplay();
        }
        private void LogWindow_WindowHiddenByUser(object? sender, EventArgs e)
        {
            // 当日志窗口被用户关闭（隐藏）时，同步取消主界面上复选框的勾选
            if (chkShowLog != null)
            {
                chkShowLog.Checked = false;
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(700, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // 1. 先创建并添加填充主区域的面板 (Dock.Fill)
            CreateMainPanel();
            // 2. 再创建顶部和底部的面板 (Dock.Top / Dock.Bottom)
            CreateTopPanel();
            CreateBottomPanel();
        }

        private void CreateTopPanel()
        {
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(51, 122, 183),
                Padding = new Padding(10)
            };
            this.Controls.Add(panelTop);

            lblTitle = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 15)
            };
            panelTop.Controls.Add(lblTitle);

            // 语言选择
            lblLang = new Label
            {
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(450, 20)
            };
            panelTop.Controls.Add(lblLang);

            cmbLanguage = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(520, 17),
                Size = new Size(80, 25)
            };
            cmbLanguage.Items.AddRange(new[] { "中文", "English" });
            cmbLanguage.SelectedIndex = 0;
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
            panelTop.Controls.Add(cmbLanguage);

            // 日志复选框
            chkShowLog = new CheckBox
            {
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(610, 20)
            };
            chkShowLog.CheckedChanged += ChkShowLog_CheckedChanged;
            panelTop.Controls.Add(chkShowLog);
        }

        private void CreateMainPanel()
        {
            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };
            this.Controls.Add(panelMain);

            // 配置信息组
            groupConfig = new GroupBox
            {
                Location = new Point(15, 15),
                Size = new Size(650, 80),
                Font = new Font("Microsoft YaHei", 9)
            };
            panelMain.Controls.Add(groupConfig);

            lblConfigInfo = new Label
            {
                Location = new Point(10, 25),
                Size = new Size(520, 40),
                ForeColor = Color.DarkBlue,
                Font = new Font("Microsoft YaHei", 8)
            };
            groupConfig.Controls.Add(lblConfigInfo);

            btnEditConfig = new Button
            {
                Location = new Point(550, 30),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(92, 184, 92),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnEditConfig.Click += BtnEditConfig_Click;
            groupConfig.Controls.Add(btnEditConfig);

            // 目标程序组
            groupTarget = new GroupBox
            {
                Location = new Point(15, 105),
                Size = new Size(650, 80),
                Font = new Font("Microsoft YaHei", 9)
            };
            panelMain.Controls.Add(groupTarget);

            lblProgramPath = new Label
            {
                Location = new Point(10, 30),
                AutoSize = true
            };
            groupTarget.Controls.Add(lblProgramPath);

            txtTargetExe = new TextBox
            {
                Location = new Point(100, 27),
                Size = new Size(430, 23)
            };
            groupTarget.Controls.Add(txtTargetExe);

            btnBrowse = new Button
            {
                Location = new Point(540, 26),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(240, 173, 78),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowse.Click += BtnBrowse_Click;
            groupTarget.Controls.Add(btnBrowse);

            // 控制组
            groupControl = new GroupBox
            {
                Location = new Point(15, 195),
                Size = new Size(650, 120),
                Font = new Font("Microsoft YaHei", 9)
            };
            panelMain.Controls.Add(groupControl);

            btnStart = new Button
            {
                Location = new Point(50, 30),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(217, 83, 79),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold)
            };
            btnStart.Click += BtnStart_Click;
            groupControl.Controls.Add(btnStart);

            btnStop = new Button
            {
                Location = new Point(200, 30),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(119, 119, 119),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
                Enabled = false
            };
            btnStop.Click += BtnStop_Click;
            groupControl.Controls.Add(btnStop);

            lblStatus = new Label
            {
                Location = new Point(350, 35),
                Size = new Size(280, 30),
                ForeColor = Color.Blue,
                Font = new Font("Microsoft YaHei", 10),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5)
            };
            groupControl.Controls.Add(lblStatus);

            progressBar = new ProgressBar
            {
                Location = new Point(50, 80),
                Size = new Size(580, 20),
                Visible = false
            };
            groupControl.Controls.Add(progressBar);
        }

        private void CreateBottomPanel()
        {
            panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(51, 122, 183)
            };
            this.Controls.Add(panelBottom);

            lblCopyright = new Label
            {
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 8)
            };
            panelBottom.Controls.Add(lblCopyright);
        }

        /// <summary>
        /// 统一更新所有需要翻译的UI文本
        /// </summary>
        private void UpdateLanguageTexts()
        {
            // 窗体与顶部
            this.Text = LanguageManager.GetText("AppTitle");
            if (lblTitle != null) lblTitle.Text = LanguageManager.GetText("AppTitle");
            if (lblLang != null) lblLang.Text = LanguageManager.GetText("Language");
            if (chkShowLog != null) chkShowLog.Text = LanguageManager.GetText("ShowLog");

            // 配置信息组
            if (groupConfig != null) groupConfig.Text = LanguageManager.GetText("ConfigInfo");
            if (btnEditConfig != null) btnEditConfig.Text = LanguageManager.GetText("EditConfig");

            // 目标程序组
            if (groupTarget != null) groupTarget.Text = LanguageManager.GetText("TargetProgram");
            if (lblProgramPath != null) lblProgramPath.Text = LanguageManager.GetText("ProgramPath");
            if (btnBrowse != null) btnBrowse.Text = LanguageManager.GetText("Browse");

            // 控制面板组
            if (groupControl != null) groupControl.Text = LanguageManager.GetText("ControlPanel");
            if (btnStart != null) btnStart.Text = LanguageManager.GetText("StartProxy");
            if (btnStop != null) btnStop.Text = LanguageManager.GetText("StopProxy");

            // 状态文本：仅在“就绪/已停止”时覆盖，避免覆盖“运行中...”
            if (lblStatus != null && (lblStatus.Text == "Ready" || lblStatus.Text == "就绪" || lblStatus.Text == "Stopped" || lblStatus.Text == "已停止"))
            {
                lblStatus.Text = LanguageManager.GetText("Ready");
            }

            // 底部版权信息（可用作标题复用）
            if (lblCopyright != null) lblCopyright.Text = LanguageManager.GetText("AppTitle");

            // 通知日志窗口
            if (logWindow != null) logWindow.UpdateLanguage();
        }


        private void UpdateConfigDisplay()
        {
            var uniqueDomains = config.targetDomains?.Distinct().ToList() ?? new System.Collections.Generic.List<string>();
            string configText = $"{LanguageManager.GetText("RedirectTarget")}: {config.redirectHost}:{config.redirectPort} | " +
                                $"{LanguageManager.GetText("ProxyPort")}: {config.proxyPort} | " +
                                $"{LanguageManager.GetText("MonitoredDomains")}: {string.Join(", ", uniqueDomains)} | " ;
            if (lblConfigInfo != null)
            {
                // 初始化时加载默认文本
                if (string.IsNullOrWhiteSpace(lblConfigInfo.Text))
                {
                    lblConfigInfo.Text = LanguageManager.GetText("LoadingConfig");
                }
                lblConfigInfo.Text = configText;
            }
        }

        private void CmbLanguage_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbLanguage != null)
            {
                string langCode = cmbLanguage.SelectedIndex == 0 ? "zh-CN" : "en-US";
                LanguageManager.LoadLanguage(langCode);
                UpdateLanguageTexts(); // 更新UI文本
                UpdateConfigDisplay(); // 更新配置信息里的文本
            }
        }

        private void ChkShowLog_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkShowLog?.Checked == true)
            {
                logWindow.Show();
            }
            else
            {
                logWindow.Hide();
            }
        }

        private void BtnEditConfig_Click(object? sender, EventArgs e)
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                Process.Start("notepad.exe", configPath);
                var result = MessageBox.Show(
                    LanguageManager.GetText("ModifyConfigPrompt"),
                    LanguageManager.GetText("EditConfig"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // 重新从文件加载配置
                    config = ConfigManager.LoadConfig();
                    // 同时更新全局静态配置，确保数据源统一
                    Program.Config = config;

                    // 重置代理对象，确保下次启动使用新配置
                    if (_proxy != null)
                    {
                        // 如果代理正在运行，先停止它
                        if (_proxy.GetProxyServer().ProxyRunning)
                        {
                            StopAll();
                        }
                        _proxy = null; // 重置代理对象
                    }

                    UpdateConfigDisplay();
                    MessageBox.Show(
                        LanguageManager.GetText("ConfigReloaded"),
                        LanguageManager.GetText("Information"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.GetText("OpenConfigFailed") + ex.Message,
                    LanguageManager.GetText("Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = LanguageManager.GetText("ExecutableFileFilter");
                if (openFileDialog.ShowDialog() == DialogResult.OK && txtTargetExe != null)
                {
                    txtTargetExe.Text = openFileDialog.FileName;
                }
            }
        }

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            if (txtTargetExe != null && (string.IsNullOrWhiteSpace(txtTargetExe.Text) || !File.Exists(txtTargetExe.Text)))
            {
                if (MessageBox.Show(
                    LanguageManager.GetText("NoTargetProgramPrompt"),
                    LanguageManager.GetText("Prompt"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }

            try
            {
                if (progressBar != null) { progressBar.Visible = true; progressBar.Value = 10; }

                // 在启动代理前进行证书检测与安装
                if (_proxy == null)
                    _proxy = new TitaniumProxy(this.config);

                // 证书检测（重要）
                if (!CertificateHelper.EnsureTrustedRootCertificate(_proxy.GetProxyServer()))
                {
                    // 未安装或用户拒绝，则中止启动
                    if (progressBar != null) progressBar.Visible = false;
                    return;
                }

                if (progressBar != null) progressBar.Value = 20;
                LogManager.WriteLine(LanguageManager.GetText("StartingProxyService"));

                // 步骤 1: 启动 Titanium 代理
                await Task.Run(() => _proxy.Start());
                if (progressBar != null) progressBar.Value = 60;
                LogManager.WriteLine(LanguageManager.GetText("ProxyServiceStarted") + config.proxyPort);

                // 步骤 2: 启动目标程序
                if (txtTargetExe != null && File.Exists(txtTargetExe.Text))
                {
                    string proxyAddress = $"http://127.0.0.1:{config.proxyPort}";
                    targetProcess = Launcher.StartProcessWithProxy(txtTargetExe.Text, proxyAddress);
                    if (targetProcess != null)
                    {
                        targetProcess.EnableRaisingEvents = true;
                        targetProcess.Exited += (s, ev) =>
                        {
                            this.Invoke(new Action(() =>
                            {
                                StopAll();
                                MessageBox.Show(
                                    LanguageManager.GetText("ProcessExited"),
                                    LanguageManager.GetText("Information"),
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            }));
                        };
                        LogManager.WriteLine(LanguageManager.GetText("TargetProcessStarted") + $"{targetProcess.ProcessName} (PID: {targetProcess.Id})");
                    }
                }

                if (progressBar != null) progressBar.Value = 100;
                if (btnStart != null) btnStart.Enabled = false;
                if (btnStop != null) btnStop.Enabled = true;
                if (lblStatus != null)
                {
                    lblStatus.Text = LanguageManager.GetText("RunningStatus") + config.proxyPort;
                    lblStatus.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLine(LanguageManager.GetText("StartupFailed") + ex.Message);
                StopAll(); // 启动失败时确保所有服务都已停止
            }
            finally
            {
                if (progressBar != null) progressBar.Visible = false;
            }
        }


        private void BtnStop_Click(object? sender, EventArgs e)
        {
            StopAll();
        }

        private void StopAll()
        {
            try
            {
                LogManager.WriteLine(LanguageManager.GetText("StoppingService"));
                _proxy?.Stop();
                LogManager.WriteLine(LanguageManager.GetText("ProxyServiceStopped"));
                try
                {
                    if (targetProcess != null && !targetProcess.HasExited)
                    {
                        targetProcess.Kill();
                        LogManager.WriteLine(LanguageManager.GetText("TargetProcessTerminated"));
                    }
                }
                catch { /* 忽略终止进程时可能出现的错误 */ }

                if (btnStart != null) btnStart.Enabled = true;
                if (btnStop != null) btnStop.Enabled = false;
                if (lblStatus != null)
                {
                    lblStatus.Text = LanguageManager.GetText("Stopped");
                    lblStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLine(LanguageManager.GetText("StopServiceError") + ex.Message);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (logWindow != null)
            {
                logWindow.WindowHiddenByUser -= LogWindow_WindowHiddenByUser;
            }

            StopAll();
            base.OnFormClosing(e);
        }
    }
}
