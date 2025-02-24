using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class LimitBreakTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;

        Button mButtonEntrance;
        GameObject mRedDot;
        GameObject mEntranceObj;
        GameObject mMaxLevelObj;
        TextMeshProUGUI mTextRequireLevel;
        TextMeshProUGUI mTextLimitBreak;
        List<LimitBreakStatUI> mStatUIList;

        GameObject mLimitBreakSkillUI;
        TextMeshProUGUI mTextLimitBreakSkillUI;

        LimitBreakUIManager mLimitBreakUIManage;
        public void Init()
        {
            mCanvas = GetComponent<Canvas>();
            mGraphicRaycaster = GetComponent<GraphicRaycaster>();

            mButtonEntrance = gameObject.FindComponent<Button>("Button_Entrance");
            mButtonEntrance.SetButtonAction(OnEntranceButton);

            mRedDot = mButtonEntrance.gameObject.FindGameObject("RedDot");
            mEntranceObj = mButtonEntrance.gameObject.FindGameObject("Entrance");
            mMaxLevelObj = mButtonEntrance.gameObject.FindGameObject("MaxLevel");

            mTextRequireLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_RequireLevel");
            mTextLimitBreak = gameObject.FindComponent<TextMeshProUGUI>("Text_LimitBreak");

            InitStatUIs();

            var limitBreakObj = gameObject.FindGameObject("Image_LimitBreak");
            mLimitBreakUIManage = limitBreakObj.GetOrAddComponent<LimitBreakUIManager>();
            mLimitBreakUIManage.Init();

            mLimitBreakSkillUI = gameObject.FindGameObject("StatUI_LimitBreakSkill");
            mTextLimitBreakSkillUI = mLimitBreakSkillUI.FindComponent<TextMeshProUGUI>("Text_LimitBreakSkill");

            var buttonInfo = gameObject.FindComponent<Button>("Button_Info");
            buttonInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_DescUI>();

                popup.Init(StringTableUtil.Get("UIString_LimitBreak"), StringTableUtil.GetDesc("LimitBreak"));
            });
        }

        void InitStatUIs()
        {
            mStatUIList = new List<LimitBreakStatUI>();

            var stat = gameObject.FindGameObject("Stat");

            InitStatUI(StatType.AtkRatio);
            InitStatUI(StatType.HpRatio);
            InitStatUI(StatType.AtkSpeedRatio);
            InitStatUI(StatType.MoveSpeedRatio);

            void InitStatUI(StatType statType)
            {
                var statUIObj = stat.FindGameObject($"StatUI_{statType}");
                var statUI = statUIObj.GetOrAddComponent<LimitBreakStatUI>();
                statUI.Init(statType);

                mStatUIList.Add(statUI);
            }
        }

        public void Localize()
        {
            Refresh();
        }

        public void Refresh()
        {
            bool isMaxLimitBreak = Lance.Account.ExpLevel.IsMaxLimitBreak();

            if (isMaxLimitBreak)
            {
                mCanvas.enabled = false;
                mGraphicRaycaster.enabled = false;
            }
            else
            {
                mCanvas.enabled = true;
                mGraphicRaycaster.enabled = true;

                bool isEnoughLevel = Lance.Account.ExpLevel.IsEnoughLimitBreakLevel();

                int limitBreak = Lance.Account.ExpLevel.GetLimitBreak();

                StringParam param = new StringParam("step", limitBreak);

                mTextLimitBreak.text = StringTableUtil.Get("UIString_LimitBreakStep", param);

                var data = DataUtil.GetLimitBreakDataByStep(limitBreak + 1);
                if (data != null)
                {
                    StringParam param2 = new StringParam("level", data.requireLevel);

                    mTextRequireLevel.text = StringTableUtil.Get("UIString_RequireLevel", param2);
                }

                mEntranceObj.SetActive(data != null);
                mMaxLevelObj.SetActive(data == null);
                mRedDot.SetActive(isMaxLimitBreak == false && isEnoughLevel);
                mButtonEntrance.SetActiveFrame(isMaxLimitBreak == false && isEnoughLevel);
                mStatUIList.ForEach(x => x.Refresh());
                mLimitBreakUIManage.Refresh();

                SkillData limitBreakSkillData = null;

                foreach (var activeSkillData in Lance.GameData.ActiveSkillData.Values)
                {
                    if (activeSkillData.requireLimitBreak > 0 &&
                        activeSkillData.requireLimitBreak == limitBreak + 1)
                    {
                        limitBreakSkillData = activeSkillData;
                        break;
                    }
                }

                mLimitBreakSkillUI.SetActive(limitBreakSkillData != null);
                if (limitBreakSkillData != null)
                {
                    StringParam param3 = new StringParam("skillName", StringTableUtil.GetName(limitBreakSkillData.id));

                    mTextLimitBreakSkillUI.text = StringTableUtil.Get("UIString_GetSkill", param3);
                }
            }
        }

        void OnEntranceButton()
        {
            // 한계 돌파 던전 시작
            Lance.GameManager.StartLimitBreak();
        }
    }
}