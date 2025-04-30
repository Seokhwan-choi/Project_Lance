using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Mosframe;
using BackEnd;

namespace Lance
{
    class PassSubTabUIManager
    {
        string mSelectedId;
        GameObject mRedDotObj;
        Button mButtonAllReceive;
        Button mButtonPurchase;
        TextMeshProUGUI mTextPrice;
        TextMeshProUGUI mTextMileage;
        PassTabUI mParent;
        PassSubNavBarUI mNavBarUI;
        DynamicVScrollView mDynamicScrollView;
        public PassTab Tab => mParent.Tab;
        public PassTabUI Parent => mParent;
        public string SelectedId => mSelectedId;
        public void Init(PassTabUI parent)
        {
            mParent = parent;
            mDynamicScrollView = parent.gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            
            mNavBarUI = new PassSubNavBarUI();
            mNavBarUI.Init(this);

            mButtonAllReceive = parent.gameObject.FindComponent<Button>("Button_AllReceive");
            mButtonAllReceive.SetButtonAction(OnAllReceiveButton);

            mRedDotObj = mButtonAllReceive.gameObject.FindGameObject("RedDot");

            mButtonPurchase = parent.gameObject.FindComponent<Button>("Button_Purchase");
            mButtonPurchase.SetButtonAction(OnPurchaseButton);
            mTextPrice = mButtonPurchase.gameObject.FindComponent<TextMeshProUGUI>("Text_Price");
            mTextMileage = parent.gameObject.FindComponent<TextMeshProUGUI>("Text_Mileage");
            PassType passType = mParent.Tab.ChangeToPassType();
            PassData data = DataUtil.GetPassDatas(passType).FirstOrDefault();
            mSelectedId = data?.id ?? string.Empty;

            Refresh();
        }

        public PassStepData GetSelectedPassStepData(int index)
        {
            return GetPassStepData(index);
        }

        public void OnRelease()
        {
            mNavBarUI.OnRelease();
            mNavBarUI = null;

            mParent = null;
            mDynamicScrollView = null;

            mButtonAllReceive = null;
            mRedDotObj = null;
            mButtonPurchase = null;
            mTextPrice = null;
            mTextMileage = null;
            mSelectedId = null;
        }

        PassStepData GetPassStepData(int index)
        {
            if (mSelectedId.IsValid())
            {
                PassStepData[] datas = DataUtil.GetPassStepDatas(mSelectedId).ToArray();
                if (datas.Length > index && index != -1)
                    return datas[index];
            }

            return null;
        }

        void OnAllReceiveButton()
        {
            var result = Lance.Account.Pass.ReceiveAllReward(mSelectedId);

            mDynamicScrollView.refresh();

            if (result.free != null && result.pay != null)
            {
                RewardResult rewardResult = new RewardResult();

                AddReward(result.free, isFree:true);
                AddReward(result.pay, isFree:false);

                void AddReward(List<int> steps, bool isFree)
                {
                    foreach (int step in steps)
                    {
                        var stepData = DataUtil.GetPassStepData(mSelectedId, step);
                        if (stepData != null)
                        {
                            var reward = Lance.GameData.RewardData.TryGet(isFree ? stepData.freeReward : stepData.payReward);
                            if (reward != null)
                            {
                                rewardResult = rewardResult.AddReward(reward);

                                PlayRewardMotion(step, isFree);
                            }
                        }
                    }
                }

                if (rewardResult.IsEmpty() == false)
                {
                    Lance.GameManager.GiveReward(rewardResult, ShowRewardType.Popup);

                    UIUtil.PopupRefreshRedDots<Popup_PassUI>();

                    Lance.Lobby.RefreshPassRedDot();

                    Param param = new Param();
                    param.Add("id", mSelectedId);
                    param.Add("result_free", result.free);
                    param.Add("result_pay", result.pay);
                    param.Add("isPurchased", Lance.Account.Pass.IsPurchased(mSelectedId));

                    Lance.BackEnd.InsertLog("AllReceivePassreward", param, 7);
                }
                else
                {
                    UIUtil.ShowSystemErrorMessage("NoReward");
                }
            }
        }

        public void RefreshRedDots()
        {
            mNavBarUI.RefreshRedDots();

            bool anyCanReceiveReward = Lance.Account.Pass.AnyCanReceiveReward(mSelectedId);
            mButtonAllReceive.SetActiveFrame(anyCanReceiveReward, 
                Const.PassAllReceiveActiveButtonFrame, Const.PassAllReceiveInactiveButtonFrame);
            mRedDotObj.SetActive(anyCanReceiveReward);
        }

        void PlayRewardMotion(int step, bool isFree)
        {
            foreach (PassItemUI passItem in GetPassItems())
            {
                if (passItem.Id == mSelectedId &&
                    passItem.Step == step)
                {
                    passItem.PlayRewardMotion(isFree);
                }
            }
        }

        IEnumerable<PassItemUI> GetPassItems()
        {
            foreach (var rect in mDynamicScrollView.GetContainters())
                yield return rect.GetComponent<PassItemUI>();
        }

        void OnPurchaseButton()
        {
            var passData = Lance.GameData.PassData.TryGet(mSelectedId);
            if (passData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.Pass.IsPurchased(mSelectedId))
            {
                UIUtil.ShowSystemErrorMessage("AlreadyPurchasedPass");

                return;
            }

            string name = StringTableUtil.GetName(mSelectedId);

            var mileageReward = new ItemInfo(ItemType.Mileage, passData.mileage);
            ItemInfo[] rewards = new ItemInfo[] { mileageReward };

            var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI>();
            popup.Init(name, rewards, OnConfirm);

            void OnConfirm()
            {
                string selectedId = mSelectedId;

                Lance.IAPManager.Purchase(passData.productId, OnFinishPurchased);

                void OnFinishPurchased()
                {
                    // 구매 처리
                    if (Lance.Account.Pass.Purchase(selectedId))
                    {
                        var mileageReward = new RewardResult();

                        mileageReward.mileage = passData.mileage;
                        // To Do 구매 완료 연출...?
                        if (mileageReward.IsEmpty() == false)
                        {
                            Lance.GameManager.GiveReward(mileageReward, ShowRewardType.Popup);
                        }

                        mDynamicScrollView.refresh();

                        UIUtil.PopupRefreshRedDots<Popup_PassUI>();

                        Lance.Lobby.RefreshPassRedDot();

                        RefreshPurchaseButton();

                        Lance.Account.UserInfo.StackPayments(passData.price);

                        Lance.GameManager.CheckQuest(QuestType.Payments, passData.price);

                        Lance.Lobby.RefreshEventRedDot();
                        Lance.Lobby.RefreshQuestRedDot();

                        Param param = new Param();
                        param.Add("id", selectedId);
                        param.Add("stackedPayments", Lance.Account.UserInfo.GetStackedPayments());
                        param.Add("mileage", passData.mileage);

                        Lance.BackEnd.InsertLog("PurchasePass", param, 7);

                        Lance.BackEnd.UpdateAllAccountInfos();
                    }
                }
            }
        }

        void RefreshPurchaseButton()
        {
            if (mSelectedId.IsValid())
            {
                string passName = StringTableUtil.GetName(mSelectedId);

                StringParam param = new StringParam("passName", passName);

                string text = StringTableUtil.Get("UIString_PurchasePass", param);

                bool isAlreadyPurchased = Lance.Account.Pass.IsAlreadyPurchased(mSelectedId);

                mButtonPurchase.SetActiveFrame(!isAlreadyPurchased,
                    Const.PassPurchaseActiveButtonFrame, Const.PassPurchaseInactiveButtonFrame,
                    text: text);

                PassData data = Lance.GameData.PassData.TryGet(mSelectedId);

                mTextPrice.text = Lance.IAPManager.GetPrcieString(data.productId);
                mTextPrice.SetColor(isAlreadyPurchased ? Const.DefaultInactiveTextColor : Const.DefaultActiveTextColor);

                mTextMileage.text = $"+{data.mileage}";
            }
        }

        public void ChangeSubTab(string id)
        {
            if (mSelectedId == id || id.IsValid() == false)
                return;

            mSelectedId = id;

            mParent.RefreshMyRewardValue();

            Refresh();
        }

        public void Refresh()
        {
            mDynamicScrollView.totalItemCount = DataUtil.GetPassStepDatasCount(mSelectedId);
            mDynamicScrollView.refresh();
            mDynamicScrollView.scrollByItemIndex(0);

            bool anyCanReceiveReward = Lance.Account.Pass.AnyCanReceiveReward(mSelectedId);
            mButtonAllReceive.SetActiveFrame(anyCanReceiveReward);
            mRedDotObj.SetActive(anyCanReceiveReward);

            RefreshPurchaseButton();
        }
    }
}
