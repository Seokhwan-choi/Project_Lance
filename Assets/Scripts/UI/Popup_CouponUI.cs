using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using LitJson;

namespace Lance
{
    class Popup_CouponUI : PopupBase
    {
        public void Init()
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_InputCoupon"));

            var inputFieldCoupon = gameObject.FindComponent<TMP_InputField>("InputField_Coupon");
            var buttonEnter = gameObject.FindComponent<Button>("Button_Enter");

            inputFieldCoupon.onValueChanged.AddListener((str) =>
            {
                buttonEnter.SetActiveFrame(inputFieldCoupon.text.IsValid());
            });

            buttonEnter.SetActiveFrame(inputFieldCoupon.text.IsValid());
            buttonEnter.SetButtonAction(UseCoupon);

            void UseCoupon()
            {
                if (inputFieldCoupon.text.IsValid() == false)
                {
                    UIUtil.ShowSystemErrorMessage("NeedCouponNumber");

                    return;
                }

                Lance.TouchBlock.SetActive(true);

                Lance.Account.Coupon.UseCoupon(inputFieldCoupon.text, (bro) =>
                {
                    if (bro.IsSuccess())
                    {
                        //UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("SuccessUsedCoupon"));

                        inputFieldCoupon.text = string.Empty;

                        RewardResult rewardResult = new RewardResult();

                        foreach (JsonData item in bro.GetFlattenJSON()["itemObject"])
                        {
                            if (item["item"].ContainsKey("itemType"))
                            {
                                if (Enum.TryParse(item["item"]["itemType"].ToString(), out ItemType ItemType))
                                {
                                    string itemId = item["item"]["itemId"].ToString();
                                    int itemCount = int.Parse(item["itemCount"].ToString());

                                    if (ItemType == ItemType.Reward)
                                    {
                                        rewardResult = rewardResult.AddReward(Lance.GameManager.RewardDataChangeToRewardResult(itemId));
                                    }
                                    else
                                    {
                                        rewardResult = rewardResult.AddReward(ItemInfoUtil.CreateItemInfo(ItemType, itemId, itemCount));
                                    }
                                }
                            }
                        }

                        if (rewardResult.IsEmpty() == false)
                        {
                            Lance.GameManager.GiveReward(rewardResult, ShowRewardType.Popup);

                            Lance.BackEnd.UpdateAllAccountInfos();
                        }
                    }
                    else
                    {
                        ShowInValidCoupon();
                    }

                    Lance.TouchBlock.SetActive(false);
                });

                void ShowInValidCoupon()
                {
                    string text = StringTableUtil.GetSystemMessage("InValidCoupon");

                    string colorText = UIUtil.GetColorString("FD4F29", text);

                    UIUtil.ShowSystemMessage(colorText);

                    SoundPlayer.PlayErrorSound();
                }
            }
        }
    }
}