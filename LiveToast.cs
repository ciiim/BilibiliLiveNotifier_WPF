using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Collections.Concurrent;

namespace LiveBot
{
    public class LiveToast
    {
        public LiveToast() { }

        public void ToastText(string text)
        {
            ToastContentBuilder toast1 = new ToastContentBuilder();
            toast1.AddArgument("action", "0")
                .AddText(text)
                .Show();
        }

        public void ToastOpened(List<UserManager.UserInfo> list)
        {
            if (list.Count == 0)
            {
                return;
            }

            if (list.Count == 1)
            {
                ToastContentBuilder toast21 = new ToastContentBuilder();
                foreach (UserManager.UserInfo info in list)
                {
                    toast21.AddArgument("action", "0")
                            .AddArgument("roomid", info.Roomid)
                            .AddHeader("3000", "标题: " + info.Title, "0")
                            .AddText(info.Roomid + "-" + info.UserName + " 开播了!")
                            .AddButton(new ToastButton()
                                .SetContent("去看")
                                .AddArgument("action", "OpenWeb"))
                            .Show();
                }
            }
            else if (list.Count > 1)
            {
                StringBuilder content = new StringBuilder();
                foreach (UserManager.UserInfo info in list)
                {
                    content.Append(info.UserName + " ");
                }
                ToastContentBuilder toast2 = new ToastContentBuilder();
                toast2.AddArgument("action", "0")
                    .AddHeader("3000", "有" + list.Count + "位主播开播了", "0")
                    .AddText(content.ToString())
                    .Show();
            }
            return;
        }

        public void ToastStreamingRoom()
        {
            ConcurrentBag<UserManager.UserInfo> roomToastList = UserManager.Instance.UserInfoList;
            int streamingCount = 0;
            foreach (UserManager.UserInfo info in UserManager.Instance.UserInfoList)
            {
                if (info.LiveState == "直播中")
                    streamingCount++;
            }
            if (streamingCount > 1)
            {
                ToastContentBuilder toast0 = new ToastContentBuilder();
                toast0.AddHeader("1000", "有" + streamingCount + "位主播在直播", "0");
                StringBuilder allName = new StringBuilder();
                foreach (UserManager.UserInfo info in roomToastList)
                {
                    if (info.LiveState == "直播中")
                    {
                        allName.Append(info.UserName + "  ");
                    }
                }
                toast0.AddText(allName.ToString())
                    .Show();
            }
            else if (streamingCount == 0)
            {
                ToastContentBuilder toast1 = new ToastContentBuilder();
                toast1.AddArgument("action", "0")
                    .AddText("没有主播在直播")
                    .AddText("干点别的吧~")
                    .Show();
            }
            else
            {
                foreach (UserManager.UserInfo info in roomToastList)
                {
                    if (info.LiveState == "直播中")
                    {
                        ToastContentBuilder toast2 = new ToastContentBuilder();
                        toast2.AddArgument("action", "0")
                            .AddArgument("roomid", info.Roomid)
                            .AddHeader("3000", "标题: " + info.Title, "0")
                            .AddText(info.Roomid + "-" + info.UserName + " 直播中")
                            .AddButton(new ToastButton()
                                .SetContent("去看")
                                .AddArgument("action", "OpenWeb"))
                            .Show();
                    }
                }

            }
        }





    }
}
