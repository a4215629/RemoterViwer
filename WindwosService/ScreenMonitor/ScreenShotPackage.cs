using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace ScreenMonitor
{
    public class ScreenShotPackage
    {
        public int XCount { get; private set; }
        public int YCount { get; private set; }
        public bool[,] ChunksChange { get; set; }
        public Image[,] ChunksBmp { get; set; }
        private byte[,][] ChunksJpgData;

        public byte[] GetJpgDta(int x, int y)
        {
            return ChunksJpgData[x, y];
        }
        private byte[] ToJpgBuffer(Image image)
        {
            Graphics graphics = Graphics.FromImage(image);
            MemoryStream ms = new MemoryStream();
            using (var tempImage = image.GetThumbnailImage(image.Width, image.Height, () => true, System.IntPtr.Zero))
            {
                //EncoderParameters parameters = new EncoderParameters(1);
                //parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                //var decoderInfo = ImageCodecInfo.GetImageDecoders().First(ifo => ifo.MimeType == "image/jpeg");
                tempImage.Save(ms,ImageFormat.Jpeg);
                var buffer = ms.ToArray();
                ms.Close();
                return buffer;
            }

        }

        public ScreenShotPackage(Bitmap bmp , int xCount, int yCount,int maxWidth)
        {
            XCount = xCount;
            YCount = yCount;
            int width = Math.Min(bmp.Width,maxWidth);
            int height = bmp.Height * width / bmp.Width;
            Bitmap formatBmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(formatBmp);
            g.InterpolationMode = InterpolationMode.Low;
            g.DrawImage(bmp, new Rectangle(0, 0, width, height), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
            g.Dispose();
            ChunksChange = new bool[xCount, yCount];
            ChunksBmp = new Image[xCount, yCount];
            ChunksJpgData = new byte[xCount, yCount][];
            for (int x = 0; x < XCount; x++)
            {
                for (int y = 0; y < YCount; y++)
                {
                    ChunksChange[x, y] = true;
                    Rectangle rect = new Rectangle();
                    rect.X = formatBmp.Width / XCount * x;
                    rect.Y = formatBmp.Height / YCount * y;
                    rect.Width = formatBmp.Width / XCount;
                    rect.Height = formatBmp.Height / YCount;
                    ChunksBmp[x, y] = formatBmp.Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    ChunksJpgData[x, y] = ToJpgBuffer(ChunksBmp[x, y]);
                }
            }
        }
        public void Compare(ScreenShotPackage mpk)
        {
            if (mpk.XCount != XCount || mpk.YCount != YCount)
                return;
            for (int x = 0; x < XCount; x++)
            {
                for (int y = 0; y < YCount; y++)
                {

                    ChunksChange[x, y] = Convert.ToBase64String(GetJpgDta(x, y)) != Convert.ToBase64String(mpk.GetJpgDta(x, y));
                    if (ChunksChange[x, y] == false)
                        ChunksChange[x, y] = false;
                }
            }
        }

        /// <summary>
        /// 按照规则合成数据包
        /// </summary>
        /// <param name="fullData">是否生成所有块</param>
        /// <returns></returns>
        public byte[] GetBytes(bool fullData = false)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes(0),0,4);
            byte[] xB = BitConverter.GetBytes(XCount);
            byte[] yB = BitConverter.GetBytes(YCount);
            byte[] chunksSizeB = new byte[XCount * YCount * 4];
            for (int x = 0; x < XCount; x++)
                for (int y = 0; y < YCount; y++)
                {
                    int p = (x * YCount + y) * 4;
                    byte[] size = fullData || ChunksChange[x, y] ? BitConverter.GetBytes(ChunksJpgData[x, y].Length) : new byte[] { 0, 0, 0, 0 };
                    chunksSizeB[p] = size[0];
                    chunksSizeB[p+1] = size[1];
                    chunksSizeB[p+2] = size[2];
                    chunksSizeB[p+3] = size[3];
                }

            ms.Write(xB, 0, xB.Length);
            ms.Write(yB, 0, yB.Length);
            ms.Write(chunksSizeB, 0, chunksSizeB.Length);
            for (int x = 0; x < XCount; x++)
                for (int y = 0; y < YCount; y++)
                    if (fullData || ChunksChange[x, y])
                        ms.Write(ChunksJpgData[x, y], 0, ChunksJpgData[x, y].Length);

            var bytes = ms.ToArray();
            byte[] lengthB = BitConverter.GetBytes(bytes.Length - 4);
            bytes[0] = lengthB[0];
            bytes[1] = lengthB[1];
            bytes[2] = lengthB[2];
            bytes[3] = lengthB[3];
            ms.Close();
            return bytes;
        }

    }
}
