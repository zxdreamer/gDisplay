using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdisplay
{
    //配置文件解析+高德解析
    public class RoadsResult
    {
        private string mRoad;           //主路名
        public string MRoad
        {
            get { return mRoad; }
            set { mRoad = value; }
        }
        private List<string> sectList;  //主路节点列表???
        public List<string> SectList
        {
            get { return sectList; }
            set { sectList = value; }
        }
        private List<int> angleList;    //主路节点角度列表
        public List<int> AngleList
        {
            get { return angleList; }
            set { angleList = value; }
        }

        private List<Byte> stateList;    //主路节点状态列表
        public List<Byte> StateList
        {
            get { return stateList; }
            set { stateList = value; }
        }
        public RoadsResult(string main, List<string> nodeList)
        {
            this.mRoad = main;
            this.sectList = nodeList;
            this.stateList = new List<byte>();
            for(int i =0;i<nodeList.Count;i++)
            {
                this.stateList.Add(0);  //1--:畅通???
            }
        }
    }

    //配置文件解析+高德解析
    public class ScreenResult
    {
        private string id;
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        private List<string> sfindRect;
        public List<string> SfindRect
        {
            get { return sfindRect; }
            set { sfindRect = value; }
        }
        private List<RoadsResult> sroads;
        public List<RoadsResult> Sroads
        {
            get { return sroads; }
            set { sroads = value; }
        }
        public ScreenResult(List<RoadsResult> rds, List<string> fdr,string sid)
        {
            this.id = sid;
            this.sfindRect = fdr;
            this.sroads = rds;                      
        }
    }
}
