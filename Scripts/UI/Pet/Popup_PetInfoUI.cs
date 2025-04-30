using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using DG.Tweening;


namespace Lance
{
    class Popup_PetInfoUI : PopupBase
    {
        string mId;
        ElementalType mType;
        bool mInAutoChangeEvolutionStat;
        GameObject mIsEquippedObj;
        PetUIMotion mPetUIMotion;
        TextMeshProUGUI mTextLevel;
        TextMeshProUGUI mTextExp;
        TextMeshProUGUI mTextPetFood;
        TextMeshProUGUI mTextElementalStone;
        TextMeshProUGUI mTextRequireElementalStone;
        Slider mSliderExp;
        SubGradeManager mSubGradeManager;

        GameObject mPetItemObj;

        GameObject mFeedObj;
        Button mButtonFeed_1;
        Button mButtonFeed_100;
        Button mButtonFeed_Max;

        GameObject mEvolutionObj;
        Button mButtonEvolution;
        GameObject mEvolutionRedDot;

        Button mButtonEquip;

        StatValueInfoUI mEquipStatValueInfo;
        List<StatValueInfoUI> mOwnStatValueInfos;

        List<PetEvolutionStatUI2> mPetEvolutionStats;

        int mSelectedPreset;
        List<ButtonChangePresetUI> mButtonChangePresetUIList;

        // 스킬
        // 액티브 스킬
        GameObject mActiveSkillUnlock;
        GameObject mActiveSkillLock;

        // 액티브 스킬 자동 시전
        Button mButtonAutoCast;
        Image mImageAutoCastFrame;
        Image mImageAutoCast;

        // 패시브 스킬
        GameObject mPassiveSkillUnlock;
        GameObject mPassiveSkillLock;

        Tweener mAutoCastTween;

        public void Init(string id)
        {
            SetUpCloseAction();

            var data = Lance.GameData.PetData.TryGet(id);

            var buttonDesc = gameObject.FindComponent<Button>("Button_PetDesc");
            buttonDesc.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_PetDescUI>();

                popup.Init();
            });

            mId = id;
            mType = data.type;
            mSelectedPreset = Lance.Account.Pet.GetCurrentPreset(id);

            var petItemUIObj = gameObject.FindGameObject("PetItemUI");
            mPetItemObj = petItemUIObj.FindGameObject("Item");

            var imageElementalType = gameObject.FindComponent<Image>("Image_ElementalType");
            imageElementalType.sprite = Lance.Atlas.GetUISprite($"Icon_Ele_{data.type}");
            
            var imagePet = gameObject.FindComponent<Image>("Image_Pet");
            mPetUIMotion = imagePet.GetOrAddComponent<PetUIMotion>();
            mPetUIMotion.Init();

            mSliderExp = gameObject.FindComponent<Slider>("Slider_Exp");
            mTextExp = gameObject.FindComponent<TextMeshProUGUI>("Text_Exp");
            mTextPetFood = gameObject.FindComponent<TextMeshProUGUI>("Text_PetFoodAmount");
            mTextElementalStone = gameObject.FindComponent<TextMeshProUGUI>("Text_ElementalStoneAmount");
            mTextRequireElementalStone = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireElementalStone");
            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
            mIsEquippedObj = gameObject.FindGameObject("IsEquipped");

            mSubGradeManager = new SubGradeManager();
            mSubGradeManager.Init(gameObject.FindGameObject("Step"));

            // 착용 효과
            GameObject equipValueInfoObj = gameObject.FindGameObject("EquipValueInfo");

            // 속성 공격
            string elementalName = StringTableUtil.GetName($"Elemental_{data.type}");
            StringParam param = new StringParam("elemental", elementalName);

            var textElementalEffect = gameObject.FindComponent<TextMeshProUGUI>("Text_ElementalEffect");
            textElementalEffect.text = StringTableUtil.Get("Desc_ElementalEffect", param);

            // 추가 데미지
            GameObject statValueInfoObj = equipValueInfoObj.FindGameObject("StatValueInfo_1");
            mEquipStatValueInfo = statValueInfoObj.GetOrAddComponent<StatValueInfoUI>();
            mEquipStatValueInfo.InitPet(id);

            // 보유 효과
            mOwnStatValueInfos = new List<StatValueInfoUI>();
            GameObject ownValueInfoObj = gameObject.FindGameObject("OwnValueInfo");
            GameObject valueInfosObj = ownValueInfoObj.FindGameObject("ValueInfos");

            for (int i = 0; i < data.ownStats.Length; ++i)
            {
                int index = i + 1;

                statValueInfoObj = valueInfosObj.FindGameObject($"StatValueInfo_{index}");
                var valueInfo = statValueInfoObj.GetOrAddComponent<StatValueInfoUI>();
                valueInfo.InitPet(id);

                string ownStatId = data.ownStats[i];

                valueInfo.SetActive(ownStatId.IsValid());
                valueInfo.SetOwnStatId(ownStatId);

                mOwnStatValueInfos.Add(valueInfo);
            }

            mFeedObj = gameObject.FindGameObject("Feed");
            mButtonFeed_1 = mFeedObj.FindComponent<Button>("Button_FeedX1");
            mButtonFeed_1.SetButtonAction(() => OnFeed(1));
            mButtonFeed_100 = mFeedObj.FindComponent<Button>("Button_FeedX100");
            mButtonFeed_100.SetButtonAction(() => OnFeed(100));
            mButtonFeed_Max = mFeedObj.FindComponent<Button>("Button_FeedMax");
            mButtonFeed_Max.SetButtonAction(OnFeedMax);

            mEvolutionObj = gameObject.FindGameObject("Evolution");
            mButtonEvolution = mEvolutionObj.FindComponent<Button>("Button_Evolution");
            mButtonEvolution.SetButtonAction(OnEvolutionButton);

            mEvolutionRedDot = mButtonEvolution.gameObject.FindGameObject("RedDot");

            mButtonEquip = gameObject.FindComponent<Button>("Button_Equip");
            mButtonEquip.SetButtonAction(OnEquipButton);

            var buttonEvolutionStatProb = gameObject.FindComponent<Button>("Button_PetEvolutionStatInfo");
            buttonEvolutionStatProb.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_PetStatProbInfoUI>();

                popup.Init();
            });

            var evolutionStatObj = gameObject.FindGameObject("EvolutionStat");

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

            mPetEvolutionStats = new List<PetEvolutionStatUI2>();

            for(int i = 0; i < Lance.GameData.PetCommonData.evolutionStatMaxSlot; ++i)
            {
                int slot = i;
                int index = slot + 1;

                var petEvolutionStatObj = evolutionStatObj.FindGameObject($"PetEvolutionStatUI_{index}");

                var petEvolutionStatUI = petEvolutionStatObj.GetOrAddComponent<PetEvolutionStatUI2>();

                petEvolutionStatUI.Init(mId, slot);

                mPetEvolutionStats.Add(petEvolutionStatUI);
            }

            var buttonChangeEvolutionStat = evolutionStatObj.FindComponent<Button>("Button_ChangeEvolutionStat");
            buttonChangeEvolutionStat.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_PetEvolutionStatUI>();

                popup.Init(mId, mSelectedPreset);

                popup.SetOnCloseAction(Refresh);
            });

            // 스킬 
            // 액티브 스킬
            var activeSkillObj = gameObject.FindGameObject("ActiveSkill");

            mActiveSkillUnlock = activeSkillObj.FindGameObject("Unlock");
            mActiveSkillLock = activeSkillObj.FindGameObject("Lock");

            var activeFirstUnlockSkillData = DataUtil.GetFirstPetSkillUnlockData(data.type, SkillType.Active);

            StringParam lockReasonParam = new StringParam("step", activeFirstUnlockSkillData?.step ?? 0);

            var textSkillLockReason = mActiveSkillLock.FindComponent<TextMeshProUGUI>("Text_LockReason");
            textSkillLockReason.text = StringTableUtil.Get("UIString_StepUnlock", lockReasonParam);

            mButtonAutoCast = mActiveSkillUnlock.FindComponent<Button>("Button_AutoCastSkill");
            mButtonAutoCast.SetButtonAction(OnAutoCastSkillButton);

            mImageAutoCastFrame = mButtonAutoCast.GetComponent<Image>();
            mImageAutoCast = mActiveSkillUnlock.FindComponent<Image>("Image_Auto");

            // 패시브 스킬
            var passiveSkillObj = gameObject.FindGameObject("PassiveSkill");

            mPassiveSkillUnlock = passiveSkillObj.FindGameObject("Unlock");
            mPassiveSkillLock = passiveSkillObj.FindGameObject("Lock");

            var passiveFirstUnlockSkillData = DataUtil.GetFirstPetSkillUnlockData(data.type, SkillType.Passive);

            StringParam lockReasonParam2 = new StringParam("step", passiveFirstUnlockSkillData?.step ?? 0);

            var textSkillLockReason2 = mPassiveSkillLock.FindComponent<TextMeshProUGUI>("Text_LockReason");
            textSkillLockReason2.text = StringTableUtil.Get("UIString_StepUnlock", lockReasonParam2);

            Refresh();

            RefreshAutoCastUI();
        }

        void InsertFeedLog(int feedCount)
        {
            Param param = new Param();
            param.Add("id", mId);
            param.Add("usedPetFood", feedCount);

            Lance.BackEnd.InsertLog("FeedPet", param, 7);

            //mLogStacker = 0;
        }

        //// 먹이주기
        //void OnFeedButton(bool isPress)
        //{
        //    mIsPress = isPress;
        //    if (!mIsPress)
        //    {
        //        mIntervalTime = 0;
        //        if (mLogStacker > 0)
        //        {
        //            Lance.Lobby.RefreshTab();
        //            Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);

        //            bool isEquipped = Lance.Account.Pet.IsEquipped(mId);
        //            if (isEquipped)
        //            {
        //                var popupUserInfo = Lance.PopupManager.GetPopup<Popup_UserInfoUI>();

        //                popupUserInfo?.RefreshEquippedPet();
        //            }

        //            if (mIsLevelUp)
        //            {
        //                Lance.GameManager.UpdatePlayerStat();

        //                mIsLevelUp = false;
        //            }
        //        }
        //    }
        //}

        //bool mIsLevelUp;

        void OnFeedMax()
        {
            // 최고 레벨인지 확인
            if (Lance.Account.Pet.IsMaxLevel(mId))
            {
                UIUtil.ShowSystemErrorMessage("IsMaxLevelPet");

                return;
            }

            int feedCount = Math.Min(Lance.Account.Currency.GetPetFood(), Lance.Account.Pet.GetMaxLevelRequirePetFood(mId));

            // 펫 먹이가 충분한지 확인
            if (feedCount == 0 || Lance.Account.Currency.IsEnoughPetFood(feedCount) == false)
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughPetFood");

                return;
            }

            string title = StringTableUtil.Get("Title_Confirm");

            StringParam param = new StringParam("petFood", feedCount);

            string desc = StringTableUtil.Get("Desc_ConfirmUsePetFood", param);

            UIUtil.ShowConfirmPopup(title, desc, () => OnFeed(feedCount), null);
        }

        void OnFeed(int feedCount)
        {
            var result = Lance.GameManager.FeedPet(mId, feedCount);

            if (result != -1)
            {
                SoundPlayer.PlayPetFeed();

                Lance.ParticleManager.AquireUI($"Pet{mType}_Feed", mPetItemObj.transform as RectTransform);

                Refresh();

                //mLogStacker += feedCount;

                int curLevel = Lance.Account.Pet.GetLevel(mId);

                if (result == 1)
                {
                    //mIsLevelUp = true;

                    Lance.GameManager.UpdatePlayerStat();
                }

                Lance.Lobby.RefreshTab();
                Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);

                bool isEquipped = Lance.Account.Pet.IsEquipped(mId);
                if (isEquipped)
                {
                    var popupUserInfo = Lance.PopupManager.GetPopup<Popup_UserInfoUI>();

                    popupUserInfo?.RefreshEquippedPet();
                }

                InsertFeedLog(feedCount);
            }
        }

        // 진화 
        void OnEvolutionButton()
        {
            if (Lance.GameManager.EvolutionPet(mId))
            {
                var petInfo = Lance.Account.Pet.GetPet(mId);

                var popup = Lance.PopupManager.CreatePopup<Popup_PetEvolutionUI>();
                popup.Init(mType, petInfo.GetStep());
                popup.SetOnCloseAction(() => Refresh());

                Lance.Lobby.RefreshTab();
                Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);

                Param param = new Param();
                param.Add("id", mId);

                Lance.BackEnd.InsertLog("EvolutionPet", param, 7);

                bool isEquipped = Lance.Account.Pet.IsEquipped(mId);
                if (isEquipped)
                {
                    var popupUserInfo = Lance.PopupManager.GetPopup<Popup_UserInfoUI>();

                    popupUserInfo?.RefreshEquippedPet();
                }
            }
        }

        // 동행
        void OnEquipButton()
        {
            if (Lance.GameManager.EquipPet(mId))
            {
                SoundPlayer.PlayPetEquip();

                Refresh();

                Lance.Lobby.RefreshTab();

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Pet);
            }
        }

        void OnChangePreset(int preset)
        {
            Lance.GameManager.ChangePetEvolutionStatPreset(mId, preset);

            mSelectedPreset = Lance.Account.Pet.GetCurrentPreset(mId);

            Refresh();
        }

        void OnAutoCastSkillButton()
        {
            SaveBitFlags.PetSkillAutoCastOff.Toggle();

            RefreshAutoCastUI();
        }

        void RefreshAutoCastUI()
        {
            if (SaveBitFlags.PetSkillAutoCastOff.IsOff())
            {
                var tm = mImageAutoCast.GetComponent<RectTransform>();

                mAutoCastTween = tm.DOLocalRotate(Vector3.forward * 360f, 1f, RotateMode.FastBeyond360)
                    .SetUpdate(isIndependentUpdate: true)
                    .SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
                mAutoCastTween.timeScale = 1f;

                mImageAutoCastFrame.sprite = Lance.Atlas.GetUISprite("Frame_Auto_Active");
                mImageAutoCast.sprite = Lance.Atlas.GetUISprite("Icon_Auto_Active");
            }
            else
            {
                mAutoCastTween.Rewind();
                mAutoCastTween.Pause();

                mImageAutoCastFrame.sprite = Lance.Atlas.GetUISprite("Frame_Auto_Inactive");
                mImageAutoCast.sprite = Lance.Atlas.GetUISprite("Icon_Auto_Inactive");
            }
        }

        void Refresh()
        {
            // 레벨 
            mTextLevel.text = $"Lv.{Lance.Account.Pet.GetLevel(mId)}";

            // 경험치
            int currentExp = Lance.Account.Pet.GetCurrentExp(mId);
            int requireExp = Lance.Account.Pet.GetRequirePetFood(mId);
            mTextExp.text = $"{currentExp} / {requireExp}";
            mSliderExp.value = (float)currentExp / (float)requireExp;

            // 등급
            int step = Lance.Account.Pet.GetStep(mId);
            if (step == 0)
            {
                mSubGradeManager.SetActive(false);
            }
            else
            {
                mSubGradeManager.SetActive(true);
                mSubGradeManager.SetSubGrade((SubGrade)step - 1);
            }

            // 이름
            SetTitleText(StringTableUtil.GetName($"Pet_{mType}_{step}"));

            // 이미지
            mPetUIMotion.RefreshSprites(mType, step);

            // 장착 효과
            mEquipStatValueInfo.Refresh();

            // 보유 효과
            foreach (var ownStatValueInfo in mOwnStatValueInfos)
            {
                ownStatValueInfo.Refresh();
            }

            // 신수 먹이 보유량
            mTextPetFood.text = Lance.Account.Currency.GetPetFood().ToString();

            // 속성석 보유량
            mTextElementalStone.text = Lance.Account.Currency.GetElementalStone().ToString();

            // 진화 특성 ( 스탯 )
            mPetEvolutionStats.ForEach(x => x.Refresh());

            // 진화 특성 프리셋
            foreach (var buttonChangePreset in mButtonChangePresetUIList)
            {
                buttonChangePreset.SetActiveSprite(buttonChangePreset.Preset == mSelectedPreset);
            }

            // 스킬 오픈 여부
            var activeSkill = Lance.Account.Pet.GetActiveSkill(mId);

            mActiveSkillUnlock.SetActive(activeSkill.IsValid());
            mActiveSkillLock.SetActive(!activeSkill.IsValid());

            // 액티브 스킬 설명
            var petSkill = DataUtil.GetPetSkill(mType, SkillType.Active, step);
            if (petSkill.IsValid())
            {
                var petSkillData = Lance.GameData.PetActiveSkillData.TryGet(petSkill);
                if (petSkillData != null)
                {
                    var imageSkill = mActiveSkillUnlock.FindComponent<Image>("Image_Skill");
                    imageSkill.sprite = Lance.Atlas.GetItemSlotUISprite($"{petSkillData.uiSprite}");

                    var textSkillValue = gameObject.FindComponent<TextMeshProUGUI>("Text_SkillValue");
                    textSkillValue.text = $"{StringTableUtil.GetName($"{StatType.SkillDmg}")} {petSkillData.skillValue * 100:F0}%";

                    StringParam coolTimeParam = new StringParam("coolTime", petSkillData.coolTime);

                    var textSkillCoolTime = gameObject.FindComponent<TextMeshProUGUI>("Text_SkillCoolTime");
                    textSkillCoolTime.text = StringTableUtil.Get("UIString_CoolTimeValue", coolTimeParam);

                    StringParam skillDescParam = new StringParam("value", $"{petSkillData.skillValue * 100:F2}");
                    skillDescParam.AddParam("targetCount", $"{petSkillData.targetCount}");
                    skillDescParam.AddParam("duration", $"{petSkillData.duration}");
                    skillDescParam.AddParam("atkDelay", $"{petSkillData.atkDelay}");

                    var textSkillDesc = mActiveSkillUnlock.FindComponent<TextMeshProUGUI>("Text_SkillDesc");
                    textSkillDesc.text = StringTableUtil.Get($"SkillDesc_{petSkillData.id}", skillDescParam);
                }
            }

            // 스킬 오픈 여부
            var passiveSkill = Lance.Account.Pet.GetPassiveSkill(mId);

            mPassiveSkillUnlock.SetActive(passiveSkill.IsValid());
            mPassiveSkillLock.SetActive(!passiveSkill.IsValid());

            // 패시브 스킬 설명
            var petPassiveSkill = DataUtil.GetPetSkill(mType, SkillType.Passive, step);
            if (petPassiveSkill.IsValid())
            {
                var petSkillData = Lance.GameData.PetPassiveSkillData.TryGet(petPassiveSkill);
                if (petSkillData != null)
                {
                    var imageSkill = mPassiveSkillUnlock.FindComponent<Image>("Image_Skill");
                    imageSkill.sprite = Lance.Atlas.GetItemSlotUISprite($"{petSkillData.uiSprite}");

                    StringParam skillDescParam = new StringParam("value", $"{petSkillData.skillValue * 100:F2}");

                    skillDescParam.AddParam("targetCount", $"{petSkillData.targetCount}");
                    skillDescParam.AddParam("duration", $"{petSkillData.duration}");
                    skillDescParam.AddParam("atkDelay", $"{petSkillData.atkDelay}");

                    var textSkillDesc = mPassiveSkillUnlock.FindComponent<TextMeshProUGUI>("Text_SkillDesc");
                    textSkillDesc.text = StringTableUtil.Get($"SkillDesc_{petSkillData.id}", skillDescParam);
                }
            }

            RefreshButtonUI();
        }

        void RefreshButtonUI()
        {
            // 먹이 주기 가능 여부
            bool isMaxLevel = Lance.Account.Pet.IsMaxLevel(mId);
            
            mFeedObj.SetActive(isMaxLevel == false);

            bool isEnoughPetFood1 = Lance.Account.Currency.IsEnoughPetFood(1);
            mButtonFeed_1.SetActiveFrame(isMaxLevel == false && isEnoughPetFood1);

            bool isEnoughPetFood100 = Lance.Account.Currency.IsEnoughPetFood(100);
            mButtonFeed_100.SetActiveFrame(isMaxLevel == false && isEnoughPetFood100);

            bool isEnoughPetFoodMax = Lance.Account.Currency.GetPetFood() > 0;
            mButtonFeed_Max.SetActiveFrame(isMaxLevel == false && isEnoughPetFoodMax);

            // 진화 가능 여부
            bool canEvolution = Lance.Account.CanEvolutionPet(mId);
            mEvolutionObj.SetActive(isMaxLevel);
            mButtonEvolution.SetActiveFrame(canEvolution);
            mEvolutionRedDot.SetActive(canEvolution);

            // 장착 여부
            bool isEquipped = Lance.Account.Pet.IsEquipped(mId);
            mIsEquippedObj.SetActive(isEquipped);
            mButtonEquip.SetActiveFrame(!isEquipped);

            if (isMaxLevel)
            {
                //mIsPress = false;

                bool isMaxStep = Lance.Account.Pet.IsMaxStep(mId);
                int requireElementalStone = Lance.Account.Pet.GetRequireElementalStone(mId);
                bool isEnoughElementalStone = Lance.Account.Currency.IsEnoughElementalStone(requireElementalStone);

                mTextRequireElementalStone.SetColor(UIUtil.GetEnoughTextColor(isEnoughElementalStone));
                mTextRequireElementalStone.text = isMaxStep ? "MAX" :$"{requireElementalStone}";
            }
        }
    }
}