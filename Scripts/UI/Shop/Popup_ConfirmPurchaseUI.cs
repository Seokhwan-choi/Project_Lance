using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class Popup_ConfirmPurchaseUI : PopupBase
    {
        public void Init(string name, ItemInfo[] rewards, Action onConfirm)
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_Purchase"));

            var textPackageName = gameObject.FindComponent<TextMeshProUGUI>("Text_PackageName");
            textPackageName.text = name;

            var buttonCancle = gameObject.FindComponent<Button>("Button_Cancle");
            buttonCancle.SetButtonAction(() => OnClose());

            var buttonConfirm = gameObject.FindComponent<Button>("Button_Confirm");
            buttonConfirm.SetButtonAction(() =>
            {
                onConfirm?.Invoke();

                Close();
            });

            var rewardsObj = gameObject.FindGameObject("Rewards");

            rewardsObj.AllChildObjectOff();

            if (rewards != null)
            {
                for (int i = 0; i < rewards.Length; ++i)
                {
                    var rewardSlotObj = Util.InstantiateUI("PackagePopupRewardSlotUI", rewardsObj.transform);

                    var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();

                    rewardSlotUI.Init(rewards[i]);
                }
            }
        }
    }
}