using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;
using BackEnd;

namespace Lance
{
    public enum SkillInventoryTab
    {
        Active,
        Passive,

        Count,
    }

    class SkillInventoryTabUIManager
    {
        SkillInventoryTab mCurTab;
        TabNavBarUIManager<SkillInventoryTab> mNavBarUI;

        GameObject mGameObject;
        List<SkillInventoryTabUI> mSkillInventoryTabUIList;
        public void Init(GameObject go)
        {
            mGameObject = go.FindGameObject("TabUIList");

            mNavBarUI = new TabNavBarUIManager<SkillInventoryTab>();
            mNavBarUI.Init(go.FindGameObject("Skill_NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(SkillInventoryTab.Active);

            mSkillInventoryTabUIList = new List<SkillInventoryTabUI>();
            InitInventoryTab<SkillInventory_ActiveTabUI>(SkillInventoryTab.Active);
            InitInventoryTab<SkillInventory_PassiveTabUI>(SkillInventoryTab.Passive);

            ShowTab(SkillInventoryTab.Active);
        }

        public void OnSelectedSkillItemUI(string id)
        {
            var data = DataUtil.GetSkillData(id);
            if (data != null)
            {
                var tab = data.type.ChangeToSkillTab();

                ChangeTab(tab);

                RefreshActiveTab();

                var tabUI = mSkillInventoryTabUIList[(int)tab];

                tabUI.OnSelectedSkillItemUI(id);
            }
        }

        public SkillInventory_SkillItemUI GetBestSkillItemUI()
        {
            var curTab = mSkillInventoryTabUIList[(int)mCurTab];

            return curTab.GetBestSkillItemUI();
        }

        public SkillInventory_SkillItemUI GetCanEquipBestSkillItemUI()
        {
            var curTab = mSkillInventoryTabUIList[(int)mCurTab];

            return curTab.GetCanEquipBestSkillItemUI();
        }

        public SkillInventory_SkillItemUI GetCanUpgradeSkillItemUI()
        {
            var curTab = mSkillInventoryTabUIList[(int)mCurTab];

            return curTab.GetCanUpgradeSkillItemUI();
        }

        public void OnUpdate()
        {
            // 현재 탭만 
            foreach (SkillInventoryTabUI tab in mSkillInventoryTabUIList)
            {
                if (mCurTab == tab.Tab)
                {
                    tab.OnUpdate();
                }
            }
        }

        public void InitInventoryTab<T>(SkillInventoryTab tab) where T : SkillInventoryTabUI
        {
            GameObject tabObj = mGameObject.Find($"{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 SkillInventoryTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabUI.Init(tab, this);

            tabUI.SetVisibile(false);

            mSkillInventoryTabUIList.Add(tabUI);
        }

        public SkillInventoryTabUI GetTab(SkillInventoryTab tab)
        {
            return mSkillInventoryTabUIList[(int)tab];
        }

        public T GetTab<T>() where T : SkillInventoryTabUI
        {
            return mSkillInventoryTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void Localize()
        {
            mNavBarUI.Localize();

            foreach(var tab in mSkillInventoryTabUIList)
            {
                tab.Localize();
            }
        }

        public void Refresh()
        {
            var curTab = mSkillInventoryTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        int OnChangeTabButton(SkillInventoryTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out SkillInventoryTab result))
            {
                ChangeTab(result);
            }
        }

        public bool ChangeTab(SkillInventoryTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(SkillInventoryTab tab)
        {
            SkillInventoryTabUI showTab = mSkillInventoryTabUIList[(int)tab];

            showTab.OnEnter();

            showTab.SetVisibile(true);
        }

        void HideTab(SkillInventoryTab tab)
        {
            SkillInventoryTabUI hideTab = mSkillInventoryTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisibile(false);
        }

        public void RefreshActiveTab()
        {
            mNavBarUI.RefreshActiveFrame(mCurTab);
        }

        public void RefreshRedDots()
        {
            for (int i = 0; i < (int)SkillInventoryTab.Count; ++i)
            {
                SkillInventoryTab tab = (SkillInventoryTab)i;

                mNavBarUI.SetActiveRedDot(tab, RedDotUtil.IsAcitveRedDotBySkillTab(tab));
            }

            foreach (var tab in mSkillInventoryTabUIList)
            {
                tab.Refresh();
            }
        }
    }

    class SkillInventoryTabUI : MonoBehaviour
    {
        protected SkillInventoryTab mTab;
        protected SkillInventoryTabUIManager mParent;
        protected List<SkillInventory_SkillItemUI> mItemUIList;
        protected Canvas mCanvas;
        protected GraphicRaycaster mGraphicRaycaster;
        // 숙련도
        TextMeshProUGUI mTextSkillProficiencyLevel;
        Slider mSliderSkillProficiency;
        TextMeshProUGUI mTextSkillProficiencyAmount;
        public SkillInventoryTab Tab => mTab;
        public SkillInventoryTabUIManager Parent => mParent;
        public virtual void Init(SkillInventoryTab tab, SkillInventoryTabUIManager parent) 
        {
            mTab = tab;
            mParent = parent;

            var buttonAllUpgrade = gameObject.FindComponent<Button>("AllUpgrade");
            buttonAllUpgrade.SetButtonAction(OnAllUpgradeButton);

            var buttonDismantle = gameObject.FindComponent<Button>("AllDismantle");
            buttonDismantle.SetButtonAction(OnAllDismantleButton);

            var buttonSkillProficiency = gameObject.FindComponent<Button>("Button_SkillProficiency");
            buttonSkillProficiency.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_SkillProficiencyUI>();
                popup.Init();
            });

            var buttonResetSkills = gameObject.FindComponent<Button>("Button_ResetSkill");
            buttonResetSkills.SetButtonAction(() =>
            {
                SkillType skillType = mTab.ChangeToSkillType();

                if (Lance.Account.CanResetSkills(skillType) == false)
                {
                    UIUtil.ShowSystemErrorMessage("CanNotResetSkillList");

                    return;
                }

                string title = StringTableUtil.Get("Title_Confirm");

                int totalPiece = 0;

                foreach (var skill in Lance.Account.SkillInventory.GatherSkills(skillType))
                {
                    string id = skill.GetSkillId();
                    int level = skill.GetLevel();

                    totalPiece += DataUtil.CalcTotalUsedSkillPiece(id, level);
                }

                StringParam descParam = new StringParam("amount", totalPiece);

                string desc = StringTableUtil.GetDesc(skillType == SkillType.Active ? "ResetActiveSkills" : "ResetPassiveSkills", descParam);

                UIUtil.ShowConfirmPopup(title, desc, () =>
                {
                    Lance.GameManager.ResetSkills(skillType);

                    mParent.Refresh();

                    Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);

                    Lance.Lobby.RefreshCollectionRedDot();
                }, null);
            });

            mTextSkillProficiencyLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_SkillProficiency_Level");
            mTextSkillProficiencyAmount = gameObject.FindComponent<TextMeshProUGUI>("Text_SkillProficiency_Value");
            mSliderSkillProficiency = gameObject.FindComponent<Slider>("Slider_SkillProficiency");

            mCanvas = GetComponentInChildren<Canvas>();
            mGraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
        }
        public virtual void OnEnter() 
        {
            Refresh(); 
        }
        public virtual void OnLeave() { }
        public virtual void Localize() 
        {
            foreach (var item in mItemUIList)
            {
                item.Localize();
            }

            // 스킬 숙련도
            int proficiencyLevel = Lance.Account.SkillInventory.GetSkillProficiencyLevel();

            mTextSkillProficiencyLevel.text = StringTableUtil.Get("Name_SkillProficiencyLevel", new StringParam("level", $"{proficiencyLevel}"));
        }
        public virtual void Refresh()
        {
            foreach(var item in mItemUIList)
            {
                item.Refresh();
            }

            // 스킬 숙련도
            int levelUpCount = Lance.Account.SkillInventory.GetStackedSkillsLevelUpCount();
            int proficiencyLevel = Lance.Account.SkillInventory.GetSkillProficiencyLevel();
            SkillProficiencyData nextData = DataUtil.CalcSkillProficiencyNextStepData(levelUpCount);
            int remainCount = DataUtil.CalcSkillProficiencyNextStepRemainCount(levelUpCount);

            mTextSkillProficiencyLevel.text = StringTableUtil.Get("Name_SkillProficiencyLevel", new StringParam("level", $"{proficiencyLevel}"));
            mTextSkillProficiencyAmount.text = $"{remainCount} / {nextData.requireCount}";
            mSliderSkillProficiency.value = (float)remainCount / (float)nextData.requireCount;
        }
        public virtual void OnUpdate() { }

        protected void InitItemUIList(IEnumerable<SkillData> datas)
        {
            mItemUIList = new List<SkillInventory_SkillItemUI>();

            GameObject listParent = gameObject.FindGameObject("SkillItemList");

            listParent.AllChildObjectOff();

            string firstId = DataUtil.GetFirstSkillData(mTab.ChangeToSkillType()).id;

            foreach(var data in datas)
            {
                GameObject itemObj = Util.InstantiateUI("SkillItemUI", listParent.transform);

                SkillInventory_SkillItemUI itemUI = itemObj.GetOrAddComponent<SkillInventory_SkillItemUI>();

                itemUI.Init(data.id);
                itemUI.SetSelected(data.id == firstId);
                itemUI.SetButtonAction(OnSelectedSkillItemUI);

                mItemUIList.Add(itemUI);
            }
        }

        public SkillInventory_SkillItemUI GetBestSkillItemUI()
        {
            Grade bestGrade = Grade.D;
            SkillInventory_SkillItemUI bestItem = null;

            foreach (var itemUI in mItemUIList)
            {
                if (Lance.Account.HaveSkill(mTab.ChangeToSkillType(), itemUI.Id) == false)
                    continue;

                var data = DataUtil.GetSkillData(itemUI.Id);
                if (data == null)
                    continue;

                if (data.grade > bestGrade)
                {
                    bestGrade = data.grade;
                    bestItem = itemUI;
                }
            }

            return bestItem;
        }

        public SkillInventory_SkillItemUI GetCanEquipBestSkillItemUI()
        {
            Grade bestGrade = Grade.D;
            SkillInventory_SkillItemUI bestItem = null;

            SkillType skillType = mTab.ChangeToSkillType();

            foreach (var itemUI in mItemUIList)
            {
                if (Lance.Account.HaveSkill(skillType, itemUI.Id) == false)
                    continue;

                if (Lance.Account.IsEquippedSkill(skillType, itemUI.Id))
                    continue;

                var data = DataUtil.GetSkillData(itemUI.Id);
                if (data == null)
                    continue;

                if (data.grade > bestGrade)
                {
                    bestGrade = data.grade;
                    bestItem = itemUI;
                }
            }

            return bestItem;
        }

        public SkillInventory_SkillItemUI GetCanUpgradeSkillItemUI()
        {
            SkillInventory_SkillItemUI canUpgradeSkillItemUI = null;

            foreach (var itemUI in mItemUIList)
            {
                if (Lance.Account.CanUpgradeSkill(mTab.ChangeToSkillType(), itemUI.Id, 1))
                {
                    canUpgradeSkillItemUI = itemUI;
                }
            }

            if (canUpgradeSkillItemUI == null)
                return GetBestSkillItemUI();
            else
                return canUpgradeSkillItemUI;
        }

        public void RefreshItemSelelected(string id)
        {
            foreach (var itemUI in mItemUIList)
            {
                itemUI.SetSelected(itemUI.Id == id);
            }
        }

        public void OnSelectedSkillItemUI(string id)
        {
            var popup = Lance.PopupManager.CreatePopup<Popup_SkillInfoUI>();
            popup.Init(this, id);
            popup.SetOnCloseAction(() =>
            {
                RefreshItemSelelected(string.Empty);

                var data = DataUtil.GetSkillData(id);
                if (data != null)
                {
                    if (data.type == SkillType.Active)
                    {
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.EquipActiveSkill);
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.UpgradeActiveSkill);
                    }
                    else
                    {
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.EquipPassiveSkill);
                        Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.UpgradePassiveSkill);
                    }
                }
            });

            RefreshItemSelelected(id);
        }

        void OnAllUpgradeButton()
        {
            SkillType skillType = mTab.ChangeToSkillType();

            int prevLevel = Lance.Account.SkillInventory.GetSkillProficiencyLevel();

            var result = Lance.Account.UpgradeAllSkills(skillType);
            if (result.Length > 0)
            {
                SoundPlayer.PlaySkillLevelUp();

                Lance.GameManager.CheckQuest(QuestType.UpgradeSkill, result.Length);

                if (skillType == SkillType.Active)
                    Lance.GameManager.CheckQuest(QuestType.UpgradeActiveSkill, result.Length);
                else
                    Lance.GameManager.CheckQuest(QuestType.UpgradePassiveSkill, result.Length);

                mParent.Refresh();

                Lance.Lobby.RefreshTabRedDot(LobbyTab.Skill);

                Lance.Lobby.RefreshCollectionRedDot();

                Param param = new Param();
                param.Add("result", result);

                Lance.BackEnd.InsertLog("UpgradeAllSkill", param, 7);

                for(int i = 0; i < result.Length; ++i)
                {
                    var itemUI = mItemUIList.Where(x => x.Id == result[i]).FirstOrDefault();
                    if (itemUI != null)
                    {
                        Lance.ParticleManager.AquireUI("SkillLevelUp", itemUI.RectTm, Vector3.up * 24f);
                    }
                }

                int newLevel = Lance.Account.SkillInventory.GetSkillProficiencyLevel();

                if (prevLevel != newLevel)
                {
                    Lance.GameManager.UpdatePlayerStat();
                }

                result = null;
            }
            else
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughSkillUpgradeRequireCount");
            }
        }

        void OnAllDismantleButton()
        {
            //if (Lance.Account.HaveLimitBreakSkill() == false)
            //{
            //    UIUtil.ShowSystemErrorMessage("HaveNotLimitBreakSkill");

            //    return;
            //}

            SkillType skillType = mTab.ChangeToSkillType();

            var result = Lance.Account.DismantleAllSkills(skillType);
            if (result.Length > 0)
            {
                SoundPlayer.PlaySkillLevelUp();

                mParent.Refresh();

                Param param = new Param();
                param.Add("result", result);

                Lance.BackEnd.InsertLog("DismantleAllSkill", param, 7);

                int totalSkillPiece = 0;

                for (int i = 0; i < result.Length; ++i)
                {
                    string id = result[i].Item1;
                    int count = result[i].Item2;

                    var itemUI = mItemUIList.Where(x => x.Id == id).FirstOrDefault();
                    if (itemUI != null)
                    {
                        // To Do : 스킬 분해 FX ?
                        Lance.ParticleManager.AquireUI("SkillLevelUp", itemUI.RectTm, Vector3.up * 24f);
                    }

                    totalSkillPiece += DataUtil.GetSkillPieceAmount(id, count);
                }

                RewardResult rewardResult = new RewardResult();
                rewardResult.skillPiece += totalSkillPiece;

                Lance.GameManager.GiveReward(rewardResult, showType:ShowRewardType.Popup);
            }
            else
            {
                UIUtil.ShowSystemErrorMessage("IsNotEnoughSkillDismantleRequireCount");
            }
        }

        public void SetVisibile(bool visible)
        {
            mCanvas.enabled = visible;
            mGraphicRaycaster.enabled = visible;
        }
    }

    static class SkillInventoryTabExt
    {
        public static SkillType ChangeToSkillType(this SkillInventoryTab tab)
        {
            if (tab == SkillInventoryTab.Active)
                return SkillType.Active;
            else
                return SkillType.Passive;
        }

        public static SkillInventoryTab ChangeToSkillTab(this SkillType skillType)
        {
            if (skillType == SkillType.Active)
                return SkillInventoryTab.Active;
            else
                return SkillInventoryTab.Passive;
        }
    }
}
