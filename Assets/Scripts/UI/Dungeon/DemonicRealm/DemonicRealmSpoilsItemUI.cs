using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class DemonicRealmSpoilsItemUI : MonoBehaviour
    {
        string mId;
        Popup_DemonicRealmSpoilsUI mParent;
        TextMeshProUGUI mTextDailyPurchaseCount;
        TextMeshProUGUI mTextRequireFriendShip;
        GameObject mModalObj;
        public void Init(Popup_DemonicRealmSpoilsUI parent, string id)
        {
            mParent = parent;

            mId = id;

            DemonicRealmSpoilsData data = Lance.GameData.DemonicRealmSpoilsData.TryGet(id);
            RewardData rewardData = Lance.GameData.RewardData.TryGet(data.reward);
            ItemInfo itemInfo = new ItemInfo(rewardData);

            bool isRandomReward = rewardData.IsRandomReward();
            var currencyObj = gameObject.FindGameObject("Currency");
            var randomObj = gameObject.FindGameObject("Random");
            currencyObj.SetActive(!isRandomReward);
            randomObj.SetActive(isRandomReward);

            if (isRandomReward)
            {
                if (rewardData.randomEquipment.IsValid())
                {
                    var randomReward = Lance.GameData.RandomEquipmentRewardData.TryGet(rewardData.randomEquipment);

                    StringParam param = new StringParam("grade", randomReward.grade);

                    TextMeshProUGUI textRandomGrade = gameObject.FindComponent<TextMeshProUGUI>("Text_RandomGrade");
                    textRandomGrade.text = StringTableUtil.Get("UIString_RandomSelect", param);

                    Image imageRandomChest = gameObject.FindComponent<Image>("Image_RandomChest");
                    imageRandomChest.sprite = Lance.Atlas.GetUISprite(data.spriteImg);

                    var randomList = gameObject.FindGameObject("RandomList");

                    randomList.AllChildObjectOff();

                    for (int i = 0; i < (int)EquipmentType.Count; ++i)
                    {
                        ItemType itemType = ((EquipmentType)i).ChangeToItemType();

                        var randomSlotObj = Util.InstantiateUI("RandomSlotUI", randomList.transform);

                        var randomSlotUI = randomSlotObj.GetOrAddComponent<RandomSlotUI>();
                        randomSlotUI.Init(itemType);
                    }
                }
                else if (rewardData.randomSkill.IsValid())
                {
                    var randomReward = Lance.GameData.RandomSkillRewardData.TryGet(rewardData.randomSkill);

                    StringParam param = new StringParam("grade", randomReward.grade);

                    TextMeshProUGUI textRandomGrade = gameObject.FindComponent<TextMeshProUGUI>("Text_RandomGrade");
                    textRandomGrade.text = StringTableUtil.Get("UIString_RandomSelect", param);

                    Image imageRandomChest = gameObject.FindComponent<Image>("Image_RandomChest");
                    imageRandomChest.sprite = Lance.Atlas.GetUISprite(data.spriteImg);

                    var randomList = gameObject.FindGameObject("RandomList");

                    randomList.AllChildObjectOff();

                    var randomSlotObj = Util.InstantiateUI("RandomSlotUI", randomList.transform);

                    var randomSlotUI = randomSlotObj.GetOrAddComponent<RandomSlotUI>();
                    randomSlotUI.Init(ItemType.Skill);
                }
                else if (rewardData.randomAccessory.IsValid())
                {
                    var randomReward = Lance.GameData.RandomAccessoryRewardData.TryGet(rewardData.randomAccessory);

                    StringParam param = new StringParam("grade", randomReward.grade);

                    TextMeshProUGUI textRandomGrade = gameObject.FindComponent<TextMeshProUGUI>("Text_RandomGrade");
                    textRandomGrade.text = StringTableUtil.Get("UIString_RandomSelect", param);

                    Image imageRandomChest = gameObject.FindComponent<Image>("Image_RandomChest");
                    imageRandomChest.sprite = Lance.Atlas.GetUISprite(data.spriteImg);

                    var randomList = gameObject.FindGameObject("RandomList");

                    randomList.AllChildObjectOff();

                    for (int i = 0; i < (int)AccessoryType.Count; ++i)
                    {
                        ItemType itemType = ((AccessoryType)i).ChangeToItemType();

                        var randomSlotObj = Util.InstantiateUI("RandomSlotUI", randomList.transform);

                        var randomSlotUI = randomSlotObj.GetOrAddComponent<RandomSlotUI>();
                        randomSlotUI.Init(itemType);
                    }
                }
            }
            else
            {
                Image imageItemIcon = gameObject.FindComponent<Image>("Image_ItemIcon");
                imageItemIcon.sprite = itemInfo.GetSprite();

                TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_ItemName");
                textName.text = itemInfo.GetName();

                TextMeshProUGUI textAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_Amount");
                StringParam param = new StringParam("amount", itemInfo.GetAmountString());
                textAmount.text = StringTableUtil.Get("UIString_Amount", param);
            }

            TextMeshProUGUI textPrice = gameObject.FindComponent<TextMeshProUGUI>("Text_Price");
            textPrice.text = $"{data.price}";

            var buttonPurchase = gameObject.FindComponent<Button>("Button_Purchase");
            buttonPurchase.SetButtonAction(OnPurchaseButton);

            mTextDailyPurchaseCount = gameObject.FindComponent<TextMeshProUGUI>("Text_PurchaseCount");
            mTextRequireFriendShip = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireFriendShip");
            mModalObj = gameObject.FindGameObject("Image_Modal");

            Refresh();
        }

        public void Refresh()
        {
            var data = Lance.GameData.DemonicRealmSpoilsData.TryGet(mId);

            int friendShipLevel = Lance.Account.DemonicRealmSpoils.GetFriendShipLevel();
            bool isSatisfiedRequireFriendShipLevel = data.requireFriendShipLevel <= friendShipLevel;
            int remainPurchaseCount = Lance.Account.DemonicRealmSpoils.GetRemainPurchaseCount(mId);
            bool isEnoughPurchaseCount = Lance.Account.DemonicRealmSpoils.IsEnoughPurchaseCount(mId, 1);

            mModalObj.SetActive(!isEnoughPurchaseCount || !isSatisfiedRequireFriendShipLevel);

            mTextDailyPurchaseCount.gameObject.SetActive(data.purchaseDailyCount > 0);
            StringParam param = new StringParam("remainCount", Lance.Account.DemonicRealmSpoils.GetRemainPurchaseCount(mId));
            param.AddParam("limitCount", data.purchaseDailyCount);
            mTextDailyPurchaseCount.text = StringTableUtil.Get("UIString_DailyPurchaseLimitCount", param);

            mTextRequireFriendShip.gameObject.SetActive(!isSatisfiedRequireFriendShipLevel);
            StringParam requireFriendShipParam = new StringParam("level", data.requireFriendShipLevel);
            mTextRequireFriendShip.text = StringTableUtil.Get("UIString_RequireFriendShipLevel", requireFriendShipParam);
        }

        void OnPurchaseButton()
        {
            var data = Lance.GameData.DemonicRealmSpoilsData.TryGet(mId);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.DemonicRealmSpoils.IsEnoughPurchaseCount(data.id, 1) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return;
            }

            var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI6>("Popup_ConfirmPurchaseUI2");
            popup.Init(data, () =>
            {
                mParent.Refresh();

                mParent.PlayHeartExplosion();
            });
        }
    }
}
