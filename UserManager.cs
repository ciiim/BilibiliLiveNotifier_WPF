using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace LiveBot
{
    public class UserManager
    {

        private UserManager()
        {
            UserInfoList = new ConcurrentBag<UserInfo>();

            RoomInfoXml = new XmlUtility();
            RoomInfoXml.Path = RoomPath;
            RoomInfoXml.Load();

            BaseConfigXml = new XmlUtility();
            BaseConfigXml.Path = BaseConfigPath;
            BaseConfigXml.Load();


            LoginInfo = Utilities.QueryToDic(BaseConfigXml.GetChildText("Config", "Cookie"));
        }
        public class UserInfo
        {
            public string UID { get; set; }
            public string UserName { get; set; }
            public string Roomid { get; set; }
            public string Title { get; set; }
            public string LiveState { get; set; }
            public UserInfo(string _uid, string _userName, string _roomid, string _title, string _liveState)
            {
                UID = _uid;
                UserName = _userName;
                Roomid = _roomid;
                Title = _title;
                LiveState = _liveState;
            }
            public UserInfo()
            {

            }

            // override object.Equals
            public override bool Equals(object obj)
            {

                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                UserInfo info = obj as UserInfo;
                if (info.LiveState == LiveState && info.UID == UID)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return LiveState.GetHashCode() + UID.GetHashCode();
            }
        }
        private class ForQuery
        {
            public ManualResetEvent e;
            public string number;

            public ForQuery(string id, ManualResetEvent manual)
            {
                number = id;
                e = manual;
            }
        }

        private XmlUtility RoomInfoXml { get; set; }
        private XmlUtility BaseConfigXml { get; set; }
        private ManualResetEvent[] roomEvents;
        private static UserManager instance;
        public Dictionary<string, string> LoginInfo { get; set; }
        public ConcurrentBag<UserInfo> UserInfoList { get; private set; }
        public static UserManager Instance
        {
            get
            {
                if (instance == null) { instance = new UserManager(); return instance; } else { return instance; }
            }
        }
        public static string RoomPath { get; } = @".\Config\RoomList.xml";
        public static string BaseConfigPath { get; } = @".\Config\BaseConfig.xml";
        #region 登录方法

        public void ReloadUserInfo()
        {
            string load = BaseConfigXml.GetChildText("Config/Settings", "LoadMethod");
            if (load == "manual")
            {
                NoLogin_ReloadUserInfo();
            }
            else if (load == "auto")
            {
                Login_ReloadUserInfo();
            }
            else
            {
                return;
            }
            return;
        }
        #region 没有登录的方法
        private void NoLogin_ReloadUserInfo()
        {
            ClearUserInfo();
            RoomInfoXml.Load();
            List<string> roomList = RoomInfoXml.GetAllChildAttr("RoomRoot", "Room", "roomid");//从xml文件内读取房间
            roomEvents = new ManualResetEvent[roomList.Count];
            int i = 0;
            foreach (string roomid in roomList)
            {
                roomEvents[i] = new ManualResetEvent(false);
                ForQuery f = new ForQuery(roomid, roomEvents[i]);
                ThreadPool.QueueUserWorkItem(new WaitCallback(QueryMutiThread), f);
                i++;
            }
            return;
        }
        #endregion
        #region 已登录的方法
        private void Login_ReloadUserInfo()
        {
            if (Expired())
            {
                SaveSetting("Config", "Cookie", "");
                SaveSetting("Config", "ExpiredTime", "");
                new LiveToast().ToastText("登录过期 请重新登录");
                return;
            }
            ClearUserInfo();
            //通过api获取用户关注列表
            if (!HasCookie())
            {
                new LiveToast().ToastText("未登录~ 获取列表失败");
                return;
            }
            try
            {
                string res = Utilities.HttpGet("https://api.live.bilibili.com/xlive/web-ucenter/user/following",
                    "page=1&page_size=29",
                    "cookie:" + "SESSDATA=" + LoginInfo["SESSDATA"]);
                Newtonsoft.Json.Linq.JObject json = Utilities.ConvertJsonString(res);
                int totalPage = int.Parse(json["data"]["totalPage"].ToString());
                roomEvents = new ManualResetEvent[totalPage];
                for (int i = 0; i < totalPage; i++)
                {
                    roomEvents[i] = new ManualResetEvent(false);
                    ForQuery f = new ForQuery((i + 1).ToString(), roomEvents[i]);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(QueryPageMutiThread), f);
                }
                return;
            }
            catch
            {
                return;
            }
        }
        #endregion
        #endregion

        #region 刷新方法
        public void RefreshListView(ListView roomListView)
        {
            ReloadUserInfo();
            Task task = new Task(new Action(() =>
            {
                if (roomEvents.Length != 0)
                    WaitHandle.WaitAll(roomEvents);
                Thread.Sleep(100);
                roomListView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    roomListView.ItemsSource = SortList();
                }));
            }));
            task.Start();
        }

        public void ReloadAndToast()
        {
            ReloadUserInfo();
            Task task = new Task(new Action(() =>
            {
                if (roomEvents.Length != 0)
                    WaitHandle.WaitAll(roomEvents);
                Thread.Sleep(100);
                new LiveToast().ToastStreamingRoom();
            }));
            task.Start();
        }

        public void AutoRefresh()
        {
            /*//测试代码
            List<UserInfo> a = new List<UserInfo>();
            a.Add(new UserInfo("1", "1", "10", "111", "直播中"));
            a.Add(new UserInfo("2", "1", "10", "111", "直播中"));
            a.Add(new UserInfo("3", "1", "10", "111", "直播中"));
            a.Add(new UserInfo("4", "这个主播", "10", "111", "未开播"));
            List<UserInfo> b = new List<UserInfo>();
            b.Add(new UserInfo("1", "1", "10", "111", "直播中"));
            b.Add(new UserInfo("2", "1", "10", "111", "直播中"));
            b.Add(new UserInfo("3", "1", "10", "111", "直播中"));
            b.Add(new UserInfo("4", "这个主播", "10", "111", "直播中"));
            List<UserInfo> existenceList_temp = b.Except(a).ToList();
            List<UserInfo> nowStreamingRoom_temp = new List<UserInfo>();
            foreach (UserInfo item in existenceList_temp)
            {
                if (item.LiveState == "直播中")
                {
                    nowStreamingRoom_temp.Add(item);
                }
            }
            new LiveToast().ToastOpened(nowStreamingRoom_temp);
            return;*/

            List<UserInfo> oldList = UserInfoList.ToList();
            ReloadUserInfo();
            Task task = new Task(new Action(() =>
            {
                if (roomEvents == null || roomEvents.Length == 0)
                {
                    return;
                }
                if (oldList.Count == 0)
                {
                    return;
                }
                WaitHandle.WaitAll(roomEvents);
                Thread.Sleep(500);

                List<UserInfo> existenceList = UserInfoList.Except(oldList).ToList();
                List<UserInfo> nowStreamingRoom = new List<UserInfo>();
                foreach (UserInfo item in existenceList)
                {
                    if (item.LiveState == "直播中")
                    {
                        nowStreamingRoom.Add(item);
                    }
                }
                new LiveToast().ToastOpened(nowStreamingRoom);
            }));
            task.Start();
        }

        #endregion

        #region 查询方法
        private static void QueryMutiThread(object obj)
        {

            UserInfo room;
            ForQuery f = (ForQuery)obj;
            try
            {
                string uid;
                string res;
                Thread.Sleep(10);
                res = Utilities.HttpGet("https://api.live.bilibili.com/xlive/web-room/v2/index/getRoomPlayInfo", "room_id=" + f.number);
                if (Utilities.ConvertJsonString(res)["data"].ToString() == "")
                {
                    room = new UserInfo(_uid: "", _userName: "房间不存在", _roomid: f.number, _title: "", _liveState: "");
                    Instance.UserInfoList.Add(room);
                    throw new Exception();
                }
                uid = ((Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(res))["data"]["uid"].ToString();
                res = Utilities.HttpGet("https://api.bilibili.com/x/space/acc/info", "mid=" + uid + "&jsonp=jsonp");
                Newtonsoft.Json.Linq.JObject userData = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(res);
                string liveState = userData["data"]["live_room"]["liveStatus"].ToString() == "1" ? "直播中" : "未开播";
                string liveTitle = userData["data"]["live_room"]["title"].ToString();
                string userName = userData["data"]["name"].ToString();
                room = new UserInfo(_uid: uid, _userName: userName, _roomid: f.number, _title: liveTitle, _liveState: liveState);
                Instance.UserInfoList.Add(room);
            }
            catch { }
            finally { f.e.Set(); }
        }

        private static void QueryPageMutiThread(object obj)
        {
            UserManager userManager = Instance;
            ForQuery f = (ForQuery)obj;
            try
            {
                string res = Utilities.HttpGet("https://api.live.bilibili.com/xlive/web-ucenter/user/following",
                        "page=" + f.number + "&page_size=29",
                        "cookie:SESSDATA=" + userManager.LoginInfo["SESSDATA"]);
                Thread.Sleep(50);
                Newtonsoft.Json.Linq.JObject json = Utilities.ConvertJsonString(res);
                foreach (Newtonsoft.Json.Linq.JToken user in json["data"]["list"])
                {
                    userManager.UserInfoList.Add(new UserInfo(user["uid"].ToString(), user["uname"].ToString(), user["roomid"].ToString(), user["title"].ToString(), user["live_status"].ToString() == "1" ? "直播中" : "未开播"));
                }
            }
            catch { }
            finally { f.e.Set(); }

        }
        #endregion

        #region XML操作方法
        public void SaveSetting(string rootPath, string nodeName, string value)
        {
            BaseConfigXml.SetChildText(rootPath, nodeName, value);
        }

        public string GetSetting(string rootPath, string nodeName)
        {
            return BaseConfigXml.GetChildText(rootPath, nodeName);
        }

        public void AddRoom(string rootPath, string nodeName, string roomid)
        {
            XmlElement room = RoomInfoXml.CreateElement("Room");
            room.SetAttribute("roomid", roomid);
            RoomInfoXml.AppendChild(rootPath, room, false);
        }

        public void DeleteRoom(string rootPath, string roomid)
        {
            XmlElement targetRoom = RoomInfoXml.CreateElement("Room");
            targetRoom.SetAttribute("roomid", roomid);
            RoomInfoXml.DeleteChild(rootPath, targetRoom);
        }
        #endregion

        private void ClearUserInfo()
        {
            UserInfoList = new ConcurrentBag<UserInfo>();
        }

        /// <summary>
        /// 对直播间列表进行排列 已直播的房间会排在上面
        /// </summary>
        /// <returns>排序好的List&lt;Userinfo&gt;</returns>
        private List<UserInfo> SortList()
        {
            return UserInfoList.OrderByDescending(item => item.LiveState).ToList();
        }

        public bool HasCookie()
        {
            return LoginInfo != null && LoginInfo.Count != 0;
        }
        /// <summary>
        /// Cookie是否过期 如果过期则返回true
        /// </summary>
        /// <returns></returns>
        public bool Expired()
        {
            if (!HasCookie())
            {
                return false;
            }
            if (Utilities.ConvertJsonString(Utilities.HttpGet("http://api.bilibili.com/x/web-interface/nav", "", "cookie:SESSDATA=" + LoginInfo["SESSDATA"]))["data"]["isLogin"].ToString() == "False")
            {
                return true;
            }
            return false;
        }

        //Discard Function
        //public List<UserInfo> QueryAll()
        //{
        //    List<UserInfo> roomToastList = new List<UserInfo>();
        //    List<string> roomList = roomInfoXml.GetChildAttr("RoomRoot", "Room", "roomid");
        //    foreach (string roomid in roomList)
        //    {
        //        UserInfo room = Query(roomid);
        //        roomToastList.Add(room);
        //    }
        //    return roomToastList;
        //}

        //public UserInfo Query(string id)
        //{
        //    UserInfo room;
        //    try
        //    {

        //        string uid;
        //        string res;
        //        res = Utilities.HttpGet("https://api.live.bilibili.com/xlive/web-room/v2/index/getRoomPlayInfo", "room_id=" + id);
        //        uid = Utilities.ConvertJsonString(res)["data"]["uid"].ToString();
        //        res = Utilities.HttpGet("https://api.bilibili.com/x/space/acc/info", "mid=" + int.Parse(uid) + "&jsonp=jsonp");
        //        Newtonsoft.Json.Linq.JObject userData = Utilities.ConvertJsonString(res);
        //        string liveState = userData["data"]["live_room"]["liveStatus"].ToString();
        //        string liveTitle = userData["data"]["live_room"]["title"].ToString();
        //        string userName = userData["data"]["name"].ToString();
        //        room = new UserInfo(_uid: uid, _userName: userName, _roomid: id, _title: liveTitle, _liveState: liveState);
        //        return room;
        //    }
        //    catch
        //    {
        //        room = new UserInfo();
        //        return room;
        //    }
        //}

    }
}
