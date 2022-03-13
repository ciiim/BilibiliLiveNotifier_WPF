using System.Windows;


namespace LiveBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserManager userManager = UserManager.Instance;

        public MainWindow()
        {
            InitializeComponent();

            if (userManager.Expired())
            {
                userManager.SaveSetting("Config", "Cookie", "");
                userManager.SaveSetting("Config", "ExpiredTime", "");
                new LiveToast().ToastText("登录过期 请重新登录");
            }

            bool isChecked = bool.Parse(userManager.GetSetting("Config/Settings", "AutoRefresh"));
            autoRefreshBtn.IsChecked = isChecked;
            openAutoRefresh = isChecked;

            autoRefreshTimer = new System.Timers.Timer(double.Parse(userManager.GetSetting("Config/Settings", "RefreshTime")) * 1000);
            autoRefreshTimer.Elapsed += AutoRefresh;
            autoRefreshTimer.AutoReset = true;
            autoRefreshTimer.Enabled = isChecked;
        }


        private System.Timers.Timer autoRefreshTimer;

        private bool openAutoRefresh;

        private void Refresh(object sender, RoutedEventArgs e)
        {
            UserManager.Instance.ReloadAndToast();
        }

        private void AutoRefresh(object sender, System.Timers.ElapsedEventArgs e)
        {
            UserManager.Instance.AutoRefresh();
        }

        private void ToggleAutoRefresh(object sender, RoutedEventArgs e)
        {
            openAutoRefresh = !openAutoRefresh;
            userManager.SaveSetting("Config/Settings", "AutoRefresh", openAutoRefresh.ToString());
            if (openAutoRefresh)
            {
                autoRefreshTimer.Interval = double.Parse(userManager.GetSetting("Config/Settings", "RefreshTime")) * 1000;
                autoRefreshTimer.Enabled = true;
            }
            else
            {
                autoRefreshTimer.Enabled = false;
            }
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            ScanQRCode code = new ScanQRCode();
            code.Show();
        }

        private void SettingBtn(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}
