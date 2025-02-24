using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    static class StringTableUtil
    {
        public static string Get(string key, StringParam param = null)
        {
            StringTable stringTable = Lance.GameData.StringTable.TryGet(key);

            int langCodeIdx = (int)Lance.LocalSave.LangCode;

            string text = stringTable?.text[langCodeIdx];

            if (param != null && param.IsValid && text.IsValid())
            {
                text = text.ApplyParam(param);

                param = null;
            }

            return text;
        }

        public static string GetDesc(string index, StringParam param = null)
        {
            return Get($"Desc_{index}", param);
        }

        public static string GetName(string index, StringParam param = null)
        {
            return Get($"Name_{index}", param);
        }

        public static string GetGrade(Grade grade, StringParam param = null)
        {
            return GetName($"{grade}", param);
        }

        public static string GetSystemMessage(string index, StringParam param = null)
        {
            return Get($"SystemMessage_{index}", param);
        }

        public static string GetTimeStr(int second)
        {
            var result = TimeUtil.SplitTime(second);

            StringParam param = new StringParam("hour", $"{result.hour:00}");
            param.AddParam("minute", $"{result.min:00}");
            param.AddParam("second", $"{result.sec:00}");

            return Get("UIString_Time", param);
        }

        public static string GetTutorialMessage(string index, StringParam param = null)
        {
            return Get($"TutorialMessage_{index}", param);
        }

        public static string ApplyParam(this string s, StringParam param)
        {
            if (s.IsValid())
            {
                foreach (KeyValuePair<string, object> p in param.Params)
                {
                    s = s.Replace($"{{{p.Key}}}", p.Value.ToString());
                }
            }

            return s;
        }
    }

    class StringParam
    {
        public Dictionary<string, object> Params;
        public bool IsValid => Params.Count > 0;
        public StringParam(string target, object param)
        {
            Params = new Dictionary<string, object>();

            AddParam(target, param);
        }

        public void AddParam(string target, object param)
        {
            if (Params.ContainsKey(target) == false)
            {
                Params.Add(target, param);
            }
        }
    }
}