using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mosframe;

namespace Lance
{
    class Popup_EquipProbUI : PopupBase
    {
        DynamicVScrollView mScrollView;
        SpawnItemUI mParent;
        SpawnData mData;
        SpawnGradeProbData mProbData;
        
        TextMeshProUGUI mTextSpawnLevel;
        ItemSlotUI mSpawnRewardSlotUI;
        Button mButtonReceive;
        GameObject mRedDotObj;
        GameObject mLeftRedDotObj;
        GameObject mRightRedDotObj;
        Image mImageModal;
        Image mImageCheck;

        List<ProbInfo> mProbInfos;
        public void Init(SpawnItemUI parent, SpawnData spawnData)
        {
            mParent = parent;

            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_SpawnProb"));

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            mProbInfos = new List<ProbInfo>();

            mData = spawnData;
            StackedSpawnInfo info = Lance.Account.Spawn.GetInfo(mData.type);
            mProbData = DataUtil.GetSpawnGradeProbData(mData.probId, info.GetStackedSpawnCount());

            // 소환 레벨
            mTextSpawnLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_SpawnLevel");

            // 소환 보상
            var spawnRewardSlotObj = gameObject.FindGameObject("SpawnRewardSlot");
            var itemSlotObj = spawnRewardSlotObj.FindGameObject("ItemSlotUI");
            mSpawnRewardSlotUI = itemSlotObj.GetOrAddComponent<ItemSlotUI>();
            mImageModal = spawnRewardSlotObj.FindComponent<Image>("Image_Modal");
            mImageCheck = spawnRewardSlotObj.FindComponent<Image>("Image_Check");

            var buttonDownLevel = gameObject.FindComponent<Button>("Button_DownLevel");
            buttonDownLevel.SetButtonAction(() => ChangeLevel(isDown:true));

            var buttonUpLevel = gameObject.FindComponent<Button>("Button_UpLevel");
            buttonUpLevel.SetButtonAction(() => ChangeLevel(isDown:false));

            mButtonReceive = gameObject.FindComponent<Button>("Button_ReceiveReward");
            mButtonReceive.SetButtonAction(OnReceiveButton);

            // 레드 닷 
            mLeftRedDotObj = buttonDownLevel.gameObject.FindGameObject("RedDot");
            mRightRedDotObj = buttonUpLevel.gameObject.FindGameObject("RedDot");
            mRedDotObj = mButtonReceive.gameObject.FindGameObject("RedDot");

            Refresh();
        }

        void ChangeLevel(bool isDown)
        {
            int currentLevel = mProbData.level;
            int changeLevel = currentLevel + (isDown ?  - 1 : 1);
            changeLevel = Mathf.Clamp(changeLevel, 1, Lance.GameData.ShopCommonData.equipSpawnMaxLevel);

            mProbData = DataUtil.GetSpawnGradeProbDataByLevel(mData.probId, changeLevel);

            Refresh();
        }

        void Refresh()
        {
            if (mProbData != null)
            {
                // 소환 레벨
                StringParam param = new StringParam("level", mProbData.level);
                mTextSpawnLevel.text = StringTableUtil.Get("UIString_SpawnLevel", param);

                SpawnRewardData rewardData = DataUtil.GetSpawnRewardData(mData.type, mProbData.level);

                bool isReceivedReward = Lance.Account.IsReceivedSpawnReward(mData.type, mProbData.level);
                bool isRepeatReward = isReceivedReward && rewardData.repeatReward.IsValid();
                bool canReceiveReward = Lance.Account.CanReceiveSpawnReward(mData.type, mProbData.level);

                mProbInfos.Clear();

                for(int i = (int)Grade.SSR; i >= 0; --i)
                {
                    Grade grade = (Grade)i;
                    float gradeProb = mProbData.prob[(int)grade];
                    if (gradeProb > 0)
                    {
                        float[] subGradeProbs = DataUtil.GetEquipmentSubgradeProbs(grade, mProbData.level);

                        var probInfo = new ProbInfo();
                        probInfo.Init(mData.type, grade, gradeProb, subGradeProbs);

                        mProbInfos.Add(probInfo);
                    }
                }

                mScrollView.totalItemCount = mProbInfos.Count;
                mScrollView.refresh();
                mScrollView.scrollByItemIndex(0);

                // 소환 보상 아이템
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

                // 보상 받았는지 확인
                mImageModal.gameObject.SetActive(isRepeatReward ? false : isReceivedReward);

                // 레드 닷
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

        public ProbInfo GetProbInfo(int index)
        {
            if (mProbInfos.Count <= index)
                return null;

            return mProbInfos[index];
        }
    }

    class ProbInfo
    {
        ItemType mItemType;
        Grade mGrade;
        float mGradeProb;
        float[] mSubGradeProb;

        public void Init(ItemType itemType, Grade grade, float gradeProb, float[] subGradeProb)
        {
            mItemType = itemType;
            mGrade = grade;
            mGradeProb = gradeProb;
            mSubGradeProb = subGradeProb;
        }

        public ItemType GetItemType()
        {
            return mItemType;
        }

        public Grade GetGrade()
        {
            return mGrade;
        }

        public float GetGradeProb()
        {
            return mGradeProb;
        }

        public float GetSubGradeProb(SubGrade subGrade)
        {
            return mSubGradeProb[(int)subGrade];
        }
    }
}