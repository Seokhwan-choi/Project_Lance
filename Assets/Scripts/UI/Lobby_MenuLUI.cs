using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Lance
{
    class Lobby_MenuLUI : MonoBehaviour
    {
        const float SaveCooltime = 15f;

        Lobby_GuideQuestUI mGuideQuestUI;

        GameObject mPassRedDot;
        GameObject mPassObj;

        GameObject mLottoRedDot;
        GameObject mLottoObj;

        GameObject mEventRedDot;
        GameObject mEventObj;

        GameObject mSaveObj;

        GameObject mStatisticsObj;          // 통계 버튼
        Lobby_StatisticsUI mStatisticsUI;

        GameObject mSpoilsObj;

        Image mImageSaveModal;
        float mSaveCoolTime;
        public void Init()
        {
            var guideQuestObj = gameObject.Find("GuideQuest");

            mGuideQuestUI = guideQuestObj.GetOrAddComponent<Lobby_GuideQuestUI>();
            mGuideQuestUI.Init();

            mPassObj = gameObject.FindGameObject("Pass");
            var buttonPass = mPassObj.FindComponent<Button>("Pass");
            buttonPass.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_PassUI>();
                popup.Init();
            });

            mPassRedDot = mPassObj.FindGameObject("RedDot");

            mLottoObj = gameObject.FindGameObject("Lotto");
            var buttonLotto = mLottoObj.FindComponent<Button>("Lotto");
            buttonLotto.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_LottoUI>();
                popup.Init();
            });

            mLottoRedDot = mLottoObj.FindGameObject("RedDot");

            mEventObj = gameObject.FindGameObject("Event");

            var buttonEvent = mEventObj.GetComponent<Button>();
            buttonEvent.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_NewEventUI>();

                popup.Init();
            });

            mEventRedDot = mEventObj.FindGameObject("RedDot");

            mSaveObj = gameObject.FindGameObject("Save");

            var buttonSave = mSaveObj.FindComponent<Button>("Button_Save");
            mImageSaveModal = buttonSave.FindComponent<Image>("Image_SaveModal");
            buttonSave.SetButtonAction(() =>
            {
                if (mSaveCoolTime > 0)
                {
                    UIUtil.ShowSystemErrorMessage("CanNotSaveAccountInfoInCoolTime");

                    return;
                }

                try
                {
                    Lance.TouchBlock.SetActive(true);

                    Lance.BackEnd.UpdateAllAccountInfos((callback) =>
                    {
                        if (callback != null)
                        {
                            if (callback.IsSuccess())
                            {
                                UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("SuccessSaveAccountInfo"));

                                Lance.TouchBlock.SetActive(false);
                            }
                            else
                            {
                                UIUtil.ShowSystemErrorMessage("FailedSaveAccountInfo");

                                Lance.TouchBlock.SetActive(false);
                            }
                        }
                        else
                        {
                            UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("SuccessSaveAccountInfo"));

                            Lance.TouchBlock.SetActive(false);
                        }
                    });
                }
                catch
                {
                    UIUtil.ShowSystemDefaultErrorMessage();

                    Lance.TouchBlock.SetActive(false);
                }

                mSaveCoolTime = SaveCooltime;

                mImageSaveModal.fillAmount = 1f;
            });

            var statisticsObj = gameObject.FindGameObject("StatisticsUI");
            mStatisticsUI = statisticsObj.GetOrAddComponent<Lobby_StatisticsUI>();
            mStatisticsUI.Init();

            mStatisticsObj = gameObject.FindGameObject("Statistics");
            var buttonStatistics = mStatisticsObj.FindComponent<Button>("Button_Statistics");
            buttonStatistics.SetButtonAction(() =>
            {
                mStatisticsUI.Show();
            });

            mSpoilsObj = gameObject.FindGameObject("Spoils");
            var buttonSpoils = mSpoilsObj.FindComponent<Button>("Button_Spoils");
            buttonSpoils.SetButtonAction(() =>
            {
                var popupSpoils = Lance.PopupManager.CreatePopup<Popup_DemonicRealmSpoilsUI>();

                popupSpoils.Init();
            });
        }

        public void RefreshGuideQuestUI()
        {
            mGuideQuestUI.Refresh();
        }

        public void RefreshContentsLockUI()
        {
            mPassObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false);
            mLottoObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false);
            mEventObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false);

            mSpoilsObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.DemonicRealm) == false);
        }

        public void OnStartStage(StageData stageData)
        {
            bool isNormal = stageData.type.IsNormal();
            bool isJousting = stageData.type.IsJousting();

            mGuideQuestUI.OnStartStage(stageData);
            mLottoObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false && isNormal);
            mPassObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false && isNormal);
            mEventObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false && isNormal);
            mSpoilsObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.DemonicRealm) == false && isNormal);
            mSaveObj.SetActive(isNormal);
            mStatisticsUI.OnStartStage();
            mStatisticsObj.SetActive(!isJousting);
        }

        public void OnUpdate(float dt)
        {
            if (mSaveCoolTime > 0)
            {
                mSaveCoolTime -= dt;

                mImageSaveModal.fillAmount = mSaveCoolTime / SaveCooltime;
            }

            mStatisticsUI.OnUpdate(dt);
        }

        public void StackReward(RewardResult reward)
        {
            mStatisticsUI.StackReward(reward);
        }

        public void StackDamage(string id, double damage)
        {
            mStatisticsUI.StackDamage(id, damage);
        }

        public void RefreshRedDots()
        {
            mGuideQuestUI.RefreshActive();
            mPassRedDot.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false && Lance.Account.Pass.AnyCanReceiveReward());
            mLottoRedDot.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false && Lance.Account.Lotto.CanDrawLotto());
            mEventRedDot.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false && Lance.Account.AnyCanReceiveNewEventReward());
        }

        public void RefreshEventRedDot()
        {
            mEventRedDot.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false && Lance.Account.AnyCanReceiveNewEventReward());
        }

        public void RefreshLottoRedDot()
        {
            mLottoRedDot.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.SpeedMode) == false && Lance.Account.Lotto.CanDrawLotto());
        }

        public void RefreshPassRedDot()
        {
            mPassRedDot.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false && Lance.Account.Pass.AnyCanReceiveReward());
        }

        public void Localize()
        {
            mGuideQuestUI.Localize();
            mStatisticsUI.Localize();
        }
    }
}