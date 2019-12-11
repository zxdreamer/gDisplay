using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdisplay
{
    public enum ydpShowEm
    {
        S1DevName = 0,
        S1DevState = 1,
        S2DevName = 2,
        S2DevState = 3,
        S3DevName = 4,
        S3DevState = 5,
        StsDevStat = 0,
        StsSndData = 1,
        RecvAMAP = 2,
        STime = 3
    }
    //public enum ydpShowStsEm
    //{
    //    DevStat=0,
    //    SndData,
    //    RecvAMAP,
    //    STime
    //}
    public enum ydpPlaceEm
    {
        StsBar = 0,
        TxtBox
    }
}
