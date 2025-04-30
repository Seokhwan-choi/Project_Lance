using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

namespace Lance
{
    static class UIUtil
    {
        public static Popup_ConfirmUI ShowConfirmPopup(string title, string desc, Action onConfirm, Action onCancel, bool ignoreModalTouch = false, bool ignoreBackButton = false)
        {
            Popup_ConfirmUI popup = Lance.PopupManager.CreatePopup<Popup_ConfirmUI>();

            popup.Init(title, desc, onConfirm, onCancel, ignoreModalTouch, ignoreBackButton);

            return popup;
        }

        public static Popup_RewardUI ShowRewardPopup(RewardResult reward, string title = null, float closeTime = 0f)
        {
            string rewardPopupName = "Popup_RewardUI";

            if (reward.equipments != null && reward.equipments.Length > 0)
                reward.equipments = reward.equipments.GatherReward();

            if (reward.accessorys != null && reward.accessorys.Length > 0)
                reward.accessorys = reward.accessorys.GatherReward();

            if ((reward.equipments?.Length ?? 0) + (reward.accessorys?.Length ?? 0) >= 15)
                rewardPopupName = "Popup_RewardUI2";

            Popup_RewardUI popup = Lance.PopupManager.CreatePopup<Popup_RewardUI>(rewardPopupName);

            popup.Init(reward, title, closeTime);

            return popup;
        }

        public static void PopupRefreshRedDots<T>() where T : PopupBase
        {
            var popup = Lance.PopupManager.GetPopup<T>();

            popup?.RefreshRedDots();
        }

        public static void ShowSystemMessage(string systemMessage, float duration = 1f)
        {
            Popup_SystemMessageUI systemPopup = Lance.PopupManager.CreatePopup<Popup_SystemMessageUI>(showMotion: false);

            systemPopup.Init(systemMessage, duration);
        }

        public static void ShowSystemDefaultErrorMessage()
        {
            ShowSystemErrorMessage("DefaultErrorMessage");
        }

        public static string GetColorString(string hexColor, object value)
        {
            return $"<#{hexColor}>{value}</color>";
        }

        public static void ShowSystemErrorMessage(string stringIdx, float duration = 2f, StringParam param = null)
        {
            var message = StringTableUtil.GetSystemMessage(stringIdx, param);

            ShowSystemMessage(message, duration);

            SoundPlayer.PlayErrorSound();
        }

        public static string GetExcaliburForceUpgradeFX(ExcaliburForceType type)
        {
            switch (type)
            {
                case ExcaliburForceType.Force_1: // 붉은색
                    return "UpgradeEssence_Chapter5";
                case ExcaliburForceType.Force_2: // 주황색
                    return "UpgradeExcalibur_Force2";
                case ExcaliburForceType.Force_3: // 노란색
                    return "UpgradeEssence_Chapter2";
                case ExcaliburForceType.Force_4: // 초록색
                    return "UpgradeEssence_Chapter1";
                case ExcaliburForceType.Force_5: // 파란색
                    return "UpgradeEssence_Chapter3";
                case ExcaliburForceType.Force_6: // 남색
                    return "UpgradeExcalibur_Force6";
                case ExcaliburForceType.Force_7: // 보라색
                default:
                    return "UpgradeExcalibur_Force7";
            }
        }

        public static string GetStatString(StatType statType, double statValue, bool isGoldTrain = false)
        {
            return statType.IsPercentType() ? $"{statValue * 100f:F2}%" :
                statType.IsNoneAlphaType() ? $"{statValue:F2}": statValue.ToAlphaString(isGoldTrain ? ShowDecimalPoint.GoldTrain : ShowDecimalPoint.Default);
        }

        public static void ShowLevelUpText(Character target)
        {
            GameObject levelUpTextObj = Lance.ObjectPool.AcquireUI("LevelUpTextUI", Lance.GameManager.DamageTextParent);

            LevelUpTextUI levelUpTextUI = levelUpTextObj.GetOrAddComponent<LevelUpTextUI>();

            levelUpTextUI.Init(target);
        }

        public static void SpeechBubbleShow(Transform tm, Action onComplete)
        {
            tm.DORewind();
            tm.DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        public static void SpeechBubbleHide(Transform tm, Action onComplete)
        {
            tm.DORewind();
            tm.DOScale(0f, 0.4f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => onComplete?.Invoke());
        }

        public static void ShowDamageText(Character target, double damage, bool isCritical, bool isSuperCritical)
        {
            GameObject damageTextObj = Lance.ObjectPool.AcquireUI("DamageTextUI", Lance.GameManager.DamageTextParent);

            DamageTextUI damageTextUI = damageTextObj.GetOrAddComponent<DamageTextUI>();

            damageTextUI.Init(target, damage, isCritical, isSuperCritical);
        }

        public static HpGaugebarUI CreateHpGaugebarUI(Character parent)
        {
            string gaugebarName = parent.IsPlayer ? "Player_HpGaugebarUI" : "Monster_HpGaugebarUI";

            GameObject gaugebarObj = Lance.ObjectPool.AcquireUI(gaugebarName, Lance.GameManager.HpGaugeParent);

            HpGaugebarUI gaugebarUI = gaugebarObj.GetOrAddComponent<HpGaugebarUI>();

            gaugebarUI.Init(parent);

            return gaugebarUI;
        }

        public static void PlayStampMotion(Image background, Image stamp)
        {
            // 배경 페이드 해주면서
            background.DOKill();
            background.color = new Color(1, 1, 1, 0f);
            background.DOFade(1f, 0.5f);

            // 도장 위에서 아래로 뙇
            stamp.transform.DORewind();
            stamp.transform.localScale = Vector2.one * 3f;
            stamp.transform.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBounce);
        }

        public static string GetEnoughTextColor(bool isEnough)
        {
            return isEnough ? Const.EnoughTextColor : Const.NotEnoughTextColor;
        }

        public static string GetActiveTextColor(bool isActive)
        {
            return isActive ? Const.DefaultActiveTextColor : Const.DefaultInactiveTextColor;
        }
    }
}