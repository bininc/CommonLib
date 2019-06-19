using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.GPS
{
    /// <summary>
    /// GNSS定位数据
    /// </summary>
    public class GGA : NmeaMsg
    {
        public GGA()
        {
            id = NmeaMsg.MsgType.GGA;
            Field f = null;

            f = new Field(new []{ 1 }, "time","UTC时间，hhmmss.sss，时分秒格式", Field.ValueType.TIME);
            fields.Add(f);

            f = new Field(new int[] { 4, 5 },"Lon", "dddmm.mmmm，度分格式（前导位数不足则补0) E（东经）或W（西经）", Field.ValueType.GEODEGREES);
            fields.Add(f);

            f = new Field(new int[] { 2, 3 },"Lat", "纬度 ddmm.mmmm，度分格式（前导位数不足则补0) N（北纬）或S（南纬）", Field.ValueType.GEODEGREES);
            fields.Add(f);

            f = new Field(new int[] { 6 },"FS", "定位状态标识，0=未定位，1=单点定位，2=伪距差分定位，3=无效PPS，6=正在估算", Field.ValueType.INTEGER);
            fields.Add(f);

            f = new Field( new int[] { 7 },"NoSv", "参与定位的卫星数量",Field.ValueType.INTEGER);
            fields.Add(f);

            f = new Field(new int[] { 8 },"HDOP", "水平精度因子（0.5 - 99.9）", Field.ValueType.DOUBLE);
            fields.Add(f);

            f = new Field(new int[] { 9 },"msl", "椭球高 单位固定M", Field.ValueType.DOUBLE);
            fields.Add(f);

            f = new Field(new[] { 11 }, "Altref", "海平面分离度 单位固定M", Field.ValueType.DOUBLE);
            fields.Add(f);

            f = new Field(new[] { 13 }, "DiffAge", "差分校正时延，单位为秒（如果不是差分定位将为空）", Field.ValueType.DOUBLE);
            fields.Add(f);

            f = new Field(new int[] { 14 },"DiffStation", "差分站ID号（如果不是差分定位将为空）", Field.ValueType.DOUBLE);
            fields.Add(f);
        }

        public override bool CanHandle(string[] nmea)
        {
            return nmea[0].Trim().EndsWith("GGA");
        }

        public override NmeaMsg CreateEmpty()
        {
            return new GGA();
        }
    }//EOC
}
