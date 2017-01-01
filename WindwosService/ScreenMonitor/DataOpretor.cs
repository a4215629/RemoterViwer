using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace ScreenMonitor
{
    class DataOpretor
    {
        public static void sys_Operate(byte[] data)
        {
            Command.GeneraterCommand(data).Execute();
        }

    }
}
