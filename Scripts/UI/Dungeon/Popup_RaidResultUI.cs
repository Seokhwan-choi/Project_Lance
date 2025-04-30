using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;


namespace Lance
{
    class Popup_RaidResultUI : PopupBase
    {
        bool mInMotion;
        public void Init(ElementalType type, double score, double bestScore)
        {
            var textTouchAnyScreenClosed = gameObject.FindComponent<TextMeshProUGUI>("Text_TouchAnyScreenClosed");
            textTouchAnyScreenClosed.gameObject.SetActive(false);

            mInMotion = true;

            SoundPlayer.PlayShowPopup();

            var buttonModal = gameObject.FindComponent<Button>("Button_Modal");
            buttonModal.SetButtonAction(() => Close());

            var textScore = gameObject.FindComponent<TextMeshProUGUI>("Text_Score");
            textScore.text = "0";

            var textBestScore = gameObject.FindComponent<TextMeshProUGUI>("Text_BestScore");
            textBestScore.text = bestScore.ToAlphaString();

            StartCoroutine(PlayBestScoreUpdateMotion());

            IEnumerator PlayBestScoreUpdateMotion()
            {
                yield return new WaitForSecondsRealtime(0.25f);

                DoCounter(textScore, 0, score, 1f, alphaString: true);

                yield return new WaitForSecondsRealtime(1f);

                if (score > bestScore)
                {
                    var bestScoreObj = gameObject.FindGameObject("BestScore");

                    bestScoreObj.transform.DOPunchScale(Vector3.one, 0.25f)
                    .SetEase(Ease.InBack);

                    textBestScore.text = score.ToAlphaString();
                }
                
                SoundPlayer.PlayUpdateRaidBestScore();

                GameObject rankRewardObj = gameObject.FindGameObject("ScoreGrade");
                rankRewardObj.SetActive(true);

                rankRewardObj.transform.localScale = Vector3.one * 3f;
                rankRewardObj.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBounce);

                RaidRewardData rankReward = DataUtil.GetRaidRewardData(type, score);
                if (rankReward != null)
                {
                    var imageRank = rankRewardObj.FindComponent<Image>("Image_Rank");
                    imageRank.sprite = Lance.Atlas.GetIconGrade(rankReward.rankGrade);
                }

                yield return new WaitForSeconds(0.5f);

                GameObject rewardsObj = gameObject.FindGameObject("Rewards");
                rewardsObj.SetActive(true);

                rewardsObj.transform.localScale = Vector3.one * 3f;
                rewardsObj.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBounce);

                if (rankReward.reward.IsValid())
                {
                    var rewardData = Lance.GameData.RewardData.TryGet(rankReward.reward);
                    if (rewardData != null)
                    {
                        rewardsObj.AllChildObjectOff();

                        var itemInfos = ItemInfoUtil.CreateItemInfos(rewardData);

                        for(int i = 0; i < itemInfos.Count; ++i)
                        {
                            var itemSlotObj = Util.InstantiateUI("ItemSlotUI", rewardsObj.transform);

                            var itemSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
                            itemSlotUI.Init(itemInfos[i]);
                        }
                    }
                }

                mInMotion = false;

                textTouchAnyScreenClosed.gameObject.SetActive(true);

                TweenerCore<double, double, NoOptions> DoCounter(TextMeshProUGUI target, double fromValue, double endValue, float duration, bool alphaString)
                {
                    double v = fromValue;

                    TweenerCore<double, double, NoOptions> t = DOTween.To(() => v, x => {
                        v = x;
                        target.text = alphaString ? v.ToAlphaString() : v.ToString();
                    }, endValue, duration);

                    t.SetTarget(target);

                    return t;
                }
            }
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            if (mInMotion)
                return;

            base.Close(immediate, hideMotion);
        }

        public override void OnBackButton(bool immediate = false, bool hideMotion = true)
        {
            if (mInMotion)
                return;

            base.OnBackButton(immediate, hideMotion);
        }
    }
}