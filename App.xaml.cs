using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows;

namespace LiveBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                ToastNotificationManagerCompat.OnActivated += ProcessAction =>
                {
                    ToastArguments args = ToastArguments.Parse(ProcessAction.Argument);
                    if (args["action"] == "0")
                    {
                        return;
                    }
                    if (args["action"] == "OpenWeb")
                    {
                        OpenStreamingRoom(args["roomid"]);
                    }
                };
            }
            catch
            {
                return;
            }

        }


        private void OpenStreamingRoom(string roomid)
        {
            Utilities.OpenHttp(@"http://live.bilibili.com/" + roomid);
        }



    }
}
