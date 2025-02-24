using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Shop_NormalTabUI : ShopTabUI
    {
        List<NormalShopItemUI> mNormalItemList;
        public override void Init(ShopTab tab)
        {
            base.Init(tab);

            var contentsObj = gameObject.FindGameObject("Contents");

            contentsObj.AllChildObjectOff();

            mNormalItemList = new List<NormalShopItemUI>();

            foreach (var data in Lance.GameData.NormalShopData.Values)
            {
                var normalItemObj = Util.InstantiateUI("NormalItemUI", contentsObj.transform);

                var normalItemUI = normalItemObj.GetOrAddComponent<NormalShopItemUI>();

                normalItemUI.Init(this, data.id);

                mNormalItemList.Add(normalItemUI);
            }

            Refresh();
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Localize()
        {
            foreach (var itemUI in mNormalItemList)
            {
                itemUI.Localize();
            }
        }

        public override void Refresh()
        {
            foreach (var itemUI in mNormalItemList)
            {
                itemUI.Refresh();
            }
        }
    }

    class NormalShopItemUI : MonoBehaviour
    {
        string mId;
        Shop_NormalTabUI mParent;
        TextMeshProUGUI mTextDailyPurchaseCount;
        GameObject mModalObj;
        public void Init(Shop_NormalTabUI parent, string id)
        {
            mParent = parent;

            mId = id;

            NormalShopData data = Lance.GameData.NormalShopData.TryGet(id);
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

            mTextDailyPurchaseCount = gameObject.FindComponent<TextMeshProUGUI>("Text_DailyPurchaseCount");
            mModalObj = gameObject.FindGameObject("Image_Modal");

            Refresh();
        }

        public void Localize()
        {
            var data = Lance.GameData.NormalShopData.TryGet(mId);
            RewardData rewardData = Lance.GameData.RewardData.TryGet(data.reward);
            ItemInfo itemInfo = new ItemInfo(rewardData);

            TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_ItemName");
            textName.text = itemInfo.GetName();

            StringParam param = new StringParam("remainCount", Lance.Account.NormalShop.GetRemainPurchaseCount(mId));
            param.AddParam("limitCount", data.purchaseDailyCount);

            mTextDailyPurchaseCount.text = StringTableUtil.Get("UIString_DailyPurchaseLimitCount", param);
        }

        public void Refresh()
        {
            bool isEnoughPurchaseCount = Lance.Account.NormalShop.IsEnoughPurchaseCount(mId);

            mModalObj.SetActive(!isEnoughPurchaseCount);

            var data = Lance.GameData.NormalShopData.TryGet(mId);

            mTextDailyPurchaseCount.gameObject.SetActive(data.purchaseDailyCount > 0);

            StringParam param = new StringParam("remainCount", Lance.Account.NormalShop.GetRemainPurchaseCount(mId));
            param.AddParam("limitCount", data.purchaseDailyCount);

            mTextDailyPurchaseCount.text = StringTableUtil.Get("UIString_DailyPurchaseLimitCount", param);
        }

        void OnPurchaseButton()
        {
            var data = Lance.GameData.NormalShopData.TryGet(mId);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.NormalShop.GetRemainPurchaseCount(data.id) <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return;
            }

            var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI2>();
            popup.Init(data, mParent.Refresh);
        }
    }
}