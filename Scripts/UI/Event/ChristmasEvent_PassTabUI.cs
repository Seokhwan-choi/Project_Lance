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
    class ChristmasEvent_PassTabUI : ChristmasTabUI
    {
        string mPassId;
        Slider mSliderMyRewardValue;
        TextMeshProUGUI mTextMyRewardValue;
        DynamicVScrollView mDynamicScrollView;

        GameObject mRedDotObj;
        Button mButtonAllReceive;
        Button mButtonPurchase;
        TextMeshProUGUI mTextPrice;
        TextMeshProUGUI mTextMileage;
        public override void Init(ChristmasTabUIManager parent, string eventId, ChristmasTab tab)
        {
            base.Init(parent, eventId, tab);

            mPassId = DataUtil.GetEventPassData(eventId).id;

            var imageEventCurrency = gameObject.FindComponent<Image>("Image_EventCurrency");
            imageEventCurrency.sprite = Lance.Atlas.GetItemSlotUISprite($"Currency_{eventId}");

            mDynamicScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");

            mButtonAllReceive = gameObject.FindComponent<Button>("Button_AllReceive");
            mButtonAllReceive.SetButtonAction(OnAllReceiveButton);

            mRedDotObj = mButtonAllReceive.gameObject.FindGameObject("RedDot");

            mButtonPurchase = gameObject.FindComponent<Button>("Button_Purchase");
            mButtonPurchase.SetButtonAction(OnPurchaseButton);

            mTextPrice = mButtonPurchase.gameObject.FindComponent<TextMeshProUGUI>("Text_Price");
            mTextMileage = gameObject.FindComponent<TextMeshProUGUI>("Text_Mileage");

            mSliderMyRewardValue = gameObject.FindComponent<Slider>("Slider_MyRewardValue");
            mTextMyRewardValue = gameObject.FindComponent<TextMeshProUGUI>("Text_MyRewardValue");
        }

        public PassStepData GetStepData(int index)
        {
            if (mEventId.IsValid())
            {
                PassStepData[] datas = DataUtil.GetPassStepDatas(mPassId).ToArray();
                if (datas.Length > index && index != -1)
                    return datas[index];
            }

            return null;
        }

        void OnAllReceiveButton()
        {
            var result = Lance.Account.Event.ReceiveAllPassReward(mEventId);

            mDynamicScrollView.refresh();

            if (result.free != null && result.pay != null)
            {
                RewardResult rewardResult = new RewardResult();

                AddReward(result.free, isFree: true);
                AddReward(result.pay, isFree: false);

                void AddReward(List<int> steps, bool isFree)
                {
                    foreach (int step in steps)
                    {
                        var stepData = DataUtil.GetPassStepData(mPassId, step);
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
                    Lance.Account.Event.SetIsChangedData(true);

                    Lance.GameManager.GiveReward(rewardResult, ShowRewardType.Popup);

                    UIUtil.PopupRefreshRedDots<Popup_NewEventUI>();

                    Lance.Lobby.RefreshEventRedDot();

                    Param param = new Param();
                    param.Add("id", mEventId);
                    param.Add("result_free", result.free);
                    param.Add("result_pay", result.pay);
                    param.Add("isPurchased", Lance.Account.Event.IsPurchasedPass(mEventId));

                    Lance.BackEnd.InsertLog("AllReceivePassreward", param, 7);
                }
                else
                {
                    UIUtil.ShowSystemErrorMessage("NoReward");
                }
            }
        }

        public override void RefreshRedDots()
        {
            bool anyCanReceiveReward = Lance.Account.Event.AnyCanReceivePassReward(mEventId);
            mButtonAllReceive.SetActiveFrame(anyCanReceiveReward,
                Const.PassAllReceiveActiveButtonFrame, Const.PassAllReceiveInactiveButtonFrame);
            mRedDotObj.SetActive(anyCanReceiveReward);
        }

        void PlayRewardMotion(int step, bool isFree)
        {
            foreach (ChristmasEventPassItemUI passItem in GetPassItems())
            {
                if (passItem.Id == mEventId &&
                    passItem.Step == step)
                {
                    passItem.PlayRewardMotion(isFree);
                }
            }
        }

        IEnumerable<ChristmasEventPassItemUI> GetPassItems()
        {
            foreach (var rect in mDynamicScrollView.GetContainters())
                yield return rect.GetComponent<ChristmasEventPassItemUI>();
        }

        void OnPurchaseButton()
        {
            var eventInfo = Lance.Account.Event.GetEventInfo(mEventId);
            if (eventInfo == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (eventInfo.IsActiveEvent() == false)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            var passData = DataUtil.GetEventPassData(mEventId);
            if (passData == null)
            {
                UIUtil.ShowSystemDefaultErrorMessage();

                return;
            }

            if (Lance.Account.Event.IsPurchasedPass(mEventId))
            {
                UIUtil.ShowSystemErrorMessage("AlreadyPurchasedPass");

                return;
            }

            string name = StringTableUtil.GetName(passData.id);

            var mileageReward = new ItemInfo(ItemType.Mileage, passData.mileage);
            ItemInfo[] rewards = new ItemInfo[] { mileageReward };

            var popup = Lance.PopupManager.CreatePopup<Popup_ConfirmPurchaseUI>();
            popup.Init(name, rewards, OnConfirm);

            void OnConfirm()
            {
                Lance.IAPManager.Purchase(passData.productId, OnFinishPurchased);

                void OnFinishPurchased()
                {
                    // 구매 처리
                    if (Lance.Account.Event.PurchasePass(mEventId))
                    {
                        Lance.Account.Event.SetIsChangedData(true);

                        var mileageReward = new RewardResult();

                        mileageReward.mileage = passData.mileage;

                        if (mileageReward.IsEmpty() == false)
                        {
                            Lance.GameManager.GiveReward(mileageReward, ShowRewardType.Popup);
                        }

                        mDynamicScrollView.refresh();

                        UIUtil.PopupRefreshRedDots<Popup_NewEventUI>();

                        RefreshPurchaseButton();

                        Lance.Account.UserInfo.StackPayments(passData.price);

                        Lance.GameManager.CheckQuest(QuestType.Payments, passData.price);

                        Lance.Lobby.RefreshEventRedDot();
                        Lance.Lobby.RefreshQuestRedDot();

                        Param param = new Param();
                        param.Add("id", mEventId);
                        param.Add("stackedPayments", Lance.Account.UserInfo.GetStackedPayments());
                        param.Add("mileage", passData.mileage);

                        Lance.BackEnd.InsertLog("PurchasePass", param, 7);

                        Lance.BackEnd.UpdateAllAccountInfos();
                    }
                }
            }
        }

        public override void OnEnter()
        {
            Lance.Account.Event.UpdatePassRewardValue(mEventId);

            Refresh();
        }

        public override void Refresh()
        {
            mDynamicScrollView.totalItemCount = DataUtil.GetPassStepDatas(mPassId).Count();
            mDynamicScrollView.refresh();
            mDynamicScrollView.scrollByItemIndex(0);

            bool anyCanReceiveReward = Lance.Account.Event.AnyCanReceivePassReward(mEventId);

            mButtonAllReceive.SetActiveFrame(anyCanReceiveReward);

            mRedDotObj.SetActive(anyCanReceiveReward);

            RefreshPurchaseButton();

            RefreshMyRewardValue();
        }

        void RefreshPurchaseButton()
        {
            if (mEventId.IsValid())
            {
                string passName = StringTableUtil.GetName($"Pass_{mEventId}");

                StringParam param = new StringParam("passName", passName);

                string text = StringTableUtil.Get("UIString_PurchasePass", param);

                bool isAlreadyPurchased = Lance.Account.Event.IsAlreadyPurchasedPass(mEventId);

                mButtonPurchase.SetActiveFrame(!isAlreadyPurchased,
                    Const.PassPurchaseActiveButtonFrame, Const.PassPurchaseInactiveButtonFrame,
                    text: text);

                PassData data = DataUtil.GetEventPassData(mEventId);

                mTextPrice.text = Lance.IAPManager.GetPrcieString(data.productId);
                mTextPrice.SetColor(isAlreadyPurchased ? Const.DefaultInactiveTextColor : Const.DefaultActiveTextColor);

                mTextMileage.text = $"+{data.mileage}";
            }
        }

        void RefreshMyRewardValue()
        {
            var passData = DataUtil.GetEventPassData(mEventId);
            if (passData != null)
            {
                double myValue = Lance.Account.Event.GetRewardValue(mEventId);

                mTextMyRewardValue.text = $"{myValue}";

                IEnumerable<PassStepData> datas = DataUtil.GetPassStepDatas(passData.id);
                double maxValue = datas.Max(x => x.requireValue);
                double minValue = datas.Min(x => x.requireValue);
                if (minValue > myValue)
                    myValue = 0;

                mSliderMyRewardValue.value = (float)(myValue / maxValue);
            }
        }
    }
}