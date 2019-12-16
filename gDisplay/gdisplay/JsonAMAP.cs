using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdisplay
{
    public class ValidJsonInfo
    {
        private Byte valStatus;
        public Byte ValStatus
        {
            get { return valStatus; }
            set { valStatus = value; }
        }
        List<AmapRoads> valRoads=new List<AmapRoads>();
        public List<AmapRoads> ValRoads
        {
            get { return valRoads; }
            set { valRoads = value; }
        }
    }

    public class AmapRoads
    {
        public string name { get; set; }
        public string status { get; set; }      //路况:
                                                //0:未知,1:畅通,2:缓行,3:拥堵,4:严重拥堵
        public string direction { get; set; }   //方向描述
        public string angle { get; set; }       //车行角度,
                                                //以正东方向为0度，逆时针方向为正
                                                //取值范围：[0,360]

        public string speed { get; set; }       //速度,单位：千米/小时
        public string lcodes { get; set; }      //locationcode的集合
                                                //道路中某一段的id，一条路包括多个locationcode
                                                //??? angle在[0-180]之间取正值，[181-359]之间取负值
        public string polyline { get; set; }    //道路坐标集,
                                                //经度和纬度使用","分隔
                                                //坐标之间使用";"分隔
                                                //例如：x1,y1;x2,y2
    }

    public class AmapEvaluation
    {
        public string expedite { get; set; }    //畅通所占百分比
        public string congested { get; set; }   //缓行所占百分比
        public string blocked { get; set; }     //拥堵所占百分比
        public string unknown { get; set; }     //未知路段所占百分比
        public string status { get; set; }      //路况:
                                                //0:未知,1:畅通,2:缓行,3:拥堵,4:严重拥堵
        public string description { get; set; } //道路描述
    }
    public class AmapTrafficinfo
    {
        public string description { get; set; }
        public AmapEvaluation evaluation { get; set; }
        public List<AmapRoads> roads { get; set; }
    }

    public class ydpAmapJson
    {
        public string status { get; set; }
        public string info { get; set; }
        public string infocode { get; set; }       
        public AmapTrafficinfo trafficinfo { get; set; }
    }
}
