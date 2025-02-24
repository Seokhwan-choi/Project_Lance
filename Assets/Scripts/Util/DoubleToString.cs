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
    // ���ϰ� ��������ִ� �ڵ尡 �־ �׳� �ܾ�Դ�.
    // ��ó: https://forestj.tistory.com/102
    static class DoubleToString
    {
        static readonly string[] CurrencyUnits = new string[] { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at", "au", "av", "aw", "ax", "ay", "az", "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu", "bv", "bw", "bx", "by", "bz", "ca", "cb", "cc", "cd", "ce", "cf", "cg", "ch", "ci", "cj", "ck", "cl", "cm", "cn", "co", "cp", "cq", "cr", "cs", "ct", "cu", "cv", "cw", "cx", };
        /// <summary>
        /// double �� �����͸� ���ĺ� ������ ����
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

            //  ��ȣ ��� ���ڿ�
            string significant = (value < 0) ? "-" : string.Empty;

            //  ������ ����
            string showNumber = string.Empty;
            //  ���� ���ڿ�
            string unityString = string.Empty;

            //  ������ �ܼ�ȭ ��Ű�� ���� ������ ���� ǥ�������� ������ �� ó��
            string[] partsSplit = value.ToString("E").Split('+');

            //  ����
            if (partsSplit.Length < 2)
            {
                return zero;
            }

            //  ���� (�ڸ��� ǥ��)
            if (!int.TryParse(partsSplit[1], out int exponent))
            {
                Debug.LogWarningFormat("Failed - ToCurrentString({0}) : partSplit[1] = {1}", value, partsSplit[1]);
                return zero;
            }

            //  ���� ���ڿ� �ε���
            int quotient = exponent / 3;

            //  �������� ������ �ڸ��� ��꿡 ���(10�� �ŵ������� ���)
            int remainder = exponent % 3;

            //  1A �̸��� �׳� ǥ��
            if (exponent < 3)
            {
                showNumber = Math.Truncate(value).ToString();
            }
            else
            {
                //  10�� �ŵ������� ���ؼ� �ڸ��� ǥ������ ����� �ش�.
                double temp = double.Parse(partsSplit[0].Replace("E", "")) * Math.Pow(10, remainder);

                //  �Ҽ� ��°�ڸ������� ����Ѵ�.
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
