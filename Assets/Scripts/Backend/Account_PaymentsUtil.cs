using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    public partial class Account
    {
        public int CalcTotalPayments()
        {
            int totalPayments = 0;

            // 패키지 구매
            totalPayments += PackageShop.CalcTotalPayments();

            // 잼 구매
            totalPayments += Shop.CalcTotalPayments();

            // 코스튬 구매
            totalPayments += Costume.CalcTotalPayments();

            // 패스 구매
            totalPayments += Pass.CalcTotalPayments();

            // 이벤트 패스 구매
            totalPayments += Event.CalcTotalPayments();

            return totalPayments;
        }
    }
}