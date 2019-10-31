using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gdisplay
{

    static class Program
    {
        public static Form1 gdFrom; //????全局变量
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //gdF
            gdFrom = new Form1();
            //Application.Run(new Form1());
            Application.Run(gdFrom);
        }
    }
}
