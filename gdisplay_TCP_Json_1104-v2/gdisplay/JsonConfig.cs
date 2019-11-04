using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdisplay
{
    public class pathCfgInfo
    {
        private int numRoads = 20;
        private string mRoad;
        public string MRoad
        {
            get{return mRoad; }
            set{ mRoad = value;}
        }

        private List<string> nodeList;
        public List<string> NodeList
        {
            get { return nodeList; }
            set { nodeList = value; }
        }
        private Byte[] mRoadArr;
        public Byte[] MRoadArr
        {
            get { return mRoadArr; }
            set { mRoadArr = value; }
        }
        private Boolean isVisited;
        public Boolean IsVisited
        {
            get { return isVisited; }
            set { isVisited = value; }
        }
        public pathCfgInfo(string main,List<string> nodeList)
        {
            this.mRoad = main;
            this.nodeList = nodeList;
            this.isVisited = false;
            this.mRoadArr = new byte[numRoads];
        }
    }
    public class CfgRoads
    {
        public string name { get; set; }
        public List<string> sections { get; set; }
    }
    public class CfgScreens
    {
        public string id { get; set; }
        public string ip { get; set; }
        public string desc { get; set; }
        public string pw { get; set; }
        public List<string> loc { get; set; }//???代表啥???
        public List<string> regions { get; set; }
        public List<CfgRoads> roads { get; set; }
    }
    public class JsonConfig
    {
        public string company { get; set; }
        public string version { get; set; }
        public string url { get; set; }
        public string port { get; set; }
        public string isAMAP { get; set; }
        public List<CfgScreens> screens { get; set; }
    }
}
