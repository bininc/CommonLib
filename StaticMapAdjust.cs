using System;
using System.IO;

namespace CommonLib
{
    public class StaticMapAdjust
    {
        readonly int AdjustLevel = 100;
        Location[,] PointArray = null;

        private static StaticMapAdjust instance = null;
        public static StaticMapAdjust Instance
        {
            get
            {
                if (instance == null)
                    instance = new StaticMapAdjust();
                return instance;
            }
        }

        StaticMapAdjust()
        {
            int a = (136 - 72 + 1) * AdjustLevel;
            int b = (55 - 17 + 1) * AdjustLevel;
            PointArray = new Location[a, b];
            LoadLoLa();
        }
        public void GetLoLa(double ALo, double ALa, out double ALoOut, out double ALaOut)
        {
            try
            {
                int x = (int)(ALo * AdjustLevel);
                int y = (int)(ALa * AdjustLevel);

                if ((x >= (72 * AdjustLevel)) && (x <= (136 * AdjustLevel)) && (y >= (17 * AdjustLevel)) && (y <= (55 * AdjustLevel)))
                {
                    Location p = PointArray[x - 7200, y - 1700];
                    ALoOut = p.Longitude;
                    ALaOut = p.Latitude;
                }
                else
                {
                    ALoOut = 0;
                    ALaOut = 0;
                }
            }
            catch (Exception ex)
            {
                ALoOut = 0;
                ALaOut = 0;
                throw ex;
            }
            return;
        }
        /// <summary>
        /// 计算偏移量
        /// </summary>
        /// <param name="lo"></param>
        /// <param name="la"></param>
        public void CalLola(ref double lo, ref double la)
        {
            double ALoOut = 0;
            double ALaOut = 0;
            GetLoLa(lo, la, out ALoOut, out ALaOut);
            lo = (lo * 1000000 + ALoOut) / 1000000;
            la = (la * 1000000 + ALaOut) / 1000000;
        }
        /// <summary>
        /// 计算偏移量-减掉便宜了
        /// </summary>
        /// <param name="lo"></param>
        /// <param name="la"></param>
        public void CalLolaDec(ref double lo, ref double la)
        {
            double ALoOut = 0;
            double ALaOut = 0;
            GetLoLa(lo, la, out ALoOut, out ALaOut);
            lo = (lo * 1000000 - ALoOut) / 1000000;
            la = (la * 1000000 - ALaOut) / 1000000;
        }
        private void LoadLoLa()
        {
            MemoryStream ms = null;
            BinaryReader br = null;
            try
            {
                ms = new MemoryStream(File.ReadAllBytes(Common.GetRootPath("Lib\\offset.dll")), false);
                br = new BinaryReader(ms);

                while (true)
                {
                    double lo = br.ReadInt16();
                    double la = br.ReadInt16();
                    int loOff = br.ReadInt16();
                    int laOff = br.ReadInt16();
                    int x = (int)(Math.Round(lo) - (72 * AdjustLevel));
                    int y = (int)(Math.Round(la) - (17 * AdjustLevel));
                    PointArray[x, y] = new Location(laOff, loOff);
                }
            }
            catch (Exception e)
            {
                if (e is EndOfStreamException)
                {
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (ms != null)
                    ms.Close();
                if (br != null)
                    br.Close();
            }
        }
    }
}
