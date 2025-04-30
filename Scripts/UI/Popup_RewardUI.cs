using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_RewardUI : PopupBase
    {
        const float TimeUpdateInterval = 1f;
        float mTimeUpdateInterval;
        float mCloseTime;

        List<ItemSlotUI> mRewardSlotUIList;

        TextMeshProUGUI mTextCloseTime;
        public void Init(RewardResult rewardResult, string title, float closeTime)
        {
            Button modalButton = gameObject.FindComponent<Button>("Button_Modal");

            modalButton.SetButtonAction(() => Close());

            title = title.IsValid() ? title : StringTableUtil.Get("Title_Reward");

            SetTitleText(title);

            mTextCloseTime = gameObject.FindComponent<TextMeshProUGUI>("Text_PopupClose");

            var rewardListObj = gameObject.FindGameObject("RewardList");

            rewardListObj.AllChildObjectOff();

            var rewardListRectTm = rewardListObj.GetComponent<RectTransform>();

            mRewardSlotUIList = ItemSlotUIUtil.CreateItemSlotUIList(rewardListRectTm, rewardResult);

            SoundPlayer.PlayShowReward();

            mCloseTime = closeTime;

            mTextCloseTime.gameObject.SetActive(mCloseTime > 0);

            StartCoroutine(DelayedInit());
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            base.Close(immediate, hideMotion);

            if (mRewardSlotUIList != null)
            {
                foreach(var rewardSlot in mRewardSlotUIList)
                {
                    Lance.ObjectPool.ReleaseUI(rewardSlot.gameObject);
                }

                mRewardSlotUIList.Clear();
                mRewardSlotUIList = null;
            }
        }

        IEnumerator DelayedInit()
        {
            yield return null;
            yield return null;
            yield return null;

            var scrollView = gameObject.GetComponentInChildren<ScrollRect>();
            if (scrollView != null)
                scrollView.verticalNormalizedPosition = 1f;
        }

        private void Update()
        {
            if (mCloseTime > 0)
            {
                float dt = Time.unscaledDeltaTime;
                mTimeUpdateInterval -= dt;
                if (mTimeUpdateInterval <= 0f)
                {
                    mTimeUpdateInterval = TimeUpdateInterval;
                    mCloseTime -= (int)TimeUpdateInterval;

                    StringParam param = new StringParam("sec", (int)mCloseTime);
                    mTextCloseTime.text = StringTableUtil.Get("UIString_ClosePopup", param);

                    if (mCloseTime <= 0 && mClosing == false)
                    {
                        Close();
                    }
                }
            }
        }
    }
}