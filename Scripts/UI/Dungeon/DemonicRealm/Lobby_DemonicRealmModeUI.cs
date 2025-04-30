using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class Lobby_DemonicRealmModeUI : MonoBehaviour
    {
        bool mIsActive;
        TextMeshProUGUI mTextDemonicRealmName;
        TextMeshProUGUI mTextDemonicRealmDesc;
        Button mButtonAutoChallenge;
        Image mImageAutoChallengeCheck;
        SkillSlotUIManager mSkillSlotUIManager;

        public void Init()
        {
            var skillSlotListObj = gameObject.FindGameObject("SkillSlotUIList");

            mSkillSlotUIManager = new SkillSlotUIManager();
            mSkillSlotUIManager.Init(skillSlotListObj, "DungeonSkillSlotUI");

            var buttonExit = gameObject.FindComponent<Button>("Button_Giveup");
            buttonExit.SetButtonAction(OnButtonGiveup);

            mButtonAutoChallenge = gameObject.FindComponent<Button>("Button_AutoChallenge");
            mButtonAutoChallenge.SetButtonAction(OnAutoChallengeButton);

            mTextDemonicRealmName = gameObject.FindComponent<TextMeshProUGUI>("Text_DemonicRealmName");
            mTextDemonicRealmDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_DemonicRealmDesc");

            mImageAutoChallengeCheck = gameObject.FindComponent<Image>("Image_Check");

            gameObject.SetActive(false);
        }

        public void OnUpdate(Player player)
        {
            if (mIsActive == false)
                return;

            mSkillSlotUIManager.OnUpdate(player);
        }

        public void Localize()
        {
            if (mStageData != null)
            {
                string title = StringTableUtil.Get($"Title_{mStageData.type}DemonicRealm");
                StringParam param = new StringParam("step", mStageData.stage);
                string step = StringTableUtil.Get("UIString_Step", param);

                mTextDemonicRealmName.text = mStageData.type.HaveNextStepDemonicRealm() || mStageData.type == StageType.Ancient ? $"{title} {step}" : $"{title}";
                mTextDemonicRealmDesc.text = StringTableUtil.Get($"Desc_{mStageData.type}DemonicRealm");

                var textAutoChallenge = gameObject.FindComponent<TextMeshProUGUI>("Text_AutoChallenge");
                textAutoChallenge.text = mStageData.type.HaveNextStepDemonicRealm() ? StringTableUtil.Get("UIString_AutoChallengeNextStep") : StringTableUtil.Get("UIString_AutoChallenge");
            }
        }

        public void Refresh()
        {
            if (mIsActive == false)
                return;

            mSkillSlotUIManager.Refresh();
        }

        StageData mStageData;
        public void OnStartStage(StageData stageData)
        {
            mStageData = stageData;
            if (stageData.type.IsDemonicRealm())
            {
                mIsActive = true;

                Refresh();

                gameObject.SetActive(mIsActive);

                string title = StringTableUtil.Get($"Title_{stageData.type}DemonicRealm");
                StringParam param = new StringParam("step", stageData.stage);
                string step = StringTableUtil.Get("UIString_Step", param);

                mTextDemonicRealmName.text = stageData.type.HaveNextStepDemonicRealm() || stageData.type == StageType.Ancient ? $"{title} {step}" : $"{title}";
                mTextDemonicRealmDesc.text = StringTableUtil.Get($"Desc_{stageData.type}DemonicRealm");

                mImageAutoChallengeCheck = gameObject.FindComponent<Image>("Image_Check");
                mImageAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.DemonicRealmAutoChallenge.IsOn());

                mButtonAutoChallenge.gameObject.SetActive(stageData.type != StageType.Raid);

                var textAutoChallenge = gameObject.FindComponent<TextMeshProUGUI>("Text_AutoChallenge");
                textAutoChallenge.text = stageData.type.HaveNextStepDemonicRealm() ? StringTableUtil.Get("UIString_AutoChallengeNextStep") : StringTableUtil.Get("UIString_AutoChallenge");
            }
            else
            {
                mIsActive = false;

                gameObject.SetActive(mIsActive);
            }
        }

        public void SetDemonicRealmName(string DemonicRealmName)
        {
            mTextDemonicRealmName.text = DemonicRealmName;
        }

        void OnButtonGiveup()
        {
            Lance.GameManager.StageManager.OnGiveupButton();
        }

        void OnAutoChallengeButton()
        {
            SaveBitFlags.DemonicRealmAutoChallenge.Toggle();

            mImageAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.DemonicRealmAutoChallenge.IsOn());
        }
    }
}