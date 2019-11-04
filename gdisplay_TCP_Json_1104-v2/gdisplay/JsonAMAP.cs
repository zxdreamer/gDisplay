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
    public class AmapEvaluation
    {
        public string expedite { get; set; }
        public string congested { get; set; }
        public string blocked { get; set; }
        public string unknown { get; set; }
        public string status { get; set; }
        public string description { get; set; }
    }
    public class AmapRoads
    {
        public string name { get; set; }
        public string status { get; set; }
        public string direction { get; set; }
        public string angle { get; set; }
        public string speed { get; set; }
        public string lcodes { get; set; }
        public string polyline { get; set; }

    }
    public class AmapTrafficinfo
    {
        public string description { get; set; }
        public AmapEvaluation evaluation { get; set; }
        public List<AmapRoads> roads { get; set; }
    }

    public class AmapJson
    {
        public string status { get; set; }
        public string infocode { get; set; }
        public string info { get; set; }
        public AmapTrafficinfo trafficinfo { get; set; }
    }
}
