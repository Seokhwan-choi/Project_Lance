using System.Collections;
using System.Collections.Generic;
using BackEnd;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;


namespace Lance
{
    class GoldTrainItemUI : MonoBehaviour
    {
        StatType mType;
        Stature_GoldTrainTabUI mParent;

        Button mButtonLevelUp;
        RectTransform mImageIconTm;
        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextCurLevelValue;
        TextMeshProUGUI mTextNextLevelValue;
        TextMeshProUGUI mTextMaxLevelValue;
        TextMeshProUGUI mTextRequireGold;

        RectTransform mCurrencyUITm;
        GameObject mNextLevelValueObj;
        GameObject mMaxLevelObj;
        GameObject mLockObj;

        const float IntervalTime = 0.1f;
        const float LogTime = 1f;
        float mIntervalTime;
        float mLogTime;
        int mLogStacker;
        bool mIsPress;
        public void Init(Stature_GoldTrainTabUI parent, StatType type)
        {
            mParent = parent;
            mType = type;

            var imageGoldTrainIcon = gameObject.FindComponent<Image>("Image_GoldTrainIcon");
            imageGoldTrainIcon.sprite = Lance.Atlas.GetUISprite($"Icon_Train_{type}");

            mImageIconTm = imageGoldTrainIcon.rectTransform;

            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.Get($"Name_{type}");

            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");

            var currencyUIObj = gameObject.FindGameObject("CurrencyUI");
            mCurrencyUITm = currencyUIObj.GetComponent<RectTransform>();
            mTextRequireGold = currencyUIObj.FindComponent<TextMeshProUGUI>("Text_Amount");

            mButtonLevelUp = gameObject.FindComponent<Button>("Button_Train");

            var eventTrigger = mButtonLevelUp.GetOrAddComponent<EventTrigger>();
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerDown, () => OnTrainButton(isPress: true));
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerUp, () => OnTrainButton(isPress: false));

            var guideActionTag = mButtonLevelUp.GetOrAddComponent<GuideActionTag>();
            guideActionTag.Tag = type.TrainTypeChangeToGuideActionType();

            var guideActionParentTag = gameObject.GetOrAddComponent<GuideActionTag>();
            guideActionParentTag.Tag = type.TrainTypeChangeToGuideActionType().ChangeToParentType();

            mNextLevelValueObj = gameObject.FindGameObject("NextLevel");
            mMaxLevelObj = gameObject.FindGameObject("MaxLevel");
            mLockObj = gameObject.FindGameObject("Lock");

            var goldTrainData = Lance.GameData.GoldTrainData.TryGet(mType);
            if (goldTrainData != null)
            {
                var textLockReason = mLockObj.FindComponent<TextMeshProUGUI>("Text_LockReason");

                StringParam param = new StringParam("trainName", StringTableUtil.GetName($"{goldTrainData.requireType}"));
                param.AddParam("trainLevel", goldTrainData.requireLevel);

                textLockReason.text = StringTableUtil.Get("UIString_RequireGoldTrain", param);
            }

            mTextCurLevelValue = mNextLevelValueObj.FindComponent<TextMeshProUGUI>("Text_CurrentLevel_Value");
            mTextNextLevelValue = mNextLevelValueObj.FindComponent<TextMeshProUGUI>("Text_NextLevel_Value");
            mTextMaxLevelValue = mMaxLevelObj.FindComponent<TextMeshProUGUI>("Text_MaxLevelValue");

            var textMaxLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_MaxLevel");

            StringParam maxLevelParam = new StringParam("level", DataUtil.GetGoldTrainStatMaxLevel(mType));

            string maxLevel = StringTableUtil.Get("UIString_MaxLevelValue", maxLevelParam);

            textMaxLevel.text = $"( {maxLevel} )";

            Refresh();
        }

        public void OnChangeGoldTrainCount()
        {
            Refresh();

            mCurrencyUITm.DORewind();
            mCurrencyUITm.localScale = Vector3.one * 1.25f;
            mCurrencyUITm.DOScale(1f, 0.25f)
                .SetEase(Ease.OutBounce)
                .SetAutoKill(false);
        }

        public void OnTabLeave()
        {
            if (mLogStacker > 0)
            {
                InsertLog();
            }
        }

        public void Localize()
        {
            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.Get($"Name_{mType}");

            var goldTrainData = Lance.GameData.GoldTrainData.TryGet(mType);
            if (goldTrainData != null)
            {
                var textLockReason = mLockObj.FindComponent<TextMeshProUGUI>("Text_LockReason");

                StringParam param = new StringParam("trainName", StringTableUtil.GetName($"{goldTrainData.requireType}"));
                param.AddParam("trainLevel", goldTrainData.requireLevel);

                textLockReason.text = StringTableUtil.Get("UIString_RequireGoldTrain", param);
            }

            Refresh();
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

                    OnTrain();
                }
            }
            else
            {
                if (mLogStacker > 0)
                {
                    mIntervalTime = 0f;
                    mLogTime -= dt;
                    if (mLogTime <= 0f)
                    {
                        mLogTime = LogTime;

                        InsertLog();
                    }
                }
            }
        }
        
        void OnTrainButton(bool isPress)
        {
            mIsPress = isPress;

            if ( !mIsPress )
            {
                if (mLogStacker > 101)
                {
                    Lance.GameManager.UpdatePlayerStat();
                }
            }
        }

        void OnTrain()
        {
            if ( Lance.GameManager.Train(mType, mParent.GoldTrainCount) )
            {
                SoundPlayer.PlayLevelUp();

                Lance.ParticleManager.AquireUI("TrainLevelUp", mImageIconTm);

                UpdateLevel();

                mParent.UpdateRequireGold();

                mLogStacker += mParent.GoldTrainCount;
            }
        }

        void InsertLog()
        {
            Param param = new Param();
            param.Add("statType", mType);
            param.Add("levelUpCount", mLogStacker);

            Lance.BackEnd.InsertLog("LevelUpTrain", param, 7);

            mLogStacker = 0;

            Lance.GameManager.UpdatePlayerStat();
        }

        public void Refresh()
        {
            UpdateLevel();
            UpdateRequireGold();
            UpdateRequireType();
        }

        void UpdateLevel()
        {
            int currentLevel = Lance.Account.GoldTrain.GetTrainLevel(mType);

            mTextLevel.text = $"Lv. {currentLevel}";

            int nextLevel = currentLevel + mParent.GoldTrainCount;

            var curStatValue = DataUtil.GetGoldTrainStatValue(mType, currentLevel);
            var nextStatValue = DataUtil.GetGoldTrainStatValue(mType, nextLevel);

            bool isMaxLevel = DataUtil.GetGoldTrainStatValue(mType, currentLevel + 1) == 0;
            if (isMaxLevel)
            {
                if (SaveBitFlags.GoldTrainMaxLevelHide.IsOn())
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(true);

                    mNextLevelValueObj.SetActive(false);
                    mMaxLevelObj.SetActive(true);

                    mTextMaxLevelValue.text = UIUtil.GetStatString(mType, curStatValue, isGoldTrain: true);
                }
            }
            else
            {
                mNextLevelValueObj.SetActive(true);
                mMaxLevelObj.SetActive(false);

                mTextCurLevelValue.text = UIUtil.GetStatString(mType, curStatValue, isGoldTrain: true);
                mTextNextLevelValue.text = UIUtil.GetStatString(mType, nextStatValue, isGoldTrain: true);
            }
        }

        public void UpdateRequireGold()
        {
            int currentLevel = Lance.Account.GoldTrain.GetTrainLevel(mType);
            bool isMaxLevel = Lance.Account.GoldTrain.IsMaxLevel(mType);
            var result = DataUtil.GetGoldTrainTotalRequireGold(mType, currentLevel, mParent.GoldTrainCount);

            string requireGoldStr = result.totalRequireGold.ToAlphaString();
            bool isEnoughGold = isMaxLevel ? false : Lance.Account.IsEnoughGold(result.totalRequireGold);

            mButtonLevelUp.SetActiveFrame(isEnoughGold); //  textInactive: Const.NotEnoughTextColor

            mTextRequireGold.SetColor(UIUtil.GetEnoughTextColor(isEnoughGold));
            mTextRequireGold.text = requireGoldStr;
        }

        public void UpdateRequireType()
        {
            if (DataUtil.HaveGoldTrainRequireType(mType))
            {
                mLockObj.SetActive(!Lance.Account.GoldTrain.IsSatisfiedRequireType(mType));
            }
            else
            {
                mLockObj.SetActive(false);
            }
        }
    }
}

