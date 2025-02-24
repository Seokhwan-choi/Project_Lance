using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using UnityEngine.EventSystems;


namespace Lance
{
    class ExcaliburForceItemUI : MonoBehaviour
    {
        ExcaliburForceType mType;

        Image mImageForceIcon;
        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextMaxLevel;
        TextMeshProUGUI mTextCurLevelValue;
        TextMeshProUGUI mTextNextLevelValue;
        TextMeshProUGUI mTextMaxLevelValue;
        TextMeshProUGUI mTextUpgradeRequire;

        Button mButtonUpgrade;
        GameObject mUpgradeRedDot;
        GameObject mNextLevelValueObj;
        GameObject mMaxLevelObj;
        ExcaliburData mData;
        Artifact_ExcaliburTabUI mParent;

        const float IntervalTime = 0.1f;
        const float LogTime = 1f;
        float mIntervalTime;
        float mLogTime;
        int mLogStacker;
        bool mIsPress;

        public ExcaliburForceType Type => mType;
        public void Init(Artifact_ExcaliburTabUI parent, ExcaliburForceType type)
        {
            mParent = parent;
            mType = type;
            mData = Lance.GameData.ExcaliburData.TryGet(type);
            mNextLevelValueObj = gameObject.FindGameObject("NextLevel");
            mMaxLevelObj = gameObject.FindGameObject("MaxLevel");

            var upgradeObj = gameObject.FindGameObject("Upgrade");

            mImageForceIcon = gameObject.FindComponent<Image>("Image_ForceIcon");
            mImageForceIcon.sprite = Lance.Atlas.GetItemSlotUISprite(mData.sprite);

            mButtonUpgrade = upgradeObj.FindComponent<Button>("Button_Upgrade");
            var eventTrigger = mButtonUpgrade.GetOrAddComponent<EventTrigger>();
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerDown, () => OnUpgradeButton(isPress: true));
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerUp, () => OnUpgradeButton(isPress: false));

            mUpgradeRedDot = upgradeObj.FindGameObject("RedDot");
            mTextUpgradeRequire = upgradeObj.FindComponent<TextMeshProUGUI>("Text_Require");
            var textStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.Get($"Name_{mData.valueType}");

            mTextCurLevelValue = mNextLevelValueObj.FindComponent<TextMeshProUGUI>("Text_CurrentLevel_Value");
            mTextNextLevelValue = mNextLevelValueObj.FindComponent<TextMeshProUGUI>("Text_NextLevel_Value");
            mTextMaxLevelValue = mMaxLevelObj.FindComponent<TextMeshProUGUI>("Text_MaxLevelValue");

            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mTextMaxLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_MaxLevel");

            Refresh();
        }

        public void OnTabLeave()
        {
            if (mLogStacker > 0)
            {
                InsertLog();
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (mIsPress)
            {
                mLogTime = LogTime;
                mIntervalTime -= dt;
                if (mIntervalTime <= 0f)
                {
                    mIntervalTime = IntervalTime;

                    OnUpgrade();
                }
            }
            else
            {
                if (mLogStacker > 0)
                {
                    mLogTime -= dt;
                    if (mLogTime <= 0f)
                    {
                        mLogTime = LogTime;

                        InsertLog();
                    }
                }
            }
        }

        void InsertLog()
        {
            Param param = new Param();
            param.Add("type", $"{mType}");
            param.Add("remainAncientEssence", Lance.Account.Currency.GetAncientEssence());
            param.Add("result", mLogStacker);

            Lance.BackEnd.InsertLog("UpgradeExcaliburForce", param, 7);

            mLogStacker = 0;
        }

        void OnUpgradeButton(bool isPress)
        {
            mIsPress = isPress;
            if (!mIsPress)
            {
                if (mIsUpgrade)
                    Lance.GameManager.UpdatePlayerStat();

                mIsUpgrade = false;
            }
        }

        bool mIsUpgrade;
        void OnUpgrade()
        {
            if (Lance.GameManager.UpgradeExcaliburForce(mType))
            {
                mIsUpgrade = true;

                SoundPlayer.PlaySkillLevelUp();

                PlayUpgradeMotion();

                mLogStacker++;

                mParent?.Refresh();

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
            }
        }

        public void PlayUpgradeMotion()
        {
            Lance.ParticleManager.AquireUI(UIUtil.GetExcaliburForceUpgradeFX(mType), mImageForceIcon.rectTransform);
        }

        public void Localize()
        {
            var textStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.Get($"Name_{mData.valueType}");
        }

        public void Refresh()
        {
            UpdateButtons();
            UpdateLevel();
            UpdateLevelValues();
        }

        void UpdateLevel()
        {
            int level = Lance.Account.Excalibur.GetLevel(mType);

            mTextLevel.text = $"Lv. {level}";

            var stepData = DataUtil.GetExcaliburForceStepData(mType, Lance.Account.Excalibur.GetExcaliburForceStep(mType));

            StringParam maxLevelParam = new StringParam("level", stepData.maxLevel);

            mTextMaxLevel.text = $"( {StringTableUtil.Get("UIString_MaxLevelValue", maxLevelParam)} )";
        }

        void UpdateLevelValues()
        {
            int curStep = Lance.Account.Excalibur.GetExcaliburForceStep(mType);
            int currentLevel = Lance.Account.Excalibur.GetLevel(mType);
            int nextLevel = currentLevel + 1;

            double curStat = currentLevel == 0 ? 0 : DataUtil.GetExcaliburForceStatValue(mType, curStep, currentLevel);
            double nextStat = DataUtil.GetExcaliburForceStatValue(mType, curStep, nextLevel);

            if (Lance.Account.Excalibur.IsMaxLevelExcaliburForce(mType))
            {
                mNextLevelValueObj.SetActive(false);
                mMaxLevelObj.SetActive(true);

                mTextMaxLevelValue.text = GetStatString(mData.valueType, curStat);

                mIsPress = false;

                if (mIsUpgrade)
                {
                    mIsUpgrade = false;

                    Lance.GameManager.UpdatePlayerStat();
                }
            }
            else
            {
                mNextLevelValueObj.SetActive(true);
                mMaxLevelObj.SetActive(false);

                mTextCurLevelValue.text = GetStatString(mData.valueType, curStat);
                mTextNextLevelValue.text = GetStatString(mData.valueType, nextStat);
            }
        }

        string GetStatString(StatType statType, double statValue)
        {
            return
                statType.IsPercentType() ?
                (((statType == StatType.AtkRatio || statType == StatType.HpRatio)) ? $"{statValue * 100f}%" : $"{statValue * 100f:F2}%") :
                statType.IsNoneAlphaType() ?
                $"{statValue:F2}" : statValue.ToAlphaString(ShowDecimalPoint.Default);
        }

        void UpdateButtons()
        {
            bool canUpgrade = Lance.Account.CanUpgradeExcaliburForce(mType);

            mButtonUpgrade.SetActiveFrame(canUpgrade);
            mUpgradeRedDot.SetActive(canUpgrade);
            mTextUpgradeRequire.text = $"{Lance.Account.Excalibur.GetUpgradeRequire(mType)}";
        }
    }
}

