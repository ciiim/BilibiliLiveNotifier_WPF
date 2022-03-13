using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace LiveBot
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Window
    {
        private UserManager userManager = UserManager.Instance;

        public Settings()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            InitUI();
            RefreshList();
        }

        private void InitUI()
        {
            secondSlider.ValueChanged += Minus_ValueChanged;
            int second = int.Parse(userManager.GetSetting("Config/Settings", "RefreshTime"));
            secondSlider.Value = second;
            shouMinus.Text = second.ToString();

            autoOpenCheckBox.IsChecked = bool.Parse(userManager.GetSetting("Config/Settings", "AutoOpen"));

        }

        #region Config Field

        private void Minus_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            shouMinus.Text = ((int)e.NewValue).ToString();
            Task.Run(() => userManager.SaveSetting("Config/Settings", "RefreshTime", ((int)e.NewValue).ToString()));

        }

        private void clearCookie_Click(object sender, RoutedEventArgs e)
        {
            //1.重置XML储存的Cookie
            //2.重置UserManager储存的Cookie

            userManager.SaveSetting("Config", "Cookie", "");
            userManager.LoginInfo = new System.Collections.Generic.Dictionary<string, string>();
            new LiveToast().ToastText("清除成功~");
        }

        private void LoadMethod_Initialized(object sender, EventArgs e)
        {
            //1.读取XML储存的读取方法
            //2.设置UI的显示
            string method = userManager.GetSetting("Config/Settings", "LoadMethod");
            foreach (ComboBoxItem item in LoadMethod.Items)
            {
                if (item.Tag.ToString() == method)
                {
                    LoadMethod.SelectedItem = item;
                    LoadMethod.SelectionChanged += SetLoadMethod;
                    return;
                }
            }
            LoadMethod.SelectionChanged += SetLoadMethod;
        }

        private void SetLoadMethod(object sender, SelectionChangedEventArgs e)
        {
            //1.读取UI的显示的读取方法
            //2.设置XML储存的读取方法
            ComboBoxItem item = (ComboBoxItem)LoadMethod.SelectedItem;
            if (item.Tag == null)
            {
                return;
            }
            userManager.SaveSetting("Config/Settings", "LoadMethod", item.Tag.ToString());
            RefreshList();
        }

        private void autoOpenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            new AutoOpen().SetMeAutoStart(true);
            userManager.SaveSetting("Config/Settings", "AutoOpen", "true");
        }

        private void autoOpenCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            new AutoOpen().SetMeAutoStart(false);
            userManager.SaveSetting("Config/Settings", "AutoOpen", "false");
        }

        #endregion

        #region LiveRoomList Field

        private void RefreshList()
        {
            if (userManager.HasCookie() && userManager.GetSetting("Config/Settings", "LoadMethod") == "auto")
            {
                addBtn.IsEnabled = false;
                deleteBtn.IsEnabled = false;
            }
            else
            {
                addBtn.IsEnabled = true;
                deleteBtn.IsEnabled = true;
            }

            Thread.Sleep(100);
            userManager.RefreshListView(roomListView);
        }

        private void DoubleClickOpenWeb(object sender, MouseButtonEventArgs e)
        {
            ListView list = sender as ListView;
            UserManager.UserInfo info = list.SelectedItem as UserManager.UserInfo;
            if (info == null)
                return;
            Utilities.OpenHttp(@"http://live.bilibili.com/" + info.Roomid);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddGrid.Visibility = Visibility.Visible;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (roomListView.SelectedItem is UserManager.UserInfo info)
            {
                userManager.DeleteRoom("RoomRoot", info.Roomid);
                userManager.RefreshListView(roomListView);
            }
            return;
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }



        #endregion

        private void AddSubmitButton(object sender, RoutedEventArgs e)
        {
            
            if (newRoomid.Text == "")
            {
                AddGrid.Visibility = Visibility.Hidden;
                return;
            }
            AddGrid.Visibility = Visibility.Hidden;
            userManager.AddRoom("RoomRoot", "Room", newRoomid.Text);
            newRoomid.Text = "";
            RefreshList();
        }

        private void newRoomid_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }
    }
}
