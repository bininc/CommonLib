﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.GPS
{
    public class Field
    {
        public enum ValueType
        {
            DOUBLE,
            TIME,
            INTEGER,
            GEODEGREES,
            CHAR,
            SPEED,
            DEGREES,
            DATE,
            STRING
        }

        public enum ActualValueType
        {
            DOUBLE,
            TIME,
            INTEGER,
            CHAR,
            STRING,
            SPEED,
            DATE,
            OTHER
        }

        public static ActualValueType Value2ActualType(ValueType t)
        {
            switch (t)
            {
                case ValueType.DOUBLE: return ActualValueType.DOUBLE;
                case ValueType.TIME: return ActualValueType.TIME;
                case ValueType.INTEGER: return ActualValueType.INTEGER;
                case ValueType.GEODEGREES: return ActualValueType.DOUBLE;
                case ValueType.CHAR: return ActualValueType.CHAR;
                case ValueType.STRING: return ActualValueType.STRING;
                case ValueType.SPEED: return ActualValueType.DOUBLE;
                case ValueType.DEGREES: return ActualValueType.DOUBLE;
                case ValueType.DATE: return ActualValueType.DATE;
            }
            return ActualValueType.OTHER;
        }

        public ActualValueType ActualType
        {
            get
            {
                return Field.Value2ActualType(this.valueType);
            }
        }

        public Field(int[] index, string name, string desc, ValueType valueType)
        {
            this.index = index;
            this.Name = name;
            this.Desc = desc;
            this.valueType = valueType;
        }

        public string Name { get; private set; }
        public string Desc { get; private set; }

        public bool HasValue
        {
            get
            {
                if (value is TimeSpan)
                    return (TimeSpan)value != TimeSpan.MinValue;
                else if (value is DateTime)
                    return (DateTime)value != DateTime.MinValue;
                else return (value != null);
            }
        }

        public int[] index = null;

        public ValueType valueType = ValueType.INTEGER;

        public object value = null;

        private string GetIndexedString(string[] nmea, int indexVal)
        {
            if (indexVal >= this.index.Length)
            {
                return null;
            }
            int i = index[indexVal];
            if ((i < 0) || (i >= nmea.Length))
            {
                return null;
            }
            return nmea[i];
        }

        public void Parse(string[] nmea)
        {
            value = null;
            if ((index == null) || (index.Length <= 0))
            {
                return;
            }
            switch (this.valueType)
            {
                case ValueType.INTEGER:
                    value = Utils.Str2Int(GetIndexedString(nmea, 0), 0);
                    break;
                case ValueType.DOUBLE:
                    value = Utils.Str2Double(GetIndexedString(nmea, 0), 0);
                    break;
                case ValueType.TIME:
                    value = Utils.Str2Time(GetIndexedString(nmea, 0));
                    break;
                case ValueType.GEODEGREES:
                    value = Utils.Str2Degrees(
                            GetIndexedString(nmea, 0),
                            Utils.Str2Char(GetIndexedString(nmea, 1)));
                    break;
                case ValueType.CHAR:
                    value = Utils.Str2Char(GetIndexedString(nmea, 0));
                    break;
                case ValueType.STRING:
                    value = GetIndexedString(nmea, 0);
                    break;
                case ValueType.DEGREES:
                    value = Utils.Str2Double(GetIndexedString(nmea, 0), -1.0);
                    break;
                case ValueType.DATE:
                    value = Utils.Str2Date(GetIndexedString(nmea, 0));
                    break;
                case ValueType.SPEED:
                    value = Utils.Str2Speed(GetIndexedString(nmea, 0));
                    break;
            }
        }

        public double GetDouble(double defaultVal)
        {
            if (!HasValue || (ActualType != ActualValueType.DOUBLE)) return defaultVal;
            double d = (double)value;
            return d;
        }

        public int GetInt(int defaultVal)
        {
            if (!HasValue || (ActualType != ActualValueType.INTEGER)) return defaultVal;
            int d = (int)value;
            return d;
        }

        public char GetChar(char defaultVal)
        {
            if (!HasValue || (ActualType != ActualValueType.CHAR)) return defaultVal;
            char d = (char)value;
            return d;
        }

        public TimeSpan GetTime(TimeSpan defaultVal)
        {
            if (!HasValue || (ActualType != ActualValueType.TIME)) return defaultVal;
            TimeSpan d = (TimeSpan)value;
            return d;
        }

        public DateTime GetDate(DateTime defaultVal)
        {
            if (!HasValue || (ActualType != ActualValueType.DATE)) return defaultVal;
            DateTime d = (DateTime)value;
            return d;
        }

        public string GetString()
        {
            if (!HasValue) return null;
            return value.ToString();
        }

        // for debug only
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("IDX: ").Append(index[0]).Append(" N:").Append(this.Name).Append(" T:").Append(this.valueType.ToString()).Append(" V:");
            if (value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                sb.Append(value);
            }
            sb.Append(" Desc:").Append(this.Desc);

            return sb.ToString();
        }

    }//EOC
}
