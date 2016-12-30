using MyWindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScreenMonitor
{
    public class Command
    {

        public static Command GeneraterCommand(byte[] objData)
        {
            Command command = new Command();
            int ReadIndex = 0;
            command.CommandType = (CommandType)Enum.Parse(typeof(CommandType), Encoding.UTF8.GetString(objData, ReadIndex, 24).Replace("\0", ""));
            ReadIndex += 24;
            if (command.CommandType == CommandType.LeftClick || command.CommandType == CommandType.RightClick)
            {
                int xl = (int)(ScreenShot.Width * float.Parse(Encoding.UTF8.GetString(objData, ReadIndex, 8).Replace("\0", "")));
                ReadIndex += 8;
                int yl = (int)(ScreenShot.Height * float.Parse(Encoding.UTF8.GetString(objData, ReadIndex, 8).Replace("\0", "")));
                command.ClickPoint = new Point(xl, yl);
                ReadIndex += 8;
            }
            command.Message = Encoding.UTF8.GetString(objData, ReadIndex, 128).Replace("\0", "");
            return command;
        }
        public CommandType CommandType { get; set; }
        public Point ClickPoint { get; set; } = new Point(0,0);
        public string Message { get; set; }

        public void Execute()
        {
            switch (CommandType)
            {
                case CommandType.LeftClick:
                    Mouse.Mouse_LeftClick(ClickPoint.X, ClickPoint.Y);
                    break;
                case CommandType.RightClick:
                    Mouse.Mouse_RightClick(ClickPoint.X, ClickPoint.Y);
                    break;
                case CommandType.Shutdown:
                    SystemCMD("shutdown -s -t 0");
                    break;
                case CommandType.Message:
                    MessageBox.Show(Message);
                    break;
            }
        }

        private static void SystemCMD(string cmdStr)
        {
            string CmdPath = @"C:\Windows\System32\cmd.exe";
            Process p = new Process();
            p.StartInfo.FileName = CmdPath;
            p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;          //不显示程序窗口
            p.Start();//启动程序
            p.StandardInput.WriteLine(cmdStr);
            p.Close();
        }
    }
    
    public enum CommandType
    {
        LeftClick,
        RightClick,
        Shutdown,
        Message
    }
}
