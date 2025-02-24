using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

namespace Lance
{
    class Popup_PetEquipmentInfoUI : PopupBase
    {
        bool mInAutoReforge;

        string mId;
        PetInventoryTabUI mParent;
        PetInventory_EquipmentItemUI mEquipmentItemUI;

        TextMeshProUGUI mTextCombineCount; // 장비 합성 횟수

        Button mButtonUpgradeX1;
        TextMeshProUGUI mTextUpgradePriceX1; // 장비 강화 비용
        Button mButtonUpgradeXMax;
        TextMeshProUGUI mTextUpgradePriceXMax; // 장비 강화 비용
        Button mButtonCombine;
        GameObject mCombineRedDot;
        Button mButtonEquip;

        StatValueInfoUI mEquipStatValueInfo;        // 착용 효과
        List<StatValueInfoUI> mOwnStatValueInfos;   // 보유 효과

        // 재화 관련
        TextMeshProUGUI mTextUpgradeStoneAmount;
        TextMeshProUGUI mTextReforgeStoneAmount;
        TextMeshProUGUI mTextElementalStoneAmount;

        // 재련 관련
        GameObject mTouchBlock;
        Button mButtonReforge;
        TextMeshProUGUI mTextReforgeNeedDesc;
        GameObject mProbObj;
        TextMeshProUGUI mTextReforgeProb;
        TextMeshProUGUI mTextReforgeBonusProb;

        TextMeshProUGUI mTextReforgeStonePrice;
        TextMeshProUGUI mTextReforgeElementalStonePrice;

        GameObject mReforgeNextStepObj;
        TextMeshProUGUI mTextReforgeCurStep;
        TextMeshProUGUI mTextReforgeNextStep;

        GameObject mReforgeMaxStepObj;
        TextMeshProUGUI mTextBonusProbDesc;

        // 자동 재련
        Button mButtonAutoReforge;
        GameObject mInAutoReforgeObj;
        Coroutine mAutoReforgeRoutine;

        const float IntervalTime = 0.1f;
        const float LogTime = 1f;
        float mIntervalTime;
        float mLogTime;
        int mLogStacker;
        bool mIsPress;

        PetEquipmentData mData;
        public void Init(PetInventoryTabUI parent, string id)
        {
            mParent = parent;
            mId = id;
            mData = Lance.GameData.PetEquipmentData.TryGet(id);

            Debug.Assert(mData != null, $"{id}의 PetEquipmentData가 존재하지 않음");

            SetUpCloseAction();

            // 장비 아이콘 초기화
            var equipmentItemObj = gameObject.FindGameObject("Equipment");
            mEquipmentItemUI = equipmentItemObj.GetOrAddComponent<PetInventory_EquipmentItemUI>();
            mEquipmentItemUI.Init();
            mEquipmentItemUI.SetId(id);
            mEquipmentItemUI.SetSelected(false);
            mEquipmentItemUI.SetIgnoreRedDot(true);

            // 버튼 초기화
            GameObject buttonsObj = gameObject.FindGameObject("Buttons");
            mButtonUpgradeX1 = buttonsObj.FindComponent<Button>("Button_UpgradeX1");
            var eventTrigger = mButtonUpgradeX1.GetOrAddComponent<EventTrigger>();
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerDown, () => OnUpgradeButton(isPress: true));
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerUp, () => OnUpgradeButton(isPress: false));
            mTextUpgradePriceX1 = mButtonUpgradeX1.gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradePriceX1");

            mButtonUpgradeXMax = buttonsObj.FindComponent<Button>("Button_UpgradeXMax");
            mButtonUpgradeXMax.SetButtonAction(OnUpgradeMax);
            mTextUpgradePriceXMax = mButtonUpgradeXMax.gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradePriceXMax");

            mButtonCombine = buttonsObj.FindComponent<Button>("Button_Combine");
            mButtonCombine.SetButtonAction(OnCombineButton);
            mCombineRedDot = mButtonCombine.gameObject.FindGameObject("RedDot");

            mButtonEquip = buttonsObj.FindComponent<Button>("Button_Equip");
            mButtonEquip.SetButtonAction(OnEquipButton);

            mTextCombineCount = mButtonCombine.gameObject.FindComponent<TextMeshProUGUI>("Text_CombineCount");

            // 착용 효과
            GameObject equipValueInfoObj = gameObject.FindGameObject("EquipValueInfo");
            GameObject statValueInfoObj = equipValueInfoObj.FindGameObject("ValueInfo");
            mEquipStatValueInfo = statValueInfoObj.GetOrAddComponent<StatValueInfoUI>();
            mEquipStatValueInfo.InitPetEquipment(id);
            mEquipStatValueInfo.Refresh();

            // 보유 효과
            mOwnStatValueInfos = new List<StatValueInfoUI>();
            GameObject ownValueInfoObj = gameObject.FindGameObject("OwnValueInfo");
            GameObject valueInfosObj = ownValueInfoObj.FindGameObject("ValueInfos");

            for (int i = 0; i < mData.ownStats.Length; ++i)
            {
                int index = i + 1;

                statValueInfoObj = valueInfosObj.FindGameObject($"StatValueInfo_{index}");
                var valueInfo = statValueInfoObj.GetOrAddComponent<StatValueInfoUI>();
                valueInfo.InitPetEquipment(id);

                string ownStatId = mData.ownStats[i];

                valueInfo.SetActive(ownStatId.IsValid());
                valueInfo.SetOwnStatId(ownStatId);

                mOwnStatValueInfos.Add(valueInfo);
            }

            if (Lance.LocalSave.IsNewPetEquipment(mId))
            {
                Lance.LocalSave.AddGetPetEquipment(mId);

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);
            }

            mTextUpgradeStoneAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradeStoneAmount");
            mTextReforgeStoneAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeStoneAmount");
            mTextElementalStoneAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_ElementalStoneAmount");

            // 재련
            mTouchBlock = gameObject.FindGameObject("TouchBlock");
            mButtonReforge = gameObject.FindComponent<Button>("Button_Reforge");
            mButtonReforge.SetButtonAction(OnReforgeButton);
            mProbObj = gameObject.FindGameObject("Prob");
            mTextReforgeNeedDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeNeedDesc");
            mTextReforgeProb = mProbObj.FindComponent<TextMeshProUGUI>("Text_ReforgeProb");
            mTextReforgeBonusProb = mProbObj.FindComponent<TextMeshProUGUI>("Text_ReforgeBonus");

            mTextReforgeStonePrice = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireReforgeStone");
            mTextReforgeElementalStonePrice = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireElementalStone");

            mReforgeNextStepObj = gameObject.FindGameObject("Reforge_NextStep");
            mReforgeMaxStepObj = gameObject.FindGameObject("Reforge_MaxStep");
            mTextReforgeCurStep = mReforgeNextStepObj.FindComponent<TextMeshProUGUI>("Text_CurStep");
            mTextReforgeNextStep = mReforgeNextStepObj.FindComponent<TextMeshProUGUI>("Text_NextStep");
            mTextBonusProbDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_BonusProb");

            // 자동 재련
            mButtonAutoReforge = gameObject.FindComponent<Button>("Button_AutoReforge");
            mButtonAutoReforge.SetButtonAction(() =>
            {
                PetEquipmentInst equipItem = Lance.Account.GetPetEquipment(mId);
                if (equipItem == null)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotPetEquipment");

                    return;
                }

                var popup = Lance.PopupManager.CreatePopup<Popup_PetEquipmentAutoReforgeUI>("Popup_AutoReforgeUI");

                popup.Init(mId, PlayAutoReforge);
            });

            mInAutoReforgeObj = gameObject.FindGameObject("InAutoReforge");
            mInAutoReforgeObj.SetActive(false);

            var buttonAutoReforgeStop = mInAutoReforgeObj.FindComponent<Button>("Button_AutoReforgeStop");
            buttonAutoReforgeStop.SetButtonAction(() =>
            {
                StopAutoReforge();
            });

            Refresh();
        }

        public override void OnClose()
        {
            base.OnClose();

            if (mLogStacker > 0)
            {
                InsertUpgradeLog();
            }

            Lance.Lobby.RefreshCollectionRedDot();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            UpdateUpgrade(dt);
        }

        void UpdateUpgrade(float dt)
        {
            if (mIsPress)
            {
                mLogTime = LogTime;
                mIntervalTime -= dt;
                if (mIntervalTime <= 0f)
                {
                    mIntervalTime = IntervalTime;

                    OnUpgrade();
                }
            }
            else
            {
                if (mLogStacker > 0)
                {
                    mIntervalTime = 0f;
                    mLogTime -= dt;
                    if (mLogTime <= 0f)
                    {
                        mLogTime = LogTime;

                        InsertUpgradeLog();

                        //Lance.Lobby.RefreshCollectionRedDot();
                    }
                }
            }
        }

        void OnUpgradeButton(bool isPress)
        {
            mIsPress = isPress;
            if (!mIsPress)
            {
                if (mLogStacker > 0)
                    Lance.GameManager.UpdatePlayerStat();
            }
        }

        void InsertUpgradeLog()
        {
            mLogStacker = 0;

            Param param = new Param();
            param.Add("id", mId);
            param.Add("upgradeCount", mLogStacker);
            param.Add("remainStone", Lance.Account.Currency.GetUpgradeStones());

            Lance.BackEnd.InsertLog("UpgradePetEquipment", param, 7);
        }

        void Refresh()
        {
            // 장비 아이콘
            mEquipmentItemUI.Refresh();
            mEquipmentItemUI.SetActiveModal(false);
            mEquipmentItemUI.SetActiveIsEquipped(false);

            bool haveitem = Lance.Account.HavePetEquipment(mId);
            bool isEquipped = Lance.Account.IsEquippedPetEquipment(mId);
            bool canCombine = Lance.Account.IsEnoughPetEquipmentCount(mId, mData.combineCount);

            mButtonEquip.SetActiveFrame(!isEquipped && haveitem);

            mButtonCombine.SetActiveFrame(canCombine);
            mCombineRedDot.SetActive(canCombine);

            bool canUpgradeX1 = Lance.Account.CanUpgradePetEquipment(mId, 1);
            mButtonUpgradeX1.SetActiveFrame(canUpgradeX1);
            // 강화 비용
            double requireStones = Lance.Account.GetPetEquipmentUpgradeRequireStones(mId, 1);
            bool isEnoughStones = Lance.Account.IsEnoughUpgradeStones(requireStones);
            mTextUpgradePriceX1.text = requireStones.ToAlphaString();
            mTextUpgradePriceX1.SetColor(isEnoughStones ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            int maxLevelRequireCount = Lance.Account.GetPetEquipmentMaxLevelRequireCount(mId);
            bool canUpgradeXMax = Lance.Account.CanUpgradePetEquipment(mId, maxLevelRequireCount);
            mButtonUpgradeXMax.SetActiveFrame(canUpgradeXMax);

            // 강화 비용
            double requireStone2 = Lance.Account.GetPetEquipmentUpgradeRequireStones(mId, maxLevelRequireCount);
            bool isEnoughStone2 = Lance.Account.IsEnoughUpgradeStones(requireStone2);
            mTextUpgradePriceXMax.text = requireStone2.ToAlphaString();
            mTextUpgradePriceXMax.SetColor(isEnoughStone2 ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            // 합성 횟수
            int count = Lance.Account.GetPetEquipmentCount(mId);
            mTextCombineCount.text = $"{count}/{mData.combineCount}";

            // 장착 효과
            mEquipStatValueInfo.Refresh();

            // 보유 효과
            foreach (var ownStatValueInfo in mOwnStatValueInfos)
            {
                ownStatValueInfo.Refresh();
            }

            // 재화
            mTextUpgradeStoneAmount.text = Lance.Account.Currency.GetUpgradeStones().ToAlphaString();
            mTextReforgeStoneAmount.text = Lance.Account.Currency.GetReforgeStone().ToAlphaString();
            mTextElementalStoneAmount.text = $"{Lance.Account.Currency.GetElementalStone()}";

            // 재련
            bool canReforge = Lance.Account.CanReforgePetEquipment(mId);
            bool isMaxLevel = Lance.Account.IsMaxLevelPetEquipment(mId);
            bool isMaxReforge = Lance.Account.IsMaxReforgeStepPetEquipment(mId);
            int curReforgeStep = Lance.Account.GetPetEquipmentReforgeStep(mId);
            int nextStep = curReforgeStep + 1;

            if (isMaxReforge)
            {
                mButtonReforge.SetActiveFrame(false);
                mButtonAutoReforge.SetActiveFrame(false);
                mTextReforgeNeedDesc.gameObject.SetActive(false);
                mReforgeMaxStepObj.gameObject.SetActive(true);
                mReforgeNextStepObj.gameObject.SetActive(false);
                mProbObj.gameObject.SetActive(false);

                mTextReforgeStonePrice.text = "0";
                mTextReforgeElementalStonePrice.text = "0";

                mTextReforgeStonePrice.SetColor(Const.NotEnoughTextColor);
                mTextReforgeElementalStonePrice.SetColor(Const.NotEnoughTextColor);
                mTextBonusProbDesc.gameObject.SetActive(false);
            }
            else
            {
                mProbObj.gameObject.SetActive(true);

                // 재련비용
                double requireReforgeStones = Lance.Account.GetPetEquipmentReforgeRequireStones(mId);
                bool isEnoughReforgeStones = Lance.Account.IsEnoughReforgeStones(requireReforgeStones);

                if (isMaxLevel)
                {
                    mButtonReforge.SetActiveFrame(canReforge);
                    mButtonAutoReforge.SetActiveFrame(isEnoughReforgeStones && haveitem);
                    mTextReforgeNeedDesc.gameObject.SetActive(false);
                    mReforgeMaxStepObj.gameObject.SetActive(false);
                    mReforgeNextStepObj.gameObject.SetActive(true);

                    mTextReforgeCurStep.text = $"+{curReforgeStep}";
                    mTextReforgeNextStep.text = $"+{nextStep}";
                }
                else
                {
                    mButtonReforge.SetActiveFrame(false);
                    mButtonAutoReforge.SetActiveFrame(isEnoughStones && isEnoughReforgeStones && haveitem);
                    mTextReforgeNeedDesc.gameObject.SetActive(true);
                    mReforgeMaxStepObj.gameObject.SetActive(false);
                    mReforgeNextStepObj.gameObject.SetActive(false);
                }

                mTextReforgeStonePrice.text = requireReforgeStones.ToAlphaString();
                mTextReforgeStonePrice.SetColor(isEnoughReforgeStones ? Const.EnoughTextColor : Const.NotEnoughTextColor);

                int requireReforgeElementalStones = Lance.Account.GetPetEquipmentReforgeRequireElementalStones(mId);
                bool isEnoughElementalStones = Lance.Account.IsEnoughElementalStones(requireReforgeElementalStones);

                mTextReforgeElementalStonePrice.text = $"{requireReforgeElementalStones}";
                mTextReforgeElementalStonePrice.SetColor(isEnoughElementalStones ? Const.EnoughTextColor : Const.NotEnoughTextColor);

                int reporgeFailCount = Lance.Account.GetPetEquipmentReforgeFailCount(mId);

                float orgProb = DataUtil.GetPetEquipmentReforgeProb(mData.grade, curReforgeStep);
                // 재련확률
                mTextReforgeProb.text = $"{orgProb * 100f:F0}%";
                mTextReforgeBonusProb.gameObject.SetActive(reporgeFailCount > 0);
                mTextReforgeBonusProb.text = $"(+{DataUtil.GetPetEquipmentReforgeFailBonusProb(mData.grade, curReforgeStep, reporgeFailCount) * 100f:F2}%)";

                float bonusProb = orgProb * Lance.GameData.PetEquipmentCommonData.reforgeFailedBonusProbValue;

                StringParam param = new StringParam("bonusProb", $"{bonusProb * 100f:F2}");

                mTextBonusProbDesc.gameObject.SetActive(true);
                mTextBonusProbDesc.text = StringTableUtil.GetDesc("ReforgeBonusProb", param);
            }

            string equipmentName = StringTableUtil.GetName(mId);
            string equipmentFullName = curReforgeStep > 0 ? $"{equipmentName} +{curReforgeStep}" : equipmentName;

            SetTitleText(equipmentFullName);
        }

        void OnUpgradeMax()
        {
            int maxRequireCount = Lance.Account.GetPetEquipmentMaxLevelRequireCount(mId);

            if (Lance.GameManager.UpgradePetEquipment(mId, maxRequireCount))
            {
                SoundPlayer.PlayEquipmentLeveUp();

                Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

                mParent.Refresh();

                Refresh();

                Param param = new Param();
                param.Add("id", mId);
                param.Add("upgradeCount", maxRequireCount);
                param.Add("remainStone", Lance.Account.Currency.GetUpgradeStones());

                Lance.BackEnd.InsertLog("UpgradePetEquipment", param, 7);

                Lance.GameManager.UpdatePlayerStat();
            }
        }

        void OnUpgrade()
        {
            if (Lance.GameManager.UpgradePetEquipment(mId, 1))
            {
                SoundPlayer.PlayEquipmentLeveUp();

                Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

                mParent.Refresh();

                Refresh();

                mLogStacker += 1;
            }
        }

        void OnCombineButton()
        {
            (string id, int combineCount) result = Lance.GameManager.CombinePetEquipment(mId);
            if (result.combineCount > 0)
            {
                SoundPlayer.PlayEquipmentCombine();

                Lance.ParticleManager.AquireUI("EquipmentCombine", mEquipmentItemUI.ItemTm);

                mParent.Refresh();

                Refresh();
            }
        }

        void OnEquipButton()
        {
            if (Lance.GameManager.EquipPetEquipment(mId))
            {
                SoundPlayer.PlayEquipmentEquip();

                mParent.Refresh();

                Refresh();
            }
        }

        void OnReforgeButton()
        {
            mIsPress = false;

            var result = Lance.GameManager.ReforgePetEquipment(mId);
            if (result != -1)
            {
                mTouchBlock.SetActive(true);

                StartCoroutine(PlayReforge(result));
            }
        }

        IEnumerator PlayReforge(int result)
        {
            Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

            SoundPlayer.PlayReforge(1);

            yield return new WaitForSeconds(0.25f);

            Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

            SoundPlayer.PlayReforge(1);

            yield return new WaitForSeconds(0.25f);

            Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

            SoundPlayer.PlayReforge(2);

            yield return new WaitForSeconds(0.5f);

            if (result == 0)
            {
                UIUtil.ShowSystemErrorMessage("Reforge_Fail");
            }
            else
            {
                UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("Reforge_Success"));
            }

            mTouchBlock.SetActive(false);

            mParent.Refresh();

            Refresh();
        }

        const int TryAutoChangeStatStartCount = 100;
        const int TryAutoChangeStatAddCount = 10;
        const int TryAutoChangeStatMaxCount = 200;

        int mTryReforgeCount;
        int mTryReforgeStackedCount;

        int mStartLevel;
        int mStartReforgeStep;
        int mTargetReforgeStep;
        double mTotalAutoReforgeUseUpgradeStone;
        double mTotalAutoReforgeUseReforgeStone;
        double mTotalAutoReforgeUseElementalStone;
        void PlayAutoReforge(int targetReforgeStep)
        {
            if (mInAutoReforge)
                return;

            mStartLevel = Lance.Account.GetPetEquipmentLevel(mId);
            mStartReforgeStep = Lance.Account.GetPetEquipmentReforgeStep(mId);

            mTargetReforgeStep = targetReforgeStep;

            mTotalAutoReforgeUseUpgradeStone = 0;
            mTotalAutoReforgeUseReforgeStone = 0;
            mTotalAutoReforgeUseElementalStone = 0;

            mTryReforgeCount = 0;
            mTryReforgeStackedCount = TryAutoChangeStatStartCount;

            mInAutoReforge = true;

            mInAutoReforgeObj.SetActive(true);

            mAutoReforgeRoutine = StartCoroutine(AutoReforge(targetReforgeStep));
        }

        void StopAutoReforge()
        {
            if (mAutoReforgeRoutine != null)
                StopCoroutine(mAutoReforgeRoutine);

            mInAutoReforgeObj.SetActive(false);

            mInAutoReforge = false;

            Lance.BackEnd.UpdateAllAccountInfos();

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);

            Param param = new Param();
            param.Add("id", mId);
            param.Add("startLevel", mStartLevel);
            param.Add("startReforgeStep", mStartReforgeStep);
            param.Add("targetReforgeStep", mTargetReforgeStep);
            param.Add("currentReforgeStep", Lance.Account.GetPetEquipmentReforgeStep(mId));
            param.Add("currentLevel", Lance.Account.GetPetEquipmentLevel(mId));
            param.Add("usedReforgeStone", mTotalAutoReforgeUseReforgeStone);
            param.Add("usedElementalStone", mTotalAutoReforgeUseElementalStone);
            param.Add("usedUpgradeStone", mTotalAutoReforgeUseUpgradeStone);
            param.Add("remainReforgeStone", Lance.Account.Currency.GetReforgeStone());

            Lance.BackEnd.InsertLog("AutoReforgePetEquipment", param, 7);

            var popup = Lance.PopupManager.CreatePopup<Popup_AutoReforgeResultUI>();

            popup.InitPetEquipment(mId, mStartLevel);

            mStartLevel = 0;
            mStartReforgeStep = 0;
            mTargetReforgeStep = 0;
            mTotalAutoReforgeUseUpgradeStone = 0;
            mTotalAutoReforgeUseReforgeStone = 0;

            mTryReforgeCount = 0;
            mTryReforgeStackedCount = 0;

            Lance.GameManager.UpdatePlayerStat();
        }

        IEnumerator AutoReforge(int targetReforgeStep)
        {
            PetEquipmentInst equipItem = Lance.Account.GetPetEquipment(mId);
            if (equipItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotPetEquipment");

                StopAutoReforge();
            }

            while (true)
            {
                // 최고 단계 재련인가?
                if (equipItem.IsMaxReforge())
                {
                    UIUtil.ShowSystemErrorMessage("IsMaxReforgeStepPetEquipment");

                    StopAutoReforge();

                    break;
                }

                // 최고 레벨인가?
                if (equipItem.IsMaxLevel() == false)
                {
                    double requireStones = equipItem.GetUpgradeRequireStones(1);

                    if (Lance.GameManager.UpgradePetEquipment(mId, 1))
                    {
                        SoundPlayer.PlayEquipmentLeveUp();

                        Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

                        mParent.Refresh();

                        Refresh();

                        mTotalAutoReforgeUseUpgradeStone += requireStones;

                        yield return new WaitForSeconds(0.05f);
                    }
                    else
                    {
                        StopAutoReforge();

                        break;
                    }
                }
                else
                {
                    double requireStones = equipItem.GetReforgeRequireStone();
                    if (Lance.Account.IsEnoughReforgeStones(requireStones) == false)
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughReforgeStone");

                        StopAutoReforge();

                        break;
                    }

                    int requireElementalStone = equipItem.GetReforgeRequireElementalStone();
                    if (Lance.Account.IsEnoughElementalStones(requireElementalStone) == false)
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughElementalStone");

                        StopAutoReforge();

                        break;
                    }

                    mTotalAutoReforgeUseReforgeStone += requireStones;
                    mTotalAutoReforgeUseElementalStone += requireElementalStone;
                    
                    // 재련 적용
                    Lance.Account.ReforgePetEquipment(mId);

                    SoundPlayer.PlayReforge(1);

                    yield return new WaitForSeconds(0.05f);

                    Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

                    SoundPlayer.PlayReforge(2);

                    yield return new WaitForSeconds(0.05f);

                    mParent.Refresh();

                    Refresh();

                    mTryReforgeCount++;
                    if (mTryReforgeCount >= mTryReforgeStackedCount)
                    {
                        mTryReforgeStackedCount += TryAutoChangeStatAddCount;

                        mTryReforgeStackedCount = Math.Min(mTryReforgeStackedCount, TryAutoChangeStatMaxCount);

                        mTryReforgeCount = 0;

                        Lance.BackEnd.UpdateAllAccountInfos();
                    }
                }

                if (targetReforgeStep == equipItem.GetReforgeStep())
                {
                    StopAutoReforge();

                    break;
                }
            }
        }
    }
}
