using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using DG.Tweening;


namespace Lance
{

    class EventPassRewardItemSlotUI : MonoBehaviour
    {
        string mEventId;
        PassStepData mStepData;
        bool mIsFree;
        GameObject mRedDotObj;
        Image mImageModal;
        Image mImageLocked;
        Image mImageReceived;
        public void Init(PassStepData stepData, string eventId, bool isFree)
        {
            mStepData = stepData;
            mEventId = eventId;
            mIsFree = isFree;
            mRedDotObj = gameObject.FindGameObject("RedDot");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
            mImageLocked = gameObject.FindComponent<Image>("Image_Locked");
            mImageReceived = gameObject.FindComponent<Image>("Image_Received");

            string rewardId = isFree ? stepData.freeReward : stepData.payReward;
            var rewardData = Lance.GameData.RewardData.TryGet(rewardId);

            var itemSlotObj = gameObject.FindGameObject("ItemSlotUI");
            var itemSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
            itemSlotUI.Init(new ItemInfo(rewardData));

            var button = GetComponentInChildren<Button>();
            button.SetButtonAction(OnButtonAction);

            Refresh();
        }

        void Refresh()
        {
            bool isSatisfied = Lance.Account.Event.IsSatisfiedValue(mEventId, mStepData.step);
            bool isAlreadyReceived = Lance.Account.Event.IsAlreadyPassRewardReceived(mEventId, mStepData.step, mIsFree);
            bool isPurchased = mIsFree ? true : Lance.Account.Event.IsPurchasedPass(mEventId);

            mImageModal.gameObject.SetActive(isSatisfied == false || isAlreadyReceived);
            mImageReceived.gameObject.SetActive(isAlreadyReceived);
            mImageLocked.gameObject.SetActive(!isPurchased);

            mRedDotObj.SetActive(Lance.Account.Event.CanReceivePassReward(mEventId, mStepData.step, mIsFree));
        }

        void OnButtonAction()
        {
            if (Lance.Account.Event.IsSatisfiedValue(mEventId, mStepData.step) == false)
                return;

            if (Lance.Account.Event.IsAlreadyPassRewardReceived(mEventId, mStepData.step, mIsFree))
                return;

            if (mIsFree == false && Lance.Account.Event.IsPurchasedPass(mEventId) == false)
                return;

            if (Lance.Account.Event.ReceivePassReward(mEventId, mStepData.step, mIsFree))
            {
                Lance.Account.Event.SetIsChangedData(true);

                Refresh();

                Lance.GameManager.GiveReward(mIsFree ? mStepData.freeReward : mStepData.payReward);

                PlayReceiveRewardMotion();

                UIUtil.PopupRefreshRedDots<Popup_NewEventUI>();

                Lance.Lobby.RefreshEventRedDot();

                Param param = new Param();
                param.Add("eventId", mEventId);
                param.Add("id", mStepData.id);
                param.Add("step", mStepData.id);
                param.Add("isFree", mIsFree);
                param.Add("isPurchased", Lance.Account.Pass.IsPurchased(mStepData.id));

                Lance.BackEnd.InsertLog("ReceivePassreward", param, 7);
            }
        }

        public void PlayReceiveRewardMotion()
        {
            // Modal 페이드 해주면서
            mImageModal.color = new Color(1, 1, 1, 0f);
            mImageModal.DOFade(1f, 0.5f)
                .SetAutoKill(false);

            // Check 위에서 아래로 뙇
            mImageReceived.transform.localScale = Vector2.one * 3f;
            mImageReceived.transform.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBounce)
                .SetAutoKill(false);
        }
    }

}
