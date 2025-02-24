using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System;
using System.Linq;
using UnityEngine.EventSystems;


namespace Lance
{
    class Popup_PetEvolutionStatUI : PopupBase
    {
        string mId;
        bool mInAutoChangeEvolutionStat;
        TextMeshProUGUI mTextElementalStone;
        List<PetEvolutionStatUI> mPetEvolutionStats;
        Button mButtonChangeEvolutionStat;
        TextMeshProUGUI mTextChangePrice;

        // 장비 옵션 자동 재설정
        GameObject mInAutoChangeEvolutionObj;
        Coroutine mAutoChangeRoutine;
        Button mButtonAutoChangeEvolutionStat;

        const float IntervalTime = 0.1f;
        const float LogTime = 1f;

        float mIntervalTime2;
        float mLogTime2;
        int mLogStacker2;
        bool mIsPress2;

        public void Init(string id, int preset)
        {
            SetUpCloseAction();

            SetTitleText($"{StringTableUtil.Get("UIString_EvolutionTrait")}_{preset+1}");

            var data = Lance.GameData.PetData.TryGet(id);

            mId = id;
            mTextElementalStone = gameObject.FindComponent<TextMeshProUGUI>("Text_ElementalStoneAmount");

            var buttonEvolutionStatProb = gameObject.FindComponent<Button>("Button_PetEvolutionStatInfo");
            buttonEvolutionStatProb.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_PetStatProbInfoUI>();

                popup.Init();
            });

            var evolutionStatObj = gameObject.FindGameObject("EvolutionStat");

            mPetEvolutionStats = new List<PetEvolutionStatUI>();

            for(int i = 0; i < Lance.GameData.PetCommonData.evolutionStatMaxSlot; ++i)
            {
                int slot = i;
                int index = slot + 1;

                var petEvolutionStatObj = evolutionStatObj.FindGameObject($"PetEvolutionStatUI_{index}");

                var petEvolutionStatUI = petEvolutionStatObj.GetOrAddComponent<PetEvolutionStatUI>();

                petEvolutionStatUI.Init(mId, slot, RefreshButtonUI);

                mPetEvolutionStats.Add(petEvolutionStatUI);
            }

            mButtonChangeEvolutionStat = evolutionStatObj.FindComponent<Button>("Button_Change");
            var eventTrigger2 = mButtonChangeEvolutionStat.GetOrAddComponent<EventTrigger>();
            eventTrigger2.AddTriggerEvent(EventTriggerType.PointerDown, () => OnChangeStatButton(isPress: true));
            eventTrigger2.AddTriggerEvent(EventTriggerType.PointerUp, () => OnChangeStatButton(isPress: false));

            mTextChangePrice = evolutionStatObj.FindComponent<TextMeshProUGUI>("Text_ChangePrice");

            mInAutoChangeEvolutionObj = gameObject.FindGameObject("InAutoChangeEvolutionStat");
            mInAutoChangeEvolutionObj.SetActive(false);

            mButtonAutoChangeEvolutionStat = gameObject.FindComponent<Button>("Button_AutoChange");
            mButtonAutoChangeEvolutionStat.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_AutoChangeOptionStatSelectUI>();

                popup.Init(StringTableUtil.Get("Title_AutoChangeEvolutionStat"),
                    DataUtil.PetEvolutionStatTypes, DataUtil.PetEvolutionStatGradeToGrade().ToArray(), PlayAutoChangeEvolutionStat);
            });

            var buttonAutoChangeEvolutionStop = mInAutoChangeEvolutionObj.FindComponent<Button>("Button_AutoChangeEvolutionStop");
            buttonAutoChangeEvolutionStop.SetButtonAction(() =>
            {
                StopAutoChangeEvolutionStat();
            });

            Refresh();
        }

        public override void OnClose()
        {
            base.OnClose();

            if (mLogStacker2 > 0)
            {
                InsertChangeStatLog();
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            UpdateChangeStat(dt);
        }
        void UpdateChangeStat(float dt)
        {
            if (mIsPress2)
            {
                mLogTime2 = LogTime;
                mIntervalTime2 -= dt;
                if (mIntervalTime2 <= 0f)
                {
                    mIntervalTime2 = IntervalTime;

                    OnChangeStat();
                }
            }
            else
            {
                if (mLogStacker2 > 0)
                {
                    mIntervalTime2 = 0f;
                    mLogTime2 -= dt;
                    if (mLogTime2 <= 0f)
                    {
                        mLogTime2 = LogTime;

                        InsertChangeStatLog();
                    }
                }
            }
        }

        void InsertChangeStatLog()
        {
            Param param = new Param();
            param.Add("id", mId);
            param.Add("usedElementalStone", mLogStacker2);

            Lance.BackEnd.InsertLog("ChangePetStat", param, 7);

            mLogStacker2 = 0;
        }

        // 진화 특성 변경
        void OnChangeStatButton(bool isPress)
        {
            mIsPress2 = isPress;
            if (!mIsPress2)
            {
                mIntervalTime2 = 0;
                if (mLogStacker2 > 0)
                {
                    Lance.GameManager.UpdatePlayerStat();
                }
            }
        }

        void OnChangeStat()
        {
            if (Lance.GameManager.ChangePetEvolutionStat(mId))
            {
                SoundPlayer.PlayPetFeed();

                Refresh();

                mLogStacker2 += 1;
            }
        }

        void Refresh()
        {
            // 속성석 보유량
            mTextElementalStone.text = Lance.Account.Currency.GetElementalStone().ToString();

            // 진화 특성 ( 스탯 )
            mPetEvolutionStats.ForEach(x => x.Refresh());

            RefreshButtonUI();
        }

        void RefreshButtonUI()
        {
            // 진화 스탯 변경
            bool anyCanChangeEvolutionStat = Lance.Account.AnyCanChangePetEvolutionStat(mId);
            bool allLocked = Lance.Account.Pet.GetEvolutionStatLockCount(mId) == Lance.GameData.PetCommonData.evolutionStatMaxSlot;
            int requireElementalStatStone = Lance.Account.GetChangePetEvolutionStatRequireStone(mId);
            bool isEnoughElementalStatStone = Lance.Account.Currency.IsEnoughElementalStone(requireElementalStatStone);

            mButtonChangeEvolutionStat.SetActiveFrame(anyCanChangeEvolutionStat && allLocked == false && isEnoughElementalStatStone);

            mTextChangePrice.SetColor(UIUtil.GetEnoughTextColor(isEnoughElementalStatStone));
            mTextChangePrice.text = $"{requireElementalStatStone}";
        }

        const int TryAutoChangeStatStartCount = 100;
        const int TryAutoChangeStatAddCount = 10;
        const int TryAutoChangeStatMaxCount = 200;

        int mTryAutoChangeEvolutionStat;
        int mTryAutoChangeEvolutionStackedStat;
        double mTotalAutoChangeUseElementalStone;
        void PlayAutoChangeEvolutionStat(StatType[] selectedStatTypes, Grade selectedGrade)
        {
            if (mInAutoChangeEvolutionStat)
                return;

            mTotalAutoChangeUseElementalStone = 0;
            mTryAutoChangeEvolutionStackedStat = TryAutoChangeStatStartCount;

            mInAutoChangeEvolutionStat = true;

            mInAutoChangeEvolutionObj.SetActive(true);

            mAutoChangeRoutine = StartCoroutine(AutoChangeEvolutionStat(selectedStatTypes, selectedGrade));
        }

        void StopAutoChangeEvolutionStat()
        {
            if (mAutoChangeRoutine != null)
                StopCoroutine(mAutoChangeRoutine);

            mInAutoChangeEvolutionObj.SetActive(false);

            mInAutoChangeEvolutionStat = false;

            Lance.BackEnd.UpdateAllAccountInfos();

            Param param = new Param();
            param.Add("id", mId);
            param.Add("usedElementalStone", mTotalAutoChangeUseElementalStone);
            param.Add("remainElementalStone", Lance.Account.Currency.GetElementalStone());

            Lance.BackEnd.InsertLog("AutoChangeEvolutionStat", param, 7);

            mTotalAutoChangeUseElementalStone = 0;
            mTryAutoChangeEvolutionStackedStat = 0;

            Lance.GameManager.UpdatePlayerStat();
        }

        IEnumerator AutoChangeEvolutionStat(StatType[] selectedStatTypes, Grade selectedGrade)
        {
            while (true)
            {
                double changePrice = Lance.Account.GetChangePetEvolutionStatRequireStone(mId);

                if (Lance.GameManager.ChangePetEvolutionStat(mId))
                {
                    SoundPlayer.PlayPetFeed();

                    Refresh();

                    mTotalAutoChangeUseElementalStone += changePrice;

                    mTryAutoChangeEvolutionStat++;
                    if (mTryAutoChangeEvolutionStat >= mTryAutoChangeEvolutionStackedStat)
                    {
                        mTryAutoChangeEvolutionStackedStat += TryAutoChangeStatAddCount;

                        mTryAutoChangeEvolutionStackedStat = Math.Min(mTryAutoChangeEvolutionStackedStat, TryAutoChangeStatMaxCount);

                        mTryAutoChangeEvolutionStat = 0;

                        Lance.BackEnd.UpdateAllAccountInfos();
                    }

                    // 조건을 만족하거나 재화가 충분하면 그만하자
                    if (IsSatisfied() || IsEnoughElementalStone() == false)
                    {
                        StopAutoChangeEvolutionStat();

                        break;
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.05f);
                    }
                }
                else
                {
                    StopAutoChangeEvolutionStat();

                    break;
                }
            }

            bool IsSatisfied()
            {
                return Lance.Account.Pet.IsSatisfied(mId, selectedStatTypes, (PetEvolutionStatGrade)((int)selectedGrade) - 1);
            }

            bool IsEnoughElementalStone()
            {
                int changePrice = Lance.Account.GetChangePetEvolutionStatRequireStone(mId);

                if (Lance.Account.Currency.IsEnoughElementalStone(changePrice) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughElementalStone");

                    return false;
                }

                return true;
            }
        }
    }
}