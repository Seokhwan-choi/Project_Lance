using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Lance
{
    class Popup_StageClearUI : PopupBase
    {
        float mCloseTime;
        public void Init(RewardResult clearReward, float closeTime = 1.5f)
        {
            var rewardSlotObj = gameObject.FindGameObject("RewardSlot");

            var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();

            rewardSlotUI.Init(new ItemInfo(clearReward));

            //var buttonModal = gameObject.FindComponent<Button>("Button_Modal");
            //buttonModal.SetButtonAction(() => Close());

            SoundPlayer.PlayShowReward();

            mCloseTime = closeTime;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (mCloseTime > 0)
            {
                mCloseTime -= dt;
                if (mCloseTime <= 0f)
                {
                    Close();
                }
            }
        }
    }
}