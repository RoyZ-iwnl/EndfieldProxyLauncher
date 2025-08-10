namespace ProxyLauncher
{
    public partial class LogWindow : Form
    {
        public event EventHandler? WindowHiddenByUser;


        private TextBox? txtLog;
        private Button? btnClear;
        private Button? btnSave;

        public LogWindow()
        {
            InitializeComponent();
            LogManager.SetLogWindow(this);
        }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.GetText("LogWindow");
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(600, 400);

            // 日志文本框
            txtLog = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen
            };
            this.Controls.Add(txtLog);

            // 底部按钮面板
            var panelButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.LightGray
            };
            this.Controls.Add(panelButtons);

            btnClear = new Button
            {
                Text = LanguageManager.GetText("Clear"),
                Location = new Point(10, 8),
                Size = new Size(80, 25)
            };
            btnClear.Click += BtnClear_Click;
            panelButtons.Controls.Add(btnClear);

            btnSave = new Button
            {
                Text = LanguageManager.GetText("SaveLog"),
                Location = new Point(100, 8),
                Size = new Size(80, 25)
            };
            btnSave.Click += BtnSave_Click;
            panelButtons.Controls.Add(btnSave);
        }

        /// <summary>
        /// 新增：公共方法，用于从外部（如MainForm）调用以更新语言
        /// </summary>
        public void UpdateLanguage()
        {
            this.Text = LanguageManager.GetText("LogWindow");
            if (btnClear != null) btnClear.Text = LanguageManager.GetText("Clear");
            if (btnSave != null) btnSave.Text = LanguageManager.GetText("SaveLog");
        }

        public void AppendLog(string message)
        {
            if (txtLog?.InvokeRequired == true)
            {
                txtLog.Invoke(new Action<string>(AppendLog), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            txtLog?.AppendText($"[{timestamp}] {message}\r\n");
            txtLog?.ScrollToCaret();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            txtLog?.Clear();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = LanguageManager.GetText("TextFileFilter");
                    saveDialog.FileName = $"ProxyLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveDialog.FileName, txtLog?.Text ?? "");
                        MessageBox.Show(
                            LanguageManager.GetText("LogSavedTo") + saveDialog.FileName,
                            LanguageManager.GetText("Information"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.GetText("SaveLogFailed") + ex.Message,
                    LanguageManager.GetText("Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 当用户点击关闭按钮时，不是真的关闭窗口，而是隐藏它
            // 这样可以保留日志内容，并且避免重复创建窗口实例
            e.Cancel = true;
            this.Hide();

            WindowHiddenByUser?.Invoke(this, EventArgs.Empty);

            base.OnFormClosing(e);
        }
    }

    public static class LogManager
    {
        private static LogWindow? _logWindow;

        public static void SetLogWindow(LogWindow logWindow)
        {
            _logWindow = logWindow;
        }

        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
            _logWindow?.AppendLog(message);
        }
    }
}

