using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace Lance
{
    class Lobby_MenuUI : MonoBehaviour
    {
        bool mInMotion;
        bool mIsActiveOpenMenu;
        GameObject mMenuOpenObj;
        Image mImageMenuArrow;
        GameObject mMenuRedDot;

        // 왼쪽 => 저장, 버프, 채팅, 랭킹
        GameObject mBuffRedDot;
        GameObject mBuffObj;

        // 오른쪽 => 퀘스트, 스테이지, 현상금, 도감, 정수
        GameObject mRankingObj;
        GameObject mJoustingLockObj;
        GameObject mJoustingUnlockObj;
        GameObject mJoustingRedDotObj;

        GameObject mQuestRedDot;
        GameObject mQuestObj;

        GameObject mBountyQuestRedDot;
        GameObject mBountyQuestObj;

        GameObject mStageObj;

        GameObject mCollectionObj;
        GameObject mCollectionRedDot;

        GameObject mEssenceLockObj;
        GameObject mEssenceUnlockObj;
        GameObject mEssenceRedDot;

        public bool IsActiveOpenMenu => mIsActiveOpenMenu;
        public void Init()
        {
            GameObject menuObj = gameObject.FindGameObject("Menus");
            mMenuOpenObj = menuObj.FindGameObject("Menu_Open");
            mMenuRedDot = menuObj.FindGameObject("RedDot");

            var buttonMenu = menuObj.FindComponent<Button>("Button_Menu");
            buttonMenu.SetButtonAction(OnButtonOpenMenu);

            mImageMenuArrow = menuObj.FindComponent<Image>("Image_Arrow");

            mIsActiveOpenMenu = true;

            InitLeft();
            InitRight();
        }

        void InitLeft()
        {
            mBuffObj = gameObject.FindGameObject("Buff");

            var buttonBuff = mBuffObj.FindComponent<Button>("Button_Buff");
            buttonBuff.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_BuffUI>();
                popup.Init();
                popup.SetOnCloseAction(() => Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.ActiveBuff));
            });

            mBuffRedDot = mBuffObj.FindGameObject("RedDot");

            ////mChattingObj = gameObject.FindGameObject("Chatting");
            ////mChattingRedDot = mChattingObj.FindGameObject("RedDot");
            ////mImageChattingModal = mChattingObj.FindComponent<Image>("Image_ChattingModal");

            //var buttonChatting = mChattingObj.FindComponent<Button>("Button_Chatting");
            //buttonChatting.SetButtonAction(() =>
            //{
                
            //    mImageChattingModal.fillAmount = 1f;
            //});

            mRankingObj = gameObject.FindGameObject("Ranking");

            var buttonRanking = mRankingObj.FindComponent<Button>("Button_Ranking");
            buttonRanking.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_RankingUI>();
                popup.Init();
                popup.SetOnCloseAction(() => Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.ConfirmRaidRank));
            });

            var joustingObj = gameObject.FindGameObject("Jousting");
            var buttonJousting = joustingObj.FindComponent<Button>("Button_Jousting");
            buttonJousting.SetButtonAction(() =>
            {
                if (ContentsLockUtil.IsLockContents(ContentsLockType.Jousting))
                {
                    ContentsLockUtil.ShowContentsLockMessage(ContentsLockType.Jousting);

                    return;
                }

                var popup = Lance.PopupManager.CreatePopup<Popup_JoustingUI>();

                popup.Init();
            });

            mJoustingLockObj = joustingObj.FindGameObject("Lock");
            mJoustingUnlockObj = joustingObj.FindGameObject("Unlock");
            mJoustingRedDotObj = joustingObj.FindGameObject("RedDot");
        }

        void InitRight()
        {
            mQuestObj = gameObject.FindGameObject("Quest");

            var buttonQuest = mQuestObj.FindComponent<Button>("Button_Quest");
            buttonQuest.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_QuestUI>();
                popup.Init();
            });

            mQuestRedDot = buttonQuest.gameObject.FindGameObject("RedDot");

            mBountyQuestObj = gameObject.FindGameObject("Bounty");

            var buttonBountyQuest = mBountyQuestObj.FindComponent<Button>("Button_Bounty");
            buttonBountyQuest.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_BountyUI>();
                popup.Init();
                popup.SetOnCloseAction(() => Lance.GameManager.CheckGuideQuestReceiveReward(QuestType.ClearBountyQuest));
            });

            mBountyQuestRedDot = buttonBountyQuest.gameObject.FindGameObject("RedDot");

            mStageObj = gameObject.FindGameObject("Stage");

            var buttonStageSelect = mStageObj.FindComponent<Button>("Button_Stage");
            buttonStageSelect.SetButtonAction(() =>
            {
                // 스테이지 선택 팝업 보여주자
                var popup = Lance.PopupManager.CreatePopup<Popup_StageSelectUI>();

                popup.Init();
            });

            mCollectionObj = gameObject.FindGameObject("Collection");

            var buttonCollection = mCollectionObj.FindComponent<Button>("Button_Collection");
            buttonCollection.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_CollectionUI>();

                popup.Init();
            });

            mCollectionRedDot = buttonCollection.gameObject.FindGameObject("RedDot");

            var essenceObj = gameObject.FindGameObject("Essence");
            var buttonEssence = essenceObj.FindComponent<Button>("Button_Essence");
            buttonEssence.SetButtonAction(() =>
            {
                if (ContentsLockUtil.IsLockContents(ContentsLockType.Essence))
                {
                    ContentsLockUtil.ShowContentsLockMessage(ContentsLockType.Essence);

                    return;
                }

                var popup = Lance.PopupManager.CreatePopup<Popup_EssenceUI>();

                popup.Init();
                popup.SetOnCloseAction(() => Lance.GameManager.CheckQuest(QuestType.ConfirmEssence, 1));
            });

            mEssenceLockObj = essenceObj.FindGameObject("Lock");
            mEssenceUnlockObj = essenceObj.FindGameObject("Unlock");
            mEssenceRedDot = essenceObj.FindGameObject("RedDot");
        }

        public void OnButtonOpenMenu()
        {
            if (mInMotion)
                return;

            mIsActiveOpenMenu = !mIsActiveOpenMenu;

            RefreshOpenMenu();
        }

        void RefreshOpenMenu()
        {
            if (mIsActiveOpenMenu)
            {
                mMenuOpenObj.SetActive(true);

                mInMotion = true;

                mImageMenuArrow.transform.DORotate(Vector3.right * 180f, 0.4f);

                UIUtil.SpeechBubbleShow(mMenuOpenObj.transform, () => mInMotion = false);
            }
            else
            {
                mInMotion = true;

                mImageMenuArrow.transform.DORotate(Vector3.zero, 0.4f);

                UIUtil.SpeechBubbleHide(mMenuOpenObj.transform, () =>
                {
                    mMenuOpenObj.SetActive(false);

                    mInMotion = false;
                });
            }
        }

        public void OnUpdate(float dt)
        {
            

            
        }

        public void RefreshContentsLockUI()
        {
            mBuffObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Buff) == false);
            mQuestObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Quest) == false);
            mRankingObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false);
            //mChattingObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false);
            mBountyQuestObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.BountyQuest) == false);
            mCollectionObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.BountyQuest) == false);
            mStageObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.BountyQuest) == false);

            bool isEssenceContentsLock = ContentsLockUtil.IsLockContents(ContentsLockType.Essence);
            
            mEssenceLockObj.SetActive(isEssenceContentsLock);
            mEssenceUnlockObj.SetActive(!isEssenceContentsLock);

            bool isJoustingContentsLock = ContentsLockUtil.IsLockContents(ContentsLockType.Jousting);

            mJoustingLockObj.SetActive(isJoustingContentsLock);
            mJoustingUnlockObj.SetActive(!isJoustingContentsLock);
        }

        //bool mIsNewMessage;
        
        bool mAnyCanActiveBuff;
        bool mAnyCanReceiveQuestReward;
        bool mAnyCanReceiveBountyQuestReward;
        bool mAnyCanCompleteCollection;
        bool mAnyCanUpgradeEssence;
        bool mCanUpgradeGloryOrb;

        //bool mAnyCanReceivePassReward;
        //bool mAnyCanReceiveEventReward;
        public void RefreshRedDots()
        {
            //mIsNewMessage = ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false && Lance.BackEnd.ChattingManager.IsNewMessage;
            
            mAnyCanActiveBuff = ContentsLockUtil.IsLockContents(ContentsLockType.Buff) == false && (Lance.Account.Buff.AnyCanActive() || Lance.Account.Buff.AnyCanLevelUp());
            mAnyCanReceiveQuestReward = ContentsLockUtil.IsLockContents(ContentsLockType.Quest) == false && Lance.Account.AnyQuestCanReceiveReward();
            mAnyCanReceiveBountyQuestReward = ContentsLockUtil.IsLockContents(ContentsLockType.BountyQuest) == false && Lance.Account.Bounty.AnyCanReceiveReward();
            mAnyCanCompleteCollection = ContentsLockUtil.IsLockContents(ContentsLockType.BountyQuest) == false && Lance.Account.AnyCompleteCollect();
            mAnyCanUpgradeEssence = ContentsLockUtil.IsLockContents(ContentsLockType.Essence) == false && Lance.Account.AnyCanUpgradeEssence();
            mCanUpgradeGloryOrb = ContentsLockUtil.IsLockContents(ContentsLockType.Jousting) == false && Lance.Account.CanUpgradeJoustGloryOrb();

            //mChattingRedDot.SetActive(mIsNewMessage);
            if (mBuffRedDot.activeSelf != mAnyCanActiveBuff)
                mBuffRedDot.SetActive(mAnyCanActiveBuff);
            if (mQuestRedDot.activeSelf != mAnyCanReceiveQuestReward)
                mQuestRedDot.SetActive(mAnyCanReceiveQuestReward);
            if (mBountyQuestRedDot.activeSelf != mAnyCanReceiveBountyQuestReward)
                mBountyQuestRedDot.SetActive(mAnyCanReceiveBountyQuestReward);
            if (mCollectionRedDot.activeSelf != mAnyCanCompleteCollection)
                mCollectionRedDot.SetActive(mAnyCanCompleteCollection);
            if (mEssenceRedDot.activeSelf != mAnyCanUpgradeEssence)
                mEssenceRedDot.SetActive(mAnyCanUpgradeEssence);
            if (mJoustingRedDotObj.activeSelf != mCanUpgradeGloryOrb)
                mJoustingRedDotObj.SetActive(mCanUpgradeGloryOrb);

            bool anyRedDot = mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanCompleteCollection || mAnyCanUpgradeEssence || mCanUpgradeGloryOrb;

            if (mMenuRedDot.activeSelf != anyRedDot)
                mMenuRedDot.SetActive(anyRedDot);
        }

        //public void RefreshChattingRedDot()
        //{
        //    mIsNewMessage = ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false && Lance.BackEnd.ChattingManager.IsNewMessage;

        //    mChattingRedDot.SetActive(mIsNewMessage);

        //    mMenuRedDot.SetActive(mIsNewMessage || mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanCompleteCollection || mAnyCanUpgradeEssence);
        //}

        //public void RefreshPassRedDot()
        //{
        //    mAnyCanReceivePassReward = ContentsLockUtil.IsLockContents(ContentsLockType.Rank) == false && Lance.Account.Pass.AnyCanReceiveReward();

        //    mPassRedDot.SetActive(mAnyCanReceivePassReward);

        //    mMenuRedDot.SetActive(mIsNewMessage || mAnyCanReceivePassReward || mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanReceiveEventReward || mAnyCanUpgradeEssence);
        //}

        public void RefreshBuffRedDot()
        {
            mAnyCanActiveBuff = ContentsLockUtil.IsLockContents(ContentsLockType.Buff) == false && (Lance.Account.Buff.AnyCanActive() || Lance.Account.Buff.AnyCanLevelUp());

            mBuffRedDot.SetActive(mAnyCanActiveBuff);

            bool anyRedDot = mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanCompleteCollection || mAnyCanUpgradeEssence || mCanUpgradeGloryOrb;

            if (mMenuRedDot.activeSelf != anyRedDot)
                mMenuRedDot.SetActive(anyRedDot);
        }

        public void RefreshQuestRedDot()
        {
            mAnyCanReceiveQuestReward = ContentsLockUtil.IsLockContents(ContentsLockType.Quest) == false && Lance.Account.AnyQuestCanReceiveReward();

            if (mQuestRedDot.activeSelf != mAnyCanReceiveQuestReward)
                mQuestRedDot.SetActive(mAnyCanReceiveQuestReward);

            bool anyRedDot = mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanCompleteCollection || mAnyCanUpgradeEssence || mCanUpgradeGloryOrb;

            if (mMenuRedDot.activeSelf != anyRedDot)
                mMenuRedDot.SetActive(anyRedDot);
        }

        public void RefreshBountyQuestRedDot()
        {
            mAnyCanReceiveBountyQuestReward = ContentsLockUtil.IsLockContents(ContentsLockType.BountyQuest) == false && Lance.Account.Bounty.AnyCanReceiveReward();

            if (mBountyQuestRedDot.activeSelf != mAnyCanReceiveBountyQuestReward)
                mBountyQuestRedDot.SetActive(mAnyCanReceiveBountyQuestReward);

            bool anyRedDot = mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanCompleteCollection || mAnyCanUpgradeEssence || mCanUpgradeGloryOrb;

            if (mMenuRedDot.activeSelf != anyRedDot)
                mMenuRedDot.SetActive(anyRedDot);
        }

        public void RefreshCollectionRedDot()
        {
            mAnyCanCompleteCollection = ContentsLockUtil.IsLockContents(ContentsLockType.BountyQuest) == false && Lance.Account.AnyCompleteCollect();

            if (mCollectionRedDot.activeSelf != mAnyCanCompleteCollection)
                mCollectionRedDot.SetActive(mAnyCanCompleteCollection);

            bool anyRedDot = mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanCompleteCollection || mAnyCanUpgradeEssence || mCanUpgradeGloryOrb;

            if (mMenuRedDot.activeSelf != anyRedDot)
                mMenuRedDot.SetActive(anyRedDot);
        }

        public void RefreshEssenceRedDot()
        {
            mAnyCanUpgradeEssence = ContentsLockUtil.IsLockContents(ContentsLockType.Essence) == false && Lance.Account.AnyCanUpgradeEssence();

            if (mEssenceRedDot.activeSelf != mAnyCanUpgradeEssence)
                mEssenceRedDot.SetActive(mAnyCanUpgradeEssence);

            bool anyRedDot = mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanCompleteCollection || mAnyCanUpgradeEssence || mCanUpgradeGloryOrb;

            if (mMenuRedDot.activeSelf != anyRedDot)
                mMenuRedDot.SetActive(anyRedDot);
        }

        public void RefreshJoustingRedDot()
        {
            mCanUpgradeGloryOrb = ContentsLockUtil.IsLockContents(ContentsLockType.Jousting) == false && Lance.Account.CanUpgradeJoustGloryOrb();

            if (mJoustingRedDotObj.activeSelf != mCanUpgradeGloryOrb)
                mJoustingRedDotObj.SetActive(mCanUpgradeGloryOrb);

            bool anyRedDot = mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanCompleteCollection || mAnyCanUpgradeEssence || mCanUpgradeGloryOrb;

            if (mMenuRedDot.activeSelf != anyRedDot)
                mMenuRedDot.SetActive(anyRedDot);
        }

        public void RefreshEventRedDot()
        {
            //mAnyCanReceiveEventReward = Lance.Account.AnyCanReceiveNewEventReward();

            //mEventRedDot.SetActive(mAnyCanReceiveEventReward);

            //mMenuRedDot.SetActive(mIsNewMessage || mAnyCanReceivePassReward || mAnyCanActiveBuff || mAnyCanReceiveQuestReward || mAnyCanReceiveBountyQuestReward || mAnyCanReceiveEventReward || mAnyCanUpgradeEssence);
        }

        public void OnStartStage(StageData stageData)
        {
            SetActive(stageData.type.IsNormal());
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}