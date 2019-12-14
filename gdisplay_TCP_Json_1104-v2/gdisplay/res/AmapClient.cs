using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gdisplay
{
    public class AmapClient
    {
        private static readonly object LockObj = new object();
        private static AmapClient AmapObj = null;
        private WebClient client = new WebClient();
        private string Amapkey = "9bb7a2b28d8b790a33f43d4144d11098";//默认:zxd申请高德API获得的key值
        private AmapClient(string amapKey)
        {
            this.Amapkey = amapKey;
        }
        /*********************
         * Instance:单例模式的属性,使用默认参数，支持空参数
         * Param：
         *      amapKey:申请高德开发者获得的key值
         * */
        public static AmapClient CreateApClientObj(string amapKey= "9bb7a2b28d8b790a33f43d4144d11098")
        {
            if (AmapObj == null)
            {
                lock (LockObj)
                {
                    if (AmapObj == null)
                        AmapObj = new AmapClient(amapKey);
                }
            }
            return AmapObj;
        }
        /*************************
         * GetJsonFromAmapAsync:异步抓取高德数据，并转化成可解析的JsonAmap形式
         * Para:
         *      rect:指定url中的矩形区域
         *      key: 高得中申请的key值
         * result：
         *      可以解析的JsonAmap对象
         * *************************/
        public async Task<ydpAmapJson> GetJsonFromAmapAsync(string rect)
        {
            ydpAmapJson ydpAmap = new ydpAmapJson();
            string url = "https://restapi.amap.com/v3/traffic/status/rectangle?rectangle=" + rect + "&key=" + Amapkey + "&extensions=all";
            client.Credentials = CredentialCache.DefaultCredentials;
            string pageHtml = null;
            try
            {
                var pageData = await client.DownloadDataTaskAsync(url);  //utf-8编码数组
                pageHtml = Encoding.UTF8.GetString(pageData);            //utf-8解码
            }
            catch
            {
                ydpLog.WriteLineLog(DateTime.Now.ToString() + "获取高德数据失败");
                return null;
            }

            ydpAmap = JsonConvert.DeserializeObject<ydpAmapJson>(pageHtml);
            return ydpAmap;
        }

    }
}
