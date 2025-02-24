using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Mosframe;
using BackEnd;

namespace Lance
{
    class PassItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        PassTabUI mParent;
        TextMeshProUGUI mTextRequireValue;
        PassRewardItemSlotUI mFreeSlotUI;
        PassRewardItemSlotUI mPaySlotUI;
        bool mInit;
        string mId;
        int mStep;
        public string Id => mId;
        public int Step => mStep;

        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            mParent = gameObject.GetComponentInParent<PassTabUI>();
            mTextRequireValue = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireValue");

            var freeRewardSlotObj = gameObject.FindGameObject("FreeRewardSlot");
            mFreeSlotUI = freeRewardSlotObj.GetOrAddComponent<PassRewardItemSlotUI>();

            var payRewardSlotObj = gameObject.FindGameObject("PayRewardSlot");
            mPaySlotUI = payRewardSlotObj.GetOrAddComponent<PassRewardItemSlotUI>();
        }

        public void OnUpdateItem(int index)
        {
            PassStepData stepData = mParent?.GetStepData(index);
            if (stepData != null)
            {
                mId = stepData.id;
                mStep = stepData.step;
                mTextRequireValue.text = $"{stepData.requireValue}";
                mFreeSlotUI.Init(stepData, isFree: true);
                mPaySlotUI.Init(stepData, isFree: false);
            }
        }

        public void PlayRewardMotion(bool isFree)
        {
            if (isFree)
                mFreeSlotUI.PlayReceiveRewardMotion();
            else
                mPaySlotUI.PlayReceiveRewardMotion();
        }
    }

    class PassRewardItemSlotUI : MonoBehaviour
    {
        PassStepData mStepData;
        bool mIsFree;
        GameObject mRedDotObj;
        Image mImageModal;
        Image mImageLocked;
        Image mImageReceived;
        public void Init(PassStepData stepData, bool isFree)
        {
            mStepData = stepData;
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
            bool isSatisfied = Lance.Account.Pass.IsSatisfiedValue(mStepData.id, mStepData.step);
            bool isAlreadyReceived = Lance.Account.Pass.IsAlreadyReceived(mStepData.id, mStepData.step, mIsFree);
            bool isPurchased = mIsFree ? true : Lance.Account.Pass.IsPurchased(mStepData.id);

            mImageModal.gameObject.SetActive(isSatisfied == false || isAlreadyReceived);
            mImageReceived.gameObject.SetActive(isAlreadyReceived);
            mImageLocked.gameObject.SetActive(!isPurchased);

            mRedDotObj.SetActive(Lance.Account.Pass.CanReceiveReward(mStepData.id, mStepData.step, mIsFree));
        }

        void OnButtonAction()
        {
            if (Lance.Account.Pass.IsSatisfiedValue(mStepData.id, mStepData.step) == false)
                return;

            if (Lance.Account.Pass.IsAlreadyReceived(mStepData.id, mStepData.step, mIsFree))
                return;

            if (mIsFree == false && Lance.Account.Pass.IsPurchased(mStepData.id) == false)
                return;

            if (Lance.Account.Pass.ReceiveReward(mStepData.id, mStepData.step, mIsFree))
            {
                Refresh();

                Lance.GameManager.GiveReward(mIsFree ? mStepData.freeReward : mStepData.payReward);

                PlayReceiveRewardMotion();

                UIUtil.PopupRefreshRedDots<Popup_PassUI>();

                Lance.Lobby.RefreshPassRedDot();

                Param param = new Param();
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