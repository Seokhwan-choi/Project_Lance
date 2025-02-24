using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class Popup_SkillProbUI : PopupBase
    {
        SpawnItemUI mParent;
        SpawnData mData;
        SpawnGradeProbData mProbData;

        TextMeshProUGUI mTextSpawnLevel;
        List<GradeProbItemUI> mProbTableItemUIList;
        ItemSlotUI mSpawnRewardSlotUI;
        Button mButtonReceive;
        GameObject mRedDotObj;
        GameObject mLeftRedDotObj;
        GameObject mRightRedDotObj;
        Image mImageModal;
        Image mImageCheck;
        public void Init(SpawnItemUI parent, SpawnData spawnData)
        {
            mParent = parent;

            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_SpawnProb"));

            mData = spawnData;
            StackedSpawnInfo info = Lance.Account.Spawn.GetInfo(mData.type);
            mProbData = DataUtil.GetSpawnGradeProbData(mData.probId, info.GetStackedSpawnCount());

            // ��ȯ ����
            mTextSpawnLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnLevel");

            // ��� Ȯ��
            mProbTableItemUIList = new List<GradeProbItemUI>();
            for (int i = 0; i < (int)Grade.SR; ++i)
            {
                Grade grade = (Grade)i;

                var probTableItemUIObj = gameObject.FindGameObject($"GradeProbItemUI_{i + 1}");
                var probTableItemUI = probTableItemUIObj.GetOrAddComponent<GradeProbItemUI>();
                probTableItemUI.Init(grade);
                probTableItemUI.Refresh(mProbData.prob[(int)grade]);

                mProbTableItemUIList.Add(probTableItemUI);
            }

            // ��ų Ÿ�� Ȯ��
            var skillTypeProbTableObj = gameObject.FindGameObject("SkillTypeProbTable");
            for (int i = 0; i < (int)SkillType.Count; ++i)
            {
                int index = i;
                SkillType skillType = (SkillType)i;
                float prob = Lance.GameData.SkillTypeProbData.prob[index];

                var itemUIObj = skillTypeProbTableObj.FindGameObject($"SkillTypeProbItemUI_{i + 1}");
                var itemUI = itemUIObj.GetOrAddComponent<SkillTypeProbItemUI>();
                itemUI.Init(prob);
            }

            // ��ȯ ����
            var spawnRewardSlotObj = gameObject.FindGameObject("SpawnRewardSlot");
            var itemSlotObj = spawnRewardSlotObj.FindGameObject("ItemSlotUI");
            mSpawnRewardSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
            mImageModal = spawnRewardSlotObj.FindComponent<Image>("Image_Modal");
            mImageCheck = spawnRewardSlotObj.FindComponent<Image>("Image_Check");

            var buttonDownLevel = gameObject.FindComponent<Button>("Button_DownLevel");
            buttonDownLevel.SetButtonAction(() => ChangeLevel(isDown: true));

            var buttonUpLevel = gameObject.FindComponent<Button>("Button_UpLevel");
            buttonUpLevel.SetButtonAction(() => ChangeLevel(isDown: false));

            mButtonReceive = gameObject.FindComponent<Button>("Button_ReceiveReward");
            mButtonReceive.SetButtonAction(OnReceiveButton);

            // ���� �� 
            mLeftRedDotObj = buttonDownLevel.gameObject.FindGameObject("RedDot");
            mRightRedDotObj = buttonUpLevel.gameObject.FindGameObject("RedDot");
            mRedDotObj = mButtonReceive.gameObject.FindGameObject("RedDot");

            Refresh();
        }

        void ChangeLevel(bool isDown)
        {
            int currentLevel = mProbData.level;
            int changeLevel = currentLevel + (isDown ? -1 : 1);
            changeLevel = Mathf.Clamp(changeLevel, 1, 6);

            if (currentLevel != changeLevel)
            {
                mProbData = DataUtil.GetSpawnGradeProbDataByLevel(mData.probId, changeLevel);

                Refresh();
            }
        }

        void Refresh()
        {
            if (mProbData != null)
            {
                // ��ȯ ����
                StringParam param = new StringParam("level", mProbData.level);
                mTextSpawnLevel.text = StringTableUtil.Get("UIString_SpawnLevel", param);

                // ��ȯ Ȯ��
                foreach (GradeProbItemUI probItemUI in mProbTableItemUIList)
                {
                    probItemUI.Refresh(mProbData.prob[(int)probItemUI.Grade]);
                }

                SpawnRewardData rewardData = DataUtil.GetSpawnRewardData(mData.type, mProbData.level);

                bool isReceivedReward = Lance.Account.IsReceivedSpawnReward(mData.type, mProbData.level);
                bool isRepeatReward = isReceivedReward && rewardData.repeatReward.IsValid();
                bool canReceiveReward = Lance.Account.CanReceiveSpawnReward(mData.type, mProbData.level);

                // ��ȯ ���� ������
                string rewardId = isRepeatReward ? rewardData.repeatReward : rewardData.reward;
                if (rewardId.IsValid())
                {
                    RewardData reward = Lance.GameData.RewardData.TryGet(rewardId);

                    mSpawnRewardSlotUI.Init(new ItemInfo(reward));

                    mSpawnRewardSlotUI.SetActive(true);
                }
                else
                {
                    mSpawnRewardSlotUI.SetActive(false);
                }

                // ���� �޾Ҵ��� Ȯ��
                mImageModal.gameObject.SetActive(isRepeatReward ? false : isReceivedReward);

                // ���� ��
                mLeftRedDotObj.SetActive(Lance.Account.AnyRangeCanReceiveSpawnReward(mData.type, mProbData.level, isLeft: true));
                mRightRedDotObj.SetActive(Lance.Account.AnyRangeCanReceiveSpawnReward(mData.type, mProbData.level, isLeft: false));
                mRedDotObj.SetActive(canReceiveReward);

                mButtonReceive.SetActiveFrame(canReceiveReward);
            }
        }

        void OnReceiveButton()
        {
            SpawnRewardData rewardData = DataUtil.GetSpawnRewardData(mData.type, mProbData.level);
            if (rewardData == null || rewardData.reward.IsValid() == false)
            {
                UIUtil.ShowSystemErrorMessage("NoSpawnReward");

                return;
            }

            if (Lance.Account.CanReceiveSpawnReward(mData.type, mProbData.level) == false)
            {
                UIUtil.ShowSystemErrorMessage("CanNotReceiveSpwanReward");

                return;
            }

            Lance.GameManager.ReceiveSpawnReward(mData.type, mProbData.level);

            Refresh();

            mParent.Refresh();

            UIUtil.PlayStampMotion(mImageModal, mImageCheck);

            Lance.Lobby.RefreshTabRedDot(LobbyTab.Spawn);
        }
    }

    class SkillTypeProbItemUI : MonoBehaviour
    {
        public void Init(float prob)
        {
            var textProb = gameObject.FindComponent<TextMeshProUGUI>("Text_Prob");
            textProb.text = $"{prob * 100f:F2}%";
        }
    }
}