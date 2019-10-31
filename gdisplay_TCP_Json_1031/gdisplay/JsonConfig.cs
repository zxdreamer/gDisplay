using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdisplay
{
    public class Roads
    {
        public string name { get; set; }
        public List<string> sections { get; set; }
    }
    public class Screens
    {
        public string id { get; set; }
        public string ip { get; set; }
        public string desc { get; set; }
        public string pw { get; set; }
        public List<string> loc { get; set; }//???代表啥???
        public List<List<string>> regions { get; set; }
        public List<Roads> roads { get; set; }
    }
    public class JsonConfig
    {
        public string company { get; set; }
        public string version { get; set; }
        public string url { get; set; }
        public string port { get; set; }
        public string isAMAP { get; set; }
        public List<Screens> screens { get; set; }
    }
}
