using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ScreenMonitor
{
    public class ScreenShotPackage
    {
        public const int type_completeImage = 1;
        public const int type_splittingImage = 2;
        
        public ScreenShot ScreenShot { get; private set; }
        public int SplitXCount { get; private set; }
        public int SplitYCount { get; private set; }
        public bool[,] ChunksChange { get; set; }
        private Bitmap CompressedBmp { get; set; }
        private Bitmap[,] ChunksBmp { get; set; }
        private byte[] CompressedJpgData{ get; set; }
        private byte[,][] ChunksJpgData;
        private bool IsSplittingCompress { get; set; }

        public byte[] GetJpgDta(int x, int y)
        {
            return ChunksJpgData[x, y];
        }
   

        public float DifferentRate { get
            {
                int count = 0;
                foreach (var item in ChunksChange)
                {
                    if (item)
                        count++;
                }
                return count /(float) ChunksChange.Length;
            }

        }
        private static byte[] ToJpgBuffer(Image image)
        {
            Graphics graphics = Graphics.FromImage(image);
            MemoryStream ms = new MemoryStream();
            using (var tempImage = image.GetThumbnailImage(image.Width, image.Height, () => true, System.IntPtr.Zero))
            {
                tempImage.Save(ms,ImageFormat.Jpeg);
                var buffer = ms.ToArray();
                ms.Close();
                return buffer;
            }

        }

        public ScreenShotPackage(ScreenShot screenShort ,int compressedMaxWidth)
        {
            this.ScreenShot = screenShort;
            var bitmap = screenShort.bitmap;
            int width = Math.Min(bitmap.Width,compressedMaxWidth);
            int height = bitmap.Height * width / bitmap.Width;
            CompressedBmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(CompressedBmp);
            g.InterpolationMode = InterpolationMode.Low;
            g.DrawImage(bitmap, new Rectangle(0, 0, width, height), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            g.Dispose();
            IsSplittingCompress = false;
        }

        public void InitializeSplitting(int splitXCount, int splitYCount)
        {
            SplitXCount = splitXCount;
            SplitYCount = splitYCount;
            ChunksChange = new bool[splitXCount, splitYCount];
            ChunksBmp = new Bitmap[splitXCount, splitYCount];
            ChunksJpgData = new byte[splitXCount, splitYCount][];
            for (int x = 0; x < SplitXCount; x++)
            {
                for (int y = 0; y < SplitYCount; y++)
                {
                    ChunksChange[x, y] = true;
                    Rectangle rect = new Rectangle();
                    rect.X = CompressedBmp.Width / SplitXCount * x;
                    rect.Y = CompressedBmp.Height / SplitYCount * y;
                    rect.Width = CompressedBmp.Width / SplitXCount;
                    rect.Height = CompressedBmp.Height / SplitYCount;
                    ChunksBmp[x, y] = CompressedBmp.Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    ChunksJpgData[x, y] = ToJpgBuffer(ChunksBmp[x, y]);
                }
            }
        }
        public void CompressByBasePackage(ScreenShotPackage basePackage)
        {
            if (IsSplittingCompress)
                return;
            if (basePackage.SplitXCount != SplitXCount || basePackage.SplitYCount != SplitYCount)
                return;
            if (basePackage.ScreenShot.Equals(ScreenShot))
            {
                for (int x = 0; x < SplitXCount; x++)
                    for (int y = 0; y < SplitYCount; y++)
                        ChunksChange[x, y] = false;
                return;
            }
            if (!basePackage.IsSplittingCompress)
                basePackage.InitializeSplitting(SplitXCount,SplitYCount);
            for (int x = 0; x < SplitXCount; x++)
                for (int y = 0; y < SplitYCount; y++)
                    ChunksChange[x, y] = Convert.ToBase64String(ChunksJpgData[x, y]) != Convert.ToBase64String(basePackage.ChunksJpgData[x, y]);
            IsSplittingCompress = true;
        }

        /// <summary>
        /// 按照规则合成数据包
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            if (IsSplittingCompress)
            {
                var sData = GenerateSplittingCompressedBytes();
                var cDaaa = GenerateCompressedBytes();
                //Console.WriteLine(sData.Length < cDaaa.Length ? "Splite image":"Completed image");
                return sData.Length < cDaaa.Length ? sData : cDaaa;
            }
            //Console.WriteLine("Completed image");
            return GenerateCompressedBytes();
        }
        private byte[] GenerateSplittingCompressedBytes()
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes(0), 0, 4);
            ms.Write(BitConverter.GetBytes(type_splittingImage), 0, 4);
            byte[] xB = BitConverter.GetBytes(SplitXCount);
            byte[] yB = BitConverter.GetBytes(SplitYCount);
            byte[] chunksSizeB = new byte[SplitXCount * SplitYCount * 4];
            for (int x = 0; x < SplitXCount; x++)
                for (int y = 0; y < SplitYCount; y++)
                {
                    int p = (x * SplitYCount + y) * 4;
                    byte[] size = ChunksChange[x, y] ? BitConverter.GetBytes(ChunksJpgData[x, y].Length) : new byte[] { 0, 0, 0, 0 };
                    chunksSizeB[p] = size[0];
                    chunksSizeB[p + 1] = size[1];
                    chunksSizeB[p + 2] = size[2];
                    chunksSizeB[p + 3] = size[3];
                }

            ms.Write(xB, 0, xB.Length);
            ms.Write(yB, 0, yB.Length);
            ms.Write(chunksSizeB, 0, chunksSizeB.Length);
            for (int x = 0; x < SplitXCount; x++)
                for (int y = 0; y < SplitYCount; y++)
                    if (ChunksChange[x, y])
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
        private byte[] GenerateCompressedBytes()
        {
            CompressedJpgData = ToJpgBuffer(CompressedBmp);
            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes(CompressedJpgData.Length + 4), 0, 4);
            ms.Write(BitConverter.GetBytes(type_completeImage), 0, 4);
            ms.Write(CompressedJpgData, 0, CompressedJpgData.Length);
            var bytes = ms.ToArray();
            ms.Close();
            return ms.ToArray();

        }
    }
}
