using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Jousting_ShopTabUI : JoustingTabUI
    {
        List<JoustShopItemUI> mJoustItemList;
        public override void Init(JoustingTabUIManager parent, JoustingTab tab)
        {
            base.Init(parent, tab);

            var contentsObj = gameObject.FindGameObject("Contents");

            contentsObj.AllChildObjectOff();

            mJoustItemList = new List<JoustShopItemUI>();

            foreach (var data in Lance.GameData.JoustShopData.Values)
            {
                var JoustItemObj = Util.InstantiateUI("JoustingShopItemUI", contentsObj.transform);

                var JoustItemUI = JoustItemObj.GetOrAddComponent<JoustShopItemUI>();

                JoustItemUI.Init(this, data.id);

                mJoustItemList.Add(JoustItemUI);
            }

            Refresh();
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void Refresh()
        {
            foreach (var itemUI in mJoustItemList)
            {
                itemUI.Refresh();
            }
        }
    }

    class JoustShopItemUI : MonoBehaviour
    {
        string mId;
        Jousting_ShopTabUI mParent;
        TextMeshProUGUI mTextDailyPurchaseCount;
        GameObject mModalObj;
        public void Init(Jousting_ShopTabUI parent, string id)
        {
            mParent = parent;

            mId = id;

            JoustShopData data = Lance.GameData.JoustShopData.TryGet(id);
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
            mModalObj = gameObject.FindGameObject("Image_Modal");

            Refresh();
        }

        public void Refresh()
        {
            bool isEnoughPurchaseCount = Lance.Account.JoustShop.IsEnoughPurchaseCount(mId);

            mModalObj.SetActive(!isEnoughPurchaseCount);

            var data = Lance.GameData.JoustShopData.TryGet(mId);

            mTextDailyPurchaseCount.gameObject.SetActive(data.purchaseDailyCount > 0);

            StringParam param = new StringParam("remainCount", Lance.Account.JoustShop.GetRemainPurchaseCount(mId));
            param.AddParam("limitCount", data.purchaseDailyCount);

            mTextDailyPurchaseCount.text = StringTableUtil.Get("UIString_DailyPurchaseLimitCount", param);
        }

        void OnPurchaseButton()
        {
            var data = Lance.GameData.JoustShopData.TryGet(mId);
            if (data == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.JoustShop.GetRemainPurchaseCount(data.id) <= 0)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPurchaseCount");

                return;
            }

            var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI5>("Popup_ConfirmPurchaseUI2");

            popup.Init(data, () =>
            {
                mParent.Refresh();

                var parentPopup = Lance.PopupManager.GetPopup<Popup_JoustingUI>();
                parentPopup?.RefreshCurrencyUI();
            });
        }
    }
}
