using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class StatureCostume_CostumeTabUI : StatureCostumeTabUI
    {
        string mSelectedCostume;
        TextMeshProUGUI mTextCostumeName;

        GameObject mPurchaseObj;
        Button mButtonPurchaseCash;
        TextMeshProUGUI mTextPurchaseCashPrice;
        TextMeshProUGUI mTextMileage;

        Button mButtonPurchaseGem;
        TextMeshProUGUI mTextPurchaseGemPrice;

        GameObject mEquipObj;
        Button mButtonEquip;
        GameObject mHowToGetObj;
        TextMeshProUGUI mTextEquip;
        TextMeshProUGUI mTextHowToGet;

        GameObject mUpgradeObj;
        Button mButtonUpgrade;

        Button mButtonInfo;

        RawImage mCostumePreview;
        TextMeshProUGUI mTextLevel;
        Image mImaeGrade;

        CostumeTabUIManager mTabUIManager;
        public override void Init(StatureCostumeTabUIManager parent, StatureCostumeTab tab)
        {
            base.Init(parent, tab);

            mTextCostumeName = gameObject.FindComponent<TextMeshProUGUI>("Text_CostumeName");

            mPurchaseObj = gameObject.FindGameObject("Purchase");
            mButtonPurchaseCash = mPurchaseObj.FindComponent<Button>("Button_CashPurchase");
            mButtonPurchaseCash.SetButtonAction(OnPurchaseButton);
            mTextPurchaseCashPrice = mPurchaseObj.FindComponent<TextMeshProUGUI>("Text_Price");
            mTextMileage = mPurchaseObj.FindComponent<TextMeshProUGUI>("Text_Mileage");

            mButtonPurchaseGem = mPurchaseObj.FindComponent<Button>("Button_GemPurchase");
            mButtonPurchaseGem.SetButtonAction(OnPurchaseButton);
            mTextPurchaseGemPrice = mPurchaseObj.FindComponent<TextMeshProUGUI>("Text_GemPrice");

            mEquipObj = gameObject.FindGameObject("Equip");
            mButtonEquip = mEquipObj.FindComponent<Button>("Button_Equip");
            mButtonEquip.SetButtonAction(OnEquipButton);
            mTextEquip = mEquipObj.FindComponent<TextMeshProUGUI>("Text_Equip");
            mHowToGetObj = gameObject.FindGameObject("HowToGet");
            mTextHowToGet = mHowToGetObj.FindComponent<TextMeshProUGUI>("Text_HowToGet");

            mUpgradeObj = gameObject.FindGameObject("Upgrade");
            mButtonUpgrade = mUpgradeObj.FindComponent<Button>("Button_Upgrade");
            mButtonUpgrade.SetButtonAction(OnUpgradeButton);

            mButtonInfo = gameObject.FindComponent<Button>("Button_CostumeInfo");
            mButtonInfo.SetButtonAction(OnUpgradeButton);

            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_CostumeLevel");
            mImaeGrade = gameObject.FindComponent<Image>("Image_CostumeGrade");

            mTabUIManager = new CostumeTabUIManager();
            mTabUIManager.Init(this);

            mCostumePreview = gameObject.FindComponent<RawImage>("Preview");

            RefreshCostume();
        }

        public override void OnEnter()
        {
            Lance.GameManager.SetActiveCostumePreview(true);

            if (mCostumePreview.texture == null)
                mCostumePreview.texture = Lance.GameManager.GetCostumePreivewRenderTexture();

            RefreshCostume();

            mTabUIManager.AllTabRefreshCostume();
        }

        public override void OnLeave()
        {
            mTabUIManager.OnLeave();

            Lance.GameManager.SetActiveCostumePreview(false);
        }

        public override void Localize()
        {
            RefreshCostume();

            mTabUIManager.Localize();
        }

        public override void Refresh()
        {
            RefreshCostume();

            mTabUIManager.Refresh();
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }

        public void OnSelectCostume(string selectedCostume)
        {
            if (mSelectedCostume == selectedCostume)
                return;

            mSelectedCostume = selectedCostume;

            RefreshCostume();

            if (Lance.LocalSave.IsNewCostume(mSelectedCostume))
            {
                Lance.LocalSave.AddGetCostume(mSelectedCostume);

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
            }
        }

        void RefreshCostume()
        {
            var costumeData = DataUtil.GetCostumeData(mSelectedCostume);
            if (costumeData != null)
            {
                StringParam levelParam = new StringParam("level", Lance.Account.GetCostumeLevel(mSelectedCostume));

                mTextLevel.text = StringTableUtil.Get("UIString_LevelValue", levelParam);
                mImaeGrade.sprite = Lance.Atlas.GetIconGrade(costumeData.grade);
                mTextCostumeName.text = StringTableUtil.GetName(mSelectedCostume);

                bool haveCostume = Lance.Account.Costume.HaveCostume(mSelectedCostume);
                bool isEquippedCostume = Lance.Account.Costume.IsEquipped(mSelectedCostume);
                bool canUpgrade = Lance.Account.CanUpgradeCostume(mSelectedCostume);

                if (haveCostume)
                {
                    mPurchaseObj.SetActive(false);
                    mEquipObj.SetActive(true);
                    mUpgradeObj.SetActive(true);

                    mButtonEquip.SetActiveFrame(!isEquippedCostume, Const.CostumeEquipButtonFrame, Const.CostumeEquippedButtonFrame, Const.DefaultActiveTextColor, Const.NotEnoughTextColor);
                    mButtonInfo.gameObject.SetActive(false);
                    //mButtonUpgrade.SetActiveFrame(canUpgrade, Const.CostumeEquipButtonFrame, Const.CostumeEquippedButtonFrame, Const.DefaultActiveTextColor, Const.NotEnoughTextColor);

                    string equipString = StringTableUtil.Get("UIString_Equip");
                    string equippedString = StringTableUtil.Get("UIString_Equipped");

                    mTextEquip.text = isEquippedCostume ? equippedString : equipString;
                }
                else
                {
                    mButtonInfo.gameObject.SetActive(true);

                    if (costumeData.productId.IsValid())
                    {
                        mPurchaseObj.SetActive(true);
                        mEquipObj.SetActive(false);
                        mUpgradeObj.SetActive(false);

                        mHowToGetObj.SetActive(false);
                        mButtonPurchaseGem.gameObject.SetActive(false);
                        mButtonPurchaseCash.gameObject.SetActive(true);

                        mTextPurchaseCashPrice.text = Lance.IAPManager.GetPrcieString(costumeData.productId);
                        mTextMileage.text = $"{costumeData.mileage}";
                    }
                    else
                    {
                        if (costumeData.gemPrice > 0)
                        {
                            if (costumeData.requireLimitBreak > 0)
                            {
                                if (costumeData.requireLimitBreak > Lance.Account.ExpLevel.GetLimitBreak())
                                {
                                    mPurchaseObj.SetActive(true);
                                    mEquipObj.SetActive(false);
                                    mUpgradeObj.SetActive(true);
                                    mHowToGetObj.SetActive(true);
                                    mButtonPurchaseGem.gameObject.SetActive(true);
                                    mButtonPurchaseCash.gameObject.SetActive(false);

                                    StringParam param = new StringParam("limitBreak", costumeData.requireLimitBreak);

                                    mTextHowToGet.text = StringTableUtil.Get("UIString_RequireLimitBreak", param);
                                }
                                else
                                {
                                    RefresGemPrice();
                                }
                            }
                            else
                            {
                                RefresGemPrice();
                            }

                            void RefresGemPrice()
                            {
                                mPurchaseObj.SetActive(true);
                                mEquipObj.SetActive(false);
                                mUpgradeObj.SetActive(false);
                                mHowToGetObj.SetActive(false);
                                mButtonPurchaseGem.gameObject.SetActive(true);
                                mButtonPurchaseCash.gameObject.SetActive(false);

                                bool isEnoughGem = Lance.Account.IsEnoughGem(costumeData.gemPrice);

                                mButtonPurchaseGem.SetActiveFrame(isEnoughGem, Const.CostumePurchaseActiveButtonFrame, Const.CostumePurchaseInactiveButtonFrame, Const.EnoughTextColor, Const.NotEnoughTextColor);

                                mTextPurchaseGemPrice.text = $"{costumeData.gemPrice}";
                            }
                        }
                        else if (costumeData.eventId.IsValid())
                        {
                            mPurchaseObj.SetActive(true);
                            mEquipObj.SetActive(true);
                            mUpgradeObj.SetActive(true);
                            mHowToGetObj.SetActive(true);
                            mButtonPurchaseGem.gameObject.SetActive(false);
                            mButtonPurchaseCash.gameObject.SetActive(false);

                            mTextHowToGet.text = StringTableUtil.GetName(costumeData.eventId);//StringTableUtil.Get("Desc_OnlyObtainEvent", passNameParam);
                        }
                    }
                }
            }
        }

        void OnPurchaseButton()
        {
            if (mSelectedCostume.IsValid() == false)
                return;

            Lance.GameManager.PurchaseCostume(mSelectedCostume, OnFinishPurchase);

            void OnFinishPurchase()
            {
                RefreshCostume();
                RefreshRedDots();
                mTabUIManager.RefreshCostumeUIs(mSelectedCostume);
                Lance.Lobby.RefreshTabRedDot(LobbyTab.Stature);
            }
        }

        void OnEquipButton()
        {
            if (mSelectedCostume.IsValid() == false)
                return;

            Lance.GameManager.EquipCostume(mSelectedCostume);

            RefreshCostume();

            mTabUIManager.RefreshCostumeUIs(mSelectedCostume);

            Lance.Lobby.UpdatePortrait();
        }

        void OnUpgradeButton()
        {
            if (mSelectedCostume.IsValid() == false)
                return;

            var popup = Lance.PopupManager.CreatePopup<Popup_CostumeInfoUI>();
            popup.Init(mSelectedCostume);
        }
    }
}