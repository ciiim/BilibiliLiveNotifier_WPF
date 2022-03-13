using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LiveBot
{
    public class Utilities
    {
        public static BitmapImage Qrencode(string context)
        {
            QRCoder.QRCodeGenerator qrcodeGen = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeData data = qrcodeGen.CreateQrCode(context, QRCoder.QRCodeGenerator.ECCLevel.L);
            QRCoder.QRCode qrcode = new QRCoder.QRCode(data);
            Bitmap qrcodeBitmap = qrcode.GetGraphic(20);
            BitmapImage qrImage = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                qrcodeBitmap.Save(ms, ImageFormat.Bmp);
                qrImage.BeginInit();
                qrImage.StreamSource = ms;
                qrImage.CacheOption = BitmapCacheOption.OnLoad;
                qrImage.EndInit();
                qrImage.Freeze();
                ms.Dispose();
            }
            return qrImage;
        }

        public static string HttpGet(string url, string getDataStr = "", string headers = "")
        {
            string finalUrl = url + (getDataStr == "" ? "" : "?") + getDataStr;
            HttpWebRequest request = WebRequest.Create(finalUrl) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            #region Header
            foreach (KeyValuePair<string, string> item in HeadersToDic(headers))
            {
                request.Headers[item.Key] = item.Value;
            }
            #endregion
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            response.Close();
            return retString;
        }

        public static string HttpPost(string url, Dictionary<string, string> dic, Dictionary<string, string> headerDic)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;


            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            #region Headers
            foreach (KeyValuePair<string, string> item in headerDic)
            {
                request.Headers.Add(item.Key, item.Value);
            }
            #endregion


            #region 添加POST参数
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (KeyValuePair<string, string> item in dic)
            {
                if (i > 0)
                {
                    builder.Append('&');
                }
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            #endregion

            byte[] content = Encoding.UTF8.GetBytes(builder.ToString());
            request.ContentLength = content.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(content, 0, content.Length);
            requestStream.Close();

            HttpWebResponse res = request.GetResponse() as HttpWebResponse;
            Stream resStream = res.GetResponseStream();
            StreamReader streamReader = new StreamReader(resStream);
            string jsonString = streamReader.ReadToEnd();
            return jsonString;
        }

        public static Newtonsoft.Json.Linq.JObject ConvertJsonString(string jsonString)
        {
            return (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
        }

        public static Dictionary<string, string> QueryToDic(string query)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            if (query == "")
                return res;
            string[] arr = query.Split('&');
            foreach (string item in arr)
            {
                string[] keyPair = item.Split('=');
                res.Add(keyPair[0], keyPair[1]);
            }
            return res;
        }

        public static Dictionary<string, string> HeadersToDic(string headers)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            if (headers == "")
                return res;
            string[] arr = headers.Split('&');
            foreach (string item in arr)
            {
                string[] keyPair = item.Split(':');
                res.Add(keyPair[0], keyPair[1]);
            }
            return res;
        }

        public static void OpenHttp(string path)
        {
            try
            {
                Process web = new Process();
                web.StartInfo.FileName = "explorer";
                web.StartInfo.Arguments = path;
                web.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //public static DateTime StampToDataTime(long stamp)
        //{
        //    DateTime time = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
        //    return time.AddSeconds(stamp);
        //}

        //public static long DataTimeToStamp(DateTime time)
        //{
        //    System.DateTime startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local); // 当地时区
        //    return (long)(DateTime.Now - startTime).TotalSeconds; // 相差秒数
        //}

    }
}
