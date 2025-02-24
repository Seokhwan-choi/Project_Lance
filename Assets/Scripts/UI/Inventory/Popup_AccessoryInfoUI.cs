using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using BackEnd;

namespace Lance
{
    class Popup_AccessoryInfoUI : PopupBase
    {
        bool mInAutoReforge;

        string mId;
        AccessoryInventoryTabUI mParent;
        Inventory_AccessoryItemUI mAccessoryItemUI;

        TextMeshProUGUI mTextCombineCount; // 장비 합성 횟수

        Button mButtonUpgradeX1;
        TextMeshProUGUI mTextUpgradePriceX1; // 장비 강화 비용
        Button mButtonUpgradeXMax;
        TextMeshProUGUI mTextUpgradePriceXMax; // 장비 강화 비용
        Button mButtonCombine;
        GameObject mCombineRedDot;
        Button mButtonEquip;
        GameObject mUnEquipButtonObj;
        Button mButtonUnEquip;

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
        TextMeshProUGUI mTextReforgeStonePrice;

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

        AccessoryData mData;
        public void Init(AccessoryInventoryTabUI parent, string id)
        {
            mParent = parent;
            mId = id;
            mData = DataUtil.GetAccessoryData(id);

            Debug.Assert(mData != null, $"{id}의 AccessoryAccessoryData가 존재하지 않음");

            SetUpCloseAction();

            // 장비 아이콘 초기화
            var AccessoryItemObj = gameObject.FindGameObject("Accessory");
            mAccessoryItemUI = AccessoryItemObj.GetOrAddComponent<Inventory_AccessoryItemUI>();
            mAccessoryItemUI.Init();
            mAccessoryItemUI.SetId(id);
            mAccessoryItemUI.SetSelected(false);
            mAccessoryItemUI.SetIgnoreRedDot(true);

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

            mUnEquipButtonObj = buttonsObj.FindGameObject("UnEquip");
            mButtonUnEquip = mUnEquipButtonObj.FindComponent<Button>("Button_UnEquip");
            mButtonUnEquip.SetButtonAction(OnUnEquipButton);

            mTextCombineCount = mButtonCombine.gameObject.FindComponent<TextMeshProUGUI>("Text_CombineCount");

            // 착용 효과
            GameObject equipValueInfoObj = gameObject.FindGameObject("EquipValueInfo");
            GameObject statValueInfoObj = equipValueInfoObj.FindGameObject("ValueInfo");
            mEquipStatValueInfo = statValueInfoObj.GetOrAddComponent<StatValueInfoUI>();
            mEquipStatValueInfo.InitAccessory(id);
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
                valueInfo.InitAccessory(id);

                string ownStatId = mData.ownStats[i];

                valueInfo.SetActive(ownStatId.IsValid());
                valueInfo.SetOwnStatId(ownStatId);

                mOwnStatValueInfos.Add(valueInfo);
            }

            if (Lance.LocalSave.IsNewAccessory(mId))
            {
                Lance.LocalSave.AddGetAccessory(mId);

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

            mTextReforgeStonePrice = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireReforgeStone");

            mReforgeNextStepObj = gameObject.FindGameObject("Reforge_NextStep");
            mReforgeMaxStepObj = gameObject.FindGameObject("Reforge_MaxStep");
            mTextReforgeCurStep = mReforgeNextStepObj.FindComponent<TextMeshProUGUI>("Text_CurStep");
            mTextReforgeNextStep = mReforgeNextStepObj.FindComponent<TextMeshProUGUI>("Text_NextStep");
            mTextBonusProbDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_BonusProb");

            // 자동 재련
            mButtonAutoReforge = gameObject.FindComponent<Button>("Button_AutoReforge");
            mButtonAutoReforge.SetButtonAction(() =>
            {
                AccessoryInst item = Lance.Account.GetAccessory(mId);
                if (item == null)
                {
                    UIUtil.ShowSystemErrorMessage("HaveNotAccessory");

                    return;
                }

                var popup = Lance.PopupManager.CreatePopup<Popup_AccessoryAutoReforgeUI>("Popup_AutoReforgeUI");

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

            Lance.BackEnd.InsertLog("UpgradeAccessory", param, 7);
        }

        void Refresh()
        {
            // 장비 아이콘
            mAccessoryItemUI.Refresh();
            mAccessoryItemUI.SetActiveModal(false);
            mAccessoryItemUI.SetActiveIsEquipped(false);

            bool haveitem = Lance.Account.HaveAccessory(mId);
            bool isEquipped = Lance.Account.IsEquippedAccessory(mId);
            bool canCombine = Lance.Account.IsEnoughAccessoryCount(mId, mData.combineCount);

            mButtonEquip.SetActiveFrame(!isEquipped && haveitem);
            mUnEquipButtonObj.SetActive(isEquipped);
            mButtonUnEquip.SetActiveFrame(isEquipped && haveitem);

            mButtonCombine.SetActiveFrame(canCombine);
            mCombineRedDot.SetActive(canCombine);

            bool canUpgradeX1 = Lance.Account.CanUpgradeAccessory(mId, 1);
            mButtonUpgradeX1.SetActiveFrame(canUpgradeX1);
            // 강화 비용
            double requireStones = Lance.Account.GetAccessoryUpgradeRequireStones(mId, 1);
            bool isEnoughStones = Lance.Account.IsEnoughUpgradeStones(requireStones);
            mTextUpgradePriceX1.text = requireStones.ToAlphaString();
            mTextUpgradePriceX1.SetColor(isEnoughStones ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            int maxLevelRequireCount = Lance.Account.GetAccessoryMaxLevelRequireCount(mId);
            bool canUpgradeXMax = Lance.Account.CanUpgradeAccessory(mId, maxLevelRequireCount);
            mButtonUpgradeXMax.SetActiveFrame(canUpgradeXMax);

            // 강화 비용
            double requireStone2 = Lance.Account.GetAccessoryUpgradeRequireStones(mId, maxLevelRequireCount);
            bool isEnoughStone2 = Lance.Account.IsEnoughUpgradeStones(requireStone2);
            mTextUpgradePriceXMax.text = requireStone2.ToAlphaString();
            mTextUpgradePriceXMax.SetColor(isEnoughStone2 ? Const.EnoughTextColor : Const.NotEnoughTextColor);

            // 합성 횟수
            int count = Lance.Account.GetAccessoryCount(mId);
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

            // 재련
            bool canReforge = Lance.Account.CanReforgeAccessory(mId);
            bool isMaxLevel = Lance.Account.IsMaxLevelAccessory(mId);
            bool isMaxReforge = Lance.Account.IsMaxReforgeStepAccessory(mId);
            int curReforgeStep = Lance.Account.GetAccessoryReforgeStep(mId);
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
                mTextReforgeStonePrice.SetColor(Const.NotEnoughTextColor);
                mTextBonusProbDesc.gameObject.SetActive(false);
            }
            else
            {
                mProbObj.gameObject.SetActive(true);

                // 재련비용
                double requireReforgeStones = Lance.Account.GetAccessoryReforgeRequireStones(mId);
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

                int reporgeFailCount = Lance.Account.GetAccessoryReforgeFailCount(mId);

                float orgProb = DataUtil.GetAccessoryReforgeProb(mData.grade, curReforgeStep);
                // 재련확률
                mTextReforgeProb.text = $"{orgProb * 100f:F0}%";
                mTextReforgeBonusProb.gameObject.SetActive(reporgeFailCount > 0);
                mTextReforgeBonusProb.text = $"(+{DataUtil.GetAccessoryReforgeFailBonusProb(mData.grade, curReforgeStep, reporgeFailCount) * 100f:F2}%)";

                float bonusProb = orgProb * Lance.GameData.AccessoryCommonData.reforgeFailedBonusProbValue;

                StringParam param = new StringParam("bonusProb", $"{bonusProb * 100f:F2}");

                mTextBonusProbDesc.gameObject.SetActive(true);
                mTextBonusProbDesc.text = StringTableUtil.GetDesc("ReforgeBonusProb", param);
            }

            string accessoryName = StringTableUtil.GetName(mId);
            string accessoryFullName = curReforgeStep > 0 ? $"{accessoryName} +{curReforgeStep}" : accessoryName;

            SetTitleText(accessoryFullName);
        }

        void OnUpgradeMax()
        {
            int maxRequireCount = Lance.Account.GetAccessoryMaxLevelRequireCount(mId);

            if (Lance.GameManager.UpgradeAccessory(mId, maxRequireCount))
            {
                SoundPlayer.PlayEquipmentLeveUp();

                Lance.ParticleManager.AquireUI("EquipmentLevelUp", mAccessoryItemUI.ItemTm);

                mParent.Refresh();

                Refresh();

                Param param = new Param();
                param.Add("id", mId);
                param.Add("upgradeCount", maxRequireCount);
                param.Add("remainStone", Lance.Account.Currency.GetUpgradeStones());

                Lance.BackEnd.InsertLog("UpgradeAccessory", param, 7);

                Lance.GameManager.UpdatePlayerStat();
            }
        }

        void OnUpgrade()
        {
            if (Lance.GameManager.UpgradeAccessory(mId, 1))
            {
                SoundPlayer.PlayEquipmentLeveUp();

                Lance.ParticleManager.AquireUI("EquipmentLevelUp", mAccessoryItemUI.ItemTm);

                mParent.Refresh();

                Refresh();

                mLogStacker += 1;
            }
        }

        void OnCombineButton()
        {
            (string id, int combineCount) result = Lance.GameManager.CombineAccessory(mId);
            if (result.combineCount > 0)
            {
                SoundPlayer.PlayEquipmentCombine();

                Lance.ParticleManager.AquireUI("EquipmentCombine", mAccessoryItemUI.ItemTm);

                mParent.Refresh();

                Refresh();
            }
        }

        void OnEquipButton()
        {
            if (Lance.GameManager.EquipAccessory(mId))
            {
                SoundPlayer.PlayEquipmentEquip();

                mParent.Refresh();

                Refresh();
            }
        }

        void OnUnEquipButton()
        {
            if (Lance.GameManager.UnEquipAccessory(mId))
            {
                SoundPlayer.PlayEquipmentEquip();

                mParent.Refresh();

                Refresh();
            }
        }

        void OnReforgeButton()
        {
            mIsPress = false;

            var result = Lance.GameManager.ReforgeAccessory(mId);
            if (result != -1)
            {
                mTouchBlock.SetActive(true);

                StartCoroutine(PlayReforge(result));
            }
        }

        IEnumerator PlayReforge(int result)
        {
            Lance.ParticleManager.AquireUI("EquipmentLevelUp", mAccessoryItemUI.ItemTm);

            SoundPlayer.PlayReforge(1);

            yield return new WaitForSeconds(0.25f);

            Lance.ParticleManager.AquireUI("EquipmentLevelUp", mAccessoryItemUI.ItemTm);

            SoundPlayer.PlayReforge(1);

            yield return new WaitForSeconds(0.25f);

            Lance.ParticleManager.AquireUI("EquipmentLevelUp", mAccessoryItemUI.ItemTm);

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

            mStartLevel = Lance.Account.GetAccessoryLevel(mId);
            mStartReforgeStep = Lance.Account.GetAccessoryReforgeStep(mId);

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

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);

            Param param = new Param();
            param.Add("id", mId);
            param.Add("startLevel", mStartLevel);
            param.Add("startReforgeStep", mStartReforgeStep);
            param.Add("targetReforgeStep", mTargetReforgeStep);
            param.Add("currentReforgeStep", Lance.Account.GetAccessoryReforgeStep(mId));
            param.Add("currentLevel", Lance.Account.GetAccessoryLevel(mId));
            param.Add("usedReforgeStone", mTotalAutoReforgeUseReforgeStone);
            param.Add("usedUpgradeStone", mTotalAutoReforgeUseUpgradeStone);
            param.Add("remainReforgeStone", Lance.Account.Currency.GetReforgeStone());

            Lance.BackEnd.InsertLog("AutoReforgeAccessory", param, 7);

            var popup = Lance.PopupManager.CreatePopup<Popup_AutoReforgeResultUI>();

            popup.InitAccessory(mId, mStartLevel);

            mStartLevel = 0;
            mStartReforgeStep = 0;
            mTargetReforgeStep = 0;
            mTotalAutoReforgeUseUpgradeStone = 0;
            mTotalAutoReforgeUseReforgeStone = 0;

            Lance.GameManager.UpdatePlayerStat();
        }

        IEnumerator AutoReforge(int targetReforgeStep)
        {
            AccessoryInst item = Lance.Account.GetAccessory(mId);
            if (item == null)
            {
                UIUtil.ShowSystemErrorMessage("HaveNotAccessory");

                StopAutoReforge();
            }

            while (true)
            {
                // 최고 단계 재련인가?
                if (item.IsMaxReforge())
                {
                    UIUtil.ShowSystemErrorMessage("IsMaxReforgeStepAccessory");

                    StopAutoReforge();

                    break;
                }

                // 최고 레벨인가?
                if (item.IsMaxLevel() == false)
                {
                    double requireStones = item.GetUpgradeRequireStones(1);

                    if (Lance.GameManager.UpgradeAccessory(mId, 1))
                    {
                        SoundPlayer.PlayEquipmentLeveUp();

                        Lance.ParticleManager.AquireUI("EquipmentLevelUp", mAccessoryItemUI.ItemTm);

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
                    double requireStones = item.GetReforgeRequireStone();
                    if (Lance.Account.IsEnoughReforgeStones(requireStones) == false)
                    {
                        UIUtil.ShowSystemErrorMessage("IsNotEnoughReforgeStone");

                        StopAutoReforge();

                        break;
                    }

                    mTotalAutoReforgeUseReforgeStone += requireStones;

                    // 재련 적용
                    Lance.Account.ReforgeAccessory(mId);

                    SoundPlayer.PlayReforge(1);

                    yield return new WaitForSeconds(0.05f);

                    Lance.ParticleManager.AquireUI("EquipmentLevelUp", mAccessoryItemUI.ItemTm);

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

                if (targetReforgeStep == item.GetReforgeStep())
                {
                    StopAutoReforge();

                    break;
                }
            }
        }
    }
}
