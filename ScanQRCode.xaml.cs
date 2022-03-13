using System;
using System.Collections.Generic;
using System.Windows;
using System.Timers;

namespace LiveBot
{
    /// <summary>
    /// ScanQRCode.xaml 的交互逻辑
    /// </summary>
    public partial class ScanQRCode : Window
    {
        private string authKey;

        private string cookie;

        private Timer listen;

        private UserManager userManager = UserManager.Instance;

        public ScanQRCode()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            GenQRcode();
        }

        private void GenQRcode()
        {
            if (userManager.LoginInfo.ContainsKey("SESSDATA"))
            {
                new LiveToast().ToastText("你已经登陆过了哦");
                Close();
            }
            Newtonsoft.Json.Linq.JObject data = Utilities.ConvertJsonString(Utilities.HttpGet("http://passport.bilibili.com/qrcode/getLoginUrl"));
            QrCode.Source = Utilities.Qrencode(data["data"]["url"].ToString());
            authKey = data["data"]["oauthKey"].ToString();
            listen = new Timer();
            listen.Elapsed += WaitLogin;
            listen.AutoReset = true;
            listen.Interval = 500;
            listen.Start();
        }

        private void WaitLogin(object sender, ElapsedEventArgs e)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("oauthKey", authKey);
            Newtonsoft.Json.Linq.JObject res = Utilities.ConvertJsonString(Utilities.HttpPost("http://passport.bilibili.com/qrcode/getLoginInfo", postData,  new Dictionary<string, string>()));
            if (res["status"].ToString() == "True")
            {
                SaveLoginInfo(res);
            }
        }

        private void SaveLoginInfo(Newtonsoft.Json.Linq.JObject res)
        {
            cookie = res["data"]["url"].ToString();
            string queryStr = new Uri(cookie).Query.Substring(1);

            userManager.LoginInfo = Utilities.QueryToDic(queryStr);
            userManager.SaveSetting("Config", "Cookie", queryStr);

            //保存时间戳
            //userManager.SaveSetting("Config", "ExpiredTime", (Utilities.DataTimeToStamp(DateTime.Now) + long.Parse(userManager.LoginInfo["Expires"])).ToString());

            new LiveToast().ToastText("登录成功~");
            listen.Stop();
            window.Dispatcher.Invoke(() => { Close(); });
        }



    }
}
