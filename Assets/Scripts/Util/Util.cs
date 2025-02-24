using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;
using Newtonsoft.Json;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    static class Util
    {
        public static bool AnyHaveValue(this int[] array)
        {
            for(int i = 0; i < array.Length; ++i)
            {
                if (array[i] > 0)
                    return true;
            }

            return false;
        }
        public static char SelectAny(string str)
        {
            if (str == string.Empty)
                return (char)0;

            float seed = Time.time * 100f;

            UnityEngine.Random.InitState((int)seed);

            return str[UnityEngine.Random.Range(0, str.Length)];
        }

        public static bool Dice(float prob = 0.5f)
        {
            var probs = new float[2] { prob, 1 - prob };

            int idx = RandomChoose(probs);

            return idx == 0;
        }

        public static int RandomChoose(float[] probs)
        {
            float total = 0;

            foreach (float elem in probs)
            {
                total += elem;
            }

            float randomPoint = UnityEngine.Random.value * total;

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                {
                    return i;
                }
                else
                {
                    randomPoint -= probs[i];
                }
            }
            return probs.Length - 1;
        }

        public static T RandomEnum<T>()
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        public static T RandomSelect<T>(T[] values)
        {
            return values[UnityEngine.Random.Range(0, values.Length)];;
        }

        public static IOrderedEnumerable<T> Shuffle<T>(IEnumerable<T> datas)
        {
            System.Random random = new System.Random();

            return datas.OrderBy(x => random.Next());
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent = null)
        {
            return GameObject.Instantiate<GameObject>(prefab, parent);
        }

        public static GameObject Instantiate(string prefabPath, Transform parent = null)
        {
            var obj = Resources.Load<GameObject>(prefabPath);

            if (obj == null)
            {
                return default(GameObject);
            }

            GameObject go = GameObject.Instantiate(obj, parent);

            return go;
        }

        public static GameObject InstantiateUI(string prefabPath, Transform parent = null)
        {
            if (parent == null)
            {
                parent = GameObject.Find("Canvas")?.transform ?? null;
            }

            var go = Instantiate($"Prefabs/UI/{prefabPath}", parent);

            //var rectTm = go.GetComponent<RectTransform>();
            //if (rectTm != null)
            //{
            //    rectTm.offsetMin = new Vector2(0, 0);
            //    rectTm.offsetMax = new Vector2(0, 0);
            //}

            return go;
        }

        public static GameObject Find(this GameObject go, string name, bool includeinactive = false)
        {
            if (go != null)
            {
                var tmList = go.GetComponentsInChildren<Transform>(includeinactive);
                foreach (var tm in tmList)
                {
                    if (tm.name == name)
                        return tm.gameObject;
                }
            }

            return null;
        }

        public static GameObject Find(this Transform tm, string name, bool includeinactive = false)
        {
            if (tm != null)
            {
                var childs = tm.GetComponentsInChildren<Transform>(includeinactive);
                foreach (var child in childs)
                {
                    if (child.name == name)
                        return child.gameObject;
                }
            }

            return null;
        }

        public static T Find<T>(this GameObject go, string name, bool includeinactive = false)
        {
            if (go != null)
            {
                var tm = go.Find(name, includeinactive);
                return tm.GetComponent<T>();
            }

            return default(T);
        }

        public static bool IsOverLapPoint2D(this GameObject go, Vector2 point)
        {
            return go.GetComponent<BoxCollider2D>()?.OverlapPoint(point) ?? false;
        }

        public static void Destroy(this GameObject go)
        {
            if (go != null)
            {
                GameObject.Destroy(go);
            }
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
            {
                comp = go.AddComponent<T>();
            }

            return comp;
        }

        public static T GetOrAddComponent<T>(this Component comp) where T : Component
        {
            T getComp = comp.GetComponent<T>();
            if (getComp == null)
            {
                getComp = comp.gameObject.AddComponent<T>();
            }

            return getComp;
        }

        public static T Get<T>(this GameObject go)
        {
            T comp = go.GetComponent<T>();

            return comp;
        }

        static public void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static bool Intersects(this GameObject go, GameObject other)
        {
            BoxCollider colliderA = go.GetComponent<BoxCollider>();
            BoxCollider colliderB = other.GetComponent<BoxCollider>();
            if (colliderA != null && colliderB != null)
            {
                Bounds boundA = colliderA.bounds;
                Bounds boundB = colliderB.bounds;

                return boundA.Intersects(boundB);
            }

            return false;
        }

        public static bool Intersects2D(this GameObject go, GameObject other)
        {
            BoxCollider2D colliderA = go.GetComponent<BoxCollider2D>();
            BoxCollider2D colliderB = other.GetComponent<BoxCollider2D>();
            if (colliderA != null && colliderB != null)
            {
                Bounds boundA = colliderA.bounds;
                Bounds boundB = colliderB.bounds;

                return boundA.Intersects(boundB);
            }

            return false;
        }

        public static void ResetAnchor(this RectTransform rt)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
        }

        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
        {
            TValue value;
            dic.TryGetValue(key, out value);

            return value;
        }

        public static float GetDistanceBy2D(Vector2 a, Vector2 b)
        {
            return (a - b).magnitude;
        }

        public static float GetDistance(Vector3 a, Vector3 b)
        {
            return (a - b).magnitude;
        }

        public static Vector3 WorldToScreenPoint(Vector3 worldPos)
        {
            return Lance.CameraManager.MainCamera.WorldToScreenPoint(worldPos);
        }

        public static GameObject FindGameObject(this GameObject go, string name, bool ignoreAssert = false)
        {
            GameObject find = go.Find(name, true);

            if (ignoreAssert == false)
                Debug.Assert(find != null, $"{go.name}의 {name} GameObject가 존재하지 않음");

            return find;
        }

        public static GameObject FindGameObject(this Transform tm, string name)
        {
            GameObject find = tm.Find(name, true);

            Debug.Assert(find != null, $"{tm.name}의 {name} GameObject가 존재하지 않음");

            return find;
        }

        public static T FindComponent<T>(this GameObject go, string name, bool ignoreAssert = false)
        {
            GameObject componentObj = go.FindGameObject(name, ignoreAssert);

            T component = componentObj.GetComponent<T>();

            if (ignoreAssert == false)
            {
                Debug.Assert(component != null, $"{go.name}의 {typeof(T).Name} {name}가 존재하지 않음");
            }

            return component;
        }

        public static T FindComponent<T>(this Component comp, string name)
        {
            GameObject componentObj = comp.gameObject.FindGameObject(name);

            T component = componentObj.GetComponent<T>();

            Debug.Assert(component != null, $"{comp.gameObject.name}의 {typeof(T).Name} {name}가 존재하지 않음");

            return component;
        }

        public static void AllChildObjectOff(this GameObject parent)
        {
            AllChildObjectOff(parent.transform);
        }

        public static void AllChildObjectOff(this Transform parent)
        {
            // 바로 아래 자식만 확인하고 불필요한 자식 Off
            foreach (Transform child in parent)
            {
                var layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement != null && layoutElement.ignoreLayout)
                    continue;

                child.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 이 코드를 실행하기 전에 한 프레임 기다려줘야함
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Texture2D CaptureTextureInRectTmArea(CanvasScaler cs, RectTransform rectTm)
        {
            // Canvas의 크기 가져오기
            Vector2 canvasSize = cs.referenceResolution;

            // UI 요소의 크기 가져오기
            Vector2 uiSize = rectTm.rect.size;

            // 화면의 크기 가져오기
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            // Canvas의 비율과 화면의 비율 중 작은 비율을 사용하여 Pixel Size 계산
            float scale = Mathf.Min(screenSize.x / canvasSize.x, screenSize.y / canvasSize.y);
            Vector2 pixelSize = uiSize * scale;

            int pixelWidth = (int)pixelSize.x;
            int pixelHeight = (int)pixelSize.y;

            Rect captureRect = new Rect(rectTm.position.x - (pixelWidth * 0.5f), rectTm.position.y - (pixelHeight * 0.5f), pixelWidth, pixelHeight);

            Texture2D captureTexture = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGB24, false);
            captureTexture.ReadPixels(captureRect, 0, 0);
            captureTexture.Apply();

            return captureTexture;
        }
    }

    public static class TextMeshProUGUIExt
    {
        public static void SetColor(this TextMeshProUGUI text, string color)
        {
            Color newColor = new Color();

            ColorUtility.TryParseHtmlString($"#{color}", out newColor);

            text.color = newColor;
        }

        public static void SetColor(this TextMeshProUGUI text, Color newColor)
        {
            text.color = newColor;
        }
    }

    public static class ImageExt
    {
        public static void SetColor(this Image image, string color)
        {
            Color newColor = new Color();

            ColorUtility.TryParseHtmlString($"#{color}", out newColor);

            image.color = newColor;
        }
    }

    public static class ButtonExt
    {
        public static void SetButtonAction(this Button button, UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                action?.Invoke();

                SoundPlayer.PlayUIButtonTouchSound();
            });
        }

        public static void SetActiveFrame(this Button button, bool isActive, string btnActive = "", string btnInactive = "", string textActive = "", string textInactive = "", string text = "")
        {
            btnActive = btnActive.IsValid() ? btnActive : Const.DefaultActiveButtonFrame;
            btnInactive = btnInactive.IsValid() ? btnInactive : Const.DefaultInactiveButtonFrame;

            textActive = textActive.IsValid() ? textActive : Const.DefaultActiveTextColor;
            textInactive = textInactive.IsValid() ? textInactive : Const.DefaultInactiveTextColor;

            string buttonName = isActive ? btnActive : btnInactive;
            string textColor = isActive ? textActive : textInactive;

            var imageFrame = button.GetComponent<Image>();
            imageFrame.sprite = Lance.Atlas.GetUISprite(buttonName);

            var textButtonNames = button.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach(var textButtonName in textButtonNames)
            {
                textButtonName.SetColor(textColor);
                textButtonName.text = text.IsValid() ? text : textButtonName.text;
            }
        }
    }

    public static class EventTriggerExt
    {
        public static void AddTriggerEvent(this EventTrigger trigger, EventTriggerType type, Action onEvent)
        {
            var triggerEvent = new EventTrigger.TriggerEvent();
            triggerEvent.AddListener((e) => onEvent());

            trigger.triggers.Add(new EventTrigger.Entry()
            {
                eventID = type,
                callback = triggerEvent,
            });
        }
    }

    public static class StringExt
    {
        public static bool IsValid(this string s)
        {
            return string.IsNullOrEmpty(s) == false;
        }

        public static bool IsValid(this ObscuredString s)
        {
            string ss = s;

            return ss.IsValid();
        }

        // delimited(구분 문자)의 줄임말, delim
        public static string[] SplitByDelim(this string s, char delim = ',')
        {
            IEnumerable<string> array = s.Split(delim).Select(x => x.Trim());

            return array.ToArray();
        }

        public static bool SplitWord(this string s, char word, out string first, out string second)
        {
            string[] splitResult = s.Split(word).Select(x => x.Trim()).ToArray();

            if (splitResult.Length <= 0 || splitResult.Length > 2)
            {
                first = string.Empty;
                second = string.Empty;

                return false;
            }
            else
            {
                first = splitResult[0];
                second = splitResult[1];

                return true;
            }
        }

        public static string ChangeToDamageFont(this string s, bool isCritical, bool isSuperCritical)
        {
            string result = string.Empty;

            for (int i = 0; i < s.Length; ++i)
            {
                result += GetDamageFontString(s[i], isCritical, isSuperCritical);
            }

            return result;

            string GetDamageFontString(char word, bool isCritical, bool isSuperCritical)
            {
                return isSuperCritical ? 
                        $"<sprite name=\"Font_Damage_Blue_{word}\">" : 
                        isCritical ? 
                        $"<sprite name=\"Font_Damage_Red_{word}\">" :
                        $"<sprite name=\"Font_Damage_Yellow_{word}\">";
            }
        }

        public static float ToFloatSafe(this string s, float defaultValue = 0)
        {
            if (s.IsValid())
            {
                float f;
                if (float.TryParse(s, out f))
                    return f;
            }

            return defaultValue;
        }

        public static double ToDoubleSafe(this string s, double defaultValue = 0)
        {
            if (s.IsValid())
            {
                double d;
                if (double.TryParse(s, out d))
                    return d;
            }

            return defaultValue;
        }

        public static int ToIntSafe(this string s, int defaultValue = 0)
        {
            if (s.IsValid())
            {
                int i;
                if (int.TryParse(s, out i))
                    return i;
            }

            return defaultValue;
        }
    }

    public static class TimeUtil
    {
        static DateTime mPivot = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public const int SecondsInMinute = 60;
        public const int SecondsInHour = 60 * 60;
        public const int SecondsInDay = 24 * SecondsInHour;
        public const int SecondsInWeek = 7 * SecondsInDay;
        public const int MinutesInHour = 60;
        public const int MinutesInDay = 24 * 60;

        static int mSyncOffset;

        public static int Now
        {
            get
            {
                return LocalNow + mSyncOffset;
            }
        }

        public static int LocalNow
        {
            get
            {
                return (int)((DateTime.UtcNow - mPivot).TotalSeconds);
            }
        }

        public static DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow.AddSeconds(mSyncOffset);
            }
        }

        public static void SyncTo(DateTime serverUtcNow)
        {
            int serverNow = (int)(serverUtcNow - mPivot).TotalSeconds;

            SyncTo(serverNow);
        }

        public static void SyncTo(int serverNow)
        {
#if UNITY_EDITOR
            return;
#endif
            mSyncOffset = serverNow - LocalNow;
        }

        public static DayOfWeek NowDayOfWeek(int timeZone = 9)
        {
            DateTime now = UtcNow.AddHours(timeZone);

            return now.DayOfWeek;
        }

        public static (int day, int hour, int min, int sec) SplitTime2(int second)
        {
            if (second < 0)
                second = 0;

            int day = second / SecondsInDay;
            second -= day * SecondsInDay;

            int hour = second / SecondsInHour;
            second -= hour * SecondsInHour;

            int min = second / SecondsInMinute;
            second -= min * SecondsInMinute;

            int sec = second;

            return (day, hour, min, sec);
        }

        public static (int hour, int min, int sec) SplitTime(int second)
        {
            if (second < 0)
                second = 0;

            int hour = second / SecondsInHour;
            second -= hour * SecondsInHour;

            int min = second / SecondsInMinute;
            second -= min * SecondsInMinute;

            int sec = second;

            return (hour, min, sec);
        }

        public static int GetNow(int timeZone)
        {
            return (int)((UtcNow.AddHours(timeZone) - mPivot).TotalSeconds);
        }

        public static int GetTotalSeconds(DateTime dateTime)
        {
            return (int)((dateTime - mPivot).TotalSeconds);
        }

        public static DateTime GetDateTime(int totalSeconds)
        {
            return mPivot.AddSeconds(totalSeconds);
        }

        public static string GetTimeStr(int second, bool ignoreHour = false)
        {
            var result = SplitTime(second);
            
            if (ignoreHour == false)
            {
                return $"{result.hour:00}:{result.min:00}:{result.sec:00}";
            }
            else
            {
                return $"{result.min:00}:{result.sec:00}";
            }
        }

        public static string GetTimeStr(int dateNum)
        {
            var result = SplitDateNum(dateNum);

            return $"{result.year:0000}/{result.month:00}/{result.day:00}";
        }

        static int CalcDateNum(DateTime date)
        {
            int year = date.Year * 10000;
            int month = date.Month * 100;
            int day = date.Day;

            // 2068 01 19
            // YYYY/MM/DD
            return year + month + day;
        }

        public static int NowDateNum(int timeZone = 9)
        {
            DateTime now = UtcNow.AddHours(timeZone);

            return CalcDateNum(now);
        }

        public static int RankNowDateNum(int timeZone = 9)
        {
            // 오전 6시를 기준으로 갱신하자
            DateTime now = UtcNow.AddHours(timeZone);

            now = now.AddHours(-6);

            return CalcDateNum(now);
        }

        public static int EndDateNum(int durationDay, int timeZone = 9)
        {
            DateTime now = UtcNow.AddHours(timeZone);
            DateTime end = now.AddDays(durationDay);

            return CalcDateNum(end);
        }

        public static bool IsActiveDateNum(int startDateNum, int endDateNum)
        {
            int nowDateNum = NowDateNum();

            if (startDateNum <= nowDateNum &&
                nowDateNum <= endDateNum)
            {
                return true;
            }

            return false;
        }

        public static int ThisWeekStartDateNum(int hourOffset = 9)
        {
            DateTime now = UtcNow.AddHours(hourOffset);
            DayOfWeek nowDayOfWeek = now.DayOfWeek;

            // 월요일을 기준으로 계산하자
            int thisWeekGap = (int)nowDayOfWeek - (int)DayOfWeek.Monday;

            DateTime thisWeek = now.AddDays(-thisWeekGap);

            return CalcDateNum(thisWeek);
        }

        public static int GetWeeklyUpdateRemainTime()
        {
            return (int)GetTimeUntilNextMonday4AM().TotalSeconds;
        }

        static TimeSpan GetTimeUntilNextMonday4AM()
        {
            DateTime now = UtcNow.AddHours(9);

            // 현재 요일을 구함 (일요일: 0, 월요일: 1, ..., 토요일: 6)
            int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;

            // 다음 주 월요일 오전 4시의 날짜와 시간을 계산
            DateTime nextMonday4AM = now.Date.AddDays(daysUntilNextMonday).AddHours(4);

            // 현재 시간이 월요일 오전 4시 이후라면, 다음 주 월요일 오전 4시로 설정
            if (now > nextMonday4AM)
            {
                nextMonday4AM = nextMonday4AM.AddDays(7);
            }

            // 다음 주 월요일 오전 4시까지 남은 시간 계산
            TimeSpan timeUntilNextMonday4AM = nextMonday4AM - now;

            return timeUntilNextMonday4AM;
        }

        public static int NextWeekDateNum(int hourOffset = 9)
        {
            DateTime now = UtcNow.AddHours(hourOffset);
            DayOfWeek nowDayOfWeek = now.DayOfWeek;

            // 다음 주 월요일을 기준으로 계산하자
            int dayOfWeekToInt = (int)nowDayOfWeek - 1;
            if (dayOfWeekToInt < 0)
                dayOfWeekToInt = 6;

            int nextWeekGap = 7 - dayOfWeekToInt;

            DateTime nextWeek = now.AddDays(nextWeekGap).Date;

            return CalcDateNum(nextWeek);
        }

        public static int RemainSecondsToNextWeek(int hourOffset = 9)
        {
            DateTime now = UtcNow.AddHours(hourOffset);
            DayOfWeek nowDayOfWeek = now.DayOfWeek;

            // 다음 주 월요일을 기준으로 계산하자
            int dayOfWeekToInt = (int)nowDayOfWeek - 1;
            if (dayOfWeekToInt < 0)
                dayOfWeekToInt = 6;

            int dayOfWeekGap = 7 - dayOfWeekToInt;

            DateTime nextWeek = now.AddDays(dayOfWeekGap).Date;

            return (int)((nextWeek - now).TotalSeconds);
        }

        public static int RemainSecondsToNextDay(int hourOffset = 9)
        {
            DateTime now = UtcNow.AddHours(hourOffset);
            DateTime tomorrow = now.AddDays(1).Date;

            return (int)((tomorrow - now).TotalSeconds);
        }

        public static int RemainSecondsToDateNum(int endDateNum, int hourOffset = 9)
        {
            DateTime now = UtcNow.AddHours(hourOffset);

            var splitResult = SplitDateNum(endDateNum);
            DateTime end = new DateTime(splitResult.year, splitResult.month, splitResult.day);

            return (int)((end - now).TotalSeconds);
        }

        // 오전 5시를 기준으로 생각
        static DateTime mRankDayCountPivot = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        public static int GetRankTotalDayCount(int hourOffset = 9)
        {
            DateTime now = UtcNow.AddHours(hourOffset);

            return (int)(now - mRankDayCountPivot).TotalDays;
        }

        public static bool IsWeekend(int hourOffset = 9)
        {
            DateTime now = UtcNow.AddHours(hourOffset);

            return now.DayOfWeek == Lance.GameData.WeekendDoubleData.startDayOfWeek ||
                now.DayOfWeek == Lance.GameData.WeekendDoubleData.endDayOfWeek;
        }

        public static (int year, int month, int day) SplitDateNum(int dateNum)
        {
            int year = dateNum / 10000;
            int month = (dateNum % 10000) / 100;
            int day = (dateNum % 10000) % 100;

            return (year, month, day);
        }

        public static (int year, int month, int day) SplitNowDateNum()
        {
            int nowDateNum = NowDateNum();

            int year = nowDateNum / 10000;
            int month = (nowDateNum % 10000) / 100;
            int day = (nowDateNum % 10000) % 100;

            return (year, month, day);
        }
    }

    public static class JsonUtil
    {
        // ======== Newton Json ========

        public static string ToJsonNewton(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static object FromJsonNewton(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public static T Base64FromJsonNewton<T>(string base64)
        {
            string json = Base64StringToJsonNewton(base64);

            return (T)FromJsonNewton(json, typeof(T));
        }

        public static string JsonToBase64StringNewton(string json)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(json);

            string base64 = Convert.ToBase64String(utf8);

            return base64;
        }

        public static string Base64StringToJsonNewton(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);

            string json = Encoding.UTF8.GetString(bytes);

            return json;
        }

        // ======== Unity Json ========

        public static string ToJsonUnity(object obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public static T FromJsonUnity<T>(string json)
        {
            return (T)JsonUtility.FromJson(json, typeof(T));
        }

        public static T Base64FromJsonUnity<T>(string base64)
        {
            string json = Base64StringToJson(base64);

            return FromJsonUnity<T>(json);
        }

        public static string JsonToBase64String(string json)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(json);

            string base64 = Convert.ToBase64String(utf8);

            return base64;
        }

        public static string Base64StringToJson(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);

            string json = Encoding.UTF8.GetString(bytes);

            return json;
        }
    }

    public static class BadWordChecker
    {
        public static bool HaveBadWord(string msg)
        {
            foreach(var badWord in Lance.GameData.BadWord)
            {
                if (msg.IndexOf(badWord.word) != -1)
                    return true;
            }

            return false;
        }
    }
}