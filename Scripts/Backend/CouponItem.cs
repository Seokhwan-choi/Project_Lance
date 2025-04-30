using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    class CouponItem
    {
        public string Uuid; // 쿠폰 아이디
        public string Title; // 쿠폰 명
        public string Type;// 시리얼/구버전
        public string Version; // 신규버전인지
        public string Redundant; // 중복가능한 쿠폰인지
    }
}