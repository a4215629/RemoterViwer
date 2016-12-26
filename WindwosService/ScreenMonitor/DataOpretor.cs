using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyWindowsAPI;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace ScreenMonitor
{
    class DataOpretor
    {
        /// <summary>
        /// 从两个数组中提取变动的数据，组成一个新的数组，减少数据量，用于网络发送。
        /// </summary>
        /// <param name="oldBuffer">上次发送的数据</param>
        /// <param name="newBuffer">新数据</param>
        /// <returns></returns>
        public static List<byte[]> compressData(byte[] oldBuffer, byte[] newBuffer, out int indexLength, out int DataLength)
        {
            byte transCount = 3;
            byte[] index = new byte[newBuffer.Length * transCount];
            byte[] newData = new byte[newBuffer.Length];
            int j = 0; int k = 0;
            bool isCame = true;
            for (int i = 0; i < newBuffer.Length; i++)
            {
                if (i > oldBuffer.Length - 1 || newBuffer[i] != oldBuffer[i])
                {
                    newData[j++] = newBuffer[i];
                    if (isCame)
                    {
                        index[k++] = (byte)(i >> 16);
                        index[k++] = (byte)(i >> 8);
                        index[k++] = (byte)i;
                        isCame = false;
                    }
                }
                else
                {
                    if (!isCame)
                    {
                        index[k++] = (byte)(i >> 16);
                        index[k++] = (byte)(i >> 8);
                        index[k++] = (byte)i;
                        isCame = true;
                    }

                }
            }
            index[k++] = (byte)(newBuffer.Length >> 16);
            index[k++] = (byte)(newBuffer.Length >> 8);
            index[k++] = (byte)newBuffer.Length;
            DataLength = j;
            indexLength = k;
            return new List<byte[]> { index, newData };
        }


        public static void sys_Operate(byte[] data)
        {
            Command.GeneraterCommand(data).Execute();
        }

    }
}
