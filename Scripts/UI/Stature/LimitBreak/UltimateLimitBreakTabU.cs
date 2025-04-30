using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class UltimateLimitBreakTabUI : MonoBehaviour
    {
        Canvas mCanvas;
        GraphicRaycaster mGraphicRaycaster;
        Button mButtonEntrance;
        GameObject mRedDot;
        GameObject mEntranceObj;
        GameObject mMaxLevelObj;
        TextMeshProUGUI mTextRequireLevel;
        TextMeshProUGUI mTextLimitBreak;
        List<UltimateLimitBreakStatUI> mStatUIList;

        UltimateLimitBreakUIManager mLimitBreakUIManage;

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
            mTextLimitBreak = gameObject.FindComponent<TextMeshProUGUI>("Text_UltimateLimitBreak");

            InitStatUIs();

            var limitBreakObj = gameObject.FindGameObject("Image_UltimateLimitBreak");
            mLimitBreakUIManage = limitBreakObj.GetOrAddComponent<UltimateLimitBreakUIManager>();
            mLimitBreakUIManage.Init();

            var buttonInfo = gameObject.FindComponent<Button>("Button_Info");
            buttonInfo.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_DescUI>();

                popup.Init(StringTableUtil.Get("UIString_UltimateLimitBreak"), StringTableUtil.GetDesc("UltimateLimitBreak"));
            });
        }

        void InitStatUIs()
        {
            mStatUIList = new List<UltimateLimitBreakStatUI>();

            var stat = gameObject.FindGameObject("Stat");

            InitStatUI(StatType.AtkRatio);
            InitStatUI(StatType.HpRatio);
            InitStatUI(StatType.AtkSpeedRatio);
            InitStatUI(StatType.MoveSpeedRatio);

            void InitStatUI(StatType statType)
            {
                var statUIObj = stat.FindGameObject($"StatUI_{statType}");
                var statUI = statUIObj.GetOrAddComponent<UltimateLimitBreakStatUI>();
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
            bool isMaxUltimateLimitBreak = Lance.Account.ExpLevel.IsMaxUltimateLimitBreak();
            if (isMaxLimitBreak)
            {
                mCanvas.enabled = true;
                mGraphicRaycaster.enabled = true;

                int limitBreak = Lance.Account.ExpLevel.GetUltimateLimitBreak();

                StringParam param = new StringParam("step", limitBreak);

                mTextLimitBreak.text = StringTableUtil.Get("UIString_UltimateLimitBreakStep", param);

                bool isEnoughLevel = Lance.Account.ExpLevel.IsEnoughUltimateLimitBreakLevel();

                var data = DataUtil.GetUltimateLimitBreakDataByStep(limitBreak + 1);
                if (data != null)
                {
                    StringParam param2 = new StringParam("level", data.requireLevel);

                    mTextRequireLevel.text = StringTableUtil.Get("UIString_RequireLevel", param2);
                }

                mEntranceObj.SetActive(data != null);
                mMaxLevelObj.SetActive(data == null);
                mRedDot.SetActive(isMaxUltimateLimitBreak == false && isEnoughLevel);
                mButtonEntrance.SetActiveFrame(isMaxUltimateLimitBreak == false && isEnoughLevel);
                mStatUIList.ForEach(x => x.Refresh());
                mLimitBreakUIManage.Refresh();
            }
            else if (isMaxUltimateLimitBreak)
            {
                mCanvas.enabled = false;
                mGraphicRaycaster.enabled = false;
            }
            else
            {
                mCanvas.enabled = false;
                mGraphicRaycaster.enabled = false;
            }
        }

        void OnEntranceButton()
        {
            // 한계 돌파 던전 시작
            Lance.GameManager.StartUltimateLimitBreak();
        }
    }
}