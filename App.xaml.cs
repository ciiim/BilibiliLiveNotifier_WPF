using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading;

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
