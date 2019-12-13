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
        Byte s1_num = 0;                     //1,2,3,4,5,0(未选中)
        Byte s2_num = 0;                     //屏体2区域编号
        Byte s3_num = 0;                     //屏体3区域编号
        Byte wk_mode = (byte)ydpModeEm.Amap;                    //默认为高德模式
        List<ScreenResult> ScnRestList = new List<ScreenResult>();

        TcpServer sv = null;
        ydpJsonConfig ydpCfg = new ydpJsonConfig();               //json配置文件对象
        //AmapClient ydpApClient = AmapClient.CreateApClientObj("51a0f16ac37bcf88e634023f1529d84a");          //获得高德数据对象
        AmapClient ydpApClient = null;
        int AMAPreqcnt = 0;         //50ms计数器
        int AMAPreqtime = 0;        //配置文件中高德请求时间
        bool[] AMAPReadly = new bool[3] { false, false, false };      //屏幕1,2,3高德数据处理完成标志位
        int AMAPScCnt = 0;          //处理哪块儿屏幕计数
        public Form1()
        {
            InitializeComponent();
            userUIInit();       //窗体控件初始化
            userJsonInit();     //Json配置文件解析
            userTcpInit();      //TCPserver初始化，并启动监听

            WriteLineLog(DateTime.Now.ToString() + "---------------------------程序开始启动----------------------------------");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void userUIInit()
        {
            //1.开启定时器
            tmDate.Start();
            //2.设置comBox工作模式
            cBoxMode.Items.Add("高德模式");
            cBoxMode.Items.Add("人工模式");       
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
            this.stsbarArr = new ToolStripStatusLabel[4] { stsbarComPort, stsbarCMD, stsbarAMAP, stsbarTime };
            this.devBoxArr = new TextBox[6] { s1_devNameBox, s1_devStateBox, s2_devNameBox, s2_devStateBox, s3_devNameBox, s3_devStateBox };

        } //async void userUIInit()

        async void userJsonInit()
        {
            try
            {
                using (StreamReader rd = new StreamReader(@".\\ytr3.json"))
                {
                    var jsonstr = await rd.ReadToEndAsync();
                    //await Task.Run(() =>
                    //{
                    ydpCfg = JsonConvert.DeserializeObject<ydpJsonConfig>(jsonstr);

                    WriteLineLog("开始解析配置文件......");
                    ydpApClient = AmapClient.CreateApClientObj(ydpCfg.AMAPkey);     //获得高德数据对象
                    AMAPreqtime = ydpCfg.AMAPreqtime * 2;    //定时器定时500ms，这里乘2
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
                        ScnRestList.Add(new ScreenResult(sigPaths, reL, bd,sid));
                    }
                    //});
                    WriteLineLog("配置文件解析结果:");
                    foreach (var sc in ScnRestList)
                    {
                        WriteLineLog("屏幕 " + sc.Id);

                        string region = "搜索区域：";
                        foreach (var rect in sc.SfindRect)
                        {
                            region += rect + " ";
                        }
                        WriteLineLog(region);

                        foreach (var path in sc.Sroads)
                        {
                            WriteLineLog(path.MRoad);
                            string nodes = "节点:";
                            foreach (var node in path.SectList)
                            {
                                nodes += node + " ";
                            }
                            WriteLineLog(nodes);
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show("配置文件不存在");
            }
            catch (Exception e)
            {
                WriteLineLog("配置文件解析异常" + e.Message);
            }
        }

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
        /*************************************************
         * WriteLineLog:日志文件写函数
         * Param：
         *      strlog：一条写入数据
         * ************************************************/
        public static int WriteLineLog(string strlog)
        {
            try
            {
                using (StreamWriter wrLog = new StreamWriter(@".\\logFile.txt", true))
                {
                    wrLog.WriteLine(strlog);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("文件系统异常" + e.Message);
                return -1;
            }

            return 0;
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
        /**********************************************
        * myRClickMenuColor_s1:屏1右键显示颜色
        * Para:
        *      color:颜色编号
        ********************************************/
        void myChangeMenuColor_s1(int color,int snum)
        {
            WriteLineLog("右击屏1：" + "区域" + snum + " " + "颜色" + color);
            //填充s1_sdarr[50]数组
            if (snum < 1 || snum > 8)
            {
                MessageBox.Show("请点击正确区域");
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
//            snum = 0;
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
                MessageBox.Show("请点击正确区域");
                return;
            }
            else if (snum >= 1 && snum <= 4)   //snum=1..4：代表同一种类型的图片
            {
                if (color == 1)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpa_red2;
                else if (color == 2)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpa_yellow2;
                else if (color == 3)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpa_green2;
            }
            else if (snum == 5 || snum == 6)
            {
                if (color == 1)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.rpd_red2;
                else if (color == 2)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.rpd_yellow2;
                else if (color == 3)
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.rpd_green2;
            }
            else if (snum == 7)
            {
                if (color == 1)
                {
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpd_red2;
                    s2_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.pe_red2;
                }
                else if (color == 2)
                {
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpd_yellow2;
                    s2_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.pe_yellow2;
                }
                else if (color == 3)
                {
                    s2_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpd_green2;
                    s2_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.pe_green2;
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
            //s2_num = 0;
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
                MessageBox.Show("请点击正确区域");
                return;
            }
            else if (snum >= 1 && snum <= 3)   //s2_num=1..3：代表同一种类型的图片
            {
                if (color == 1)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_red3;
                else if (color == 2)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_yellow3;
                else if (color == 3)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.pa_green3;
            }
            else if (snum == 4)
            {
                if (color == 1)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpb_red3;
                else if (color == 2)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpb_yellow3;
                else if (color == 3)
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpb_green3;
            }
            else if (snum == 5)
            {
                if (color == 1)
                {
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpc_red3;
                    s3_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.lpd_red3;
                }
                else if (color == 2)
                {
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpc_yellow3;
                    s3_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.lpd_yellow3;
                }
                else if (color == 3)
                {
                    s3_picBoxArr[snum - 1].Image = global::gdisplay.Properties.Resources.lpc_green3;
                    s3_picBoxArr[snum].Image = global::gdisplay.Properties.Resources.lpd_green3;
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
            //s3_num = 0;
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
            s2_num = 7;
        }

        private void s2_pd_MouseUp(object sender, MouseEventArgs e)
        {
            s2_pd.ContextMenuStrip = cMenu2_Color;
            s2_num = 7;
        }

        private void Red2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s2(1,s2_num);
        }

        private void Yellow2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s2(2, s2_num);
        }

        private void Green2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s2(3, s2_num);
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
            for (int i = 0; i < sv.connects.Length; i++)
            {
                if (!sv.connects[i].isUse)
                    continue;

                int res = sv.SendData(sv.connects[i], CMD, CMD.Length);
                if (res > 0)
                {
                    Array.Copy(CMD, sArr, res);
                    UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsSndData, "DATA: To [DEV1] " + System.Text.Encoding.ASCII.GetString(sArr));
                }
                if (res == -1)
                {
                    MessageBox.Show("Dev:" + sv.connects[i].devid + "已断开");
                    UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsSndData, "DATA: To [DEV1] " + "NO Data");
                }
                else if (res == -2)
                {
                    MessageBox.Show("Dev:" + sv.connects[i].devid + "发送失败");
                    UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsSndData, "DATA: To [DEV1] " + "NO Data");
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
                WriteLineLog("另一种数据格式：" + pathDir);
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

            int scIndex = ScnRestList.FindIndex((item) =>         //此键值不再配置文件所列范围内
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
                    stsbarAMAP.Text = "高德解析数据异常";
                    WriteLineLog(DateTime.Now.ToString() + "高德解析数据异常");
                }
            }
        }
        int IsValidJsonResult(ydpAmapJson json)
        {
            WriteLineLog(DateTime.Now.ToString() + " " + json.status + " " + json.info);
            if (json.status == "0")
            {
                if (json.info == "DAILY_QUERY_OVER_LIMIT")
                {
                    WriteLineLog("当前key值已经超限");
                    return -1;
                }
                else if (json.info == "INVALID_USER_KEY")
                {
                    WriteLineLog("AMAP请求参数错误");
                    return -2;
                }
                else
                    return -3;
            }

            return 0;
        }

        async void AMAPReqAndFullMRoadsList()
        {
            //1.遍历屏体列表
            //foreach (var screen in ScnRestList)
            ScreenResult screen = ScnRestList[AMAPScCnt];
            //            {
            List<ydpAmapJson> scAmapJson = new List<ydpAmapJson>();
            ydpAmapJson ydpApJson = null;

            //2.对某一屏规定的区域向高德发送请求
            foreach (string region in screen.SfindRect)
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
                        scAmapJson.Add(ydpApJson);

                    ydpApJson = null;
                }
            }

            //3.统一处理一块屏的所有AmapJson数据
            AmapJsonToSteArr(scAmapJson, screen);
            //            }
        }

        //private void DynamicPixShow(ScreenResult screen)
        //{
        //    if (screen.Id == null)    //
        //        return;
        //    switch(screen.Id)
        //    {
        //        case "ydp001":
        //            Dictionary<string, Byte> dic_ids_sta = new Dictionary<string, byte>();  //dictionary记录路段和状态数组的对应关系，key是路段，value是路段对应的状态数组
        //            //1.遍历屏体主路
        //            foreach (var mroad in screen.Sroads)
        //            {
        //                //2.遍历主路中的路段和状态数组，并建立映射
        //                for(int i=0,j=0;i<mroad.IdsList.ToArray().Length&&
        //                                j<mroad.StateList.ToArray().Length; i++,j++)
        //                {
        //                    dic_ids_sta.Add(mroad.IdsList[i], mroad.StateList[j]);
        //                }
        //            }
        //            int band_num = 0;               //屏体光带计数
        //            //3.遍历光带列表组
        //            foreach(var blst in screen.Band)
        //            {
        //                int mx_sta = 0;
        //                band_num++;
        //                //4.遍历每一条光带，并找到光带的最大状态数组的值
        //                foreach(var rds in blst)
        //                {
        //                    mx_sta = Math.Max(mx_sta, dic_ids_sta[rds]);
        //                }
        //                //5.改变屏体颜色
        //                myChangeMenuColor_s1(mx_sta, band_num);
        //            }
        //            break;
        //        case "ydp002":
        //            break;
        //        case "ydp003":
        //            break;
        //        default:
        //            break;
        //    }
        //}
        private void DynamicPixShow(ScreenResult screen)
        {
            if (screen.Id == null)    //
                return;
            Dictionary<string, Byte> dic_ids_sta = new Dictionary<string, byte>();  //dictionary记录路段和状态数组的对应关系，key是路段，value是路段对应的状态数组
                                                                                    //1.遍历屏体主路
            foreach (var mroad in screen.Sroads)
            {
                //2.遍历主路中的路段和状态数组，并建立映射
                for (int i = 0, j = 0; i < mroad.IdsList.ToArray().Length &&
                                j < mroad.StateList.ToArray().Length; i++, j++)
                {
                    dic_ids_sta.Add(mroad.IdsList[i], mroad.StateList[j]);
                }
            }
            int band_num = 0;               //屏体光带计数
                                            //3.遍历光带列表组
            foreach (var blst in screen.Band)
            {
                int mx_sta = 0;
                band_num++;
                //4.遍历每一条光带，并找到光带的最大状态数组的值
                foreach (var rds in blst)
                {
                    mx_sta = Math.Max(mx_sta, dic_ids_sta[rds]);
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
        private void tmDate_Tick(object sender, EventArgs e)
        {
            //1.在状态栏显示时间
            string localtime = DateTime.Now.ToString(" yyyy-MM-dd HH:mm:ss");
            stsbarTime.Text = localtime;
            AMAPreqcnt++;
            //1.高德定时时间到60s，逐屏填充数据，填完发送
            if (AMAPreqcnt >= AMAPreqtime)
            {
                AMAPreqcnt = 0;
                AMAPReqAndFullMRoadsList();              //高德请求，抓取一屏路况交通态势                
                AMAPReadly[AMAPScCnt] = true;            //屏幕AMAPScCnt抓取完成标志位置true

                ydpBroadCastSend(ScnRestList[AMAPScCnt]);//发送一屏路况交通态势
                
                DynamicPixShow(ScnRestList[AMAPScCnt]);  //PC上动态显示图片

                AMAPScCnt = (AMAPScCnt + 1) % 3;
            }

            //2.逐屏广播模式发送数据
            ydpBroadCastSend(null);
            ydpLoopRecv(ScnRestList[AMAPScCnt]);                //???怎样保证等待500ms
        }

        /*********************************************************************
         * ytrBroadCastSend:广播形式发送，遍历connects中的每个描述符，依次处理               
         * Param：
         *       无
         * *******************************************************************/
        private void ydpBroadCastSend(ScreenResult screen)
        {
            int len = sv.connects.Length;
            for (int i = 0; i < len; i++)
            {
                //1 对于没有连接的描述符，直接跳过
                if (!sv.connects[i].isUse)
                    continue;

                //2 对于刚连接等待询问设备号的描述符，发送查询指令
                if ( screen==null && sv.connects[i].bAskId)
                {
                    sv.connects[i].bAskId = false;
                    byte[] ckArr = DataPacked(1, null);

                    ydpSendData(sv.connects[i], ckArr, ckArr.Length);
                }

                //3 对于已经准备好一屏数据的描述符，发送屏幕显示的数据
                //AMAPScCnt 记录哪一屏数据准备好
                if (sv.connects[i].devid == null)         //设备连接成功但是未绑定
                    continue;
                //if (screen != null && screen.Id == sv.connects[i].devid.Replace("\0", ""))
                if (screen != null && screen.Id == sv.connects[i].devid)
                {
                    AMAPReadly[AMAPScCnt] = false;
                    WriteLineLog("屏幕" + screen.Id);
                    //await Task.   Delay(10);        //???防止连续发送   

                    byte[] showArr = DataPacked(2, screen);
                    ydpSendData(sv.connects[i], showArr, showArr.Length);

                    foreach (var mroad in screen.Sroads)
                    {
                        string str = mroad.MRoad + "状态数组: ";
                        for (int j = 0; j < mroad.StateList.Count; j++)
                        {
                            str += "0x" + mroad.StateList[j] + " ";
                            mroad.StateList[j] = 0x01;    //先默认是无效数据
                        }
                        WriteLineLog(str);
                    }
                }
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
                Program.gdFrom.UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsSndData, "发送指令:" + ByteToRawStr(arr));
                Form1.WriteLineLog(DateTime.Now.ToString() + ":发送指令:" + ByteToRawStr(arr));
            }
        }
        /*****************************************************************
         * ytrLoopRecv:接受应答，遍历connects中的每个描述符，依次处理
         *             对于查询应答("91")指令：02 30 30 '9' '1' 0/1 id xx xx 03,接应答结果存到devid中
         *             对于发送显示数据应答("81")指令：02 30 30 '8' '1' 0/1 xx xx 03
         * Param：
         *       无
         * ****************************************************************/
        private void ydpLoopRecv(ScreenResult screen)
        {
            int len = sv.connects.Length;
            for (int i = 0; i < len; i++)
            {
                //1 此描述符接受到数据
                if (sv.connects[i].bRecvData)
                {
                    sv.connects[i].bRecvData = false;
                    byte[] rBuff = sv.connects[i].readBuff;
                    //2 接受到39 31状态码，代表查询应答
                    if (rBuff[3] == 0x39 && rBuff[4] == 0x31)
                    {
                        if (rBuff[5] == 0x31)
                        {
                            byte[] idbt = new byte[7];
                            //3 记录屏幕id
                            for (int j = 0; j < 6; j++)
                            {
                                idbt[j] = rBuff[j + 6];
                            }
                            sv.connects[i].devid = Encoding.Default.GetString(idbt).Replace("\0","");
                            screen.IsAns = true;
                            MessageBox.Show("[Dev:" + sv.connects[i].devid + "]" + " 已连接");
                            Program.gdFrom.UpdateState(ydpPlaceEm.StsBar, ydpShowEm.StsDevStat, "[Dev:" + sv.connects[i].devid + "]" + "已连接");
                            Form1.WriteLineLog(DateTime.Now.ToString() + ":应答的设备号" + sv.connects[i].devid);
                        }
                    }
                    else if (rBuff[3] == 0x38 && rBuff[4] == 0x31)
                    {
                        //接受应答处理函数????
                    }
                }
            }
        }
        /*********************************************************************
         * DataPacked:广播形式发送，遍历connects中的每个描述符，依次处理
         * Param：
         *      flag:标志是查询指令打包还是发送显示数据指令打包
         *      screen:记录屏幕信息
         * Result:
         *       要发送的状态数组
         *       对于查询("91")指令：数据包：02 30 30 '9' '1' xx xx 03
         *       对于发送状态("81")指令：数据包：02 30 30 '8' '1' '{' id ':' xx} ...
         * *******************************************************************/
        private byte[] DataPacked(int flag, ScreenResult screen)
        {
            List<byte> res = new List<byte>();
            res.Add(0x02);              //前三位固定 0x02 0x30 0x30
            res.Add(0x30);
            res.Add(0x30);

            if (flag == 1)              //打包查询设备号
            {
                res.Add(0x39);
                res.Add(0x31);
            }
            else if (flag == 2)          //打包显示数据
            {
                res.Add(0x38);
                res.Add(0x31);
                byte[] tmp1 = Encoding.Default.GetBytes("{");
                byte[] tmp2 = Encoding.Default.GetBytes(":");
                byte[] tmp3 = Encoding.Default.GetBytes("}");
                //1. 获得屏幕的一条路
                foreach (var road in screen.Sroads)
                {
                    //2.获得屏幕上路的路段编号和路段状态
                    for (int i = 0; i < road.IdsList.Count; i++)
                    {
                        //3.填充"{id:x}"
                        res.Add(tmp1[0]);
                        byte[] idbt = Encoding.Default.GetBytes(road.IdsList[i]);
                        foreach (var id in idbt)
                        {
                            res.Add(id);
                        }
                        res.Add(tmp2[0]);
                        res.Add(road.StateList[i]);
                        res.Add(tmp3[0]);
                    }
                }
            }

            res.Add(0x03);          //包尾0x03
            return res.ToArray();
        }
        /**********************************************************
         * ByteToRawStr:将byte[]数组直译成字符串，便于日志文件，状态栏记录
         * Param:
         *      arr:需要转换的数组
         * Result:
         *      数组直译后的字符串
         * */
        public string ByteToRawStr(byte[] arr)
        {
            string res = null;
            string[] state = new string[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            for (int i = 0; i < arr.Length; i++)
            {
                string shi = state[(arr[i] & 0xf0) >> 4];
                string ge = state[arr[i] & 0x0f];
                res += (shi + ge + " ");
            }

            return res;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S1DevName, "DATA: To [DEV1] " + "NO Data");
            UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S2DevName, "DATA: To [DEV1] " + "NO Data");
            UpdateState(ydpPlaceEm.TxtBox, ydpShowEm.S3DevName, "DATA: To [DEV1] " + "NO Data");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Byte[] arr = new byte[5]{ 49, 50, 51, 52, 53 };
            //MessageBox.Show(Encoding.Default.GetString(arr));
        }

        private void s3_pa1_MouseUp(object sender, MouseEventArgs e)
        {
            s3_pa1.ContextMenuStrip = cMenu3_Color;
            s3_num = 1;
        }

        private void s3_pa2_MouseUp(object sender, MouseEventArgs e)
        {
            s3_pa2.ContextMenuStrip = cMenu3_Color;
            s3_num = 2;
        }

        private void s3_pa3_MouseUp(object sender, MouseEventArgs e)
        {
            s3_pa3.ContextMenuStrip = cMenu3_Color;
            s3_num = 3;
        }

        private void s3_pb_MouseUp(object sender, MouseEventArgs e)
        {
            s3_pb.ContextMenuStrip = cMenu3_Color;
            s3_num = 4;
        }

        private void s3_pd_MouseUp(object sender, MouseEventArgs e)
        {
            s3_pd.ContextMenuStrip = cMenu3_Color;
            s3_num = 5;
        }

        private void s3_pc_MouseUp(object sender, MouseEventArgs e)
        {
            s3_pc.ContextMenuStrip = cMenu3_Color;
            s3_num = 5;
        }

        private void Red3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s3(1,s3_num);
        }

        private void Yellow3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s3(2,s3_num);
        }

        private void Green3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myChangeMenuColor_s3(3, s3_num);
        }

        private void cBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBoxMode.Text == "高德模式")
                wk_mode = (byte)ydpModeEm.Amap;
            else if (cBoxMode.Text == "人工模式")
                wk_mode = (byte)ydpModeEm.manual;

            //让发送按钮变灰??????
        }
    }
}
