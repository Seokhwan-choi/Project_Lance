using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Lance
{
    class Lobby_StatisticsUI : MonoBehaviour
    {
        bool mInMotion;
        CanvasGroup mCanvasGroup;
        StatisticsTabUIManager mTabUIManager;
        public void Init()
        {
            mCanvasGroup = GetComponent<CanvasGroup>();

            mTabUIManager = new StatisticsTabUIManager();
            mTabUIManager.Init(gameObject);

            mInMotion = false;

            var buttonClose = gameObject.FindComponent<Button>("Button_Close");
            buttonClose.SetButtonAction(Hide);
        }

        public void OnStartStage()
        {
            if (SaveBitFlags.AutoResetStatictics.IsOn())
            {
                mTabUIManager.ResetInfo();
            }
        }

        public void Show()
        {
            if (mInMotion)
                return;

            Lance.Lobby.SetActiveRewardUI(false);

            mInMotion = true;

            transform.DORewind();

            var sequence = DOTween.Sequence()
                .Join(transform.DOLocalMove(new Vector3(0f, -58f, 0f), 0.5f).SetEase(Ease.OutBack))
                .Join(DOTween.To(() => mCanvasGroup.alpha, (f) => mCanvasGroup.alpha = f, 1f, 1f))
                .OnComplete(() => mInMotion = false);
        }

        public void Hide()
        {
            if (mInMotion)
                return;

            mInMotion = true;

            transform.DORewind();

            var sequence = DOTween.Sequence()
                .Join(transform.DOLocalMove(new Vector3(-400f, -58f, 0f), 1f).SetEase(Ease.InBack))
                .Join(DOTween.To(() => mCanvasGroup.alpha, (f) => mCanvasGroup.alpha = f, 0f, 1f))
                .OnComplete(() => mInMotion = false);

            Lance.Lobby.SetActiveRewardUI(true);
        }

        public void Localize()
        {
            mTabUIManager.Localize();
        }

        public void OnUpdate(float dt)
        {
            mTabUIManager.OnUpdate(dt);
        }

        public void StackReward(RewardResult reward)
        {
            mTabUIManager.StackReward(reward);
        }

        public void StackDamage(string id, double damage)
        {
            mTabUIManager.StackDamage(id, damage);
        }
    }
}