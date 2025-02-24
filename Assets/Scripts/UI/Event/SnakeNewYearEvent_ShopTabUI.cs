using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


namespace Lance
{
    class SnakeNewYearEvent_ShopTabUI : SnakeNewYearTabUI
    {
        List<SnakeNewYearEventShopItemUI> mEventItemList;
        public override void Init(SnakeNewYearTabUIManager parent, string eventId, SnakeNewYearTab tab)
        {
            base.Init(parent, eventId, tab);

            var contentsObj = gameObject.FindGameObject("Contents");

            contentsObj.AllChildObjectOff();

            mEventItemList = new List<SnakeNewYearEventShopItemUI>();

            foreach (var data in Lance.GameData.EventShopData.Values.Where(x => x.eventId == eventId))
            {
                var eventItemObj = Util.InstantiateUI("SnakeNewYearEventShopItemUI", contentsObj.transform);

                var eventItemUI = eventItemObj.GetOrAddComponent<SnakeNewYearEventShopItemUI>();

                eventItemUI.Init(this, eventId, data.id);

                mEventItemList.Add(eventItemUI);
            }

            Refresh();
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Refresh()
        {
            foreach (var itemUI in mEventItemList)
            {
                itemUI.Refresh();
            }
        }
    }

    class SnakeNewYearEventShopItemUI : MonoBehaviour
    {
        string mEventId;
        string mId;
        SnakeNewYearEvent_ShopTabUI mParent;
        TextMeshProUGUI mTextPurchaseCount;
        GameObject mModalObj;
        public void Init(SnakeNewYearEvent_ShopTabUI parent, string eventId, string id)
        {
            mParent = parent;
            mEventId = eventId;
            mId = id;

            EventShopData data = Lance.GameData.EventShopData.TryGet(id);
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

            mTextPurchaseCount = gameObject.FindComponent<TextMeshProUGUI>("Text_PurchaseCount");
            mModalObj = gameObject.FindGameObject("Image_Modal");

            Refresh();
        }

        public void Refresh()
        {
            int remainCount = Lance.Account.Event.GetRemainPurchaseCount(mEventId, mId);

            mModalObj.SetActive(remainCount <= 0);

            var data = Lance.GameData.EventShopData.TryGet(mId);

            mTextPurchaseCount.gameObject.SetActive(data.purchaseCount > 0);

            StringParam param = new StringParam("remainCount", remainCount);
            param.AddParam("limitCount", data.purchaseCount);

            mTextPurchaseCount.text = StringTableUtil.Get("UIString_PurchaseLimitCount", param);
        }

        void OnPurchaseButton()
        {
            var data = Lance.GameData.EventShopData.TryGet(mId);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.Event.GetRemainPurchaseCount(mEventId, mId) <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return;
            }

            var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI4>("Popup_ConfirmPurchaseUI2");

            popup.Init(data, OnConfirm);

            void OnConfirm()
            {
                var popup = Lance.PopupManager.GetPopup<Popup_NewEventUI>();

                popup.Refresh();
            }
        }
    }
}