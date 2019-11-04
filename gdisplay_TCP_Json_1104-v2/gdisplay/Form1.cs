using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;

namespace gdisplay
{
    public partial class Form1 : Form
    {
        public static StreamWriter wrLog = new StreamWriter(".\\二环南路西段.txt",true);
        Byte s1_num = 0;                     //1,2,3,4,5,0(未选中)
        Byte[] s1SendArr = new byte[50];     //向屏幕1发送数据的缓存 
        //List<string> s1NodeList = new List<string>();       //用于按顺序存储屏1的路名
        Dictionary<string,List<string>> FindRectsDict = new Dictionary<string, List<string>>();     //用于存储屏1的搜索区域
        Dictionary<string,List<pathCfgInfo>> PathsDict = new Dictionary<string, List<pathCfgInfo>>();   //用于存储屏ID和路号,搜索路径
        Dictionary<string, string> IpsDict = new Dictionary<string, string>();  //用于存储屏ID和IP的对应关系
        Dictionary<string, int> NumsDict=new Dictionary<string, int>();
        Byte s2_num = 0;
        Byte[] s2SendArr = new byte[50];
        List<string> s2NodeList = new List<string>();       //用于按顺序存储屏1的路名
        List<string> s2FindRectList = new List<string>();   //用于存储屏1的搜索区域
        TcpServer sv=null;
        JsonConfig ydpCfg = new JsonConfig();               //json配置文件对象
        AmapJson ydpApJson = new AmapJson();                //json高德地图抓取数据
        AmapClient ydpApClient = AmapClient.CreateApClientObj("51a0f16ac37bcf88e634023f1529d84a");          //获得高德数据对象
        public Form1()
        {
            InitializeComponent();
            userUIInit();
            userTcpInit();
            //this.status_info.Text = "登录时间：" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        async void userUIInit()
        {
            //1.开启定时器
            tmDate.Start();
            //2.设置comBox工作模式
            cBox.Items.Add("人工模式");
            cBox.Items.Add("高德模式");
            cBox.SelectedIndex = 0;
            //3.设置listview显示模式
            lstview_s1.Columns.Add(new ColumnHeader() { Text = "ID", Width = 25 });
            lstview_s1.Columns.Add(new ColumnHeader() { Text = "屏幕1" });
            lstview_s1.Columns[1].Width = lstview_s1.ClientSize.Width - lstview_s1.Columns[0].Width;

            lstview_s2.Columns.Add(new ColumnHeader() { Text = "ID", Width = 25 });
            lstview_s2.Columns.Add(new ColumnHeader() { Text = "屏幕2" });
            lstview_s2.Columns[1].Width = lstview_s2.ClientSize.Width - lstview_s2.Columns[0].Width;

            lstview_s3.Columns.Add(new ColumnHeader() { Text = "ID", Width = 25 });
            lstview_s3.Columns.Add(new ColumnHeader() { Text = "屏幕3" });
            lstview_s3.Columns[1].Width = lstview_s3.ClientSize.Width - lstview_s3.Columns[0].Width;
            //4.设置后面筛选用的数组，简化switch-case
            this.s1_pixBox = new System.Windows.Forms.PictureBox[5] { s1_pa1, s1_pa2, s1_pa3, s1_pb, s1_pc };
            this.s2_pixBox = new System.Windows.Forms.PictureBox[8] { s2_pa1, s2_pa2, s2_pa3, s2_pa4, s2_pb, s2_pc, s2_pd, s2_pe };
            this.stsbarArr = new ToolStripStatusLabel[4] { stsbarComPort, stsbarCMD, stsbarMAP, stsbarTime };
            this.devBoxArr = new TextBox[4] { s1_devNameBox, s1_devStateBox, s2_devNameBox, s2_devStateBox };
            //5.解析json配置文件
            using (StreamReader rd = new StreamReader("E:\\ytr3.json"))
            {
                var jsonstr = await rd.ReadToEndAsync();
                await Task.Run(() =>
                {
                    ydpCfg = JsonConvert.DeserializeObject<JsonConfig>(jsonstr);
                    int ver = 1;
                    foreach (CfgScreens screens in ydpCfg.screens)
                    {
                        //1.添加屏id和数字的Dict
                        NumsDict.Add(screens.id, ver);          
                        ver++;
                        //2.添加屏id和IP的Dict
                        IpsDict.Add(screens.id, screens.ip);
                        //3.添加屏id和搜索区域的Dict
                        List<string> reL = new List<string>();  
                        foreach(string rect in screens.regions)
                        {
                            reL.Add(rect);
                        }
                        FindRectsDict.Add(screens.id, reL);
                        //4.添加屏id和路结构的Dict
                        List<pathCfgInfo> sigPaths = new List<pathCfgInfo>();
                        foreach (CfgRoads roads in screens.roads)
                        {                            
                            string mainRoad = roads.name;
                            List<string> nodeList = new List<string>();
                            foreach (string road in roads.sections)
                                nodeList.Add(road);

                            pathCfgInfo path = new pathCfgInfo(mainRoad,nodeList);
                            sigPaths.Add(path);
                        }
                        PathsDict.Add(screens.id, sigPaths);
                    }
                });
#region
                //用于调试配置文件的读取
                //using (StreamWriter wr = new StreamWriter("E:\\test.txt"))
                //{
                //    foreach (string r in IpsDict.Keys)
                //        wr.WriteLine(IpsDict[r]);

                //    foreach (var val in PathsDict.Values)
                //    {
                //        foreach (var road in val)
                //        {
                //            wr.Write(road.Main + " ");
                //            foreach (var r in road.NodeList)
                //                wr.Write(r + " ");
                //            wr.WriteLine("");
                //        }
                //    }
                //}
#endregion
            }

        } //async void userUIInit()

        void userTcpInit()
        {
            sv = new TcpServer();
            sv.Start("127.0.0.1", 1234);

            //以广播模式发送设备寻址
            //询问谁是1,2,3号设备(在connect中增加一个设备编号)
            //
            //sv.connects[i];
            //for i in 50:
            //  sv.connects[i].socket.Send(arr, 0, len);
            //  等待应答
            //  更新devNum成员
            
        }

        /*********************************************
        //UpdateState：有TcpServer更新UI控件数据
        //Para:
        //     msgType: 指定显示容器
        //              0:在状态栏显示
        //              1:在tabControl显示
        //     msgData: 在msgData指定容器中的显示位置
        //              0,1,2...n:依次从左到右，从上到下的控件
        //     text   ：显示文本
        ************************************************/
        public void UpdateState(int msgType, int msgData, string text)
        {
            switch (msgType)
            {
                case 0:        //在状态栏显示
                    stsbarArr[msgData].Text = text;
                    break;
                case 1:       //在tabControl显示
                    devBoxArr[msgData].Text = text;
                    break;
                default:
                    break;
            }
        }
        /**********************************************
        //myRClickMenuColor_s1:屏1右键显示颜色
        //Para:
        //     color:颜色编号
        ********************************************/
        void myRClickMenuColor_s1(int color)
        {
            //填充s1_sdarr[50]数组
            if (s1_num<1 || s1_num>8)
            {
                MessageBox.Show("请点击正确区域");
                return;
            }
            else if (s1_num>=1 && s1_num <=3)
            {
                if (color == 1)
                    s1_pixBox[s1_num - 1].Image = global::gdisplay.Properties.Resources.pa_red1;
                else if (color == 2)
                    s1_pixBox[s1_num - 1].Image = global::gdisplay.Properties.Resources.pa_yellow1;
                else if (color == 3)
                    s1_pixBox[s1_num - 1].Image = global::gdisplay.Properties.Resources.pa_green1;
            }
            else if(s1_num==4 || s1_num==5)
            {
                if (color == 1)
                    s1_pixBox[s1_num - 1].Image = global::gdisplay.Properties.Resources.pbc_red1;
                else if (color == 2)
                    s1_pixBox[s1_num - 1].Image = global::gdisplay.Properties.Resources.pbc_yellow1;
                else if (color == 3)
                    s1_pixBox[s1_num - 1].Image = global::gdisplay.Properties.Resources.pbc_green1;
            }
            s1_num = 0;
        }
        /**********************************************
        //myRClickMenuColor_s2:屏2右键显示颜色
        //Para:
        //     color:颜色编号
        ********************************************/
        void myRClickMenuColor_s2(int color)
        {
            if (s2_num < 1 || s2_num > 8)
            {
                MessageBox.Show("请点击正确区域");
                return;
            }
            else if (s2_num>=1 && s2_num<=4)   //s2_num=1..4：代表同一种类型的图片
            {
                if(color==1)
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.lpa_red2;
                else if(color == 2)
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.lpa_yellow2;
                else if(color==3)
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.lpa_green2;
            }
            else if(s2_num==5 || s2_num==6)
            {
                if (color == 1)
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.rpd_red2;
                else if (color == 2)
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.rpd_yellow2;
                else if (color == 3)
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.rpd_green2;
            }
            else if(s2_num==7)
            {
                if (color == 1)
                {
                    s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.lpd_red2;
                    s2_pixBox[s2_num].Image = global::gdisplay.Properties.Resources.pe_red2;
                }
                else if (color == 2)
                {
                    s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.lpd_yellow2;
                    s2_pixBox[s2_num].Image = global::gdisplay.Properties.Resources.pe_yellow2;
                }                    
                else if (color == 3)
                {
                    s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.lpd_green2;
                    s2_pixBox[s2_num].Image = global::gdisplay.Properties.Resources.pe_green2;
                }                   
            }
            //屏2中半圆形路段与写向左上的路段不同时变化时，需要取消这段注释
            #region
            /*************************************************
            //else if (s2_num == 8)
            //{
            //    if (color == 1)
            //        s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.pe_red2;
            //    else if (color == 2)
            //        s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.pe_yellow2;
            //    else if (color == 3)
            //        s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.pe_green2;
            //}
            **************************************************/
#endregion
            s1_num = 0;
        }
        private void s1_pa3_MouseUp(object sender, MouseEventArgs e)
        {
            s1_pa3.ContextMenuStrip = cMenu1_Color;
            s1_num = 3;
        }

        private void s1_pa2_MouseUp(object sender, MouseEventArgs e)
        {
            s1_pa2.ContextMenuStrip = cMenu1_Color;
            s1_num = 2;
        }

        private void s1_pa1_MouseUp(object sender, MouseEventArgs e)
        {
            s1_pa1.ContextMenuStrip = cMenu1_Color;
            s1_num = 1;
        }
        private void s1_pb_MouseUp(object sender, MouseEventArgs e)
        {
            s1_pb.ContextMenuStrip = cMenu1_Color;
            s1_num = 4;
        }
        private void s1_pc_MouseUp(object sender, MouseEventArgs e)
        {
            s1_pc.ContextMenuStrip = cMenu1_Color;
            s1_num = 5;
        }

        //myRClickMenuColor(int color)
        //1:红色
        //2:黄色
        //3:绿色
        private void Red1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myRClickMenuColor_s1(1);
        }

        private void Yellow1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myRClickMenuColor_s1(2);
        }

        private void Green1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myRClickMenuColor_s1(3);
        }

        private void s2_pa1_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pa1.ContextMenuStrip = cMenu2_Color;
            s2_num = 1;
        }

        private void s2_pa2_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pa2.ContextMenuStrip = cMenu2_Color;
            s2_num = 2;
        }

        private void s2_pa3_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pa3.ContextMenuStrip = cMenu2_Color;
            s2_num = 3;
        }

        private void s2_pa4_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pa4.ContextMenuStrip = cMenu2_Color;
            s2_num = 4;
        }

        private void s2_pc_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pc.ContextMenuStrip = cMenu2_Color;
            s2_num = 6;
        }

        private void s2_pb_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pb.ContextMenuStrip = cMenu2_Color;
            s2_num = 5;
        }

        private void s2_pe_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pe.ContextMenuStrip = cMenu2_Color;
            //s2_num = 8;
            s2_num = 7;
        }

        private void s2_pd_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pd.ContextMenuStrip = cMenu2_Color;
            s2_num = 7;
        }

        private void Red2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myRClickMenuColor_s2(1);
        }

        private void Yellow2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myRClickMenuColor_s2(2);
        }

        private void Green2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myRClickMenuColor_s2(3);
        }

        private void s1_BtnSnd_Click(object sender, EventArgs e)
        {
            //使用DEV1的设备
            //1.遍历connects，查找1号屏对应的socket

            //2.发送s1_sdarr[50]数据

            //3.等待接受应答???
            //
            Byte[] CMD = new byte[8] { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37 };
            Byte[] sArr = new byte[8];
            for(int i =0;i<sv.connects.Length;i++)
            {
                if (!sv.connects[i].isUse)
                    continue;

                int res = sv.SendData(sv.connects[i], CMD, CMD.Length);
                if (res>0)
                {
                    Array.Copy(CMD, sArr, res);
                    UpdateState(0, 1, "DATA: To [DEV1] " + System.Text.Encoding.ASCII.GetString(sArr));
                }
                if(res==-1)
                {
                    MessageBox.Show("Dev" + sv.connects[i].devNum + "已断开");
                    UpdateState(0, 1, "DATA: To [DEV1] " + "NO Data");
                }
                else if(res == -2)
                {
                    MessageBox.Show("Dev" + sv.connects[i].devNum + "发送失败");
                    UpdateState(0, 1, "DATA: To [DEV1] " + "NO Data");
                }
            }
        }

        private void s2_BtnSnd_Click(object sender, EventArgs e)
        {
            
        }
        //发送DEV1寻址
        //发问：我要寻找1号设备，返回1号设备的设备号
        //以广播模式发送
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //发送设备寻址
            if(tabControl1.SelectedTab.Name=="tabPage2")
            {
                //选中1号设备
                //MessageBox.Show("选中1号设备");
            }
            else if(tabControl1.SelectedTab.Name=="tabPage3")
            {
                //选中2号设备
                //MessageBox.Show("选中2号设备");
            }
            else if(tabControl1.SelectedTab.Name == "tabPage4")
            {
                //选中3号设备
                //MessageBox.Show("选中3号设备");
            }
        }
        /* ********************************************************
         * ApJsonToStArr:高德地图中获得的Json填充主路的状态数组，
         *               根据id判断是哪个屏，根据json数据判断是哪条主路
         * Param:
         *      jap:AMAP的json数据
         *      id:屏幕键值
         * *****************************************************/
        public void ApJsonToStArr(ValidJsonInfo vdjson, string id)
        {
            if (vdjson == null)                  
                return;
            if (!PathsDict.ContainsKey(id))                       //此键值不再配置文件所列范围内
                return;

            //1. 提取出屏幕的List<pathCfgInfo>
            List<pathCfgInfo> tmpPaths = PathsDict[id];           //暂存屏的pathCfgInfo列表            

            //2. 根据局部路段状态填充tmpPaths[roadIndex].MRoadArr数组 
            Byte overSte = vdjson.ValStatus;                      //0：未知,1:畅通,2:缓行,3:拥堵,4:严重拥堵
            foreach (AmapRoads road in vdjson.ValRoads)
            {
                int roadIndex = tmpPaths.FindIndex((item) =>      //Lambda表达式，搜索road是否在屏规定的路段内
                {
                    return item.MRoad.Equals(road.name);
                });
                if (roadIndex == -1)                              //不是要找的主干道
                    continue;
                else if (tmpPaths[roadIndex].IsVisited == true)   //主干路已经访问过，继续向下搜索json数据
                    continue;
                //2.1 填充主路数组
                else
                {
                    /*tmpPaths[roadIndex].IsVisited = true; */        //标志已经填充,发送完之后改成false
                    string pathDesp = road.direction;             //描述方向:从xxx到yyy
                    string pathSta = road.status;                 //拥堵状况
                    string pathSpeed = road.speed;                //速度
                    //2.1.1 先去掉'从'字,将路段按照'到'分隔
                    string substr = pathDesp.Substring(1);
                    string[] arrPath = substr.Split(new char[] { '到' }, 2);
                    if (arrPath.Length != 2)   //"xxx附近"这种暂不处理
                        continue;
                    //2.1.2 获得路节点在配置文件中的索引
                    int index1 = tmpPaths[roadIndex].NodeList.FindIndex((item) =>
                    {
                        return item.Equals(arrPath[0]);
                    });
                    int index2 = tmpPaths[roadIndex].NodeList.FindIndex((item) =>
                    {
                        return item.Equals(arrPath[1]);
                    });
                    if (index1 == -1 || index2 == -1)      //高德返回的数据不再配置文件给定的列表路段节点中
                        continue;
                    else if (index1 > index2)              //高德返回的路径与配置文件给出的路段相反
                        continue;
                    //2.1.3 开始填充主路数组  
                    else                                   //正向且在搜索的主路上
                    {
                        tmpPaths[roadIndex].IsVisited = true;
                        //a.根据整体路况，预填充数组
                        for (int i =0;i< tmpPaths[roadIndex].NodeList.Count-1;i++)
                            tmpPaths[roadIndex].MRoadArr[i]=(overSte);

                        //b.改变tmpPaths[roadIndex].MRoadArr数组中的路段之间的所有值
                        for (int i = index1; i < index2; i++)
                            //s1SendArr[i] = Byte.Parse(pathSta); //string转成Byte
                            tmpPaths[roadIndex].MRoadArr[i] = Byte.Parse(pathSta);
                    }
                }
            }
        }
        private ValidJsonInfo JsonToValid(AmapJson jap,string key)
        {
            ValidJsonInfo val = new ValidJsonInfo();
            if(jap == null)
            {
                //stsbarMAP.Text = "[DEV" + key + "]" + "获得高德数据失败";
                MessageBox.Show("[DEV" + key + "]" + "获得高德数据失败");
                return null;
            }
            try
            {
                string overSte = jap.trafficinfo.evaluation.status;   //0：未知,1:畅通,2:缓行,3:拥堵,4:严重拥堵
                val.ValStatus = Byte.Parse(overSte);
                val.ValRoads = jap.trafficinfo.roads;
                return val;
            }
            catch
            {
                return null;
            }
        }
        private async void tmDate_Tick(object sender, EventArgs e)
        {
            //1.在状态栏显示时间
            string localtime = DateTime.Now.ToString(" yyyy-MM-dd HH:mm:ss");
            stsbarTime.Text = localtime;

            //2.对屏i规定的区域向高德发送请求
            foreach (var key in FindRectsDict.Keys)          //每个key代表一个屏
            {
                foreach (string rect in FindRectsDict[key]) //每个rect代表一个搜索区域
                {
                    int reSendCont = 0;
                    ValidJsonInfo valJson;
                    //2.1 按区域异步发送请求，失败，连发三次
                    do
                    {
                        ydpApJson = await ydpApClient.GetJsonFromAmapAsync(rect);
                        valJson = JsonToValid(ydpApJson, key);
                        if (valJson == null)              //保证第一次成功不延迟，失败延迟
                            Thread.Sleep(300);
                    } while (valJson == null && reSendCont++ < 3);
                    //2.2 对于返回的Json数据填充状态数组
                    if (valJson != null)
                        ApJsonToStArr(valJson, key);
                }
                //通过写入文件模拟发送过程
                wrLog.WriteLine("屏幕" + key + "->IP:" + IpsDict[key]);
                foreach (var paths in PathsDict[key])
                {
                    wrLog.Write(paths.MRoad + "的状态数组: ");
                    for (int i = 0; i < paths.MRoadArr.Length; i++)
                        wrLog.Write("0x" + paths.MRoadArr[i] + " ");
                    wrLog.Write("\n");
                }
                wrLog.WriteLine(" ");

                foreach (pathCfgInfo road in PathsDict[key])
                    road.IsVisited = false;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Byte[] arr = new byte[5]{ 49, 50, 51, 52, 53 };
            MessageBox.Show(Encoding.Default.GetString(arr));
        }
    }
}
