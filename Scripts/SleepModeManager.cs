using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Lance
{
    class SleepModeManager
    {
        const float SleepModeWaitTime = 300;    // 300초 약 5분
        const float SleepModeIntervalTime = 60;

        int mIntervalCount;
        float mSleepModeIntervalTime;
        int mWaitCount;
        float mSleepModeWaitTime;
        bool mIsSleepMode;
        CanvasGroup mCanvasGroup;
        TextMeshProUGUI mTextTime;
        TextMeshProUGUI mTextBattery;
        TextMeshProUGUI mTextStage;
        RewardResult mStackRewardResult;
        GameObject mSleepModeObj;
        GameObject mRewardSlotsObj;
        List<ItemSlotUI> mRewardSlotList;
        public void Init()
        {
            var alwaysFrontCanvas = GameObject.Find("AlwaysFront_Canvas");
            mSleepModeObj = alwaysFrontCanvas.FindGameObject("SleepMode");
            mCanvasGroup = mSleepModeObj.GetOrAddComponent<CanvasGroup>();
            mTextTime = mSleepModeObj.FindComponent<TextMeshProUGUI>("Text_Time");
            mTextBattery = mSleepModeObj.FindComponent<TextMeshProUGUI>("Text_Battery");
            mTextStage = mSleepModeObj.FindComponent<TextMeshProUGUI>("Text_Stage");

            var scrollEventTrigger = mSleepModeObj.FindComponent<EventTrigger>("Scroll View");
            scrollEventTrigger.AddTriggerEvent(EventTriggerType.BeginDrag, () => OnDemandRendering.renderFrameInterval = 1);
            scrollEventTrigger.AddTriggerEvent(EventTriggerType.EndDrag, () => OnDemandRendering.renderFrameInterval = 60);

            mRewardSlotsObj = mSleepModeObj.FindGameObject("Rewards");
            mRewardSlotsObj.AllChildObjectOff();

            mRewardSlotList = new List<ItemSlotUI>();

            var sleepModeUnlock = mSleepModeObj.FindGameObject("SleepModeUnlock2");
            var eventTrigger = sleepModeUnlock.GetOrAddComponent<EventTrigger>();
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerDown, () => OnPress(isPress: true));
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerUp, () => OnPress(isPress: false));

            mSleepModeWaitTime = SleepModeWaitTime;
            mSleepModeIntervalTime = 0;

            mIsSleepMode = false;
            mSleepModeObj.SetActive(false);
        }

        public bool IsSleepMode()
        { 
            return mIsSleepMode; 
        }

        bool mIsPressed;
        Tweener mTweener;
        void OnPress(bool isPress)
        {
            mIsPressed = isPress;

            if (!isPress)
            {
                mTweener = DOTween.To((f) => mCanvasGroup.alpha = f, mCanvasGroup.alpha, 1f, 0.5f)
                    .OnComplete(() =>
                    {
                        OnDemandRendering.renderFrameInterval = 60;

                        mTweener.Kill();
                    });
            }
        }

        public void Localize()
        {
            var readerUIs = mSleepModeObj.GetComponentsInChildren<StringTableUIReader>(true);
            foreach (var readerUI in readerUIs)
            {
                readerUI.Localize();
            }
        }

        public void OnAnyScreenTouch()
        {
            mSleepModeWaitTime = SleepModeWaitTime;
        }

        public void OnUpdate(StageData stageData, float dt)
        {
            if (mIsSleepMode == false)
            {
                mSleepModeWaitTime -= dt;
                if (mSleepModeWaitTime <= 0f)
                {
                    if (SaveBitFlags.AutoSleepMode.IsOn())
                    {
                        SetSleepMode(true);

                        mSleepModeWaitTime = SleepModeWaitTime;

                        RefreshUI(stageData);
                    }
                    else
                    {
                        mWaitCount++;
                        if (mWaitCount == 12)
                        {
                            mWaitCount = 0;

                            ResetResource();
                        }

                        mSleepModeWaitTime = SleepModeWaitTime;
                    }
                }
            }
            else
            {
                if (mIsPressed)
                {
                    OnDemandRendering.renderFrameInterval = 1;

                    mCanvasGroup.alpha = mCanvasGroup.alpha - (dt * 0.5f);

                    if (mCanvasGroup.alpha <= 0.4)
                    {
                        SetSleepMode(false);
                    }
                }
                else
                {
                    mSleepModeIntervalTime -= dt;
                    if (mSleepModeIntervalTime <= 0f)
                    {
                        mIntervalCount++;
                        if (mIntervalCount >= 50)
                        {
                            ResetResource();

                            mIntervalCount = 0;
                        }
                        
                        RefreshUI(stageData);
                    }
                }
            }
        }

        void RefreshUI(StageData stageData)
        {
            mSleepModeIntervalTime = SleepModeIntervalTime;

            if (mStackRewardResult.IsEmpty())
            {
                foreach (var rewardSlotUI in mRewardSlotList)
                {
                    Lance.ObjectPool.ReleaseUI(rewardSlotUI.gameObject);
                }

                mRewardSlotList.Clear();
            }
            else
            {
                var rectTm = mRewardSlotsObj.GetComponent<RectTransform>();

                if (mRewardSlotList.Count > 0)
                {
                    var splitReward = mStackRewardResult.Split();

                    for (int i = 0; i < splitReward.Count; ++i)
                    {
                        if (mRewardSlotList.Count > i)
                        {
                            var itemSlotUI = mRewardSlotList[i];

                            itemSlotUI.Init(splitReward[i]);
                        }
                        else
                        {
                            var itemSlotObj = Lance.ObjectPool.AcquireUI("ItemSlotUI", rectTm);

                            var itemSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
                            itemSlotUI.Init(splitReward[i]);

                            mRewardSlotList.Add(itemSlotUI);
                        }
                    }
                }
                else
                {
                    mRewardSlotList = ItemSlotUIUtil.CreateItemSlotUIList(rectTm, mStackRewardResult);
                }
            }

            var today = TimeUtil.UtcNow.AddHours(9);

            mTextTime.text = $"{today.Hour:00}:{today.Minute:00}";
            mTextBattery.text = $"Battery : {SystemInfo.batteryLevel * 100f:F0}%";

            RefreshStageName(stageData);
        }

        bool mStartSleepModeBGM;
        bool mStartSleepModeSFX;

        public void SetSleepMode(bool sleepMode)
        {
            mIsSleepMode = sleepMode;

            mSleepModeObj.SetActive(sleepMode);

            if (mIsSleepMode)
            {
                // 기존의 약 1 / 3 프레임으로 작동한다고 생각하면 됨
                OnDemandRendering.renderFrameInterval = 60;

                mStartSleepModeBGM = SaveBitFlags.BGMSound.Get();
                mStartSleepModeSFX = SaveBitFlags.SFXSound.Get();

                SaveBitFlags.BGMSound.Set(false);
                SaveBitFlags.SFXSound.Set(false);
                Lance.SoundManager.OnChangeBitFlag();

                mSleepModeIntervalTime = 0;
                mIntervalCount = 0;
                mCanvasGroup.alpha = 1f;

                mStackRewardResult = new RewardResult();

                RefreshRewardSlots();
            }
            else
            {
                OnDemandRendering.renderFrameInterval = 1;

                SaveBitFlags.BGMSound.Set(mStartSleepModeBGM);
                SaveBitFlags.SFXSound.Set(mStartSleepModeSFX);

                Lance.SoundManager.OnChangeBitFlag();

                mSleepModeWaitTime = SleepModeWaitTime;

                mWaitCount = 0;

                mIsPressed = false;

                mStackRewardResult = new RewardResult();

                RefreshRewardSlots();
            }
        }

        public void StackRewardInSleepMode(RewardResult rewardResult)
        {
            if (mIsSleepMode)
                mStackRewardResult = mStackRewardResult.AddReward(rewardResult);
        }

        public void OnChangeStage(StageData stageData)
        {
            RefreshStageName(stageData);
        }

        void RefreshStageName(StageData stageData)
        {
            if (stageData.type.IsDungeon())
            {
                string title = StringTableUtil.Get($"Title_{stageData.type}Dungeon");
                StringParam param = new StringParam("step", stageData.stage);
                string step = StringTableUtil.Get("UIString_Step", param);

                mTextStage.text = stageData.type == StageType.Raid ? $"{title}" : $"{title} {step}";
            }
            else if (stageData.type.IsJousting())
            {
                mTextStage.text = StringTableUtil.Get("Title_Jousting");
            }
            else if (stageData.type.IsLimitBreak())
            {
                mTextStage.text = StringTableUtil.Get("UIString_Limitbreak");
            }
            else if (stageData.type == StageType.UltimateLimitBreak)
            {
                mTextStage.text = StringTableUtil.Get("UIString_UltimateLimitBreak");
            }
            else
            {
                mTextStage.text = StageRecordsUtil.ChangeStageInfoToString((stageData.diff, stageData.chapter, stageData.stage));
            }
        }

        void RefreshRewardSlots()
        {
            foreach (var rewardSlot in mRewardSlotList)
            {
                Lance.ObjectPool.ReleaseUI(rewardSlot.gameObject);
            }

            mRewardSlotList.Clear();
        }

        void ResetResource()
        {
            Lance.GameManager.StartCoroutine(UnloadUnusedAssets());
        }

        IEnumerator UnloadUnusedAssets()
        {
            yield return Resources.UnloadUnusedAssets();

            DOTween.KillAll(complete: true, Const.DoTweenKillIgnores);

            DOTween.ClearCachedTweens();

            GC.Collect(0, GCCollectionMode.Optimized);
        }

        //Tweener mTweener;
        //void OnEndDrag()
        //{
        //    mTweener?.Rewind();

        //    if (mSliderSleepModeUnlock.value > 0.66f)
        //    {
        //        float startValue = mSliderSleepModeUnlock.value;
        //        float endValue = 1f;

        //        mTweener = DOTween.To((f) => mSliderSleepModeUnlock.value = f, startValue, endValue, 0.5f)
        //            .OnComplete(() =>
        //            {
        //                mTweener.Kill();

        //                SetSleepMode(false);
        //            }); 
        //    }
        //    else
        //    {
        //        float startValue = mSliderSleepModeUnlock.value;
        //        float endValue = 0f;

        //        mTweener = DOTween.To((f) => mSliderSleepModeUnlock.value = f, startValue, endValue, 0.5f)
        //            .OnComplete(() =>
        //            {
        //                mTweener.Kill();
        //            });
        //    }
        //}
    }
}