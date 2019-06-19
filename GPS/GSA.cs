using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.GPS
{
    /// <summary>
    /// GNSS精度因子与有效卫星信息
    /// </summary>
    public class GSA : NmeaMsg
    {
        public GSA()
        {
            id = NmeaMsg.MsgType.GSA;
            Field f = null;

            f = new Field(new int[] { 1 }, "Smode", "定位模式指定状态 M-手动指定2D或3D定位 A-自动切换2D或3D定位", Field.ValueType.CHAR);
            fields.Add(f);

            f = new Field(new int[] { 2 }, "FS", "定位模式 1-未定位 2-2D定位 3-3D定位", Field.ValueType.INTEGER);
            fields.Add(f);

            for (int i = 1; i <= 12; i++)
            {
                f = new Field(new[] { i + 2 }, "sv" + i, "卫星号 GPS卫星号1~32 北斗卫星号161~197(160+北斗PRN号)", Field.ValueType.INTEGER);
                fields.Add(f);
            }

            f = new Field(new int[] { 15 }, "PDOP", "位置精度因子，0.0-127.00", Field.ValueType.DOUBLE);
            fields.Add(f);

            f = new Field(new int[] { 16 }, "HDOP", "水平精度因子，0.0-127.00", Field.ValueType.DOUBLE);
            fields.Add(f);

            f = new Field(new int[] { 17 }, "VDOP", "垂向精度因子，0.0-127.00", Field.ValueType.DOUBLE);
            fields.Add(f);
        }

        public override bool CanHandle(string[] nmea)
        {
            return nmea[0].Trim().EndsWith("GSA");
        }

        public override NmeaMsg CreateEmpty()
        {
            return new GSA();
        }

    }//EOC
}
