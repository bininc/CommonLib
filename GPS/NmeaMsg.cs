using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommLiby;

namespace CommonLib.GPS
{
    public class NmeaMsg
    {
        public enum MsgType
        {
            Error, Done, GGA, RMC, GLL, GSA, GPGSV, BDGSV, GPRS
        }

        public enum GnssMode : byte
        {
            Unkown,
            GPS,
            BD,
            GPS_BD
        }

        public static List<NmeaMsg> handlers { get; private set; }

        public static void InitDefaults()
        {
            Add(new GGA());
            Add(new RMC());
            Add(new GLL());
            Add(new GSA());
            Add(new GPGSV());
            Add(new BDGSV());
            Add(new GPRS());
        }

        public static void Add(NmeaMsg msg)
        {
            if (msg == null)
            {
                return;
            }
            if (handlers == null)
            {
                handlers = new List<NmeaMsg>();
            }

            bool suc = handlers.Any(n => n.id == msg.id);
            if (suc) return; //去除重复语句

            handlers.Add(msg);
        }


        public MsgType id = MsgType.Error;

        public DateTime LastNmeaTime { get; private set; }

        public static NmeaMsg GetMsgHandle(MsgType msgType)
        {
            if (handlers == null)
            {
                return null;
            }

            foreach (NmeaMsg handler in handlers)
            {
                if (handler.id == msgType)
                {
                    return handler;
                }
            }

            return null;
        }

        protected List<Field> fields = new List<Field>();

        protected NmeaMsg()
        {
        }

        public NmeaMsg(MsgType id)
        {
            this.id = id;
        }

        public List<Field> Fields
        {
            get
            {
                return fields;
            }
        }

        public GnssMode GNSSMode { get; private set; }

        // true if can parse a given nmea
        public virtual bool CanHandle(string[] nmea)
        {
            return false;
        }

        public virtual NmeaMsg CreateEmpty()
        {
            return new NmeaMsg();
        }

        public virtual void FromNMEA(string[] p)
        {
            if (p == null) return;
            if (p.Length < 1) return;

            if (id != MsgType.GPRS && id != MsgType.Done && id != MsgType.Error)
            {
                string mode = p[0].TrimStart().TrimStart('$').Substring(0, 2);
                if (mode == "GP") GNSSMode = GnssMode.GPS;
                else if (mode == "BD") GNSSMode = GnssMode.BD;
                else if (mode == "GN") GNSSMode = GnssMode.GPS_BD;
                else GNSSMode = GnssMode.Unkown;
            }

            foreach (Field f in fields)
            {
                if (f == null) continue;
                f.Parse(p);
            }
            LastNmeaTime = DateTimeHelper.Now;
        }

        public static NmeaMsg Parse(string[] nmea)
        {
            if ((nmea == null) || (nmea.Length <= 0))
            {
                return null;
            }
            if (handlers == null)
            {
                return null;
            }

            foreach (NmeaMsg m in handlers)
            {
                if (m.CanHandle(nmea))
                {
                    NmeaMsg result = m.CreateEmpty();
                    result.FromNMEA(nmea);
                    m.LastNmeaTime = DateTimeHelper.Now;
                    return result;
                }
            }

            return null;
        }

        public Field GetFieldByName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) return null;
            return Fields.FirstOrDefault(f => f.Name == fieldName);
        }

        // for debug only
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(id.ToString()).Append(" ");
            foreach (Field f in fields)
            {
                if (f == null) continue;
                sb.Append(f.ToString());
                sb.Append(" ~ ");
            }
            return sb.ToString();
        }
    }//EOC
}
