using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System;
using System.Linq;

namespace Lance
{
    class Popup_EquipmentInfoUI : PopupBase
    {
        string mId;
        bool mInAutoChangeOption;
        bool mInAutoReforge;
        InventoryTabUI mParent;
        Inventory_EquipmentItemUI mEquipmentItemUI;

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

        // 재련 관련
        GameObject mTouchBlock;
        Button mButtonReforge;
        
        TextMeshProUGUI mTextReforgeNeedDesc;
        GameObject mProbObj;
        TextMeshProUGUI mTextReforgeProb;
        TextMeshProUGUI mTextReforgeBonusProb;
        TextMeshProUGUI mTextReforgePrice;

        GameObject mReforgeNextStepObj;
        TextMeshProUGUI mTextReforgeCurStep;
        TextMeshProUGUI mTextReforgeNextStep;

        GameObject mReforgeMaxStepObj;
        TextMeshProUGUI mTextBonusProbDesc;

        // 자동 재련
        Button mButtonAutoReforge;
        GameObject mInAutoReforgeObj;
        Coroutine mAutoReforgeRoutine;

        // 장비 옵션
        List<EquipmentOptionStatUI> mEquipmentOptionStats;
        Button mButtonChangeOptionStat;
        TextMeshProUGUI mTextChangePrice;

        int mSelectedPreset;
        List<ButtonChangePresetUI> mButtonChangePresetUIList;

        // 장비 옵션 자동 재설정
        GameObject mInAutoChangeOptionObj;
        Coroutine mAutoChangeRoutine;
        Button mButtonAutoChangeOptionStat;

        const float IntervalTime = 0.1f;
        const float LogTime = 1f;
        float mIntervalTime;
        float mLogTime;
        int mLogStacker;
        bool mIsPress;

        float mIntervalTime2;
        float mLogTime2;
        double mLogStacker2;
        bool mIsPress2;

        EquipmentData Data => DataUtil.GetEquipmentData(mId);
        public void Init(InventoryTabUI parent, string id)
        {
            mParent = parent;
            mId = id;

            Debug.Assert(Data != null, $"{id}의 EquipmentData가 존재하지 않음");

            SetUpCloseAction();

            // 장비 아이콘 초기화
            var equipmentItemObj = gameObject.FindGameObject("Equipment");
            mEquipmentItemUI = equipmentItemObj.GetOrAddComponent<Inventory_EquipmentItemUI>();
            mEquipmentItemUI.Init();
            mEquipmentItemUI.SetId(id);
            mEquipmentItemUI.SetSelected(false);
            mEquipmentItemUI.SetIgnoreRedDot(true);

            // 버튼 초기화
            GameObject buttonsObj = gameObject.FindGameObject("Buttons");
            mButtonUpgradeX1 = buttonsObj.FindComponent<Button>("Button_UpgradeX1");
            var eventTrigger = mButtonUpgradeX1.GetOrAddComponent<EventTrigger>();
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerDown, () => OnUpgradeButton(isPress:true));
            eventTrigger.AddTriggerEvent(EventTriggerType.PointerUp, () => OnUpgradeButton(isPress:false));
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
            mEquipStatValueInfo.InitEquipment(id);
            mEquipStatValueInfo.Refresh();

            // 보유 효과
            mOwnStatValueInfos = new List<StatValueInfoUI>();
            GameObject ownValueInfoObj = gameObject.FindGameObject("OwnValueInfo");
            GameObject valueInfosObj = ownValueInfoObj.FindGameObject("ValueInfos");

            for (int i = 0; i < Data.ownStats.Length; ++i)
            {
                int index = i + 1;

                statValueInfoObj = valueInfosObj.FindGameObject($"StatValueInfo_{index}");
                var valueInfo = statValueInfoObj.GetOrAddComponent<StatValueInfoUI>();
                valueInfo.InitEquipment(id);
                
                string ownStatId = Data.ownStats[i];

                valueInfo.SetActive(ownStatId.IsValid());
                valueInfo.SetOwnStatId(ownStatId);

                mOwnStatValueInfos.Add(valueInfo);
            }

            if (Lance.LocalSave.IsNewEquipment(mId))
            {
                Lance.LocalSave.AddGetEquipment(mId);

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);
            }

            mTextUpgradeStoneAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_UpgradeStoneAmount");
            mTextReforgeStoneAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeStoneAmount");

            // 재련
            mTouchBlock = gameObject.FindGameObject("TouchBlock");
            mButtonReforge = gameObject.FindComponent<Button>("Button_Reforge");
            mButtonReforge.SetButtonAction(OnReforgeButton);

            mProbObj = gameObject.FindGameObject("Prob");
            mTextReforgeNeedDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgeNeedDesc");
            mTextReforgeProb = mProbObj.FindComponent<TextMeshProUGUI>("Text_ReforgeProb");
            mTextReforgeBonusProb = mProbObj.FindComponent<TextMeshProUGUI>("Text_ReforgeBonus");
            mTextReforgePrice = gameObject.FindComponent<TextMeshProUGUI>("Text_ReforgePrice");
            mReforgeNextStepObj = gameObject.FindGameObject("Reforge_NextStep");
            mReforgeMaxStepObj = gameObject.FindGameObject("Reforge_MaxStep");
            mTextReforgeCurStep = mReforgeNextStepObj.FindComponent<TextMeshProUGUI>("Text_CurStep");
            mTextReforgeNextStep = mReforgeNextStepObj.FindComponent<TextMeshProUGUI>("Text_NextStep");
            mTextBonusProbDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_BonusProb");

            // 자동 재련
            mButtonAutoReforge = gameObject.FindComponent<Button>("Button_AutoReforge");
            mButtonAutoReforge.SetButtonAction(() =>
            {
                EquipmentInst equipItem = Lance.Account.GetEquipment(mId);
                if (equipItem == null)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotEquipment");

                    return;
                }

                var popup = Lance.PopupManager.CreatePopup<Popup_AutoReforgeUI>();

                popup.Init(mId, PlayAutoReforge);
            });

            mInAutoReforgeObj = gameObject.FindGameObject("InAutoReforge");
            mInAutoReforgeObj.SetActive(false);

            var buttonAutoReforgeStop = mInAutoReforgeObj.FindComponent<Button>("Button_AutoReforgeStop");
            buttonAutoReforgeStop.SetButtonAction(() =>
            {
                StopAutoReforge();
            });

            // 옵션 변경 프리셋
            mButtonChangePresetUIList = new List<ButtonChangePresetUI>();

            var presetButtons = gameObject.FindGameObject("PresetButtons");

            for (int i = 0; i < Lance.GameData.PetCommonData.evolutionStatMaxPreset; ++i)
            {
                int preset = i;

                var buttonPresetObj = presetButtons.FindGameObject($"Button_ChangePreset_{preset + 1}");

                var buttonPresetUI = buttonPresetObj.GetOrAddComponent<ButtonChangePresetUI>();

                buttonPresetUI.Init(preset, OnChangePreset);

                mButtonChangePresetUIList.Add(buttonPresetUI);
            }

            mSelectedPreset = Lance.Account.GetEquipmentCurrentPreset(id);

            // 옵션 변경
            var buttonOptionStatProb = gameObject.FindComponent<Button>("Button_OptionStatInfo");
            buttonOptionStatProb.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_EquipmentOptionInfoUI>();

                popup.Init();
            });

            var optionStatObj = gameObject.FindGameObject("OptionStat");

            mEquipmentOptionStats = new List<EquipmentOptionStatUI>();

            for (int i = 0; i < Lance.GameData.EquipmentCommonData.optionStatMaxCount; ++i)
            {
                int slot = i;
                int index = slot + 1;

                var optionStatUIObj = optionStatObj.FindGameObject($"OptionStatUI_{index}");

                var optionStatUI = optionStatUIObj.GetOrAddComponent<EquipmentOptionStatUI>();

                optionStatUI.Init(mId, slot, Refresh);

                mEquipmentOptionStats.Add(optionStatUI);
            }

            mButtonChangeOptionStat = optionStatObj.FindComponent<Button>("Button_Change");
            var eventTrigger2 = mButtonChangeOptionStat.GetOrAddComponent<EventTrigger>();
            eventTrigger2.AddTriggerEvent(EventTriggerType.PointerDown, () => OnChangeStatButton(isPress: true));
            eventTrigger2.AddTriggerEvent(EventTriggerType.PointerUp, () => OnChangeStatButton(isPress: false));

            mInAutoChangeOptionObj = gameObject.FindGameObject("InAutoChangeOption");
            mInAutoChangeOptionObj.SetActive(false);

            mButtonAutoChangeOptionStat = optionStatObj.FindComponent<Button>("Button_AutoChange");
            mButtonAutoChangeOptionStat.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_AutoChangeOptionStatSelectUI>();

                popup.Init(StringTableUtil.Get("Title_AutoChangeOption"), 
                    DataUtil.EquipmentOptionStatTypes, DataUtil.EquipmentOptionStatGradeToGrade().ToArray(), PlayAutoChangeStat);
            });

            var buttonAutoChangeOptionStop = mInAutoChangeOptionObj.FindComponent<Button>("Button_AutoChangeOptionStop");
            buttonAutoChangeOptionStop.SetButtonAction(() =>
            {
                StopAutoChangeStat();
            });

            mTextChangePrice = optionStatObj.FindComponent<TextMeshProUGUI>("Text_ChangePrice");

            Refresh();
        }

        public override void OnClose()
        {
            base.OnClose();

            if (mLogStacker > 0)
            {
                InsertUpgradeLog();
            }

            if (mLogStacker2 > 0)
            {
                InsertChangeStatLog();
            }

            Lance.Lobby.RefreshCollectionRedDot();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            UpdateUpgrade(dt);
            UpdateChangeStat(dt);
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

                        Lance.Lobby.RefreshCollectionRedDot();
                    }
                }
            }
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
            param.Add("usedReforgeStone", mLogStacker2);
            param.Add("remainReforgeStone", Lance.Account.Currency.GetReforgeStone());

            Lance.BackEnd.InsertLog("ChangeOptionStat", param, 7);

            mLogStacker2 = 0;
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

            Lance.BackEnd.InsertLog("UpgradeEquipment", param, 7);
        }

        void OnChangePreset(int preset)
        {
            Lance.GameManager.ChangeEquipmentOptionStatPreset(mId, preset);

            mSelectedPreset = Lance.Account.GetEquipmentCurrentPreset(mId);

            Refresh();
        }

        void Refresh()
        {
            if (mId.IsValid() == false)
                return;

            EquipmentData data = DataUtil.GetEquipmentData(mId);
            if (data == null)
                return;

            // 장비 아이콘
            mEquipmentItemUI.Refresh();
            mEquipmentItemUI.SetActiveModal(false);
            mEquipmentItemUI.SetActiveIsEquipped(false);

            bool haveitem = Lance.Account.HaveEquipment(mId);
            bool isEquipped = Lance.Account.IsEquippedEquipment(mId);
            
            
            bool canCombine = Lance.Account.IsEnoughCount(mId, data.combineCount);

            mButtonEquip.SetActiveFrame(!isEquipped && haveitem);

            mButtonCombine.SetActiveFrame(canCombine);
            mCombineRedDot.SetActive(canCombine);

            bool canUpgradeX1 = Lance.Account.CanUpgradeEquipment(mId, 1);
            mButtonUpgradeX1.SetActiveFrame(canUpgradeX1);
            // 강화 비용
            double requireStones = Lance.Account.GetEquipmentUpgradeRequireStones(mId, 1);
            bool isEnoughStones = Lance.Account.IsEnoughUpgradeStones(requireStones);
            mTextUpgradePriceX1.text = requireStones.ToAlphaString();
            mTextUpgradePriceX1.SetColor(isEnoughStones ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            int maxLevelRequireCount = Lance.Account.GetEquipmentMaxLevelRequireCount(mId);
            bool canUpgradeXMax = Lance.Account.CanUpgradeEquipment(mId, maxLevelRequireCount);
            mButtonUpgradeXMax.SetActiveFrame(canUpgradeXMax);

            // 강화 비용
            double requireStone2 = Lance.Account.GetEquipmentUpgradeRequireStones(mId, maxLevelRequireCount);
            bool isEnoughStone2 = Lance.Account.IsEnoughUpgradeStones(requireStone2);
            mTextUpgradePriceXMax.text = requireStone2.ToAlphaString();
            mTextUpgradePriceXMax.SetColor(isEnoughStone2 ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            // 합성 횟수
            int count = Lance.Account.GetEquipmentCount(mId);
            mTextCombineCount.text = $"{count}/{data.combineCount}";

            // 장착 효과
            mEquipStatValueInfo.Refresh();

            // 보유 효과
            foreach(var ownStatValueInfo in mOwnStatValueInfos)
            {
                ownStatValueInfo.Refresh();
            }

            // 재화
            mTextUpgradeStoneAmount.text = Lance.Account.Currency.GetUpgradeStones().ToAlphaString();
            mTextReforgeStoneAmount.text = Lance.Account.Currency.GetReforgeStone().ToAlphaString();

            // 재련
            bool canReforge = Lance.Account.CanReforgeEquipment(mId);
            bool isMaxLevel = Lance.Account.IsMaxLevelEquipment(mId);
            bool isMaxReforge = Lance.Account.IsMaxReforgeStepEquipment(mId);
            int curReforgeStep = Lance.Account.GetEquipmentReforgeStep(mId);
            int nextStep = curReforgeStep + 1;

            if (isMaxReforge)
            {
                mButtonReforge.SetActiveFrame(false);
                mButtonAutoReforge.SetActiveFrame(false);
                mTextReforgeNeedDesc.gameObject.SetActive(false);
                mReforgeMaxStepObj.gameObject.SetActive(true);
                mReforgeNextStepObj.gameObject.SetActive(false);
                mProbObj.gameObject.SetActive(false);

                mTextReforgePrice.text = "0";
                mTextReforgePrice.SetColor(Const.NotEnoughTextColor);
                mTextBonusProbDesc.gameObject.SetActive(false);
            }
            else
            {
                mProbObj.gameObject.SetActive(true);

                // 재련비용
                double requireReforgeStones = Lance.Account.GetEquipmentReforgeRequireStones(mId);
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

                mTextReforgePrice.text = requireReforgeStones.ToAlphaString();
                mTextReforgePrice.SetColor(isEnoughReforgeStones ? Const.EnoughTextColor : Const.NotEnoughTextColor);

                int reporgeFailCount = Lance.Account.GetEquipmentReforgeFailCount(mId);

                float orgProb = DataUtil.GetEquipmentReforgeProb(data.grade, curReforgeStep);
                // 재련확률
                mTextReforgeProb.text = $"{orgProb * 100f:F0}%";
                mTextReforgeBonusProb.gameObject.SetActive(reporgeFailCount > 0);
                mTextReforgeBonusProb.text = $"(+{DataUtil.GetEquipmentReforgeFailBonusProb(data.grade, curReforgeStep, reporgeFailCount)*100f:F2}%)";

                float bonusProb = orgProb * Lance.GameData.EquipmentCommonData.reforgeFailedBonusProbValue;

                StringParam param = new StringParam("bonusProb", $"{bonusProb * 100f:F2}");

                mTextBonusProbDesc.gameObject.SetActive(true);
                mTextBonusProbDesc.text = StringTableUtil.GetDesc("ReforgeBonusProb", param);
            }

            string equipmentName = StringTableUtil.GetName(mId);
            string equipmentFullName = curReforgeStep > 0 ? $"{equipmentName} +{curReforgeStep}" : equipmentName;

            SetTitleText(equipmentFullName);

            // 장비 옵션
            bool anyCanChangeOptionStat = Lance.Account.AnyCanChangeOptionStat(mId);
            bool allLocked = Lance.Account.GetEquipmentOptionStatLockCount(mId) == Lance.GameData.EquipmentCommonData.optionStatMaxCount;
            double requireReforgeStone = Lance.Account.GetEquipmentOptionChangePrice(mId);
            bool isEnoughReforgeStone = Lance.Account.Currency.IsEnoughReforgeStone(requireReforgeStone);

            mButtonChangeOptionStat.SetActiveFrame(anyCanChangeOptionStat && allLocked == false && isEnoughReforgeStone);
            mButtonAutoChangeOptionStat.SetActiveFrame(anyCanChangeOptionStat && allLocked == false && isEnoughReforgeStone);

            mTextChangePrice.SetColor(UIUtil.GetEnoughTextColor(isEnoughReforgeStone));
            mTextChangePrice.text = requireReforgeStone.ToAlphaString();

            mEquipmentOptionStats.ForEach(x => x.Refresh());

            // 장비 옵션 프리셋
            foreach (var buttonChangePreset in mButtonChangePresetUIList)
            {
                buttonChangePreset.SetActiveSprite(buttonChangePreset.Preset == mSelectedPreset);
            }
        }

        void OnUpgradeMax()
        {
            int maxRequireCount = Lance.Account.GetEquipmentMaxLevelRequireCount(mId);

            if (Lance.GameManager.UpgradeEquipment(mId, maxRequireCount))
            {
                SoundPlayer.PlayEquipmentLeveUp();

                Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

                mParent.Refresh();

                Refresh();

                Param param = new Param();
                param.Add("id", mId);
                param.Add("upgradeCount", maxRequireCount);
                param.Add("remainStone", Lance.Account.Currency.GetUpgradeStones());

                Lance.BackEnd.InsertLog("UpgradeEquipment", param, 7);

                Lance.GameManager.UpdatePlayerStat();
            }
        }

        void OnUpgrade()
        {
            if ( Lance.GameManager.UpgradeEquipment(mId, 1) )
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
            (string id, int combineCount) result = Lance.GameManager.CombineEquipment(mId);
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
            if (Lance.GameManager.EquipEquipment(mId))
            {
                SoundPlayer.PlayEquipmentEquip();

                mParent.Refresh();

                Refresh();
            }
        }

        void OnReforgeButton()
        {
            mIsPress = false;

            var result = Lance.GameManager.ReforgeEquipment(mId);
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

        // 장비 옵션 변경
        void OnChangeStatButton(bool isPress)
        {
            if (mInAutoChangeOption)
                return;

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
            double changePrice = Lance.Account.GetEquipmentOptionChangePrice(mId);

            if (Lance.GameManager.ChangeEquipmentOptionStat(mId))
            {
                SoundPlayer.PlayEquipmentLeveUp();

                Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

                mParent.Refresh();

                //bool isEquipped = Lance.Account.IsEquippedEquipment(mId);
                //if (isEquipped)
                //{
                //    var popup = Lance.PopupManager.GetPopup<Popup_UserInfoUI>();

                //    popup?.RefresEquippedEquipments();
                //}

                Refresh();

                mLogStacker2 += changePrice;
            }
        }

        int mStartLevel;
        int mStartReforgeStep;
        int mTargetReforgeStep;
        int mTryReforgeCount;
        double mTotalAutoReforgeUseUpgradeStone;
        double mTotalAutoReforgeUseReforgeStone;
        void PlayAutoReforge(int targetReforgeStep)
        {
            if (mInAutoReforge)
                return;

            mStartLevel = Lance.Account.GetEquipmentLevel(mId);
            mStartReforgeStep = Lance.Account.GetEquipmentReforgeStep(mId);

            mTargetReforgeStep = targetReforgeStep;

            mTotalAutoReforgeUseUpgradeStone = 0;
            mTotalAutoReforgeUseReforgeStone = 0;

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

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Inventory);

            Param param = new Param();
            param.Add("id", mId);
            param.Add("startLevel", mStartLevel);
            param.Add("startReforgeStep", mStartReforgeStep);
            param.Add("targetReforgeStep", mTargetReforgeStep);
            param.Add("currentReforgeStep", Lance.Account.GetEquipmentReforgeStep(mId));
            param.Add("currentLevel", Lance.Account.GetEquipmentLevel(mId));
            param.Add("usedReforgeStone", mTotalAutoReforgeUseReforgeStone);
            param.Add("usedUpgradeStone", mTotalAutoReforgeUseUpgradeStone);
            param.Add("remainReforgeStone", Lance.Account.Currency.GetReforgeStone());

            Lance.BackEnd.InsertLog("AutoReforge", param, 7);

            var popup = Lance.PopupManager.CreatePopup<Popup_AutoReforgeResultUI>();
            popup.InitEquipment(mId, mStartLevel);

            mStartLevel = 0;
            mStartReforgeStep = 0;
            mTargetReforgeStep = 0;
            mTotalAutoReforgeUseUpgradeStone = 0;
            mTotalAutoReforgeUseReforgeStone = 0;

            Lance.GameManager.UpdatePlayerStat();
        }

        IEnumerator AutoReforge(int targetReforgeStep)
        {
            EquipmentInst equipItem = Lance.Account.GetEquipment(mId);
            if (equipItem == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotEquipment");

                StopAutoReforge();
            }

            while (true)
            {
                // 최고 단계 재련인가?
                if (equipItem.IsMaxReforge())
                {
                    UIUtil.ShowSystemErrorMessage("IsMaxReforgeStepEquipment");

                    StopAutoReforge();

                    break;
                }

                // 최고 레벨인가?
                if (equipItem.IsMaxLevel() == false)
                {
                    double requireStones = equipItem.GetUpgradeRequireStones(1);

                    if (Lance.GameManager.UpgradeEquipment(mId, 1))
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

                    mTotalAutoReforgeUseReforgeStone += requireStones;

                    // 재련 적용
                    Lance.Account.ReforgeEquipment(mId);

                    SoundPlayer.PlayReforge(1);

                    yield return new WaitForSeconds(0.05f);

                    Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

                    SoundPlayer.PlayReforge(2);

                    yield return new WaitForSeconds(0.05f);

                    mParent.Refresh();

                    Refresh();

                    mTryReforgeCount++;

                    if (mTryReforgeCount >= 5)
                    { 
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

        const int TryAutoChangeStatStartCount = 100;
        const int TryAutoChangeStatAddCount = 10;
        const int TryAutoChangeStatMaxCount = 200;

        int mTryAutoChangeStat;
        int mTryAutoChangeStackedStat;
        double mTotalAutoChangeUseReforge;
        void PlayAutoChangeStat(StatType[] selectedStatTypes, Grade selectedGrade)
        {
            if (mInAutoChangeOption)
                return;

            mTotalAutoChangeUseReforge = 0;
            mTryAutoChangeStat = 0;
            mTryAutoChangeStackedStat = TryAutoChangeStatStartCount;

            mInAutoChangeOption = true;

            mInAutoChangeOptionObj.SetActive(true);

            mAutoChangeRoutine = StartCoroutine(AutoChangeStat(selectedStatTypes, selectedGrade));
        }

        void StopAutoChangeStat()
        {
            if (mAutoChangeRoutine != null)
                StopCoroutine(mAutoChangeRoutine);

            mInAutoChangeOptionObj.SetActive(false);

            mInAutoChangeOption = false;

            Lance.BackEnd.UpdateAllAccountInfos();

            Param param = new Param();
            param.Add("id", mId);
            param.Add("usedReforgeStone", mTotalAutoChangeUseReforge);
            param.Add("remainReforgeStone", Lance.Account.Currency.GetReforgeStone());

            Lance.BackEnd.InsertLog("AutoChangeOptionStat", param, 7);

            mTotalAutoChangeUseReforge = 0;
            mTryAutoChangeStat = 0;
            mTryAutoChangeStackedStat = 0;

            Lance.GameManager.UpdatePlayerStat();
        }

        IEnumerator AutoChangeStat(StatType[] selectedStatTypes, Grade selectedGrade)
        {
            while(true)
            {
                double changePrice = Lance.Account.GetEquipmentOptionChangePrice(mId);

                if (Lance.GameManager.ChangeEquipmentOptionStat(mId))
                {
                    SoundPlayer.PlayEquipmentLeveUp();

                    Lance.ParticleManager.AquireUI("EquipmentLevelUp", mEquipmentItemUI.ItemTm);

                    mParent.Refresh();

                    Refresh();

                    mTotalAutoChangeUseReforge += changePrice;

                    mTryAutoChangeStat++;
                    if ( mTryAutoChangeStat >= mTryAutoChangeStackedStat)
                    {
                        mTryAutoChangeStackedStat += TryAutoChangeStatAddCount;

                        mTryAutoChangeStackedStat = Math.Min(mTryAutoChangeStackedStat, TryAutoChangeStatMaxCount);

                        mTryAutoChangeStat = 0;

                        Lance.BackEnd.UpdateAllAccountInfos();
                    }

                    // 조건을 만족하거나 재화가 충분하면 그만하자
                    if (IsSatisfied() || IsEnoughReforgeStone() == false)
                    {
                        StopAutoChangeStat();

                        break;
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.05f);
                    }
                }
                else
                {
                    StopAutoChangeStat();

                    break;
                }
            }

            bool IsSatisfied()
            {
                var equipmentInst = Lance.Account.GetEquipment(mId);

                return equipmentInst.IsSatisfied(selectedStatTypes, (EquipmentOptionStatGrade)((int)selectedGrade)-1);
            }

            bool IsEnoughReforgeStone()
            {
                double changePrice = Lance.Account.GetEquipmentOptionChangePrice(mId);

                if (Lance.Account.Currency.IsEnoughReforgeStone(changePrice) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughReforgeStone");

                    return false;
                }

                return true;
            }
        }
    }
}
