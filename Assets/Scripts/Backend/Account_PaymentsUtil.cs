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

            // ��Ű�� ����
            totalPayments += PackageShop.CalcTotalPayments();

            // �� ����
            totalPayments += Shop.CalcTotalPayments();

            // �ڽ�Ƭ ����
            totalPayments += Costume.CalcTotalPayments();

            // �н� ����
            totalPayments += Pass.CalcTotalPayments();

            // �̺�Ʈ �н� ����
            totalPayments += Event.CalcTotalPayments();

            return totalPayments;
        }
    }
}