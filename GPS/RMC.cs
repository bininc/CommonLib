using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.GPS
{
    /// <summary>
    /// 推荐的最少数据
    /// </summary>
    public class RMC : NmeaMsg
    {
        public RMC()
        {
            id = NmeaMsg.MsgType.RMC;
            Field f = null;

            f = new Field(new int[] { 1 }, "time", "UTC时间，格式为hhmmss.sss", Field.ValueType.TIME);
            fields.Add(f);

            f = new Field(new int[] { 2 }, "status", "位置有效标识 V-无效 A-有效", Field.ValueType.CHAR);
            fields.Add(f);

            f = new Field(new int[] { 3, 4 }, "Lat", "纬度格式为 ddmm.mmmmmm", Field.ValueType.GEODEGREES);
            fields.Add(f);

            f = new Field(new int[] { 5, 6 }, "Lon", "经度格式为 dddmm.mmmmmm", Field.ValueType.GEODEGREES);
            fields.Add(f);

            f = new Field(new int[] { 7 }, "spd", "地面速率，单位为节", Field.ValueType.SPEED);
            fields.Add(f);

            f = new Field(new int[] { 8 }, "cog", "地面航向，单位为度，从北向起顺时针计算", Field.ValueType.DEGREES);
            fields.Add(f);

            f = new Field(new int[] { 9 }, "date", "UTC日期，格式为ddmmyy", Field.ValueType.DATE);
            fields.Add(f);

            f = new Field(new[] { 10 }, "mv", "磁偏角,固定填空", Field.ValueType.DOUBLE);
            fields.Add(f);

            f = new Field(new[] { 11 }, "mvE", "偏磁角，固定填E", Field.ValueType.CHAR);
            fields.Add(f);

            f = new Field(new[] { 12 }, "mode", "定位模式 N-未定位 A-单点定位 D-差分定位", Field.ValueType.CHAR);
            fields.Add(f);
        }

        public override bool CanHandle(string[] nmea)
        {
            return nmea[0].Trim().EndsWith("RMC");
        }

        public override NmeaMsg CreateEmpty()
        {
            return new RMC();
        }

    }//EOC
}
