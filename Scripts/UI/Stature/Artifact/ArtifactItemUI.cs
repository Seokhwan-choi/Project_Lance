using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using UnityEngine.EventSystems;


namespace Lance
{
    class ArtifactItemUI : MonoBehaviour
    {
        ArtifactTabUI mParent;
        string mId;
        Image mImageArtifactIcon;
        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextCurLevelValue;
        TextMeshProUGUI mTextNextLevelValue;
        TextMeshProUGUI mTextMaxLevelValue;
        TextMeshProUGUI mTextArtifactCount;
        TextMeshProUGUI mTextUpgradeProb;
        TextMeshProUGUI mTextUpgradeBonusProb;

        GameObject mUpgradeObj;
        GameObject mDismantleObj;
        GameObject mSellObj;
        Button mButtonUpgrade;
        Button mButtonDismantle;
        Button mButtonSell;
        GameObject mUpgradeRedDot;
        GameObject mDismantleObjRedDot;
        GameObject mSellObjRedDot;

        GameObject mLockObj;
        GameObject mNextLevelValueObj;
        GameObject mMaxLevelObj;

        ArtifactData mData;

        const float IntervalTime = 0.1f;
        const float LogTime = 1f;
        float mIntervalTime;
        float mLogTime;
        List<ArtifactLevelUpResult> mLogStacker;
        bool mIsPress;

        public string Id => mId;
        public void Init(ArtifactTabUI parent, string id)
        {
            mParent = parent;
            mId = id;
            mData = DataUtil.GetArtifactData(mId);

            Debug.Assert(mData != null, $"{id}의 Artifact Data가 존재하지 않음");

            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.GetName(id);

            mNextLevelValueObj = gameObject.FindGameObject("NextLevel");
            mMaxLevelObj = gameObject.FindGameObject("MaxLevel");
            mUpgradeObj = gameObject.FindGameObject("Upgrade");
            mDismantleObj = gameObject.FindGameObject("Dismantle");
            mSellObj = gameObject.FindGameObject("Sell");
            mLockObj = gameObject.FindGameObject("Lock");

            mImageArtifactIcon = gameObject.FindComponent<Image>("Image_ArtifactIcon");
            mImageArtifactIcon.sprite = Lance.Atlas.GetItemSlotUISprite(mData.sprite);

            mLogStacker = new List<ArtifactLevelUpResult>();
            mButtonUpgrade = mUpgradeObj.FindComponent<Button>("Button_Upgrade");
            var eventTrigger = mButtonUpgrade.GetOrAddComponent<EventTrigger>();
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerDown, () => OnUpgradeButton(isPress: true));
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerUp, () => OnUpgradeButton(isPress: false));

            mUpgradeRedDot = mUpgradeObj.FindGameObject("RedDot");

            mButtonDismantle = mDismantleObj.FindComponent<Button>("Button_Dismantle");
            mButtonDismantle.SetButtonAction(OnDismantleButton);
            mDismantleObjRedDot = mDismantleObj.FindGameObject("RedDot");

            mButtonSell = mSellObj.FindComponent<Button>("Button_Sell");
            mButtonSell.SetButtonAction(OnSellButton);
            mSellObjRedDot = mSellObj.FindGameObject("RedDot");

            var textStatName = mNextLevelValueObj.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.Get($"Name_{mData.statType}");
            var textStatName2 = mMaxLevelObj.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName2.text = StringTableUtil.Get($"Name_{mData.statType}");

            var frameDeco = gameObject.FindComponent<Image>("Frame_Deco_2");
            frameDeco.sprite = Lance.Atlas.GetItemSlotUISprite(DataUtil.IsAncientArtifact(mId) ? "Frame_Deco_Artifact2_3" : "Frame_Deco_Artifact2_4");

            mTextCurLevelValue = mNextLevelValueObj.FindComponent<TextMeshProUGUI>("Text_CurrentLevel_Value");
            mTextNextLevelValue = mNextLevelValueObj.FindComponent<TextMeshProUGUI>("Text_NextLevel_Value");
            mTextMaxLevelValue = mMaxLevelObj.FindComponent<TextMeshProUGUI>("Text_MaxLevelValue");

            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mTextArtifactCount = gameObject.FindComponent<TextMeshProUGUI>("Text_Count");
            mTextUpgradeProb = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradeProb");
            mTextUpgradeBonusProb = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradeBonusProb");

            var textMaxLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_MaxLevel");
            StringParam maxLevelParam = new StringParam("level", DataUtil.GetArtifactMaxLevel(id));
            textMaxLevel.text = $"( {StringTableUtil.Get("UIString_MaxLevelValue", maxLevelParam)} )";

            Refresh();
        }

        public void OnTabLeave()
        {
            if (mLogStacker.Count > 0)
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
                if (mLogStacker.Count > 0)
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
            param.Add("id", mId);
            param.Add("result", mLogStacker);

            Lance.BackEnd.InsertLog("ArtifactUpgradeResult", param, 7);

            mLogStacker.Clear();
        }

        void OnUpgradeButton(bool isPress)
        {
            mIsPress = isPress;
            if (!mIsPress)
            {
                if (mIsLevelUp)
                {
                    mIsLevelUp = false;

                    mParent.Parnet.RefreshContentsLockUI();

                    Lance.GameManager.UpdatePlayerStat();
                }
            }
        }

        bool mIsLevelUp;
        void OnUpgrade()
        {
            int prevLevel = Lance.Account.GetLevelArtifact(mId);

            var result = Lance.GameManager.LevelUpArtifact(mId);
            if (result != ArtifactLevelUpResult.Error)
            {
                if (result == ArtifactLevelUpResult.Fail)
                {
                    UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("ArtifactLevelUp_Fail"));

                    SoundPlayer.PlayArtifactLevelUpFail();

                    Lance.ParticleManager.AquireUI("ArtifactLevelUp_Fail", mImageArtifactIcon.rectTransform);
                }
                else if (result == ArtifactLevelUpResult.Success)
                {
                    UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("ArtifactLevelUp_Success"));

                    SoundPlayer.PlayArtifactLevelUpSuccess();

                    Lance.ParticleManager.AquireUI("ArtifactLevelUp_Success", mImageArtifactIcon.rectTransform);
                }

                int newLevel = Lance.Account.GetLevelArtifact(mId);

                if (prevLevel < newLevel)
                    mIsLevelUp = true;

                mLogStacker.Add(result);

                Refresh();

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
            }
        }

        void OnDismantleButton()
        {
            bool isAncient = DataUtil.IsAncientArtifact(mId);

            if (Lance.Account.IsMaxLevelArtifact(mId) == false)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (!isAncient)
            {
                if (Lance.Account.Artifact.IsAllArtifactMaxLevel() == false)
                {
                    UIUtil.ShowSystemDefaultErrorMessage();

                    return;
                }
            }

            int remainCount = Lance.Account.GetCountArtifact(mId);
            if (remainCount <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughDismantleArtifact");

                return;
            }

            if (isAncient)
                Lance.GameManager.DismantleAncientArtifact(mId);
            else
                Lance.GameManager.DismantleArtifact(mId);

            Refresh();

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
        }

        void OnSellButton()
        {
            if (Lance.Account.IsMaxLevelArtifact(mId) == false)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            int remainCount = Lance.Account.Artifact.GetCount(mId);
            if (remainCount <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughSellArtifact");

                return;
            }

            Lance.GameManager.SellArtifact(mId);

            Refresh();

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
        }

        public void Localize()
        {
            var textName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textName.text = StringTableUtil.GetName(mId);

            var textStatName = mNextLevelValueObj.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.Get($"Name_{mData.statType}");

            var textStatName2 = mMaxLevelObj.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName2.text = StringTableUtil.Get($"Name_{mData.statType}");

            UpdateLevelValues();
            UpdateArtifactUpgradeInfo();
        }

        public void Refresh()
        {
            UpdateButtons();
            UpdateLevel();
            UpdateLevelValues();
            UpdateArtifactUpgradeInfo();
        }

        void UpdateLevel()
        {
            int level = Lance.Account.GetLevelArtifact(mId);

            mTextLevel.text = $"Lv. {level}";

            mLockObj.SetActive(level == 0);
        }

        void UpdateLevelValues()
        {
            int currentLevel = Lance.Account.GetLevelArtifact(mData.id);
            int nextLevel = currentLevel + 1;

            (StatType statType, double statValue) curStat = currentLevel == 0 ? (mData.statType, 0) : (mData.statType, mData.baseValue + (currentLevel * mData.levelUpValue));
            (StatType statType, double statValue) nextStat = (mData.statType, mData.baseValue + (nextLevel * mData.levelUpValue));

            if (Lance.Account.IsMaxLevelArtifact(mId))
            {
                mNextLevelValueObj.SetActive(false);
                mMaxLevelObj.SetActive(true);

                mTextMaxLevelValue.text = UIUtil.GetStatString(curStat.statType, curStat.statValue);

                mIsPress = false;

                if (mIsLevelUp)
                {
                    mIsLevelUp = false;

                    mParent.Parnet.RefreshContentsLockUI();

                    Lance.GameManager.UpdatePlayerStat();
                }
            }
            else
            {
                mNextLevelValueObj.SetActive(true);
                mMaxLevelObj.SetActive(false);

                mTextCurLevelValue.text = UIUtil.GetStatString(curStat.statType, curStat.statValue);
                mTextNextLevelValue.text = UIUtil.GetStatString(nextStat.statType, nextStat.statValue);
            }
        }

        void UpdateArtifactUpgradeInfo()
        {
            int count = Lance.Account.GetCountArtifact(mId);
            int level = Lance.Account.GetLevelArtifact(mId);
            level = Mathf.Max(1, level);

            ArtifactLevelUpData levelUpdata = DataUtil.GetArtifactLevelUpData(mId, level);
            int requireCount = levelUpdata?.requireCount ?? 1;

            mTextArtifactCount.text = $"{count}/{requireCount}";

            if (levelUpdata != null)
            {
                mTextUpgradeProb.gameObject.SetActive(true);
                mTextUpgradeBonusProb.gameObject.SetActive(true);

                StringParam param = new StringParam("prob", $"{levelUpdata.prob * 100f:F0}");

                mTextUpgradeProb.text = StringTableUtil.Get("UIString_UpgradeProb", param);

                int failedCount = Lance.Account.GetUpgradeFailedCountArtifact(mId);

                float bonusProb = failedCount * levelUpdata.prob * Lance.GameData.ArtifactCommonData.failedBonusProbValue;

                StringParam bonusProbParam = new StringParam("bonusProb", $"{bonusProb * 100f:F2}");

                mTextUpgradeBonusProb.text = StringTableUtil.Get("UIString_UpgradeFailedBonusProb", bonusProbParam);
            }
            else
            {
                mTextUpgradeProb.gameObject.SetActive(false);
                mTextUpgradeBonusProb.gameObject.SetActive(false);
            }
        }
        
        void UpdateButtons()
        {
            bool isMaxLevel = Lance.Account.IsMaxLevelArtifact(mId);
            bool isAncient = DataUtil.IsAncientArtifact(mId);
            bool isAllMaxLevel = Lance.Account.Artifact.IsAllArtifactMaxLevel();

            mUpgradeObj.SetActive(!isMaxLevel);
            mDismantleObj.SetActive(isAncient ? (isMaxLevel) : (isMaxLevel && isAllMaxLevel));
            mSellObj.SetActive(!isAncient && isMaxLevel && !isAllMaxLevel);

            bool canLevelUp = Lance.Account.CanLevelUpArtifact(mId);
            mButtonUpgrade.SetActiveFrame(canLevelUp);
            mUpgradeRedDot.SetActive(canLevelUp);

            bool canDismantle = Lance.Account.CanDismantleArtifact(mId);
            mButtonDismantle.SetActiveFrame(canDismantle);
            mDismantleObjRedDot.SetActive(canDismantle);

            bool canSell = Lance.Account.CanSellArtifact(mId);
            mButtonSell.SetActiveFrame(canSell);
            mSellObjRedDot.SetActive(canSell);
        }
    }
}

