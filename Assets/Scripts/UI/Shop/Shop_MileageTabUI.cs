using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Shop_MileageTabUI : ShopTabUI
    {
        TextMeshProUGUI mTextMyMileage;
        List<MileageShopItemUI> mMilieageItemList;
        public override void Init(ShopTab tab)
        {
            base.Init(tab);

            mTextMyMileage = gameObject.FindComponent<TextMeshProUGUI>("Text_MyMileage");

            var contentsObj = gameObject.FindGameObject("Contents");

            contentsObj.AllChildObjectOff();

            mMilieageItemList = new List<MileageShopItemUI>();

            foreach(var data in Lance.GameData.MileageShopData.Values)
            {
                var mileageItemObj = Util.InstantiateUI("MileageItemUI", contentsObj.transform);

                var mileageItemUI = mileageItemObj.GetOrAddComponent<MileageShopItemUI>();

                mileageItemUI.Init(this, data.id);

                mMilieageItemList.Add(mileageItemUI);
            }

            Refresh();
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Localize()
        {
            foreach (var itemUI in mMilieageItemList)
            {
                itemUI.Localize();
            }
        }

        public override void Refresh()
        {
            mTextMyMileage.text = $"{Lance.Account.MileageShop.GetMileage()}";

            foreach (var itemUI in mMilieageItemList)
            {
                itemUI.Refresh();
            }
        }
    }

    class MileageShopItemUI : MonoBehaviour
    {
        string mId;
        Shop_MileageTabUI mParent;
        TextMeshProUGUI mTextDailyPurchaseCount;
        GameObject mModalObj;
        public void Init(Shop_MileageTabUI parent, string id)
        {
            mParent = parent;

            mId = id;

            MileageShopData data = Lance.GameData.MileageShopData.TryGet(id);
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

                    for(int i = 0; i < (int)EquipmentType.Count; ++i)
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
            MileageShopData data = Lance.GameData.MileageShopData.TryGet(mId);
            RewardData rewardData = Lance.GameData.RewardData.TryGet(data.reward);
            ItemInfo itemInfo = new ItemInfo(rewardData);

            if (rewardData.randomEquipment.IsValid())
            {
                var randomReward = Lance.GameData.RandomEquipmentRewardData.TryGet(rewardData.randomEquipment);

                StringParam param0 = new StringParam("grade", randomReward.grade);

                TextMeshProUGUI textRandomGrade = gameObject.FindComponent<TextMeshProUGUI>("Text_RandomGrade");
                textRandomGrade.text = StringTableUtil.Get("UIString_RandomSelect", param0);
            }
            else if (rewardData.randomSkill.IsValid())
            {
                var randomReward = Lance.GameData.RandomSkillRewardData.TryGet(rewardData.randomSkill);

                StringParam param1 = new StringParam("grade", randomReward.grade);

                TextMeshProUGUI textRandomGrade = gameObject.FindComponent<TextMeshProUGUI>("Text_RandomGrade");
                textRandomGrade.text = StringTableUtil.Get("UIString_RandomSelect", param1);
            }
            else if (rewardData.randomAccessory.IsValid())
            {
                var randomReward = Lance.GameData.RandomAccessoryRewardData.TryGet(rewardData.randomAccessory);

                StringParam param2 = new StringParam("grade", randomReward.grade);

                TextMeshProUGUI textRandomGrade = gameObject.FindComponent<TextMeshProUGUI>("Text_RandomGrade");
                textRandomGrade.text = StringTableUtil.Get("UIString_RandomSelect", param2);
            }

            TextMeshProUGUI textName = gameObject.FindComponent<TextMeshProUGUI>("Text_ItemName");
            textName.text = itemInfo.GetName();

            int remainCount = Lance.Account.MileageShop.GetRemainPurchaseCount(mId);

            StringParam param = new StringParam("remainCount", remainCount);
            param.AddParam("limitCount", data.purchaseDailyCount);

            mTextDailyPurchaseCount.text = StringTableUtil.Get("UIString_DailyPurchaseLimitCount", param);
        }

        public void Refresh()
        {
            int remainCount = Lance.Account.MileageShop.GetRemainPurchaseCount(mId);

            mModalObj.SetActive(remainCount <= 0);

            var data = Lance.GameData.MileageShopData.TryGet(mId);

            mTextDailyPurchaseCount.gameObject.SetActive(data.purchaseDailyCount > 0);

            StringParam param = new StringParam("remainCount", remainCount);
            param.AddParam("limitCount", data.purchaseDailyCount);

            mTextDailyPurchaseCount.text = StringTableUtil.Get("UIString_DailyPurchaseLimitCount", param);
        }

        void OnPurchaseButton()
        {
            var data = Lance.GameData.MileageShopData.TryGet(mId);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.MileageShop.GetRemainPurchaseCount(data.id) <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return ;
            }

            var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI3>("Popup_ConfirmPurchaseUI2");
            popup.Init(data, mParent.Refresh);
        }
    }
}
