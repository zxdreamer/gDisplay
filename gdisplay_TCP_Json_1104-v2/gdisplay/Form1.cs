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
        Byte s2_num = 0;                     //屏体区域编号

        List<ScreenResult> ScnRestList=new List<ScreenResult>();
                                           
        TcpServer sv=null;
        ydpJsonConfig ydpCfg = new ydpJsonConfig();               //json配置文件对象
        //AmapClient ydpApClient = AmapClient.CreateApClientObj("51a0f16ac37bcf88e634023f1529d84a");          //获得高德数据对象
        AmapClient ydpApClient = null;
        int AMAPreqcnt = 0;
        int AMAPreqtime = 0;
        public Form1()
        {
            InitializeComponent();
            userUIInit();       //窗体控件初始化
            userJsonInit();     //Json配置文件解析
            userTcpInit();      //TCPserver初始化，并启动监听

            WriteLog(DateTime.Now.ToString() + "---------------------------程序开始启动----------------------------------");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void userUIInit()
        {
            //1.开启定时器
            tmDate.Start();
            //2.设置comBox工作模式
            cBoxMode.Items.Add("人工模式");
            cBoxMode.Items.Add("高德模式");
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
            this.stsbarArr = new ToolStripStatusLabel[4] { stsbarComPort, stsbarCMD, stsbarAMAP, stsbarTime };
            this.devBoxArr = new TextBox[4] { s1_devNameBox, s1_devStateBox, s2_devNameBox, s2_devStateBox };
            
        } //async void userUIInit()

        async void userJsonInit()
        {
            try
            {
                using (StreamReader rd = new StreamReader("E:\\ytr3.json"))
                {
                    var jsonstr = await rd.ReadToEndAsync();
                    await Task.Run(() =>
                    {
                        ydpCfg = JsonConvert.DeserializeObject<ydpJsonConfig>(jsonstr);

                        WriteLog("开始解析配置文件......");
                        ydpApClient = AmapClient.CreateApClientObj(ydpCfg.AMAPkey);     //获得高德数据对象
                        AMAPreqtime = ydpCfg.AMAPreqtime;
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
                            foreach (CfgRoads roads in screens.roads)
                            {
                                string mainRoad = roads.name;

                                List<string> nodeList = new List<string>();
                                foreach (string road in roads.sections)
                                    nodeList.Add(road);

                                List<int> angList = new List<int>();
                                foreach (int anl in roads.angles)
                                    angList.Add(anl);

                                RoadsResult path = new RoadsResult(mainRoad, nodeList, angList);
                                sigPaths.Add(path);
                            }
                            //5.依次将每一块屏的配置信息添加进去
                            ScnRestList.Add(new ScreenResult(sigPaths, reL, sid));
                        }
                    });
                    WriteLog("配置文件解析结果:");
                    foreach(var sc in ScnRestList)
                    {
                        WriteLog(sc.Id);
                        foreach(var path in sc.Sroads)
                        {
                            WriteLog(path.MRoad);
                            foreach(var node in path.SectList)
                            {
                                WriteLog(node);
                            }
                        }
                        string region = "";
                        foreach(var rect in sc.SfindRect)
                        {
                            region += rect + " ";
                        }
                        WriteLog(region);
                    }
                }
            }
            catch(FileNotFoundException e)
            {
                MessageBox.Show("配置文件不存在");
            }
            catch(Exception e)
            {
                WriteLog("配置文件解析异常" + e.Message);
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
        public static int WriteLog(string strlog)
        {
            try
            {
                using (StreamWriter wrLog = new StreamWriter(@".\\logFile.txt",true))
                {
                    wrLog.WriteLine(strlog);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("文件系统异常" + e.Message);
                return -1;
            }

            return 0;
        }
        /************************************************************
        //UpdateState：有TcpServer更新UI控件数据
        //Para:
        //     msgType: 指定显示容器
        //              0:在状态栏显示
        //              1:在tabControl显示
        //     msgData: 在msgData指定容器中的显示位置
        //              0,1,2...n:依次从左到右，从上到下的控件
        //     text   ：显示文本
        **************************************************************/
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
                    s1_picBoxArr[s1_num - 1].Image = global::gdisplay.Properties.Resources.pa_red1;
                else if (color == 2)
                    s1_picBoxArr[s1_num - 1].Image = global::gdisplay.Properties.Resources.pa_yellow1;
                else if (color == 3)
                    s1_picBoxArr[s1_num - 1].Image = global::gdisplay.Properties.Resources.pa_green1;
            }
            else if(s1_num==4 || s1_num==5)
            {
                if (color == 1)
                    s1_picBoxArr[s1_num - 1].Image = global::gdisplay.Properties.Resources.pbc_red1;
                else if (color == 2)
                    s1_picBoxArr[s1_num - 1].Image = global::gdisplay.Properties.Resources.pbc_yellow1;
                else if (color == 3)
                    s1_picBoxArr[s1_num - 1].Image = global::gdisplay.Properties.Resources.pbc_green1;
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
                    s2_picBoxArr[s2_num-1].Image = global::gdisplay.Properties.Resources.lpa_red2;
                else if(color == 2)
                    s2_picBoxArr[s2_num-1].Image = global::gdisplay.Properties.Resources.lpa_yellow2;
                else if(color==3)
                    s2_picBoxArr[s2_num-1].Image = global::gdisplay.Properties.Resources.lpa_green2;
            }
            else if(s2_num==5 || s2_num==6)
            {
                if (color == 1)
                    s2_picBoxArr[s2_num-1].Image = global::gdisplay.Properties.Resources.rpd_red2;
                else if (color == 2)
                    s2_picBoxArr[s2_num-1].Image = global::gdisplay.Properties.Resources.rpd_yellow2;
                else if (color == 3)
                    s2_picBoxArr[s2_num-1].Image = global::gdisplay.Properties.Resources.rpd_green2;
            }
            else if(s2_num==7)
            {
                if (color == 1)
                {
                    s2_picBoxArr[s2_num - 1].Image = global::gdisplay.Properties.Resources.lpd_red2;
                    s2_picBoxArr[s2_num].Image = global::gdisplay.Properties.Resources.pe_red2;
                }
                else if (color == 2)
                {
                    s2_picBoxArr[s2_num - 1].Image = global::gdisplay.Properties.Resources.lpd_yellow2;
                    s2_picBoxArr[s2_num].Image = global::gdisplay.Properties.Resources.pe_yellow2;
                }                    
                else if (color == 3)
                {
                    s2_picBoxArr[s2_num - 1].Image = global::gdisplay.Properties.Resources.lpd_green2;
                    s2_picBoxArr[s2_num].Image = global::gdisplay.Properties.Resources.pe_green2;
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
            if(tabCtlSelect.SelectedTab.Name=="tabPage2")
            {
                //选中1号设备
                //MessageBox.Show("选中1号设备");
            }
            else if(tabCtlSelect.SelectedTab.Name=="tabPage3")
            {
                //选中2号设备
                //MessageBox.Show("选中2号设备");
            }
            else if(tabCtlSelect.SelectedTab.Name == "tabPage4")
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
                    try
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
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString() + mrIndex + pathDir + pathAngle);
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
        List<int> FromAndToDeal(string pathDir,List<string> nodeList)
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
                res[0]=pre;
                res[1]=back;
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
        List<int> RoadDirToIndex(string pathDir,int pathAngle,List<string> nodeList,List<int> angleList)
        {
            List<int> res=null;
            int index1 = pathDir.IndexOf("从");
            int index2 = pathDir.IndexOf("到");
            int index3 = pathDir.IndexOf("附近");
            if(index1!=-1 && index2!=-1)    //解析从xxx到xxx
            {
                res = FromAndToDeal(pathDir, nodeList);
            }
            else if(index3 != -1)           //解析xxx附近
            {
                res = NearbyDeal(pathDir, pathAngle,nodeList, angleList);
            }
            else                           //新播报格式
            {
                WriteLog("另一种数据格式：" + pathDir);
            }
            return res;
        }
        /* ********************************************************
         * AmapJsonToSteArr:高德地图中获得的Json填充主路的状态数组，
         *                  根据id判断是哪个屏，根据json数据判断是哪条主路
         * Param:
         *      jap:AMAP的json数据
         *      id:屏幕键值
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

            foreach(var sigJson in jsonRegionList)
            {
                try                 //解决AMAPjson缺少数据造成的异常
                {
                    string wholeDesc = sigJson.trafficinfo.description;   //???整体描述不知道怎么处理
                    using (StreamWriter w = new StreamWriter(".\\description.txt",true))
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
                catch(Exception e)
                {                    
                    stsbarAMAP.Text = "高德解析数据异常";
                    WriteLog(DateTime.Now.ToString() + "高德解析数据异常");
                }
            }
        }
        int IsValidJsonResult(ydpAmapJson json)
        {
            if (json.status == "0")
            {
                if (json.info == "DAILY_QUERY_OVER_LIMIT")
                {
                    //wr.WriteLine("当前key值已经超限");
                    WriteLog("当前key值已经超限");
                    return -1;
                }
                else if (json.info == "INVALID_USER_KEY")
                {
                    //wr.WriteLine("AMAP请求参数错误");
                    WriteLog("AMAP请求参数错误");
                    return -2;
                }
                else
                    return -3;
            }

            return 0;
        }

        async void AMAPReqAndFullMRoadsList()
        {
            //1.对屏规定的区域向高德发送请求
            foreach(var screen in ScnRestList)
            {
                List<ydpAmapJson> scAmapJson = new List<ydpAmapJson>();
                ydpAmapJson ydpApJson=null;
                foreach (string region in screen.SfindRect)
                {
                    int reSendCont = 0;
                    //2 按区域异步发送请求，失败，连发三次
                    do
                    {
                        ydpApJson = await ydpApClient.GetJsonFromAmapAsync(region);
                        if (ydpApJson == null)              //保证第一次成功不延迟，失败延迟
                            await Task.Delay(20000);        //异步延迟
                    } while (ydpApJson == null && reSendCont++ < 3);
                    //3.对于返回的AmapJson数据到一个列表中，后面统一处理
                    if (ydpApJson != null)
                    {
                        if(0 == IsValidJsonResult(ydpApJson))
                            scAmapJson.Add(ydpApJson);

                        ydpApJson = null;
                    }
                }

                //4.统一处理一块屏的所有AmapJson数据
                AmapJsonToSteArr(scAmapJson, screen);

                //5.通过写入文件模拟发送过程
                WriteLog("屏幕" + screen.Id);
                foreach (var mroad in screen.Sroads)
                {
                    //wrLog.Write(mroad.MRoad + "状态数组: ");
                    string str = mroad.MRoad + "状态数组: ";
                    for (int i = 0; i < mroad.StateList.Count; i++)
                    {
                        //wrLog.Write("0x" + mroad.StateList[i] + " ");
                        str += "0x" + mroad.StateList[i] + " ";
                        mroad.StateList[i] = 0;    //先默认是无效数据
                    }
                    WriteLog(str);
                }
            }
        }
        private void tmDate_Tick(object sender, EventArgs e)
        {
            //1.在状态栏显示时间
            string localtime = DateTime.Now.ToString(" yyyy-MM-dd HH:mm:ss");
            stsbarTime.Text = localtime;
            AMAPreqcnt++;
            if(AMAPreqcnt >= AMAPreqtime)
            {
                AMAPreqcnt = 0;
                AMAPReqAndFullMRoadsList();   //高德请求，抓取路况交通态势
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (StreamReader rd = new StreamReader("bvajlhbv.txt"))
            {
                string s = rd.ReadLine();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Byte[] arr = new byte[5]{ 49, 50, 51, 52, 53 };
            MessageBox.Show(Encoding.Default.GetString(arr));
        }
    }
}
