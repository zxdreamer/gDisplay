using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdisplay
{
    public class RoadsResult
    {
        private int numRoads = 20;
        private string mRoad;
        public string MRoad
        {
            get { return mRoad; }
            set { mRoad = value; }
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
        public RoadsResult(string main, List<string> nodeList)
        {
            this.mRoad = main;
            this.nodeList = nodeList;
            this.isVisited = false;
            this.mRoadArr = new byte[numRoads];
        }
    }
    public class ScreenResult
    {
        private List<RoadsResult> sroads;
        public List<RoadsResult> Sroads
        {
            get { return sroads; }
            set { sroads = value; }
        }
        private List<string> sfindRect;
        public List<string> SfindRect
        {
            get { return sfindRect; }
            set { sfindRect = value; }
        }
        public ScreenResult(List<RoadsResult> rds, List<string> fdr)
        {
            this.sroads = rds;
            this.sfindRect = fdr;
        }

    }
}
