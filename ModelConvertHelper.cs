using CommonLib.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using CommLiby;

namespace CommonLib
{
    /// <summary>
    /// 功能说明：实体转换辅助类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelConvertHelper<T> where T : new()
    {

        /// <summary>
        /// 从一行数据中为对象赋值
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static T CreateTFromRow(DataRow row)
        {
            if (row == null) return default(T);

            T t = new T();
            // 获得此模型的公共属性

            MemberInfo[] mems = GetMemberInfos(t);
            foreach (MemberInfo pi in mems)
            {
                Type memType;
                string memName = pi.Name;
                PropertyInfo p = pi as PropertyInfo;
                FieldInfo f = pi as FieldInfo;
                if (p != null)
                {
                    memType = p.PropertyType;

                    #region 设置属性值
                    // 检查DataTable是否包含此列
                    if (row.Table.Columns.Contains(memName))
                    {
                        // 判断此属性是否有Setter
                        if (!p.CanWrite) continue;

                        object value = row[memName];
                        if (value != DBNull.Value)
                        {
                            value = Common.Convert2Type(memType, value);
                            p.SetValue(t, value, null);
                        }
                    }
                    else
                    {
                        if (memName == "FIRSTBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = row["FIRSTX"].ToString();
                            value.Y = row["FIRSTY"].ToString();
                            value.Z = row["FIRSTZ"].ToString();
                            p.SetValue(t, value, null);
                        }
                        else if (memName == "SECONDBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = row["SECONDX"].ToString();
                            value.Y = row["SECONDY"].ToString();
                            value.Z = row["SECONDZ"].ToString();
                            p.SetValue(t, value, null);
                        }
                        else if (memName == "THIRDBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = row["THIRDX"].ToString();
                            value.Y = row["THIRDY"].ToString();
                            value.Z = row["THIRDZ"].ToString();
                            p.SetValue(t, value, null);
                        }
                        else if (memName == "FOURBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = row["FOURX"].ToString();
                            value.Y = row["FOURY"].ToString();
                            value.Z = row["FOURZ"].ToString();
                            p.SetValue(t, value, null);
                        }
                    }
                    #endregion
                }
                else if (f != null)
                {
                    memType = f.FieldType;

                    #region 设置字段值
                    // 检查DataTable是否包含此列
                    if (row.Table.Columns.Contains(memName))
                    {
                        object value = row[memName];
                        if (value != DBNull.Value)
                        {
                            value = Common.Convert2Type(memType, value);
                            f.SetValue(t, value);
                        }
                    }
                    else
                    {
                        if (memName == "FIRSTBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = row["FIRSTX"].ToString();
                            value.Y = row["FIRSTY"].ToString();
                            value.Z = row["FIRSTZ"].ToString();
                            f.SetValue(t, value);
                        }
                        else if (memName == "SECONDBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = row["SECONDX"].ToString();
                            value.Y = row["SECONDY"].ToString();
                            value.Z = row["SECONDZ"].ToString();
                            f.SetValue(t, value);
                        }
                        else if (memName == "THIRDBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = row["THIRDX"].ToString();
                            value.Y = row["THIRDY"].ToString();
                            value.Z = row["THIRDZ"].ToString();
                            f.SetValue(t, value);
                        }
                        else if (memName == "FOURBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = row["FOURX"].ToString();
                            value.Y = row["FOURY"].ToString();
                            value.Z = row["FOURZ"].ToString();
                            f.SetValue(t, value);
                        }
                    }
                    #endregion
                }
            }

            return t;
        }
        private static T CreateTFromDictionary(Dictionary<string, string> dicData)
        {
            T t = new T();
            if (dicData == null) return t;
            // 获得此模型的公共属性

            MemberInfo[] mems = GetMemberInfos(t);
            foreach (MemberInfo pi in mems)
            {
                Type memType;
                string memName = pi.Name;
                PropertyInfo p = pi as PropertyInfo;
                FieldInfo f = pi as FieldInfo;
                if (p != null)
                {
                    memType = p.PropertyType;

                    #region 设置属性值
                    // 检查DicData是否包含此列
                    if (dicData.ContainsKey(memName))
                    {
                        // 判断此属性是否有Setter
                        if (!p.CanWrite) continue;

                        object value = dicData[memName];
                        if (value != DBNull.Value)
                        {
                            value = Common.Convert2Type(memType, value);
                            p.SetValue(t, value, null);
                        }
                    }
                    else
                    {
                        if (memName == "FIRSTBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = dicData["FIRSTX"].ToString();
                            value.Y = dicData["FIRSTY"].ToString();
                            value.Z = dicData["FIRSTZ"].ToString();
                            p.SetValue(t, value, null);
                        }
                        else if (memName == "SECONDBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = dicData["SECONDX"].ToString();
                            value.Y = dicData["SECONDY"].ToString();
                            value.Z = dicData["SECONDZ"].ToString();
                            p.SetValue(t, value, null);
                        }
                        else if (memName == "THIRDBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = dicData["THIRDX"].ToString();
                            value.Y = dicData["THIRDY"].ToString();
                            value.Z = dicData["THIRDZ"].ToString();
                            p.SetValue(t, value, null);
                        }
                        else if (memName == "FOURBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = dicData["FOURX"].ToString();
                            value.Y = dicData["FOURY"].ToString();
                            value.Z = dicData["FOURZ"].ToString();
                            p.SetValue(t, value, null);
                        }
                    }
                    #endregion
                }
                else if (f != null)
                {
                    memType = f.FieldType;

                    #region 设置字段值
                    // 检查DicData是否包含此列
                    if (dicData.ContainsKey(memName))
                    {
                        object value = dicData[memName];
                        if (value != DBNull.Value)
                        {
                            value = Common.Convert2Type(memType, value);
                            f.SetValue(t, value);
                        }
                    }
                    else
                    {
                        if (memName == "FIRSTBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = dicData["FIRSTX"].ToString();
                            value.Y = dicData["FIRSTY"].ToString();
                            value.Z = dicData["FIRSTZ"].ToString();
                            f.SetValue(t, value);
                        }
                        else if (memName == "SECONDBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = dicData["SECONDX"].ToString();
                            value.Y = dicData["SECONDY"].ToString();
                            value.Z = dicData["SECONDZ"].ToString();
                            f.SetValue(t, value);
                        }
                        else if (memName == "THIRDBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = dicData["THIRDX"].ToString();
                            value.Y = dicData["THIRDY"].ToString();
                            value.Z = dicData["THIRDZ"].ToString();
                            f.SetValue(t, value);
                        }
                        else if (memName == "FOURBP")
                        {
                            BoundaryPoint value = new BoundaryPoint();
                            value.X = dicData["FOURX"].ToString();
                            value.Y = dicData["FOURY"].ToString();
                            value.Z = dicData["FOURZ"].ToString();
                            f.SetValue(t, value);
                        }
                    }
                    #endregion
                }
            }

            return t;
        }

        public static List<T> ConvertToModelfromRows(DataRow[] rows)
        {
            // 定义集合
            List<T> ts = new List<T>();
            foreach (DataRow dr in rows)
            {
                T t = CreateTFromRow(dr);
                ts.Add(t);
            }
            return ts;
        }

        public static List<T> ConvertToModel(DataTable dt)
        {
            if (dt == null) return null;

            // 定义集合
            List<T> ts = new List<T>();

            foreach (DataRow dr in dt.Rows)
            {
                T t = CreateTFromRow(dr);
                ts.Add(t);
            }

            return ts;
        }

        public static T ConvertToOneModel(DataRow dr)
        {
            T t = CreateTFromRow(dr);
            return t;
        }

        /// <summary>
        /// 将一个Model转换为一个集合列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertOneModelToDictionary(T model, bool oracle = true)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Dictionary<string, object> dicobj = ConvertOneModelToDic(model);
            foreach (KeyValuePair<string, object> pair in dicobj)
            {
                object value = pair.Value;
                string memName = pair.Key;
                string valuestr = null;
                if (value != null)
                {
                    if (value is DateTime)
                    {
                        DateTime dt = (DateTime)value;
                        if (dt.IsValid())
                        {
                            if (oracle)
                                valuestr = string.Format(",to_date('{0}','yyyy-mm-dd hh24:mi:ss')", dt.ToFormatDateTimeStr());
                            else
                                valuestr = dt.ToFormatDateTimeStr();
                        }
                    }
                    else if (value is bool)
                    {
                        bool b = (bool) value;
                        valuestr = b ? "1" : "0";
                    }
                    else if (value is BinDateTime)
                    {
                        BinDateTime binDateTime = value as BinDateTime;
                        if (binDateTime != null && binDateTime.DateTime.IsValid())
                        {
                            if (oracle)
                                valuestr = string.Format(",to_date('{0}','yyyy-mm-dd hh24:mi:ss')",
                                    binDateTime.DateTime.ToFormatDateTimeStr());
                            else
                                valuestr = binDateTime.ToString();
                        }
                    }
                    else if (value is BoundaryPoint)
                    {
                        BoundaryPoint bp = value as BoundaryPoint;
                        dic.Add(memName.Replace("BP", "X"), bp.X);
                        dic.Add(memName.Replace("BP", "Y"), bp.Y);
                        memName = memName.Replace("BP", "Z");
                        valuestr = bp.Z;
                    }
                    else
                    {
                        valuestr = value.ToString();
                        if (valuestr == "undefined" || valuestr == "null")
                            valuestr = null;
                    }
                }
                dic.Add(memName, valuestr);
            }

            return dic;
        }

        public static Dictionary<string, object> ConvertOneModelToDic(T model)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (model == null) return dic;

            MemberInfo[] mems = GetMemberInfos(model);
            foreach (MemberInfo pi in mems)
            {
                Type memType;
                object value;
                string memName = pi.Name;
                PropertyInfo p = pi as PropertyInfo;
                FieldInfo f = pi as FieldInfo;
                if (p != null)
                {
                    memType = p.PropertyType;
                    value = p.GetValue(model, null);
                }
                else if (f != null)
                {
                    memType = f.FieldType;
                    value = f.GetValue(model);
                }
                else
                {
                    continue;
                }
                dic.Add(memName, value);
            }
            return dic;
        }

        /// <summary>
        /// 获得公用属性和字段
        /// </summary>
        /// <returns></returns>
        public static MemberInfo[] GetMemberInfos(T model)
        {
            if (model == null) return new MemberInfo[0];

            Type t = model.GetType();

            PropertyInfo[] propertys = t.GetProperties(); // 获得此模型的公共属性
            FieldInfo[] fields = t.GetFields();  //获得公共字段
            MemberInfo[] mems = new MemberInfo[propertys.Length + fields.Length];
            for (int i = 0; i < mems.Length; i++)
            {
                if (i < propertys.Length)
                    mems[i] = propertys[i];
                else
                    mems[i] = fields[i - propertys.Length];
            }
            return mems;
        }

        /// <summary>
        /// 转换为对应实体
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static T ConvertToOneModel(Dictionary<string, string> dicData)
        {
            T t = CreateTFromDictionary(dicData);
            return t;
        }
    }
}