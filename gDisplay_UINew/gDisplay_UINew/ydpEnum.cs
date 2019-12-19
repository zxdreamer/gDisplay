using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gDisplay_UINew
{
    public enum ydpShowEm
    {
        S1DevName = 0,   //设备信息
        S1DevState = 1,
        S2DevName = 2,
        S2DevState = 3,
        S3DevName = 4,
        S3DevState = 5,

        StsDevStat = 0, //状态栏信息
        StsSndData = 1,
        StsMode = 2,
        STime = 3
    }
    public enum ydpPlaceEm
    {
        StsBar = 0,
        TxtBox
    }
    public enum ydpModeEm
    {
        Amap = 0,     //高德模式
        Manual      //手动模式
    }
    //诱导屏工作状态的状态机枚举
    public enum ydpMachEm
    {
        Idle = 0,         //未连接
        AcceptDone,     //链接成功
        RecvId,         //等待接受设备ID状态
        Manual,         //接收到数据
        RecvReply,        //询问设备号应答成功
        TxReady,        //发送态势信息应答成功
    }
    //要发送数据是否准备好的状态机枚举
    public enum ydpOkMachEm
    {
        Unok = 0,           //数据为准备好
        Amap,           //高德模式下数据准备好
        Manual          //手动模式下数据准备好
    }
}
