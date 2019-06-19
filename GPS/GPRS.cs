using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.GPS
{
    /// <summary>
    /// 车机网络状态(扩展)
    /// </summary>
    public class GPRS : NmeaMsg
    {
        public GPRS()
        {
            id = NmeaMsg.MsgType.GPRS;
            Field f = null;

            f = new Field(new int[] { 1 }, "mac", "车机识别码", Field.ValueType.STRING);
            fields.Add(f);

            f = new Field(new int[] { 2 }, "state", "状态，0-未连接 1-已连接", Field.ValueType.INTEGER);
            fields.Add(f);

            f = new Field(new[] { 3 }, "mileage", "里程单位：米", Field.ValueType.INTEGER);
            fields.Add(f);
        }

        public override bool CanHandle(string[] nmea)
        {
            return nmea[0].Trim().EndsWith("GPRS");
        }

        public override NmeaMsg CreateEmpty()
        {
            return new GPRS();
        }
    }
}
