using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System.Collections.Generic;

namespace Lance
{
    class Popup_CostumeInfoUI : PopupBase
    {
        string mId;
        TextMeshProUGUI mTextMyUpgradeStone;
        TextMeshProUGUI mTextMyCosumeUpgrade;
        GameObject mCostumeObj;
        Button mButtonUpgrade;
        TextMeshProUGUI mTextRequireUpgradeStone;
        TextMeshProUGUI mTextRequireUpgrade;

        StatValueInfoUI mLevelValueInfo;
        List<StatValueInfoUI> mOwnStatValueInfo;
        public void Init(string id)
        {
            mId = id;

            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_CostumeUpgrade"));

            mTextMyUpgradeStone = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradeStoneAmount");
            mTextMyCosumeUpgrade = gameObject.FindComponent<TextMeshProUGUI>("Text_CostumeUpgradeAmount");
            mCostumeObj = gameObject.FindGameObject("Costume");

            var costumeData = DataUtil.GetCostumeData(id);

            InitCostumeStaticInfo(costumeData);

            var levelValueInfoObj = gameObject.FindGameObject("LevelValueInfo");

            mLevelValueInfo = levelValueInfoObj.GetOrAddComponent<StatValueInfoUI>();
            mLevelValueInfo.InitCostume(id);

            mOwnStatValueInfo = new List<StatValueInfoUI>();
            var ownValueInfosObj = gameObject.FindGameObject("OwnValueInfos");

            for (int i = 0; i < costumeData.ownStats.Length; ++i)
            {
                int index = i + 1;

                var statValueInfoObj = ownValueInfosObj.FindGameObject($"StatValueInfo_{index}");
                var valueInfo = statValueInfoObj.GetOrAddComponent<StatValueInfoUI>();
                valueInfo.InitCostume(id);

                string ownStatId = costumeData.ownStats[i];

                valueInfo.SetActive(ownStatId.IsValid());
                valueInfo.SetOwnStatId(ownStatId);

                mOwnStatValueInfo.Add(valueInfo);
            }

            var buttonInfo = gameObject.FindComponent<Button>("Button_Info");
            buttonInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_DescUI>();
                popup.Init(StringTableUtil.Get("Title_CostumeUpgrade"), StringTableUtil.GetDesc("CostumeUpgrade"));
            });

            mButtonUpgrade = gameObject.FindComponent<Button>("Button_Upgrade");
            mButtonUpgrade.SetButtonAction(OnUpgradeButton);

            mTextRequireUpgradeStone = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradeStonePrice");
            mTextRequireUpgrade = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradePrice");

            Refresh();
        }

        public override void OnClose()
        {
            base.OnClose();

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
        }

        void OnUpgradeButton()
        {
            if (Lance.GameManager.UpgradeCostume(mId))
            {
                SoundPlayer.PlaySkillLevelUp();

                Lance.ParticleManager.AquireUI("SkillLevelUp", mCostumeObj.GetComponent<RectTransform>());

                Refresh();

                Param param = new Param();
                param.Add("id", mId);
                param.Add("remainUpgradeStone", Lance.Account.Currency.GetUpgradeStones());
                param.Add("remainCostumeUpgrade", Lance.Account.Currency.GetCostumeUpgrade());

                Lance.BackEnd.InsertLog("UpgradeCostume", param, 7);

                Lance.GameManager.UpdatePlayerStat();
            }
        }

        public void Refresh()
        {
            mTextMyUpgradeStone.text = Lance.Account.Currency.GetUpgradeStones().ToAlphaString();
            mTextMyCosumeUpgrade.text = Lance.Account.Currency.GetCostumeUpgrade().ToAlphaString();

            mLevelValueInfo.Refresh();

            foreach(var ownStatValueInfo in mOwnStatValueInfo)
                ownStatValueInfo.Refresh();

            mButtonUpgrade.SetActiveFrame(Lance.Account.CanUpgradeCostume(mId));

            bool isMaxLevel = Lance.Account.IsMaxLevelCostume(mId);
            double requireStones = Lance.Account.GetCostumeUpgradeRequireStones(mId);
            bool isEnoughRequireStones = Lance.Account.IsEnoughUpgradeStones(requireStones) && isMaxLevel == false;
            mTextRequireUpgradeStone.text = $"{(isMaxLevel ? "0" : requireStones.ToAlphaString())}";
            mTextRequireUpgradeStone.SetColor(isEnoughRequireStones ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            double require = Lance.Account.GetCostumeUpgradeRequire(mId);
            bool isEnoughRequireCostumeUpgrade = Lance.Account.IsEnoughCostumeUpgrade(require) && isMaxLevel == false;
            mTextRequireUpgrade.text = $"{(isMaxLevel ? "0" : require.ToAlphaString())}";
            mTextRequireUpgrade.SetColor(isEnoughRequireCostumeUpgrade ? Const.EnoughTextColor : Const.NotEnoughTextColor);
        }

        void InitCostumeStaticInfo(CostumeData costumeData)
        {
            var imageCostumeBody = gameObject.FindComponent<Image>("Image_CostumeBody");
            imageCostumeBody.gameObject.SetActive(costumeData.type == CostumeType.Body);
            var imageCostumeWeapon = gameObject.FindComponent<Image>("Image_CostumeWeapon");
            imageCostumeWeapon.gameObject.SetActive(costumeData.type == CostumeType.Weapon);
            var imageCostumeEtc = gameObject.FindComponent<Image>("Image_CostumeEtc");
            imageCostumeEtc.gameObject.SetActive(costumeData.type == CostumeType.Etc);

            var imageGrade = gameObject.FindComponent<Image>("Image_Grade");
            imageGrade.sprite = Lance.Atlas.GetIconGrade(costumeData.grade);

            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_CostumeName");
            textName.text = StringTableUtil.GetName(costumeData.id);

            var textLevelInfo = gameObject.FindComponent<TextMeshProUGUI>("Text_LevelInfo");

            StringParam maxLevelParam = new StringParam("maxLevel", costumeData.maxLevel);

            textLevelInfo.text = StringTableUtil.Get("UIString_LevelInfo", maxLevelParam);

            if (costumeData.type == CostumeType.Body)
            {
                imageCostumeBody.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
            }
            else if (costumeData.type == CostumeType.Weapon)
            {
                imageCostumeWeapon.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
            }
            else
            {
                imageCostumeEtc.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
            }
        }
    }
}