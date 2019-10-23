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
        Byte s1_num = 0;//1,2,3,4,5,0(未选中)
        Byte s2_num = 0;
        public Form1()
        {
            InitializeComponent();
            userUIInit();
            userTcpInit();
            //this.status_info.Text = "登录时间：" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }
        void TcpCallbackResult(string text, int area)
        {
            if (area == 0)
            {
                //第一个区域显示text
            }
            else if (area == 1)
            {
                //第二个区域显示text
                stadata_info.Text = text;
            }
            else if (area == 2)
            {
                //第三个区域显示text
            }
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
        }

        void userTcpInit()
        {
            TcpServer sv = new TcpServer();
            sv.TcpResultEvent += new TcpServer.TcpResultDeg(TcpCallbackResult);
            //try
            //{
            sv.Start("127.0.0.1", 1234);
            //staserver_info.Text = "    服务器已开启     ";
            //}
            //catch (Exception e)
            //{
            //    staserver_info.Text = "    服务器已关闭     ";
            //    staserver_info.ForeColor = Color.Red;
            //}
            //以广播模式发送设备寻址
            //询问谁是1,2,3号设备(在connect中增加一个设备编号)
            //
            //sv.connects[i];
            //for i in 50:
            //  sv.connects[i].socket.Send(arr, 0, len);
            //  等待应答
            //  更新devNum成员
            
        }

        void myRClickMenuColor_s1(int color)
        {
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
                    //s2_pa4.Image= global::gdisplay.Properties.Resources.lpa_red2;
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
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.lpd_red2;
                else if (color == 2)
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.lpd_yellow2;
                else if (color == 3)
                    s2_pixBox[s2_num-1].Image = global::gdisplay.Properties.Resources.lpd_green2;
            }
            else if (s2_num == 8)
            {
                if (color == 1)
                    s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.pe_red2;
                else if (color == 2)
                    s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.pe_yellow2;
                else if (color == 3)
                    s2_pixBox[s2_num - 1].Image = global::gdisplay.Properties.Resources.pe_green2;
            }
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
            s2_num = 8;
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
                MessageBox.Show("选中1号设备");
            }
            else if(tabControl1.SelectedTab.Name=="tabPage3")
            {
                //选中2号设备
                MessageBox.Show("选中2号设备");
            }
            else if(tabControl1.SelectedTab.Name == "tabPage4")
            {
                //选中3号设备
                MessageBox.Show("选中3号设备");
            }
        }
    }
}
