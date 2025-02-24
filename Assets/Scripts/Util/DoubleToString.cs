using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    enum ShowDecimalPoint
    {
        Default,
        GoldTrain,
    }
    // 편하게 만들어져있는 코드가 있어서 그냥 긁어왔다.
    // 출처: https://forestj.tistory.com/102
    static class DoubleToString
    {
        static readonly string[] CurrencyUnits = new string[] { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at", "au", "av", "aw", "ax", "ay", "az", "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu", "bv", "bw", "bx", "by", "bz", "ca", "cb", "cc", "cd", "ce", "cf", "cg", "ch", "ci", "cj", "ck", "cl", "cm", "cn", "co", "cp", "cq", "cr", "cs", "ct", "cu", "cv", "cw", "cx", };
        /// <summary>
        /// double 형 데이터를 알파벳 단위로 변경
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToAlphaString(this double value, ShowDecimalPoint showDp = ShowDecimalPoint.Default)
        {
            string zero = "0";

            if (-1d < value && value < 1d)
            {
                return zero;
            }

            if (double.IsInfinity(value))
            {
                return "Infinity";
            }

            //  부호 출력 문자열
            string significant = (value < 0) ? "-" : string.Empty;

            //  보여줄 숫자
            string showNumber = string.Empty;
            //  단위 문자열
            string unityString = string.Empty;

            //  패턴을 단순화 시키기 위해 무조건 지수 표현식으로 변경한 후 처리
            string[] partsSplit = value.ToString("E").Split('+');

            //  예외
            if (partsSplit.Length < 2)
            {
                return zero;
            }

            //  지수 (자릿수 표현)
            if (!int.TryParse(partsSplit[1], out int exponent))
            {
                Debug.LogWarningFormat("Failed - ToCurrentString({0}) : partSplit[1] = {1}", value, partsSplit[1]);
                return zero;
            }

            //  몫은 문자열 인덱스
            int quotient = exponent / 3;

            //  나머지는 정수부 자릿수 계산에 사용(10의 거듭제곱을 사용)
            int remainder = exponent % 3;

            //  1A 미만은 그냥 표현
            if (exponent < 3)
            {
                showNumber = Math.Truncate(value).ToString();
            }
            else
            {
                //  10의 거듭제곱을 구해서 자릿수 표현값을 만들어 준다.
                double temp = double.Parse(partsSplit[0].Replace("E", "")) * Math.Pow(10, remainder);

                //  소수 둘째자리까지만 출력한다.
                if (showDp == ShowDecimalPoint.Default)
                {
                    showNumber = $"{temp:F2}";
                }
                else
                {
                    showNumber = $"{temp:F5}";
                }
            }

            unityString = CurrencyUnits[quotient];

            return $"{significant}{showNumber}{unityString}";
        }
    }
}
