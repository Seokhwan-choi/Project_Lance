using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class Lobby_DungeonModeUI : MonoBehaviour
    {
        bool mIsActive;
        TextMeshProUGUI mTextDungeonName;
        TextMeshProUGUI mTextDungeonDesc;
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

            mTextDungeonName = gameObject.FindComponent<TextMeshProUGUI>("Text_DungeonName");
            mTextDungeonDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_DungeonDesc");

            mImageAutoChallengeCheck = gameObject.FindComponent<Image>("Image_Check");

            gameObject.SetActive(false);
        }

        public void OnUpdate(Player player)
        {
            if (mIsActive == false)
                return;

            mSkillSlotUIManager.OnUpdate(player);
        }

        public void Refresh()
        {
            if (mIsActive == false)
                return;

            mSkillSlotUIManager.Refresh();
        }

        public void Localize()
        {
            if (mStageData != null)
            {
                string title = StringTableUtil.Get($"Title_{mStageData.type}Dungeon");
                StringParam param = new StringParam("step", mStageData.stage);
                string step = StringTableUtil.Get("UIString_Step", param);

                mTextDungeonName.text = mStageData.type.HaveNextStepDungeon() || (mStageData.type == StageType.Ancient || mStageData.type == StageType.Pet) ? $"{title} {step}" : $"{title}";
                mTextDungeonDesc.text = StringTableUtil.Get($"Desc_{mStageData.type}Dungeon");

                var textAutoChallenge = gameObject.FindComponent<TextMeshProUGUI>("Text_AutoChallenge");
                textAutoChallenge.text = mStageData.type.HaveNextStepDungeon() ? StringTableUtil.Get("UIString_AutoChallengeNextStep") : StringTableUtil.Get("UIString_AutoChallenge");
            }
        }

        StageData mStageData;
        public void OnStartStage(StageData stageData)
        {
            mStageData = stageData;

            if (stageData.type.IsDungeon())
            {
                mIsActive = true;

                Refresh();

                gameObject.SetActive(mIsActive);

                string title = StringTableUtil.Get($"Title_{stageData.type}Dungeon");
                StringParam param = new StringParam("step", stageData.stage);
                string step = StringTableUtil.Get("UIString_Step", param);

                mTextDungeonName.text = stageData.type.HaveNextStepDungeon() || (mStageData.type == StageType.Ancient || mStageData.type == StageType.Pet) ? $"{title} {step}" : $"{title}";
                mTextDungeonDesc.text = StringTableUtil.Get($"Desc_{stageData.type}Dungeon");

                mImageAutoChallengeCheck = gameObject.FindComponent<Image>("Image_Check");
                mImageAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.DungeonAutoChallenge.IsOn());

                mButtonAutoChallenge.gameObject.SetActive(stageData.type != StageType.Raid);

                var textAutoChallenge = gameObject.FindComponent<TextMeshProUGUI>("Text_AutoChallenge");
                textAutoChallenge.text = stageData.type.HaveNextStepDungeon() ? StringTableUtil.Get("UIString_AutoChallengeNextStep") : StringTableUtil.Get("UIString_AutoChallenge");
            }
            else
            {
                mIsActive = false;

                gameObject.SetActive(mIsActive);
            }
        }

        public void SetDungeonName(string dungeonName)
        {
            mTextDungeonName.text = dungeonName;
        }

        void OnButtonGiveup()
        {
            Lance.GameManager.StageManager.OnGiveupButton();
        }

        void OnAutoChallengeButton()
        {
            SaveBitFlags.DungeonAutoChallenge.Toggle();

            mImageAutoChallengeCheck.gameObject.SetActive(SaveBitFlags.DungeonAutoChallenge.IsOn());
        }
    }
}