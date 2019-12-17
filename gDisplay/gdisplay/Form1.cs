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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace gdisplay
{
    public partial class Form1 : Form
    {
        Byte s1_num = 0;                     //1,2,3,4,5,0(未选中)
        Byte s2_num = 0;                     //屏体2区域编号
        Byte s3_num = 0;                     //屏体3区域编号

        const int SCREEN_NUMS = 3;            //屏体的数量
        ydpModeEm wk_mode = ydpModeEm.Amap;                    //默认为高德模式
        List<ScreenResult> scrResultList = new List<ScreenResult>();

        TcpServer sv = null;
        ydpJsonConfig ydpCfg = new ydpJsonConfig();               //json配置文件对象
        //AmapClient ydpApClient = AmapClient.CreateApClientObj("51a0f16ac37bcf88e634023f1529d84a");          //获得高德数据对象
        AmapClient ydpApClient = null;
        int AMAPreqcnt = 0;         //50ms计数器
        int AMAPreqtime = 0;        //配置文件中高德请求时间
        int AmapIndex = 0;          //处理哪块儿屏幕计数
        int tcpPort = 8000;
        string tcpIp = "";
        public Form1()
        {
            ydpLog.WriteLineLog(DateTime.Now.ToString() + "---------------------------程序开始启动----------------------------------");
            InitializeComponent();
            userJsonInit();     //Json配置文件解析
            userUIInit();       //窗体控件初始化            
            userTcpInit();      //TCPserver初始化，并启动监听
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void userUIInit()
        {
            //1.这些数组的内存要在使用之前先申请，所以放到最前面
            this.stsbarArr = new ToolStripStatusLabel[4] { stsbarDev, stsbarSta, stsbarMode, stsbarTime };
            this.devBoxArr = new TextBox[6] { s1_devNameBox, s1_devStateBox, s2_devNameBox, s2_devStateBox, s3_devNameBox, s3_devStateBox };
            this.sMenuArr = new ContextMenuStrip[3] { cMenu1_Color, cMenu2_Color, cMenu3_Color };
            //2.设置comBox工作模式
            if(ydpCfg.isAMAP==true)
            {
                cBoxMode.Items.Add("高德模式");
                cBoxMode.Items.Add("人工模式");
            } 
            else
            {
                cBoxMode.Items.Add("人工模式");
                cBoxMode.Items.Add("高德模式");                
            }
            cBoxMode.SelectedIndex = 0;
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
            this.s1_picBoxArr = new System.Windows.Forms.PictureBox[5] { s1_pa1, s1_pa2, s1_pa3, s1_pb, s1_pc };
            this.s2_picBoxArr = new System.Windows.Forms.PictureBox[8] { s2_pa1, s2_pa2, s2_pa3, s2_pa4, s2_pb, s2_pc, s2_pd, s2_pe };
            this.s3_picBoxArr = new System.Windows.Forms.PictureBox[6] { s3_pa1, s3_pa2, s3_pa3, s3_pb, s3_pc, s3_pd };
            //5.开启定时器
            ydpTimer.Start();
        } //async void userUIInit()

        void userJsonInit()
        {
            try
            {
                using (StreamReader rd = new StreamReader(@".\\ytr3.json"))
                {
                    //var jsonstr = await rd.ReadToEndAsync();
                    var jsonstr = rd.ReadToEnd();
                    ydpCfg = JsonConvert.DeserializeObject<ydpJsonConfig>(jsonstr);

                    ydpLog.WriteLineLog("开始解析配置文件......");
                    ydpApClient = AmapClient.CreateApClientObj(ydpCfg.AMAPkey);     //获得高德数据对象
                    AMAPreqtime = ydpCfg.AMAPreqtime * 2;    //定时器定时500ms，这里乘2
                    tcpPort = ydpCfg.TCPport;
                    tcpIp = ydpCfg.IpAddr;
                    foreach (CfgScreens screens in ydpCfg.screens)
                    {
                        //1.获得屏幕id
                        string sid = screens.id;

                        //2.添加屏id和搜索区域的Dict
                        List<string> reL = new List<string>();
                        foreach (string rect in screens.regions)
                        {
                            reL.Add(rect);
                        }

                        //4.添加屏id和路结构的Dict
                        List<RoadsResult> sigPaths = new List<RoadsResult>();
                        foreach (CfgRoads road in screens.roads)
                        {
                            string mainRoad = road.name;
                            //4.1 添加路段节点
                            List<string> nodeList = new List<string>();
                            foreach (string node in road.sections)
                                nodeList.Add(node);
                            //4.2 添加路段编号
                            List<string> idsList = new List<string>();
                            foreach (string id in road.ids)
                                idsList.Add(id);
                            //4.3 添加路段角度
                            List<int> angList = new List<int>();
                            foreach (int anl in road.angles)
                                angList.Add(anl);

                            RoadsResult path = new RoadsResult(mainRoad, nodeList, angList, idsList);
                            sigPaths.Add(path);
                        }
                        //5.屏幕光带显示与路段解析对应关系解析
                        List<List<string>> bd = screens.band;
                        //6.依次将每一块屏的配置信息添加进去
                        scrResultList.Add(new ScreenResult(sigPaths, reL, bd,sid));
                    }
                    //});
                    ydpLog.WriteLineLog("配置文件解析结果:");
                    foreach (var sc in scrResultList)
                    {
                        ydpLog.WriteLineLog("屏幕 " + sc.Id);

                        string region = "搜索区域：";
                        foreach (var rect in sc.SfindRect)
                        {
                            region += rect + " ";
                        }
                        ydpLog.WriteLineLog(region);

                        foreach (var path in sc.Sroads)
                        {
                            ydpLog.WriteLineLog(path.MRoad);
                            string nodes = "节点:";
                            foreach (var node in path.SectList)
                            {
                                nodes += node + " ";
                            }
                            ydpLog.WriteLineLog(nodes);
                        }
                    }
                }
                ydpLog.WriteLineLog(DateTime.Now.ToString() + "---------------------------配置文件解析完成----------------------------------");
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show("配置文件不存在");
            }
            catch (Exception e)
            {
                ydpLog.WriteLineLog("配置文件解析异常" + e.Message);
            }
        }

        void userTcpInit()
        {
            sv = new TcpServer();
            //sv.Start("127.0.0.1", 1234);
            sv.Start(tcpIp, tcpPort);
            //扩展：IP自动获取本地
        }
        /* ***********************************************************
        * UpdateState：更新UI控件数据
        * Para:
        *      msgType: 指定显示容器
        *               0:在状态栏显示
        *               1:在tabControl显示
        *      msgData: 在msgData指定容器中的显示位置
        *               0,1,2...n:依次从左到右，从上到下的控件
        *      text   ：显示文本
        * *************************************************************/
        public void UpdateState(ydpPlaceEm msgType, ydpShowEm msgData, string text)
        {
            int msg = (int)msgData;
            switch (msgType)
            {
                case ydpPlaceEm.StsBar:        //在状态栏显示
                    stsbarArr[msg].Text = text;
                    break;
                case ydpPlaceEm.TxtBox:       //在tabControl显示
                    devBoxArr[msg].Text = text;
                    break;
                default:
                    break;
            }
        }
        /********************************************************
         * TxBoxState:用于屏体的两个TextBox显示
         * Param:
         *      sc:与屏体绑定的connect对象
         *      onOroff:打开关闭指示符 0：未连接，1：连接成功
         * *******************************************************/
        public void TxBoxState(Connect sc, int onOoff)
        {
            string[] arr = { "未连接", "连接成功" };
            if (sc.devid == "ydp001")
            {
                UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S1DevName, "[Dev:" + sc.devid + "]");
                UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S1DevState, arr[onOoff]);
            }
            else if (sc.devid == "ydp002")
            {
                UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S2DevName, "[Dev:" + sc.devid + "]");
                UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S2DevState, arr[onOoff]);
            }
            else if (sc.devid == "ydp003")
            {
                UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S3DevName, "[Dev:" + sc.devid + "]");
                UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S3DevState, arr[onOoff]);
            }
        }
        /**********************************************
        * myRClickMenuColor_s1:屏1右键显示颜色
        * Para:
        *      color:颜色编号
        ********************************************/
        void myChangeMenuColor_s1(int color,int snum)
        {
            //填充s1_sdarr[50]数组
            if (snum < 1 || snum > 8)
            {
                MessageBox.Show("请点击正确区域1");
                return;
            }
            else if (snum >= 1 && snum <= 3)
            {               
                if (color == 1)
                    s1_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_green1;                  
                else if (color == 2)
                    s1_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_yellow1;
                else if (color == 3 || color == 4)
                    s1_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_red1;
            }
            else if (snum == 4 || snum == 5)
            {
                if (color == 1)
                    s1_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pbc_green1;
                else if (color == 2)
                    s1_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pbc_yellow1;
                else if (color == 3 || color==4)
                    s1_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pbc_red1;
            }
            //s1_StaArr[snum - 1] = (byte)color;             
            scrResultList[0].ColorArr[snum - 1] = (byte)color; //将当前的路段颜色存入路段数组中
        }
        /**********************************************
        //myRClickMenuColor_s2:屏2右键显示颜色
        //Para:
        //     color:颜色编号
        ********************************************/
        void myChangeMenuColor_s2(int color,int snum)
        {
            //WriteLineLog("右击屏2：" + "区域" + snum + " " + "颜色" + color);
            if (snum < 1 || snum > 8)
            {
                MessageBox.Show("请点击正确区域2");
                return;
            }
            else if (snum >= 1 && snum <= 4)   //snum=1..4：代表同一种类型的图片
            {
                if (color == 1)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpa_green2;
                else if (color == 2)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpa_yellow2;
                else if (color == 3 || color == 4)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpa_red2;
            }
            else if (snum == 5 || snum == 6)
            {
                if (color == 1)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.rpd_green2;
                else if (color == 2)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.rpd_yellow2;
                else if (color == 3 || color == 4)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.rpd_red2;
            }
            else if (snum == 7)
            {
                if (color == 1)
                {
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpd_green2;
                    s2_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.pe_green2;
                }
                else if (color == 2)
                {
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpd_yellow2;
                    s2_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.pe_yellow2;
                }
                else if (color == 3 || color == 4)
                {
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpd_red2;
                    s2_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.pe_red2;
                }
            }
            //屏2中半圆形路段与写向左上的路段不同时变化时，需要取消这段注释
            #region
            /*************************************************
            //else if (snum == 8)
            //{
            //    if (color == 1)
            //        s2_pixBox[snum - 1].Image = global::gdisplay.Properties.Resources.pe_red2;
            //    else if (color == 2)
            //        s2_pixBox[snum - 1].Image = global::gdisplay.Properties.Resources.pe_yellow2;
            //    else if (color == 3)
            //        s2_pixBox[snum - 1].Image = global::gdisplay.Properties.Resources.pe_green2;
            //}
            **************************************************/
            #endregion
            scrResultList[1].ColorArr[snum - 1] = (byte)color; //将当前的路段颜色存入路段数组中
        }
        /**********************************************
        //myRClickMenuColor_s2:屏2右键显示颜色
        //Para:
        //     color:颜色编号
        ********************************************/
        void myChangeMenuColor_s3(int color,int snum)
        {
            //WriteLineLog("右击屏3：" + "区域" + snum + " " + "颜色" + color);
            if (snum < 1 || snum > 6)
            {
                MessageBox.Show("请点击正确区域3");
                return;
            }
            else if (snum >= 1 && snum <= 3)   //s2_num=1..3：代表同一种类型的图片
            {
                if (color == 1)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_green3;
                else if (color == 2)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_yellow3;
                else if (color == 3 || color == 4)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_red3;
            }
            else if (snum == 4)
            {
                if (color == 1)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpb_green3;
                else if (color == 2)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpb_yellow3;
                else if (color == 3 || color == 4)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpb_red3;
            }
            else if (snum == 5)
            {
                if (color == 1)
                {
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpc_green3;
                    s3_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.lpd_green3;
                }
                else if (color == 2)
                {
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpc_yellow3;
                    s3_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.lpd_yellow3;
                }
                else if (color == 3 || color == 4)
                {
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpc_red3;
                    s3_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.lpd_red3;
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
            scrResultList[2].ColorArr[snum - 1] = (byte)color; //将当前的路段颜色存入路段数组中
        }

        private void manualPixShow(PictureBox s_pix,int scn)
        {
            if(wk_mode == ydpModeEm.Manual)
            {
                s_pix.ContextMenuStrip = sMenuArr[scn];   //弹出右键菜单栏
                sMenuArr[scn].Show(MousePosition.X, MousePosition.Y);
            }
            else
            {
                MessageBox.Show("高德模式不能手动点击！");
            }

        }

        private void s1_pa3_MouseUp(object sender, MouseEventArgs e)
        {
            //s1_pa3.ContextMenuStrip = cMenu1_Color;
            s1_num = 3;
            manualPixShow(s1_pa3, 0);
        }

        private void s1_pa2_MouseUp(object sender, MouseEventArgs e)
        {
            //s1_pa2.ContextMenuStrip = cMenu1_Color;
            s1_num = 2;
            manualPixShow(s1_pa2, 0);
        }

        private void s1_pa1_MouseUp(object sender, MouseEventArgs e)
        {
            //s1_pa1.ContextMenuStrip = cMenu1_Color;
            s1_num = 1;
            manualPixShow(s1_pa1, 0);
        }
        private void s1_pb_MouseUp(object sender, MouseEventArgs e)
        {
            //s1_pb.ContextMenuStrip = cMenu1_Color;
            s1_num = 4;
            manualPixShow(s1_pb, 0);
        }
        private void s1_pc_MouseUp(object sender, MouseEventArgs e)
        {
            //s1_pc.ContextMenuStrip = cMenu1_Color;
            s1_num = 5;
            manualPixShow(s1_pc, 0);
        }

        //myRClickMenuColor(int color)
        //1:红色
        //2:黄色
        //3:绿色
        private void Red1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s1(3,s1_num);
        }

        private void Yellow1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s1(2,s1_num);
        }

        private void Green1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s1(1,s1_num);
        }

        private void s2_pa1_MouseUp(object sender, MouseEventArgs e)
        {
            //s2_pa1.ContextMenuStrip = cMenu2_Color;
            s2_num = 1;
            manualPixShow(s2_pa1, 1);
        }

        private void s2_pa2_MouseUp(object sender, MouseEventArgs e)
        {
            //s2_pa2.ContextMenuStrip = cMenu2_Color;
            s2_num = 2;
            manualPixShow(s2_pa2, 1);
        }

        private void s2_pa3_MouseUp(object sender, MouseEventArgs e)
        {
            //s2_pa3.ContextMenuStrip = cMenu2_Color;
            s2_num = 3;
            manualPixShow(s2_pa3, 1);
        }

        private void s2_pa4_MouseUp(object sender, MouseEventArgs e)
        {
            //s2_pa4.ContextMenuStrip = cMenu2_Color;
            s2_num = 4;
            manualPixShow(s2_pa4, 1);
        }

        private void s2_pc_MouseUp(object sender, MouseEventArgs e)
        {
            //s2_pc.ContextMenuStrip = cMenu2_Color;
            s2_num = 6;
            manualPixShow(s2_pc, 1);
        }

        private void s2_pb_MouseUp(object sender, MouseEventArgs e)
        {
            //s2_pb.ContextMenuStrip = cMenu2_Color;
            s2_num = 5;
            manualPixShow(s2_pb, 1);
        }

        private void s2_pe_MouseUp(object sender, MouseEventArgs e)
        {
            //s2_pe.ContextMenuStrip = cMenu2_Color;
            s2_num = 7;
            manualPixShow(s2_pe, 1);
        }

        private void s2_pd_MouseUp(object sender, MouseEventArgs e)
        {
            //s2_pd.ContextMenuStrip = cMenu2_Color;
            s2_num = 7;
            manualPixShow(s2_pd, 1);
        }

        private void Red2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s2(3,s2_num);
        }

        private void Yellow2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s2(2, s2_num);
        }

        private void Green2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s2(1, s2_num);
        }

        //发送DEV1寻址
        //发问：我要寻找1号设备，返回1号设备的设备号
        //以广播模式发送
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //发送设备寻址
            if (tabCtlSelect.SelectedTab.Name == "tabPage2")
            {
                //选中1号设备
                //MessageBox.Show("选中1号设备");
            }
            else if (tabCtlSelect.SelectedTab.Name == "tabPage3")
            {
                //选中2号设备
                //MessageBox.Show("选中2号设备");
            }
            else if (tabCtlSelect.SelectedTab.Name == "tabPage4")
            {
                //选中3号设备
                //MessageBox.Show("选中3号设备");
            }
        }
        /****************************************************
         * FromAndToDeal:解析从xxx附近
         * Param：
         *      pathDir:AMAP中一条主路描述
         *      nodeList:屏的主路节点列表
         * Result：
         *      路段在节点中的索引
         * *************************************************/
        List<int> NearbyDeal(string pathDir, int pathAngle, List<string> nodeList, List<int> angleList)
        {
            List<int> res = new List<int>() { 0, 0 };

            string pathName = pathDir.Substring(0, pathDir.Length - 2); //去掉"附近"这两个字
            //1.获得主路索引
            int mrIndex = nodeList.FindIndex((item) =>
            {
                return item.Equals(pathName);
            });
            if (mrIndex != -1)
            {
                //2.索引为最后一个节点，只返回前面一段路
                if (mrIndex == nodeList.Count - 1)
                {
                    if (Math.Abs(pathAngle - angleList[mrIndex - 1]) <= 30)
                    {
                        res[0] = mrIndex - 2;
                        res[1] = mrIndex - 1;
                    }
                }
                else
                {
                    if (Math.Abs(pathAngle - angleList[mrIndex]) <= 30)     //误差暂定成30
                    {
                        //3.索引为0，只返回后面一段路
                        if (mrIndex == 0)
                        {
                            res[0] = 0;
                            res[1] = 1;
                        }
                        //4.索引为(0,nodeList.Count-1),返回前后两段路
                        else
                        {
                            res[0] = mrIndex - 1;
                            res[1] = mrIndex + 1;
                        }
                    }
                }
            }
            return res;
        }
        /****************************************************
         * FromAndToDeal:解析从xxx到xxx
         * Param：
         *      pathDir:AMAP中一条主路描述
         *      nodeList:屏的主路节点列表
         * Result：
         *      路段在节点中的索引
         * **************************************************/
        List<int> FromAndToDeal(string pathDir, List<string> nodeList)
        {
            List<int> res = new List<int>() { 0, 0 };
            string substr = pathDir.Substring(1);
            string[] arrPath = substr.Split(new char[] { '到' }, 2);
            if (arrPath.Length != 2)
                return res;

            int pre = nodeList.FindIndex((item) =>
            {
                return item.Equals(arrPath[0]);
            });
            int back = nodeList.FindIndex((Item) =>
            {
                return Item.Equals(arrPath[1]);
            });
            if (pre != -1 && back != -1 && back > pre)              //代表有效数据
            {
                res[0] = pre;
                res[1] = back;
            }
            return res;
        }
        /* ********************************************************
         * RoadDirToIndex: 由方向判断nodelist中节点的做引
         * Param:
         *      pathDir:AMAP的json数据的某条路的方向信息
         *      nodeList:配置文件中某条路的节点列表
         * Result:
         *      MRoadArr数组的起止索引
         * *****************************************************/
        List<int> RoadDirToIndex(string pathDir, int pathAngle, List<string> nodeList, List<int> angleList)
        {
            List<int> res = null;
            int index1 = pathDir.IndexOf("从");
            int index2 = pathDir.IndexOf("到");
            int index3 = pathDir.IndexOf("附近");
            if (index1 != -1 && index2 != -1)    //解析从xxx到xxx
            {
                res = FromAndToDeal(pathDir, nodeList);
            }
            else if (index3 != -1)           //解析xxx附近
            {
                res = NearbyDeal(pathDir, pathAngle, nodeList, angleList);
            }
            else                           //新播报格式
            {
                ydpLog.WriteLineLog("另一种数据格式：" + pathDir);
            }
            return res;
        }
        /***************************************************************
         * WholeDescFullSteList:通过AmapJson中description填充状态数组
         * Param：
         *      wholeDesc:description
         *      screen   :配置文件中的整屏信息
         * **************************************************************/
        private void WholeDescFullSteList(string wholeDesc, ScreenResult screen)
        {
            if (wholeDesc == null)
                return;

            string[] descArr = wholeDesc.Split(';');
            foreach (string desc in descArr)
            {
                string[] pathd = desc.Split(':');
                int index = screen.Sroads.FindIndex((item) =>
                {
                    return item.MRoad.Equals(pathd[0]);
                });
                if (index == -1)
                    continue;

                if (pathd[1].IndexOf("畅通") != -1)
                {
                    for (int i = 0; i < screen.Sroads[index].SectList.Count; i++)
                        screen.Sroads[index].StateList[i] = 1;
                }
                else if (pathd[1].IndexOf("缓慢") != -1)
                {

                }
                else if (pathd[1].IndexOf("严重拥堵") != -1)
                {

                }
            }
        }
        /* ********************************************************
         * AmapJsonToSteArr:高德地图中获得的Json填充主路的状态数组，
         *                  根据id判断是哪个屏，根据json数据判断是哪条主路
         * Param:
         *      jsonRegionList:AMAP的json数据列表
         *      screen:屏幕配置文件的整屏json信息
         * *****************************************************/
        public void AmapJsonToSteArr(List<ydpAmapJson> jsonRegionList, ScreenResult screen)
        {
            if (jsonRegionList.Count == 0)
                return;

            int scIndex = scrResultList.FindIndex((item) =>         //此键值不再配置文件所列范围内
            {
                return item.Id.Equals(screen.Id);
            });

            if (scIndex == -1)
                return;

            foreach (var sigJson in jsonRegionList)
            {
                try                 //解决AMAPjson缺少数据造成的异常
                {
                    string wholeDesc = sigJson.trafficinfo.description;   //???整体描述不知道怎么处理
                    WholeDescFullSteList(wholeDesc, screen);              //通过description预填充数组

                    using (StreamWriter w = new StreamWriter(".\\description.txt", true))
                    {
                        w.WriteLine(wholeDesc);
                    }
                    foreach (var road in sigJson.trafficinfo.roads)
                    {
                        int mrIndex = screen.Sroads.FindIndex((item) =>
                        {
                            return item.MRoad.Equals(road.name);
                        });
                        if (mrIndex != -1)
                        {
                            string pathDir = road.direction;
                            Byte pathSta = Byte.Parse(road.status);
                            int pathAngle = int.Parse(road.angle);
                            int pathSpeed = 0;
                            try             //speed容易丢失 单独处理
                            {
                                pathSpeed = int.Parse(road.speed);
                            }
                            catch (Exception)
                            { }

                            List<int> nodes = RoadDirToIndex(pathDir, pathAngle, screen.Sroads[mrIndex].SectList, screen.Sroads[mrIndex].AngleList);
                            if (nodes.Count != 2 || (nodes[0] == 0 && nodes[1] == 0))
                                continue;

                            for (int i = nodes[0]; i < nodes[1]; i++)
                            {
                                screen.Sroads[mrIndex].StateList[i] = Math.Max(screen.Sroads[mrIndex].StateList[i], pathSta);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    stsbarMode.Text = "高德解析数据异常";
                    ydpLog.WriteLineLog(DateTime.Now.ToString() + "高德解析数据异常");
                }
            }
        }
        /*****************************************************************
         * IsValidJsonResult：判断高德请求的数据是否正确
         * Param:
         *      json：高德返回的json形式的数据
         * ***************************************************************/
        int IsValidJsonResult(ydpAmapJson json)
        {
            ydpLog.WriteLineLog(DateTime.Now.ToString() + " " + json.status + " " + json.info);
            if (json.status == "0")
            {
                if (json.info == "DAILY_QUERY_OVER_LIMIT")
                {
                    ydpLog.WriteLineLog("当前key值已经超限");
                    return -1;
                }
                else if (json.info == "INVALID_USER_KEY")
                {
                    ydpLog.WriteLineLog("AMAP请求参数错误");
                    return -2;
                }
                else
                    return -3;
            }

            return 0;
        }
        /*****************************************************************
         * AMAPReqAndFullMRoadsList：遍历屏体搜索区域，获得高德请求结果，
         *                           将结果存入到列表中，调用AmapJsonToSteArr
         *                           进行数据对应处理
         * Param:
         *      无
         * ***************************************************************/
        async void AMAPReqAndFullMRoadsList()
        {
            //1.获得屏体句柄
            ScreenResult scnHandle = scrResultList[AmapIndex];

            List<ydpAmapJson> ydpAJlist = new List<ydpAmapJson>();
            ydpAmapJson ydpApJson = null;

            //2.对某一屏规定的区域向高德发送请求
            foreach (string region in scnHandle.SfindRect)
            {
                int reSendCont = 0;

                //2.1 按区域异步发送请求，失败，连发三次
                do
                {
                    ydpApJson = await ydpApClient.GetJsonFromAmapAsync(region);
                    if (ydpApJson == null)              //保证第一次成功不延迟，失败延迟
                        await Task.Delay(10000);        //异步延迟
                } while (ydpApJson == null && reSendCont++ < 3);

                //2.2 对于返回的AmapJson数据到一个列表中，后面统一处理
                if (ydpApJson != null)
                {
                    if (0 == IsValidJsonResult(ydpApJson)) //返回结果是否正确
                        ydpAJlist.Add(ydpApJson);

                    ydpApJson = null;
                }
            }

            //3.统一处理一块屏的所有AmapJson数据
            AmapJsonToSteArr(ydpAJlist, scnHandle);
        }
        private void ydpTimer_Tick(object sender, EventArgs e)
        {
            //1.在状态栏显示时间
            string localtime = DateTime.Now.ToString(" yyyy-MM-dd HH:mm:ss");
            stsbarTime.Text = localtime;
            AMAPreqcnt++;

            //2.状态转移函数，根据接受和发送的数据，进行状态转移
            DoState(AmapIndex);

            //对于手动模式，到此不再向下运行
            //对于高德模式而言，定时时间到自动发送交通态势信息
            if (wk_mode == ydpModeEm.Amap)
            {
                //3.高德定时时间到60s，逐屏填充数据，填完发送
                if (AMAPreqcnt >= AMAPreqtime)
                {
                    AMAPreqcnt = 0;
                    AmapIndex = (AmapIndex + 1) % SCREEN_NUMS;
                    //4.高德请求，抓取一屏路况交通态势
                    AMAPReqAndFullMRoadsList();
                    //5.屏幕AMAPScCnt状态机转向高德模式抓取数据完成
                    scrResultList[AmapIndex].DataOk = ydpOkMachEm.Amap;
                    DynamicPixShow(AmapIndex);                   
                }
            }
        }

        private Dictionary<string,Byte> SectMapBand(int scnIndex)
        {
            ScreenResult screen = scrResultList[scnIndex];
            if (screen.Id == null)    //
                return null;

            Dictionary<string, Byte> dic_rid_sta = new Dictionary<string, byte>();  //dictionary记录路段和状态数组的对应关系，key是路段，value是路段对应的状态数组
                                                                                    //1.遍历屏体主路
            foreach (var mroad in screen.Sroads)
            {
                //2.遍历主路中的路段和状态数组，并建立映射
                for (int i = 0, j = 0; i < mroad.IdsList.ToArray().Length &&
                                j < mroad.StateList.ToArray().Length; i++, j++)
                {
                    dic_rid_sta.Add(mroad.IdsList[i], mroad.StateList[j]);
                }
            }

            return dic_rid_sta;
        }
        /*****************************************************************
         * DynamicPixShow：在屏体上动态改变路段的颜色
         * Param:
         *      scnIndex：在ScreenResult中的索引
         * ***************************************************************/
        private void DynamicPixShow(int scnIndex)
        {
            Dictionary<string,Byte> dic_rid_sta = SectMapBand(scnIndex);
            ScreenResult screen = scrResultList[scnIndex];
            int band_num = 0;               //屏体光带计数
                                            //3.遍历光带列表组
            foreach (var blst in screen.Band)
            {
                int mx_sta = 0;
                band_num++;
                //4.遍历每一条光带，并找到光带的最大状态数组的值
                foreach (var rds in blst)
                {
                    mx_sta = Math.Max(mx_sta, dic_rid_sta[rds]);
                }
                //5.改变屏体颜色
                if (screen.Id == "ydp001")
                    myChangeMenuColor_s1(mx_sta, band_num);
                else if (screen.Id == "ydp002")
                    myChangeMenuColor_s2(mx_sta, band_num);
                else if (screen.Id == "ydp003")
                    myChangeMenuColor_s3(mx_sta, band_num);
            }
        }
        /*****************************************************************
         * ydpSendData：发送失败连发三次
         * Param:
         *      con:发送数据的描述符
         *      arr:发送数组
         *      size:数组长度
         * ***************************************************************/
        private void ydpSendData(Connect con, Byte[] arr, int size)
        {
            int res = 0;
            int sdCnt = 0;
            do
            {
                res = sv.SendData(con, arr, size);
                sdCnt++;
            } while (res < 0 && sdCnt < 3);

            if (res > 0)
            {
                Program.gdFrom.UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsSndData, "已发送指令");
                ydpLog.WriteLineLog(DateTime.Now.ToString() + ":发送指令:" + ydpLog.ByteToRawStr(arr,0,size));
            }
        }
        /*********************************************************************
         * DoState:状态转移处理               
         * Param：
         *       scnIndex:屏体在ScnRestList中的索引
         * *******************************************************************/
        private void DoState(int scnIndex)
        {
            if (scnIndex < 0 || scnIndex > SCREEN_NUMS)
                return;

            int len = sv.connects.Length;
            ScreenResult screen = scrResultList[scnIndex];
            for (int i = 0; i < len; i++)
            {
                switch(sv.connects[i].conState)
                {
                    case ydpMachEm.Idle:
                        break;
                    case ydpMachEm.AcceptDone:
                        byte[] ckArr = AmapDataPack(1, 0);

                        ydpSendData(sv.connects[i], ckArr, ckArr.Length);
                        //a.状态转移2 AcceptDone->RecvId 
                        sv.connects[i].conState = ydpMachEm.RecvId;
                        sv.connects[i].delayCnt=0;
                        break;
                    case ydpMachEm.RecvId:
                    case ydpMachEm.RecvReply:
                        //超时处理
                        sv.connects[i].delayCnt++;
                        if (sv.connects[i].delayCnt == 10)
                        {
                            //错误提示????
                            //d.状态转移5 RecvId->TxReady 
                            sv.connects[i].conState = ydpMachEm.TxReady;
                            break;
                            //sv.connects[i].Close();
                        }
                        //收到一帧数据
                        if (sv.connects[i].flagFrame == true)
                        {
                            sv.connects[i].fsStart = -1;
                            sv.connects[i].flagFrame = false;
                            //sv.connects[i].delayCnt = 0;  
                            //3. 包分类解析
                            byte[] rBuff = sv.connects[i].frameBuff;  //????
                            /********************************************
                             * 对于查询设备号应答("91")指令：02 30 30 '9' '1' lenH lenL 0 / 1 id xx xx 03,应答结果存到devid中
                             * 对于发送显示数据应答("81")指令：02 30 30 '8' '1' lenH lenL 0 / 1 xx xx 03
                             * ****************************************/
                            //3.1 接受到39 31状态码，代表查询应答
                            if (rBuff[3] == 0x39 && rBuff[4] == 0x31)//?????
                            {
                                if (rBuff[7] == 0x30)
                                {
                                    //sv.connects[i].delayCnt = 0;    //接收到正确的应答就将等待计数标志位清0
                                    byte[] idbt = new byte[8];          //????改格式
                                    for (int j = 0; j < 8; j++)
                                    {
                                        idbt[j] = rBuff[j + 8];
                                    }
                                    sv.connects[i].devid = Encoding.Default.GetString(idbt).Replace("\0", "");
                                    //b.状态转移3 RecvId->DevAns 
                                    sv.connects[i].conState = ydpMachEm.TxReady;

                                    //如果设备不再设备列表中怎么办???
                                    TxBoxState(sv.connects[i], 1);
                                    //MessageBox.Show("[Dev:" + sv.connects[i].devid + "]" + " 已连接");
                                    Program.gdFrom.UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsDevStat, "[Dev:" + sv.connects[i].devid + "]" + "已连接");
                                    ydpLog.WriteLineLog(DateTime.Now.ToString() + ":设备" + sv.connects[i].devid+" 连接成功!");
                                }
                            }
                            //3.2. 接受到38 31状态码，代表发送状态数组应答
                            else if (rBuff[3] == 0x38 && rBuff[4] == 0x31)
                            {
                                //接受应答处理函数????
                                if(rBuff[7] == 0x30)
                                {
                                    //DynamicPixShow(AmapIndex);          //PC上动态显示图片
                                                                                     //c.状态转移4 RecvId->InfoAns 
                                    sv.connects[i].conState = ydpMachEm.TxReady;
                                }
                            }
                        }
                        break;
                    case ydpMachEm.TxReady:             //发送数据应答成功
                        
                        if (screen.Id==sv.connects[i].devid && screen.DataOk==ydpOkMachEm.Manual)
                        {
                            screen.DataOk = ydpOkMachEm.Unok;
                            ydpLog.WriteLineLog("处于手动模式屏幕" + screen.Id);

                            byte[] manualArr = ManualDataPack(scnIndex);
                            sv.connects[i].delayCnt = 0;
                            ydpSendData(sv.connects[i], manualArr, manualArr.Length);
                            sv.connects[i].conState = ydpMachEm.RecvReply;

                            ydpLog.WriteLineLog("用户点击的交通态势:"+ydpLog.ByteToRawStr(manualArr, 0, manualArr.Length));
                        }
                        else if (screen.Id == sv.connects[i].devid && screen.DataOk==ydpOkMachEm.Amap) 
                        {
                            //是要发送的一屏数据&&并且数据已经准备好
                            screen.DataOk = ydpOkMachEm.Unok;   //屏幕scnIndex的状态机转向数据未准备好状态

                            ydpLog.WriteLineLog("处于高德模式屏幕" + screen.Id);

                            byte[] amapArr = AmapDataPack(2, AmapIndex);
                            sv.connects[i].delayCnt = 0;
                            ydpSendData(sv.connects[i], amapArr, amapArr.Length);
                            sv.connects[i].conState = ydpMachEm.RecvReply;

                            foreach (var mroad in screen.Sroads)
                            {
                                string str = mroad.MRoad + "状态数组: ";
                                for (int j = 0; j < mroad.StateList.Count; j++)
                                {
                                    str += "0x" + mroad.StateList[j] + " ";
                                    mroad.StateList[j] = 0x01;    //先默认是畅通
                                }
                                ydpLog.WriteLineLog(str);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        /*********************************************************************
         * DataPacked:高德模式下数据发送数据之前的打包操作，借助ScreenResult类中的StateList
         * Param：
         *      flag:标志是查询指令打包还是发送显示数据指令打包
         *      screen:记录屏幕信息
         * Result:
         *       要发送的状态数组
         *       对于查询("91")指令：数据包：02 30 30 '9' '1' xx xx 03
         *       对于发送状态("81")指令：数据包：02 30 30 '8' '1' '{' id ':' xx} ...
         * *******************************************************************/
        private byte[] AmapDataPack(int flag, int scnIndex)
        {
            ScreenResult screen = scrResultList[scnIndex];
            List<byte> res = new List<byte>();
            res.Add(0x02);              //前三位固定 0x02 0x30 0x30
            res.Add(0x30);
            res.Add(0x30);

            int plLen = 0;              //payLoad数据长度
            if (flag == 1)              //打包查询设备号
            {
                res.Add(0x39);
                res.Add(0x31);
                res.Add(0x00);
                res.Add(0x00);
            }
            else if (flag == 2)          //打包显示数据
            {
                res.Add(0x38);
                res.Add(0x31);
                res.Add(0x00);
                res.Add(0x00);
                byte[] tmp1 = Encoding.Default.GetBytes("{");
                byte[] tmp2 = Encoding.Default.GetBytes(":");
                byte[] tmp3 = Encoding.Default.GetBytes("}");
                //1. 获得屏幕SectionList和Band映射值
                Dictionary<string,Byte> dic_rid_sta = SectMapBand(scnIndex);
                Byte bdIndex = 0;
                //2. 遍历Band列表
                foreach(var blst in screen.Band)
                {
                    //3.构造出格式{bdIndex:state}的形式，这是一节数据
                    res.Add(tmp1[0]);
                    res.Add(bdIndex);
                    res.Add(tmp2[0]);
                    int mx_sta = 0;
                    foreach(var rds in blst)
                    {
                        mx_sta = Math.Max(mx_sta, dic_rid_sta[rds]);
                    }
                    res.Add((Byte)mx_sta);
                    res.Add(tmp3[0]);
                    bdIndex++;
                }
            }
            res[5] = (byte)((plLen & 0xff00) >> 8);  //补充包有效载荷长度
            res[6] = (byte)((plLen & 0xff));
            res.Add(0x00);          //校验位
            res.Add(0x00);
            res.Add(0x03);          //包尾0x03
            return res.ToArray();
        }
        /******************************************************************************
         * ManualDataPack:根据屏幕上的颜色打包成要发送的数组帧格式,借助ScreenResult类中的ColorArr
         * Param:
         *      scn:屏体列表索引
         * Result:
         *      要发送数组的数据帧
         * ***************************************************************************/
        private byte[] ManualDataPack(int scnIndex)
        {
            ScreenResult screen = scrResultList[scnIndex];
            Dictionary<string, Byte> dic_rid_sta = new Dictionary<string, byte>();  //dictionary记录路段和状态数组的对应关系，key是路段，value是路段对应的状态数组

            int index = 0;
            List<Byte> sndList = new List<byte>();
            sndList.Add(0x02);
            sndList.Add(0x30);
            sndList.Add(0x30);
            sndList.Add(0x38);
            sndList.Add(0x31);
            sndList.Add(0x00);
            sndList.Add(0x00);
            byte[] tmp1 = Encoding.Default.GetBytes("{");
            byte[] tmp2 = Encoding.Default.GetBytes(":");
            byte[] tmp3 = Encoding.Default.GetBytes("}");
            int plLen = 0;

            foreach (var bdlst in screen.Band)
            {
                sndList.Add(tmp1[0]);
                plLen++;
                sndList.Add((Byte)index);
                sndList.Add(tmp2[0]);
                plLen++;
                sndList.Add(screen.ColorArr[index]);
                sndList.Add(tmp3[0]);
                plLen++;
                index++;
            }
            sndList[5] = (byte)((plLen & 0xff00) >> 8);
            sndList[6] = (byte)((plLen & 0xff));
            sndList.Add(0x00);
            sndList.Add(0x00);
            sndList.Add(0x03);
            return sndList.ToArray();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            byte[] arr = { 0x31, 0x32, 0x33, 0x34 };
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Byte[] arr = new byte[5]{ 49, 50, 51, 52, 53 };
            //MessageBox.Show(Encoding.Default.GetString(arr));
        }

        private void s3_pa1_MouseUp(object sender, MouseEventArgs e)
        {
            //s3_pa1.ContextMenuStrip = cMenu3_Color;
            s3_num = 1;
            manualPixShow(s3_pa1, 2);
        }

        private void s3_pa2_MouseUp(object sender, MouseEventArgs e)
        {
            //s3_pa2.ContextMenuStrip = cMenu3_Color;
            s3_num = 2;
            manualPixShow(s3_pa2, 2);
        }

        private void s3_pa3_MouseUp(object sender, MouseEventArgs e)
        {
            //s3_pa3.ContextMenuStrip = cMenu3_Color;
            s3_num = 3;
            manualPixShow(s3_pa3, 2);
        }

        private void s3_pb_MouseUp(object sender, MouseEventArgs e)
        {
            //s3_pb.ContextMenuStrip = cMenu3_Color;
            s3_num = 4;
            manualPixShow(s3_pb, 2);
        }

        private void s3_pd_MouseUp(object sender, MouseEventArgs e)
        {
            //s3_pd.ContextMenuStrip = cMenu3_Color;
            s3_num = 5;
            manualPixShow(s3_pd, 2);
        }

        private void s3_pc_MouseUp(object sender, MouseEventArgs e)
        {
            //s3_pc.ContextMenuStrip = cMenu3_Color;
            s3_num = 5;
            manualPixShow(s3_pc, 2);
        }

        private void Red3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s3(3,s3_num);
        }

        private void Yellow3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s3(2,s3_num);
        }

        private void Green3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s3(1, s3_num);
        }

        private void cBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBoxMode.Text == "高德模式")
            {
                AmapIndex = 0;              //保证在高德模式下每次都是从第一块屏发送
                wk_mode = ydpModeEm.Amap;
                s1_BtnSnd.Enabled = false;
                s2_BtnSnd.Enabled = false;
                s3_BtnSnd.Enabled = false;
                s1_BtnSnd.ForeColor = Color.Black;
                s2_BtnSnd.ForeColor = Color.Black;
                s2_BtnSnd.ForeColor = Color.Black;
                ydpLog.WriteLineLog("目前工作在高德模式!!!");
                UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsMode, "高德模式");
            }               
            else if (cBoxMode.Text == "人工模式")
            {
                wk_mode = ydpModeEm.Manual;
                s1_BtnSnd.Enabled = true;
                s2_BtnSnd.Enabled = true;
                s3_BtnSnd.Enabled = true;
                s1_BtnSnd.ForeColor = Color.LightSeaGreen;
                s2_BtnSnd.ForeColor = Color.LightSeaGreen;
                s2_BtnSnd.ForeColor = Color.LightSeaGreen;
                ydpLog.WriteLineLog("目前工作在手动模式!!!");
                UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsMode, "人工模式");
            }
        }

        private void s1_BtnSnd_Click(object sender, EventArgs e)
        {
            AmapIndex = 0;
            byte[] sndArr = ManualDataPack(0);
            scrResultList[0].DataOk = ydpOkMachEm.Manual;//屏幕1状态机转向手动模式抓取完成
        }

        private void s2_BtnSnd_Click(object sender, EventArgs e)
        {
            AmapIndex = 1;
            byte[] sndArr = ManualDataPack(1);
            scrResultList[1].DataOk = ydpOkMachEm.Manual;//屏幕2状态机转向手动模式抓取完成
        }

        private void s3_BtnSnd_Click(object sender, EventArgs e)
        {
            AmapIndex = 2;
            byte[] sndArr = ManualDataPack(2);
            scrResultList[2].DataOk = ydpOkMachEm.Manual;//屏幕3状态机转向手动模式抓取完成
        }
    }
}
