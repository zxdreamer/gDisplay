using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gdisplay
{
    class ydpLog
    {
        /*************************************************
         * WriteLineLog:日志文件写函数,写入一行数据
         * Param：
         *      strlog：一行数据
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
        /*************************************************
         * WriteLineLog:日志文件写函数,同一行追加方式写入数据
         * Param：
         *      strlog：写入数据
         * ************************************************/
        public static int WriteAddLog(string strlog)
        {
            try
            {
                using (StreamWriter wrLog = new StreamWriter(@".\\logFile.txt", true))
                {
                    wrLog.Write(strlog);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("文件系统异常" + e.Message);
                return -1;
            }

            return 0;
        }
        /**********************************************************
         * ByteToRawStr:将byte[]数组直译成字符串，便于日志文件，状态栏记录
         * Param:
         *      arr:需要转换的数组
         * Result:
         *      数组直译后的字符串
         * *******************************************************/
        public static string ByteToRawStr(byte[] arr,int start,int len)
        {
            string res = null;
            string[] state = new string[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            for (int i = start; i < len; i++)
            {
                string shi = state[(arr[i] & 0xf0) >> 4];
                string ge = state[arr[i] & 0x0f];
                res += (shi + ge + " ");
            }

            return res;
        }
    }
}
