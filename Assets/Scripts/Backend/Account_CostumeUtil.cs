using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Lance
{
    public partial class Account
    {
        public int GetCostumeLevel(string id)
        {
            var costumeInst = Costume.GetCostume(id);
            if (costumeInst == null)
                return 0;

            return costumeInst.GetLevel();
        }

        public bool IsMaxLevelCostume(string id)
        {
            var costumeInst = Costume.GetCostume(id);
            if (costumeInst == null)
                return false;

            return costumeInst.IsMaxLevel();
        }

        public Grade GetCostumeGrade(string id)
        {
            var costumeInst = Costume.GetCostume(id);
            if (costumeInst == null)
                return Grade.B;

            return costumeInst.GetGrade();
        }

        public double GetCostumeUpgradeRequireStones(string id)
        {
            var costumeInst = Costume.GetCostume(id);
            if (costumeInst == null)
                return double.MaxValue;

            return costumeInst.GetUpgradeRequireStone();
        }

        public double GetCostumeUpgradeRequire(string id)
        {
            var costumeInst = Costume.GetCostume(id);
            if (costumeInst == null)
                return double.MaxValue;

            return costumeInst.GetUpgradeRequire();
        }

        public bool CanUpgradeCostume(string id)
        {
            // �κ��丮 ����
            CostumeInst costume = Costume.GetCostume(id);
            if (costume == null)
                return false;

            return CanUpgradeCostume(costume);
        }

        public bool CanUpgradeCostume(CostumeInst costume)
        {
            if (costume.IsMaxLevel())
                return false;

            double requireStones = costume.GetUpgradeRequireStone();
            if (IsEnoughUpgradeStones(requireStones) == false)
                return false;

            double require = costume.GetUpgradeRequire();
            if (IsEnoughCostumeUpgrade(require) == false)
                return false;

            return true;
        }

        public void UpgradeCostume(string id)
        {
            // �κ��丮�� �ִ� ��� ��ȭ ����
            CostumeInst costume = Costume.GetCostume(id);
            if (costume == null || costume.IsMaxLevel())
                return;

            double requireStones = costume.GetUpgradeRequireStone();
            if (IsEnoughUpgradeStones(requireStones) == false)
                return;

            double require = costume.GetUpgradeRequire();
            if (IsEnoughCostumeUpgrade(require) == false)
                return;

            // �ڽ�Ƭ ������
            costume.LevelUp();

            // ��ȭ�� ���
            UseUpgradeStones(requireStones);

            // ��Ÿ�� ���
            UseCostumeUpgrade(require);

            Costume.SetIsChangedData(true);
        }

    }
}
