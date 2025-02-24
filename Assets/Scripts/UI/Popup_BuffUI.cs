using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_BuffUI : PopupBase
    {
        List<BuffItemUI> mBuffItemUIList;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Buff"));

            var contentsObj = gameObject.FindGameObject("Contents");

            contentsObj.AllChildObjectOff();

            mBuffItemUIList = new List<BuffItemUI>();

            foreach (BuffData data in Lance.GameData.BuffData.Values)
            {
                var buffItemObj = Util.InstantiateUI("BuffItemUI", contentsObj.transform);

                var buffItemUI = buffItemObj.GetOrAddComponent<BuffItemUI>();

                buffItemUI.Init(data);

                mBuffItemUIList.Add(buffItemUI);
            }

            bool purchasedRemovedAD = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

            Refresh(purchasedRemovedAD);

            if (purchasedRemovedAD)
            {
                Lance.GameManager.CheckQuest(QuestType.ActiveBuff, 1);
            }
        }

        public void Refresh(bool purchasedRemovedAD)
        {
            foreach(var buffItemUI in mBuffItemUIList)
            {
                buffItemUI.Refresh(purchasedRemovedAD);
            }
        }

        public override void RefreshRedDots()
        {
            foreach (var buffItemUI in mBuffItemUIList)
            {
                buffItemUI.RefreshRedDot();
            }
        }
    }

    class BuffItemUI : MonoBehaviour
    {
        BuffData mData;
        GameObject mAdObj;
        GameObject mFreeObj;

        TextMeshProUGUI mTextDesc;

        TextMeshProUGUI mTextRequireKillCount;
        TextMeshProUGUI mTextLevelUp;
        TextMeshProUGUI mTextLevel;
        Slider mSliderRequireKillCount;
        Button mButtonLevelUp;
        GameObject mLevelUpRedDotObj;

        Button mButtonActive;
        GameObject mActiveRedDotObj;
        TextMeshProUGUI mTextRemainTime;
        
        public void Init(BuffData data)
        {
            mData = data;
            mAdObj = gameObject.FindGameObject("Ad");
            mFreeObj = gameObject.FindGameObject("Free");

            mSliderRequireKillCount = gameObject.FindComponent<Slider>("Slider_Exp");
            mTextRequireKillCount = gameObject.FindComponent<TextMeshProUGUI>("Text_Exp");
            mTextLevelUp = gameObject.FindComponent<TextMeshProUGUI>("Text_LevelUp");
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mButtonLevelUp = gameObject.FindComponent<Button>("Button_LevelUp");
            mButtonLevelUp.SetButtonAction(OnButtonLevelUp);
            mLevelUpRedDotObj = mButtonLevelUp.gameObject.FindGameObject("RedDot");

            var imageBuffIcon = gameObject.FindComponent<Image>("Image_BuffIcon");
            imageBuffIcon.sprite = Lance.Atlas.GetBuffIcon(data.type);

            // 버프 설명 & 이름
            var textBuffName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            textBuffName.text = StringTableUtil.Get($"Name_Buff_{mData.type}");

            mTextDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");

            //// 버프 지속시간 ex) 60m
            //int minute = Mathf.RoundToInt(mData.duration / (float)TimeUtil.SecondsInMinute);
            //StringParam param2 = new StringParam("minute", $"{minute}");
            //var textBuffDurationTime = gameObject.FindComponent<TextMeshProUGUI>("Text_DurationTime");
            //textBuffDurationTime.text = StringTableUtil.Get("UIString_TimeOfMinute", param2);

            mButtonActive = gameObject.FindComponent<Button>("Button_Active");
            mButtonActive.SetButtonAction(OnButtonActive);
            mActiveRedDotObj = mButtonActive.gameObject.FindGameObject("RedDot");
            mTextRemainTime = gameObject.FindComponent<TextMeshProUGUI>("Text_RemainTime");

            var guideActionTag = mButtonActive.GetOrAddComponent<GuideActionTag>();
            guideActionTag.Tag = mData.type.BuffTypeChangeToGuideActionType();

            var imageBackground = GetComponent<Image>();
            imageBackground.sprite = GetBuffBackground();
        }

        Sprite GetBuffBackground()
        {
            string backgroundName = string.Empty;

            if (mData.type == StatType.AtkRatio)
            {
                backgroundName = "Button_Deco_Stroke_LightBrown_1";
            }
            else if (mData.type == StatType.ExpAmount)
            {
                backgroundName = "Button_Deco_Stroke_GrayIndigo_1";
            }
            else
            {
                backgroundName = "Button_Deco_Stroke_LightYellowBrown_1";
            }

            return Lance.Atlas.GetUISprite(backgroundName);
        }

        void OnButtonLevelUp()
        {
            if (Lance.GameManager.LevelUpBuff(mData.id, mData.type))
            {
                Refresh(false);

                Lance.Lobby.RefreshBuffRedDot();
            }
        }

        void OnButtonActive()
        {
            if ( Lance.GameManager.ActiveBuff(mData.id, mData.type) )
            {
                Refresh(false);

                Lance.Lobby.RefreshBuffRedDot();
            }
        }

        public void Refresh(bool purchasedRemovedAD)
        {
            bool isFirstActive = Lance.Account.Buff.GetStackedActiveCount(mData.id) == 0;
            bool isAlreadyActive = Lance.Account.Buff.IsAlreadyActiveBuff(mData.id);
            bool canActiveBuff = Lance.Account.Buff.CanActiveBuff(mData.id);

            mButtonActive.SetActiveFrame(canActiveBuff);

            mAdObj.SetActive(!isFirstActive && isAlreadyActive == false);
            mFreeObj.SetActive(isFirstActive && isAlreadyActive == false);
            mTextRemainTime.gameObject.SetActive(isAlreadyActive);
            mTextRemainTime.SetColor("2A2220");
            mTextRemainTime.text = purchasedRemovedAD ? StringTableUtil.Get("UIString_Infinity_Buff") : TimeUtil.GetTimeStr(Mathf.RoundToInt(Lance.Account.Buff.GetBuffDurationTime(mData.id)), ignoreHour: true);

            bool canLevelUpBuff = Lance.Account.Buff.CanLevelUpBuff(mData.id);
            bool isMaxLevelBuff = Lance.Account.Buff.IsMaxLevel(mData.id);

            mButtonLevelUp.SetActiveFrame(canLevelUpBuff);

            int myKillCount = Lance.Account.Buff.GetMonsterKillCount(mData.id);
            int requireKillCount = Lance.Account.Buff.GetRequireMonsterKillCount(mData.id);

            float killCountValue = isMaxLevelBuff ? 0f : (float)myKillCount / (float)requireKillCount;

            mTextLevelUp.SetColor(canLevelUpBuff ? "323232" : Const.NotEnoughTextColor);
            mSliderRequireKillCount.value = killCountValue;
            mTextRequireKillCount.text = isMaxLevelBuff ? "MAX LEVEL" : $"{myKillCount}/{requireKillCount}";
            mTextLevel.text = $"Lv.{Lance.Account.Buff.GetLevel(mData.id)}";

            StringParam param = new StringParam("value", $"{Lance.Account.Buff.GetBuffValue(mData.id) * 100f}%");

            mTextDesc.text = StringTableUtil.Get($"Desc_Buff_{mData.type}", param);

            RefreshRedDot();

            //int remainCount = Lance.Account.Buff.GetBuffRemainCount(mData.id);
            //int maxCount = mData.dailyCount;

            //StringParam param = new StringParam("remainCount", $"{remainCount}");
            //param.AddParam("maxCount", $"{maxCount}");

            //mTextRemainCount.SetColor(UIUtil.GetEnoughTextColor(remainCount > 0));
            //mTextRemainCount.text = StringTableUtil.Get("UIString_RemainDailyCount", param);
        }

        public void RefreshRedDot()
        {
            mActiveRedDotObj.SetActive(Lance.Account.Buff.CanActiveBuff(mData.id));
            mLevelUpRedDotObj.SetActive(Lance.Account.Buff.CanLevelUpBuff(mData.id));
        }
    }
}