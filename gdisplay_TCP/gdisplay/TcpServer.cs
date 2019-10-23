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
        public const int BUFFER_SIZE = 1024;
        public byte[] readBuff = new byte[BUFFER_SIZE];  //读缓冲区
        public int buffCount = 0;                        //当前缓冲区大小
        public bool isUse = false;

        public Connect()
        {
            readBuff = new byte[BUFFER_SIZE];
        }
        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
        }
        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }
        //获取客户端IP地址和端口
        public string GetAdress()
        {
            if (!isUse)
            {
                return "don't get IP:Port";
            }
            return socket.RemoteEndPoint.ToString();
        }
        public void Close()
        {
            if (!isUse)
                return;
            //Console.WriteLine("[断开连接]" + GetAdress());
            socket.Close();
            isUse = false;
        }
    }
    class TcpServer
    {
        public Socket listenfd;
        public Connect[] connects;
        public int maxConnectCount = 50;

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
            listenfd.BeginAccept(AcceptCb, null);  //state可以传递用户的数据，比如listrnfd????UI有问题
            //Console.WriteLine("[服务器]启动成功");
            //在控制栏显示服务器已经开启???
        }

        //获取连接池索引，返回负数表示获取失败
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
                else if (connects[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        //Accept是BeginAccept的回调函数，它成立了如下3件事情
        //（1）给新的连接分配connect
        //（2）异步接收客户端数据
        //（3）再次调用BeginAccept实现循环
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
                    //Console.WriteLine("[警告]连接已满");
                    MessageBox.Show("[警告]连接已满");                    
                }
                else
                {
                    //3.构造connect，绑定缓存，绑定fd
                    Connect connect = connects[index];
                    connect.Init(socket);  //socket->connect,isUse=true
                    string adr = connect.GetAdress();
                    //Console.WriteLine("客户端[" + adr + "]connect池ID:" + index);
                    //需要把每台设备的连接情况显示到一个text上???
                    //4.启动异步接受
                    //参数的含义:接受buffer,填充开始位置，填充长度，xxx，异步回调函数，传给回调函数的参数
                    connect.socket.BeginReceive(connect.readBuff, connect.buffCount, connect.BuffRemain(), SocketFlags.None, ReceiveCb, connect);
                }
                //5.再次开启等待连接事件
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb失败" + e.Message);
            }
        }

        //ReceiveCb是BeginReceive的回调函数，它处理了3件事情
        //（1）接收并处理消息，因为有多个客户端，服务端收到消息后，要把它转发给所有人
        //（2）如果收到客户端关闭连接的信号（count==0），则断开连接
        //（3）继续调用BeginReceive接收下一个数据
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
                    Console.WriteLine("收到[" + connect.GetAdress() + "]断开连接");
                    connect.Close();
                    return;
                }
                string str = System.Text.Encoding.UTF8.GetString(connect.readBuff, 0, count);
                Console.WriteLine("收到[" + connect.GetAdress() + "]数据:" + str);
                str = connect.GetAdress() + ":" + str;
                byte[] sdbytes = System.Text.Encoding.Default.GetBytes(str);
                //广播???
                //for(int i=0;i<connects.Length;i++)
                //{
                //    if (connects[i] == null)
                //        continue;
                //    if (!connects[i].isUse)
                //        continue;
                //    Console.WriteLine("将消息转播给" + connects[i].GetAdress());
                //    connects[i].socket.Send(sdbytes);
                //}
                Console.WriteLine("将消息转播给" + connect.GetAdress());
                connect.socket.Send(sdbytes);
                //3.再一次启动异步调用接受函数
                connect.socket.BeginReceive(connect.readBuff, connect.buffCount, connect.BuffRemain(), SocketFlags.None, ReceiveCb, connect);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb失败:" + e.Message);
                Console.WriteLine("收到[" + connect.GetAdress() + "]断开连接");
                connect.Close();
            }
        }
    }
}
