using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gdisplay
{
    public partial class Form1 : Form
    {
        Byte s1_num = 0;               //1,2,3,4,5,0(未选中)
        Byte[] s1_sdarr = new byte[50];//向屏幕1发送数据的缓存
        Byte s2_num = 0;
        Byte[] s2_sdarr = new byte[50];
        TcpServer sv=null;
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

        void userUIInit()
        {
            cBox.Items.Add("人工模式");
            cBox.Items.Add("高德模式");
            cBox.SelectedIndex = 0;
            lstview_s1.Columns.Add(new ColumnHeader() { Text = "ID", Width = 25 });
            lstview_s1.Columns.Add(new ColumnHeader() { Text = "屏幕1" });
            lstview_s1.Columns[1].Width = lstview_s1.ClientSize.Width - lstview_s1.Columns[0].Width;

            lstview_s2.Columns.Add(new ColumnHeader() { Text = "ID", Width = 25 });
            lstview_s2.Columns.Add(new ColumnHeader() { Text = "屏幕2" });
            lstview_s2.Columns[1].Width = lstview_s2.ClientSize.Width - lstview_s2.Columns[0].Width;

            lstview_s3.Columns.Add(new ColumnHeader() { Text = "ID", Width = 25 });
            lstview_s3.Columns.Add(new ColumnHeader() { Text = "屏幕3" });
            lstview_s3.Columns[1].Width = lstview_s3.ClientSize.Width - lstview_s3.Columns[0].Width;

            this.s1_pixBox = new System.Windows.Forms.PictureBox[5] { s1_pa1, s1_pa2, s1_pa3, s1_pb, s1_pc };
            this.s2_pixBox = new System.Windows.Forms.PictureBox[8] { s2_pa1, s2_pa2, s2_pa3, s2_pa4, s2_pb, s2_pc, s2_pd, s2_pe };
            this.stsbarArr = new ToolStripStatusLabel[4] { stsbarComPort, stsbarCMD, stsbarMAP, stsbarTime };
            this.devBoxArr = new TextBox[4] { s1_devNameBox, s1_devStateBox, s2_devNameBox, s2_devStateBox };
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
    }
}
