using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;


namespace Lance
{
    //class Inventory_EquipmentInfoUI : MonoBehaviour
    //{
    //    string mId;
    //    InventoryTabUI mParent;
    //    Inventory_EquipmentItemUI mEquipmentItemUI;

    //    TextMeshProUGUI mTextName; // ��� �̸�
    //    TextMeshProUGUI mTextUpgradePrice; // ��� ��ȭ ���

    //    Equipment_StatValueInfo mEquipStatValueInfo;        // ���� ȿ��
    //    List<Equipment_StatValueInfo> mOwnStatValueInfos;   // ���� ȿ��

    //    public string Id => mId;
    //    public void Init(InventoryTabUI parent)
    //    {
    //        mId = string.Empty;
    //        mParent = parent;

    //        var skillItemObj = gameObject.FindGameObject("Equipment");

    //        mEquipmentItemUI = skillItemObj.GetOrAddComponent<Inventory_EquipmentItemUI>();
    //        mEquipmentItemUI.Init();
    //        mEquipmentItemUI.SetSelected(false);

    //        // ��ư �ʱ�ȭ
    //        GameObject buttonsObj = gameObject.FindGameObject("Buttons");
    //        Button buttonUpgrade = buttonsObj.FindComponent<Button>("Button_Upgrade");
    //        buttonUpgrade.SetButtonAction(OnUpgradeButton);
    //        Button buttonEquip = buttonsObj.FindComponent<Button>("Button_Equip");
    //        buttonEquip.SetButtonAction(OnEquipButton);
    //        Button buttonClose = gameObject.FindComponent<Button>("Button_Close");
    //        buttonClose.SetButtonAction(OnCloseButton);

    //        mTextName = gameObject.FindComponent<TextMeshProUGUI>("Text_EquipmentName");
    //        mTextUpgradePrice = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradePrice");

    //        // ���� ȿ��
    //        GameObject equipValueInfoObj = gameObject.FindGameObject("EquipValueInfo");
    //        GameObject statValueInfoObj = equipValueInfoObj.FindGameObject("ValueInfo");
    //        mEquipStatValueInfo = statValueInfoObj.GetOrAddComponent<Equipment_StatValueInfo>();
    //        mEquipStatValueInfo.Init();

    //        // ���� ȿ��
    //        mOwnStatValueInfos = new List<Equipment_StatValueInfo>();
    //        GameObject ownValueInfoObj = gameObject.FindGameObject("OwnValueInfo");
    //        GameObject valueInfosObj = ownValueInfoObj.FindGameObject("ValueInfos");

    //        for(int i = 0; i < Lance.GameData.EquipmentCommonData.ownOptionMaxCount; ++i)
    //        {
    //            int index = i + 1;

    //            statValueInfoObj = valueInfosObj.FindGameObject($"StatValueInfo_{index}");

    //            var statValueInfo = statValueInfoObj.GetOrAddComponent<Equipment_StatValueInfo>();
    //            statValueInfo.Init();

    //            mOwnStatValueInfos.Add(statValueInfo);
    //        }

    //        gameObject.SetActive(false);
    //    }

    //    public void ChangeInfo(string equipmentId)
    //    {
    //        gameObject.SetActive(true);

    //        mId = equipmentId;

    //        Refresh();
    //    }

    //    public void Refresh()
    //    {
    //        if (mId.IsValid() == false)
    //            return;

    //        EquipmentData data = DataUtil.GetEquipmentData(mId);
    //        if (data == null)
    //            return;

    //        // ��� ������ ������ ����
    //        mEquipmentItemUI.SetId(mId);

    //        // To Do : ��� �̸�
    //        mTextName.text = mId;

    //        // ��ȭ ���
    //        double requireStones = Lance.Account.GetEquipmentUpgradeRequireStones(mId);
    //        bool isEnoughStones = Lance.Account.IsEnoughUpgradeStones(requireStones);
    //        mTextUpgradePrice.text = UIUtil.GetSatisfiedColorString(isEnoughStones, requireStones);

    //        // ��� ȿ��
    //        mEquipStatValueInfo.SetEquipmentId(mId);
    //        mEquipStatValueInfo.Refresh();

    //        for(int i = 0; i < data.ownStats.Length; ++i)
    //        {
    //            if (mOwnStatValueInfos.Count <= i)
    //                continue;

    //            var valueInfo = mOwnStatValueInfos[i];
    //            string ownStatId = data.ownStats[i];
    //            if (ownStatId.IsValid())
    //            {
    //                var ownStatData = Lance.GameData.OwnStatData.TryGet(ownStatId);
    //                if (ownStatData != null)
    //                {
    //                    valueInfo.SetActive(true);
    //                    valueInfo.SetEquipmentId(mId);
    //                    valueInfo.SetOwnStatId(ownStatId);
    //                    valueInfo.Refresh();
    //                }
    //            }
    //            else
    //            {
    //                valueInfo.SetActive(false);
    //            }
    //        }
    //    }

    //    void OnUpgradeButton()
    //    {
    //        if (mId.IsValid() == false)
    //            return;

    //        if (Lance.Account.HaveEquipment(mId) == false)
    //            return;

    //        var equipItem = Lance.Account.GetEquipment(mId);
    //        if (equipItem == null)
    //            return;

    //        double requireStones = equipItem.GetUpgradeRequireStones();
    //        if (Lance.Account.IsEnoughUpgradeStones(requireStones) == false)
    //            return;

    //        Lance.Account.UpgradeEquipment(mId);

    //        // To Do : ��ȭ �Ϸ� UI�� ���� ǥ�� & ����

    //        mParent.Refresh();
    //    }

    //    void OnCombineButton()
    //    {
    //        if (mId.IsValid() == false)
    //            return;

    //        if (Lance.Account.HaveEquipment(mId) == false)
    //            return;

    //        (string id, int combineCount) result = Lance.Account.CombineEquipment(mId);

    //        // To Do : �ռ� ����� ���� �������� & �Ҹ�

    //        mParent.Refresh();
    //    }

    //    void OnEquipButton()
    //    {
    //        if (mId.IsValid() == false)
    //            return;

    //        if (Lance.Account.HaveEquipment(mId) == false)
    //            return;

    //        if (Lance.Account.EquipEquipment(mId) == false)
    //            return;

    //        // To Do : ���� �Ϸ� �Ҹ� ?

    //        mParent.Refresh();
    //    }

    //    void OnCloseButton()
    //    {
    //        mId = string.Empty;

    //        gameObject.SetActive(false);

    //        mParent.RefreshItemSelelected();
    //    }
    //}

    //class Equipment_StatValueInfo : MonoBehaviour
    //{
    //    string mEquipmentId;
    //    string mOwnStatId;

    //    GameObject mLevelObj;
    //    GameObject mMaxLevelObj;
    //    TextMeshProUGUI mTextStatName;
    //    TextMeshProUGUI mTextCurrentValue;
    //    TextMeshProUGUI mTextNextValue;
    //    TextMeshProUGUI mTextMaxValue;

    //    public void Init()
    //    {
    //        InternalInit();
    //    }

    //    public void SetEquipmentId(string equipmentId)
    //    {
    //        mEquipmentId = equipmentId;
    //    }

    //    public void SetOwnStatId(string id)
    //    {
    //        mOwnStatId = id;
    //    }

    //    void InternalInit()
    //    {
    //        mLevelObj = gameObject.FindGameObject("Level");
    //        mMaxLevelObj = gameObject.FindGameObject("MaxLevel");

    //        mTextStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
    //        mTextCurrentValue = mLevelObj.FindComponent<TextMeshProUGUI>("Text_CurrentValue");
    //        mTextNextValue = mLevelObj.FindComponent<TextMeshProUGUI>("Text_NextValue");
    //        mTextMaxValue = mMaxLevelObj.FindComponent<TextMeshProUGUI>("Text_MaxValue");
    //    }

    //    public void SetActive(bool active)
    //    {
    //        gameObject.SetActive(active);
    //    }

    //    public void Refresh()
    //    {
    //        EquipmentData equipmentData = DataUtil.GetEquipmentData(mEquipmentId);
            
    //        if (equipmentData != null)
    //        {
    //            int level = Lance.Account.GetEquipmentLevel(mEquipmentId);
    //            int nextLevel = level + 1;
    //            int maxLevel = equipmentData.maxLevel;

    //            bool isMaxlevel = level == maxLevel;

    //            mLevelObj.SetActive(isMaxlevel == false);
    //            mMaxLevelObj.SetActive(isMaxlevel);
                
    //            double value = 0;
    //            double nextValue = 0;
    //            StatType statType = StatType.Atk;
    //            bool isPercentType = false;

    //            string ownStatId = mOwnStatId;
    //            if (ownStatId.IsValid())
    //            {
    //                OwnStatData ownStatData = Lance.GameData.OwnStatData.TryGet(ownStatId);
    //                if (ownStatData != null)
    //                {
    //                    statType = ownStatData.valueType;
    //                    isPercentType = statType.IsPercentType();
    //                    value = ownStatData.baseValue + (level * ownStatData.levelUpValue);
    //                    nextValue = ownStatData.baseValue + (nextLevel * ownStatData.levelUpValue);
    //                }
    //            }
    //            else
    //            {
    //                statType = equipmentData.valueType;
    //                isPercentType = statType.IsPercentType();

    //                double baseValue = equipmentData.baseValue;

    //                value = baseValue + ((baseValue * equipmentData.levelUpValue) * level);
    //                nextValue = baseValue + ((baseValue * equipmentData.levelUpValue) * nextLevel);
    //            }

    //            mTextStatName.text = $"{statType}";
    //            mTextMaxValue.text = isPercentType ? $"{value * 100:F3}%" : $"{value.ToAlphaString()}";
    //            mTextCurrentValue.text = isPercentType ? $"{value * 100:F3}%" : $"{value.ToAlphaString()}";
    //            mTextNextValue.text = isPercentType ? $"{nextValue * 100:F3}%" : $"{nextValue.ToAlphaString()}";
    //        }
    //    }
    //}
}