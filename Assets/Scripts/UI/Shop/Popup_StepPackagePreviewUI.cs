using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Popup_StepPackagePreviewUI : PopupBase
    {
        string mPackageId;
        int mSelectedStep;
        TextMeshProUGUI mTextPackageName;
        TextMeshProUGUI mTextPrice;
        List<ItemSlotUI> mRewardSlotUIList;
        public void Init(string id)
        {
            mPackageId = id;
            mSelectedStep = 1;

            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_PackagePreview"));

            mTextPackageName = gameObject.FindComponent<TextMeshProUGUI>("Text_PackageName");
            mTextPrice = gameObject.FindComponent<TextMeshProUGUI>("Text_Price");

            var buttonPrev = gameObject.FindComponent<Button>("Button_Prev");
            buttonPrev.SetButtonAction(() => OnPreviewButton(true));
            var buttonNext = gameObject.FindComponent<Button>("Button_Next");
            buttonNext.SetButtonAction(() => OnPreviewButton(false));

            mRewardSlotUIList = new List<ItemSlotUI>();

            Refresh();
        }

        void OnPreviewButton(bool isPrev)
        {
            int maxStep = DataUtil.GetPackageShopDataMaxStep(mPackageId);

            mSelectedStep += isPrev ? -1 : +1;

            mSelectedStep = Mathf.Clamp(mSelectedStep, 1, maxStep);

            Refresh();
        }

        void Refresh()
        {
            var data = DataUtil.GetPackageShopData(mPackageId, mSelectedStep);
            if (data != null)
            {
                // 이름
                mTextPackageName.text = StringTableUtil.GetName($"{data.id}{mSelectedStep}");

                // 가격
                mTextPrice.text = Lance.IAPManager.GetPrcieString(data.productId);

                // 보상
                RewardData reward = Lance.GameData.RewardData.TryGet(data.reward);
                if (reward != null)
                {
                    List<ItemInfo> itemInfos = ItemInfoUtil.CreateItemInfos(reward);

                    if (data.mileage > 0)
                    {
                        var mileageItemInfo = new ItemInfo(ItemType.Mileage, data.mileage);

                        itemInfos.Add(mileageItemInfo);
                    }

                    if(mRewardSlotUIList.Count > 0)
                    {
                        foreach(var rewardSlot in mRewardSlotUIList)
                        {
                            rewardSlot.gameObject.Destroy();
                        }

                        mRewardSlotUIList.Clear();
                    }

                    var rewardsObj = gameObject.FindGameObject("Rewards");

                    rewardsObj.AllChildObjectOff();

                    if (itemInfos != null)
                    {
                        for (int i = 0; i < itemInfos.Count; ++i)
                        {
                            var rewardSlotObj = Util.InstantiateUI("PackagePopupRewardSlotUI", rewardsObj.transform);

                            var rewardSlotUI = rewardSlotObj.GetOrAddComponent<ItemSlotUI>();

                            rewardSlotUI.Init(itemInfos[i]);

                            mRewardSlotUIList.Add(rewardSlotUI);
                        }
                    }
                }
            }
        }
    }
}