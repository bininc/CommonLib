using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.GPS
{
    /// <summary>
    /// 可见的GNSS卫星 每条GSV消息只包含4颗卫星的信息
    /// </summary>
    public class GPGSV : NmeaMsg
    {

        public GPGSV()
        {
            id = NmeaMsg.MsgType.GPGSV;
            Field f = null;

            f = new Field(new int[] { 1 }, "NoMsg", "GSV消息总数，最小值为1", Field.ValueType.INTEGER);
            fields.Add(f);

            f = new Field(new int[] { 2 }, "MsgNo", "本条GSV消息的编号，最小值为1", Field.ValueType.INTEGER);
            fields.Add(f);

            f = new Field(new int[] { 3 }, "NoSV", "本系统可见卫星的总数", Field.ValueType.INTEGER);
            fields.Add(f);

            for (int i = 0; i < 4; ++i)
            {
                int t = 4 * i;

                f = new Field(new int[] { t + 4 }, "Sv" + (i + 1), "卫星号", Field.ValueType.INTEGER);
                fields.Add(f);

                f = new Field(new int[] { t + 5 }, "elv" + (i + 1), "卫星的仰角(0~90度)", Field.ValueType.DOUBLE);
                fields.Add(f);

                f = new Field(new int[] { t + 6 }, "az" + (i + 1), "卫星的方位角(0~359度)", Field.ValueType.DOUBLE);
                fields.Add(f);

                f = new Field(new int[] { t + 7 }, "cno" + (i + 1), "卫星的载噪比(0~99dBHz)", Field.ValueType.INTEGER);
                fields.Add(f);
            }
        }

        public override bool CanHandle(string[] nmea)
        {
            return nmea[0].Trim().Equals("$GPGSV");
        }

        public override NmeaMsg CreateEmpty()
        {
            return new GPGSV();
        }

    }//EOC
}
