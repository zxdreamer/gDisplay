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

        private List<Byte> stateList;    //主路节点状态列表，仅stateList由高德解析得到
        public List<Byte> StateList      
        {
            get { return stateList; }
            set { stateList = value; }
        }

        private List<string> idsList;        //主路路段编号
        public List<string> IdsList
        {
            get { return idsList; }
            set { idsList = value; }
        }
        public RoadsResult(string main, List<string> nodeList,List<int> angList,List<string> iList)
        {
            this.mRoad = main;
            this.sectList = nodeList;
            this.stateList = new List<byte>();
            for(int i =0;i<nodeList.Count;i++)
            {
                this.stateList.Add(1);  //1--:畅通???
            }
            this.angleList = angList;
            this.idsList = iList;
        }
    }

    //配置文件解析+高德解析
    public class ScreenResult
    {
        private string id;                  //屏体Id    
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        private ydpOkMachEm dataOk;            //判断屏体的高德发送模式数据是否准备好
        public ydpOkMachEm DataOk
        {
            get { return dataOk; }
            set { dataOk = value; }      
        }
        private List<string> sfindRect;     //屏体搜索矩形区域列表
        public List<string> SfindRect
        {
            get { return sfindRect; }
            set { sfindRect = value; }
        }
        private List<RoadsResult> sroads;   //主路列表
        public List<RoadsResult> Sroads
        {
            get { return sroads; }
            set { sroads = value; }
        }
        private List<List<string>> band;
        public List<List<string>> Band
        {
            get { return band; }
            set { band = value; }
        }
        private Byte[] colorArr;
        public Byte[] ColorArr
        {
            get { return colorArr; }
            set { colorArr = value; }
        }


        public ScreenResult(List<RoadsResult> rds, List<string> fdr,List<List<string>> bd,string sid)
        {
            this.id = sid;
            this.sfindRect = fdr;
            this.sroads = rds;
            this.band = bd;
            this.dataOk = ydpOkMachEm.Unok;
            this.colorArr = new byte[10];
        }
    }
}
