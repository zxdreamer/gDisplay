using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gDisplay_UINew
{
    public class CfgRoads
    {
        public string name { get; set; }
        public List<string> sections { get; set; }
        public List<string> ids { get; set; }
        public List<int> angles { get; set; }
    }
    public class CfgScreens
    {
        public string id { get; set; }
        public string ip { get; set; }
        public string desc { get; set; }
        public string pw { get; set; }
        public string loc { get; set; }
        public List<string> regions { get; set; }
        public List<CfgRoads> roads { get; set; }
        public List<List<string>> band { get; set; }
    }
    public class ydpJsonConfig
    {
        public string company { get; set; }
        public string version { get; set; }
        public string url { get; set; }
        public int TCPport { get; set; }
        public string IpAddr { get; set; }
        public bool isAMAP { get; set; }
        public string AMAPcount { get; set; }
        public string AMAPkey { get; set; }
        public int AMAPreqtime { get; set; }
        public string USER1 { get; set; }
        public string USER2 { get; set; }
        public List<CfgScreens> screens { get; set; }
    }
}
