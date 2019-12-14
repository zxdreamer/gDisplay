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
    public enum ydpPlaceEm
    {
        StsBar = 0,
        TxtBox
    }
    public enum ydpModeEm
    {
        Amap=0,     //高德模式
        Manual      //手动模式
    }
    public enum ydpMachEm
    {
        Idle=0,   //未连接
        AcceptDone,     //链接成功
        RecvId,         //等待接受设备ID状态
        RecvInfo,       //等待接受消息应答状态
        RecvDone,       //接收到数据
        DevAns,         //询问设备号应答成功
        InfoAns,        //发送态势信息应答成功
    }
}
