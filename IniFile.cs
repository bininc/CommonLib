using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CommonLib
{
    public class IniFile
    {
        SortedDictionary<string, string> data = new SortedDictionary<string, string>();
        public IniFile() : base() { }
        public void Load(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string folder = "[]";
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim();
                    if (s.Length == 0 || s[0] == ';') continue;
                    if (s[0] == '[')
                    {
                        folder = s;
                        continue;
                    }
                    string key, value;
                    int delim = s.IndexOf('=');
                    if (delim < 0)
                    {
                        key = folder + s.Replace("[", string.Empty).Replace("]", string.Empty);
                        value = string.Empty;
                    }
                    else
                    {
                        key = folder + s.Remove(delim).TrimEnd().Replace("[", string.Empty).Replace("]", string.Empty);
                        value = s.Substring(delim + 1).TrimStart();
                    }
                    if (!data.ContainsKey(key)) data.Add(key, value);
                    else data[key] = value;
                }
            }
        }
        public void Save(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                string folder = "[]";
                foreach (string key in data.Keys)
                {
                    int delim = key.IndexOf(']');
                    string keyFolder = key.Remove(delim + 1);
                    string keyName = key.Substring(delim + 1);
                    if (keyFolder != folder)
                    {
                        folder = keyFolder;
                        sw.WriteLine(folder);
                    }
                    sw.WriteLine(keyName + " = " + data[key]);
                }
            }
        }
        public ICollection<string> Keys { get { return data.Keys; } }
        public bool ContainKey(string key) { return data.ContainsKey(key); }
        public void Remove(string key) { data.Remove(key); }
        public void Add<T>(string key, T value)
        {
            AddRawValue(key, AddQuotes(value.ToString()));
        }
        public T Get<T>(string key)
        {
            if (typeof(T) == typeof(string)) return (T)(object)RemoveQuotes(GetRawValue(key));
            if (typeof(T).BaseType == typeof(Enum)) return (T)Enum.Parse(typeof(T), RemoveQuotes(GetRawValue(key)));
            MethodInfo parseMethod = typeof(T).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
            if (parseMethod == null) throw new ArgumentException();
            return (T)parseMethod.Invoke(null, new object[] { RemoveQuotes(GetRawValue(key)) });
        }
        public void Set<T>(string key, T value)
        {
            SetRawValue(key, AddQuotes(value.ToString()));
        }
        private string GetRawValue(string key) { return data[key]; }
        void SetRawValue(string key, string value) { data[key] = value; }
        void AddRawValue(string key, string value)
        {
            key = key.Trim();
            value = value.Trim();
            int folderBegin = key.IndexOf('[');
            int folderEnd = key.IndexOf(']');
            if (folderBegin != 0 || folderEnd < 0) throw new ArgumentException("key");
            data.Add(key, value);
        }
        static string AddQuotes(string s) { return "\"" + s + "\""; }
        static string RemoveQuotes(string s)
        {
            if (string.IsNullOrEmpty(s) || s[0] != '\"') return s;
            s = s.Substring(1);
            if (s.Length != 0 && s[s.Length - 1] == '\"') s = s.Remove(s.Length - 1);
            return s;
        }
    }
}
