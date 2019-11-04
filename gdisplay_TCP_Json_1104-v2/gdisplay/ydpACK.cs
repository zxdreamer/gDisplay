using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gdisplay
{
    public class ydpACK
    {
        public int ydp_ack_process(Connect con,byte[] ackArr,int n)
        {
            if (ackArr.Length < n)
                return -1;

            return n;
        }
    }
}
