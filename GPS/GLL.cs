using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.GPS
{
    /// <summary>
    /// 地理位置经度/纬度
    /// </summary>
    public class GLL : NmeaMsg
    {
        public GLL()
        {

            id = NmeaMsg.MsgType.GLL;
            Field f = null;

            f = new Field(new int[] { 1, 2 },"Lat","纬度，格式ddmm.mmmmmm",Field.ValueType.GEODEGREES);
            fields.Add(f);

            f = new Field(new int[] { 3, 4 },"Lon","经度，格式dddmm.mmmmmm",Field.ValueType.GEODEGREES);   
            fields.Add( f);

            f = new Field(new int[] { 5 },"time","UTC时间 hhmmss.sss",Field.ValueType.TIME);
            fields.Add(f);

            f = new Field( new int[] { 6 },"Valid","位置有效标识 V-无效 A-有效",Field.ValueType.CHAR);
            fields.Add(f);

            f = new Field(new[] {7}, "Mode", "定位模式 V-无效 A-有效", Field.ValueType.CHAR);
            fields.Add(f);
        }

        public override bool CanHandle(string[] nmea)
        {
            return nmea[0].Trim().EndsWith("GLL");
        }

        public override NmeaMsg CreateEmpty()
        {
            return new GLL();
        }

    }//EOC
}
