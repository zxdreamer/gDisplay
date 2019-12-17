using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
namespace gdisplay
{
    public class Connect
    {
        public Socket socket;
        public const int RXBUFFER_SIZE = 1550;
        public byte[] readBuff;                             //读缓冲区
        public byte[] frameBuff;                            //帧缓冲
        public int frameLen;                                //帧长度
        public string devid;                                //设备编号

        public bool flagFrame;                              //标记状态机即将处于RecvDone状态
        public bool flagClose;                              //标记状态机即将处于Idle状态
        public ydpMachEm conState;                          //状态机
                                                            //0:未连接，1:已连接，2:已绑定设备号，3:通信应答正确
        public int rxCnt;                                   //接受数据计数器
        public int fsStart;                                 //帧开始标记
        public int delayCnt;

        public bool flagLog;
        public Connect()
        {
            this.readBuff = new byte[RXBUFFER_SIZE];
            this.frameBuff = new byte[RXBUFFER_SIZE*2];
            this.frameLen = 0;
            this.devid = null;

            this.flagFrame = false;
            this.flagClose = false;
            this.conState = ydpMachEm.Idle;

            this.rxCnt = 0;
            this.fsStart = -1;
            this.delayCnt = 0;

            this.flagLog = false;
        }
        public void Init(Socket socket)
        {
            this.socket = socket;
            this.conState = ydpMachEm.AcceptDone;
        }
        //获取客户端IP地址和端口
        public string GetRemoteAdress()
        {
            if(conState == ydpMachEm.Idle)
            {
                return "don't get IP:Port";
            }
            return socket.RemoteEndPoint.ToString();
        }
        public void Close()
        {
            if(conState==ydpMachEm.Idle)
                return;
            socket.Close();
            devid = null;
            this.flagClose = true;
            ydpLog.WriteLineLog(DateTime.Now.ToString() + ":设备" + this.devid + " 已断开!");

        }
    }
    class TcpServer
    {
        public Socket listenfd;
        public Connect[] connects;
        public int maxConnectCount = 50;
        public ydpACK obj_ydpAck;         //诱导屏接受处理函数
        //Start：启动TcpServer
        public void Start(string host, int port)
        {       
            //1.connects数组初始化：connects的每个元素绑定1k缓存
            connects = new Connect[maxConnectCount];
            for (int i = 0; i < maxConnectCount; i++)
                connects[i] = new Connect();

            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            //2. bind 和 listen
            listenfd.Bind(ipEp);
            listenfd.Listen(maxConnectCount);
            //3. 调用异步函数BeginAccept,参数使用委托
            listenfd.BeginAccept(AcceptCb, null);  //state可以传递用户的数据，比如listernfd????UI有问题
        }

        /**************************************************
        //NewIndex：获取连接池索引，返回负数表示获取失败
        **************************************************/
        public int NewIndex()
        {
            //connects可让UI来代用，加入超时机制，发起回调收不到的异常处理????
            if (connects == null)
                return -1;
            for (int i = 0; i < connects.Length; i++)
            {
                if (connects[i] == null)
                {
                    connects[i] = new Connect();
                    return i;
                }
                //else if (connects[i].isUse == false)
                else if ( connects[i].conState==ydpMachEm.Idle)
                {
                    return i;
                }
            }
            return -1;
        }
        /******************************************************
        //Accept是BeginAccept的回调函数，它成立了如下3件事情
        //（1）给新的连接分配connect
        //（2）异步接收客户端数据
        //（3）再次调用BeginAccept实现循环
        *******************************************************/
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                //1.获得连接到的客户端的fd
                Socket socket = listenfd.EndAccept(ar);
                //2.从connects中查找一个空位置
                int index = NewIndex();
                if (index < 0)
                {
                    socket.Close();
                }
                else
                {
                    //3.构造connect，绑定缓存，绑定fd
                    Connect connect = connects[index];
                    connect.Init(socket);  //socket->connect,isUse=true
                    string adr = connect.GetRemoteAdress();

                    //需要把每台设备的连接情况显示到状态栏和日志文件
                    //Program.gdFrom.UpdateState(0, 0, "[Dev:" + connect.devid + "]" + "已连接");
                    //ydpLog.WriteLineLog(DateTime.Now.ToString()+":[Dev:" + connect.devid + "]" + "已连接");

                    //4.连接成功标志位有效
                    //connect.bAskId = true;
                    //connect.conState = ydpMachEm.DoneConn;   //标志链接成功

                    //5.启动异步接受
                    //参数的含义:接受buffer,填充开始位置，填充长度，xxx，异步回调函数，传给回调函数的参数
                    connect.socket.BeginReceive(connect.readBuff, 0, Connect.RXBUFFER_SIZE, SocketFlags.None, ReceiveCb, connect);
                }
                //6.再次开启等待连接事件
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                MessageBox.Show("Receive断开"); 
            }
        }
        /******************************************************************
        //ReceiveCb是BeginReceive的回调函数，它处理了3件事情
        //（1）接收并处理消息，因为有多个客户端，服务端收到消息后，要把它转发给所有人
        //（2）如果收到客户端关闭连接的信号（count==0），则断开连接
        //（3）继续调用BeginReceive接收下一个数据
        *******************************************************************/
        private void ReceiveCb(IAsyncResult ar)
        {
            //1.获得传入的connect
            Connect connect = (Connect)ar.AsyncState;   //异步相关的信息
            try
            {
                //2.获得传入的数据的字节数
                int count = connect.socket.EndReceive(ar);  
                if (count <= 0)
                {
                    //在状态栏label3处显示客户端断开连接
                    Program.gdFrom.UpdateState(0, 0, "[Dev:" + connect.devid + "]" + "已断开");
                    Program.gdFrom.TxBoxState(connect, 0);
                    ydpLog.WriteLineLog(DateTime.Now.ToString()+":[Dev:" + connect.devid + "]" + "已断开");
                    connect.Close();
                    return;
                }

                string str = ydpLog.ByteToRawStr(connect.readBuff,0,count);
                if(connect.flagLog == false)   //???????
                {
                    ydpLog.WriteAddLog("用户输入数据");
                    connect.flagLog = true;
                }
                ydpLog.WriteAddLog(":" + str+"\n");
                //3.数据包处理形成帧
                int index = 0;
                //3.1.先将数据从读缓冲中复制到帧缓冲中
                for(int i=0;i < count; i++)
                {
                    index = connect.rxCnt;
                    connect.frameBuff[index] = connect.readBuff[i];
                    connect.rxCnt++;
                }                

                //3.2. 判断是否找到了包头,未找到包头，则去寻找包头
                if(connect.fsStart == -1)       
                {
                    for(int i = connect.rxCnt-count;i < connect.rxCnt; i++)
                    {
                        if (connect.frameBuff[i] == 0x02)
                        {
                            connect.fsStart = i;
                            break;
                        }
                    }
                }
                //3.3. 找到包头且此包中包含有效载荷的长度
                if(connect.fsStart > -1 && (connect.rxCnt- connect.fsStart)>=7)  //长度包含在这一包之内
                {
                    int payLoadH = connect.frameBuff[connect.fsStart + 5];
                    int payLoadL = connect.frameBuff[connect.fsStart + 6];
                    int plLen = (payLoadH << 8) + payLoadL;
                    //包格式:02 30 30 cmd cmd lenH lenL payload ...... CRC1 CRC2 03 
                    if(connect.frameBuff[6 + plLen + 2 + 1]==0x03)
                    {
                        //3.4. 标记已经成包
                        connect.flagFrame = true;
                        connect.frameLen = plLen;
                        connect.rxCnt = 0;
                        string uStr = ydpLog.ByteToRawStr(connect.frameBuff,connect.fsStart, 6 + plLen + 2 + 1);
                        ydpLog.WriteLineLog("解析用户数据:"+uStr);
                    }
                }

                //4.再一次启动异步调用接受函数
                connect.socket.BeginReceive(connect.readBuff, 0, Connect.RXBUFFER_SIZE, SocketFlags.None, ReceiveCb, connect);
            }
            catch (Exception e)
            {
                //MessageBox.Show("Receive断开");
                //Program.gdFrom.UpdateState(0, 0, "[Dev:" + connect.devid + "]" + "断开");
                ydpLog.WriteLineLog(DateTime.Now.ToString() + ":[Dev:" + connect.devid + "]" + "已断开");
                connect.Close();
            }
        }
        /*****************************************************
        //SendData:发送数据包
        //Para:
        //    con:  client句柄
        //    arr:  发送缓存
        //    size: 发送数据长度
        *****************************************************/
        public int SendData(Connect con, Byte[] arr, int size)
        {
            int nRes = 0;
            //MessageBox.Show("发送OK1");
            if (size == 0)
                return 0;
            ////检测数据包是否正确
            try
            {               
                nRes = con.socket.Send(arr, size, 0);
            }
            catch (ObjectDisposedException e)
            {
                nRes = -1;
                Program.gdFrom.UpdateState(0, 0, "[Dev:" + con.devid + "]" + "已断开!");
                con.Close();
            }
            catch(Exception e)
            {
                nRes = -2;
                Program.gdFrom.UpdateState(0, 0, "[Dev:" + con.devid + "]" + "发送失败!");
            }

            return nRes;
        }
    }
}
