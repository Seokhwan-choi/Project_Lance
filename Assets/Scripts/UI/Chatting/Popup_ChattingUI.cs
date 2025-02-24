using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mosframe;
using TMPro;
using UnityEngine.UI;
using BackEndChat;
using System;

namespace Lance
{
    class Popup_ChattingUI : PopupBase
    {
        GameObject mLoadingObj;
        DynamicVScrollView mChattingScrollView;
        TextMeshProUGUI mTextChannnelNumber;
        TextMeshProUGUI mTextUserCount;
        TMP_InputField mInputField;
        Button mButtonInput;
        LeaderboardInfo mRankInfo;
        GameObject mNoticeObj;

        public void Init()
        {
            SetTitleText(StringTableUtil.Get("Title_Chatting"));

            SetUpCloseAction();

            mNoticeObj = gameObject.FindGameObject("Notice");

            mChattingScrollView = gameObject.FindComponent<DynamicVScrollView>("ChattingScrollView");

            mInputField = gameObject.FindComponent<TMP_InputField>("InputField_Chat");
            mInputField.onValueChanged.RemoveAllListeners();
            mInputField.onValueChanged.AddListener(OnInputFieldChange);

            mButtonInput = gameObject.FindComponent<Button>("Button_Input");
            mButtonInput.SetButtonAction(SendChatMessage);

            mTextChannnelNumber = gameObject.FindComponent<TextMeshProUGUI>("Text_ChannelNumber");
            mTextUserCount = gameObject.FindComponent<TextMeshProUGUI>("Text_UserCount");

            mLoadingObj = gameObject.FindGameObject("Loading");
            mLoadingObj.SetActive(true);

            string rankName = RankingTab.PowerLevel.ChangeToTableName();

            mRankInfo = Lance.Account.Leaderboard.GetLeaderboardInfo(rankName);

            if (mRankInfo.MyUserLeaderboardItem == null)
            {
                mRankInfo.GetMyRank((success, rankItem) => { 
                    if (!success)
                    {
                        mRankInfo = Lance.Account.Leaderboard.GetLeaderboardInfo(RankingTab.Stage.ChangeToTableName());
                    }
                });
            }

            Lance.BackEnd.ChattingManager.OnChattingPopup(OnJoinChannel);

            //Lance.BackEnd.ChattingManager.SetNewMessage(false);
            //Lance.Lobby.RefreshChattingRedDot();
        }

        private void Update()
        {
            if (BackendManager2.IsCountedRank())
            {
                if (mNoticeObj.activeSelf == false)
                    mNoticeObj.SetActive(true);
            }
            else
            {
                if (mNoticeObj.activeSelf)
                    mNoticeObj.SetActive(false);
            }
        }

        void OnInputFieldChange(string s)
        {
            mButtonInput?.SetActiveFrame(s.IsValid() && 200 >= System.Text.Encoding.Default.GetByteCount(s));
        }

        void SendChatMessage()
        {
            // ���ڸ� �Է��ؾ� ���� �� �ִ�.
            if (mInputField.text.IsValid() == false)
            {
                UIUtil.ShowSystemErrorMessage("NeedMessage");

                return;
            }

            // �Է��� ���ڰ� 200����Ʈ ������ �ȵȴ�. ( �ѱ۷� �� 50���� )
            int bytecount = System.Text.Encoding.Default.GetByteCount(mInputField.text);
            if (bytecount > 200)
            {
                UIUtil.ShowSystemErrorMessage("TooLongMessage");

                return;
            }

            string sendMessage = $"{mInputField.text}\\rank{mRankInfo.MyUserLeaderboardItem?.rank}";

            Lance.BackEnd.SendChatMessage(sendMessage);

            if (Lance.Account.UserInfo.StackChatCount(1))
            {
                Lance.GameManager.CheckQuest(QuestType.SendChatMessage, 1);

                Lance.Lobby.UpdateAchievement();
            }

            mInputField.text = string.Empty;
        }

        public void Refresh()
        {
            mChattingScrollView.refresh();
        }

        public void OnJoinChannel(ChannelInfo channelInfo)
        {
            StartCoroutine(JoinChannel(channelInfo));
        }

        IEnumerator JoinChannel(ChannelInfo channelInfo)
        {
            yield return new WaitForSeconds(1f);

            // ä�� ��ȣ
            mTextChannnelNumber.text = $"{channelInfo.ChannelNumber}";

            // ���� ��
            mTextUserCount.text = $"{channelInfo.Players.Count + Lance.GameData.CommonData.chattingDummyPlayerCount} / {channelInfo.MaxCount}";

            // ä�ó��� �����ֱ�
            mChattingScrollView.totalItemCount = channelInfo.Messages.Count;

            //mChattingScrollView.scrollByItemIndex(channelInfo.Messages.Count - 1);

            if (mChattingScrollView.totalItemCount >= 5)
            {
                mChattingScrollView.ignoreIsBottom = false;
            }
            else
            {
                RefreshIgnoreIsBottom();
            }

            yield return null;
            mLoadingObj.SetActive(false);
        }

        public void OnJoinChannelPlayer(ChannelInfo channelInfo)
        {
            // ä�ù濡 ���� ����

            // ���� ��
            mTextUserCount.text = $"{channelInfo.Players.Count + Lance.GameData.CommonData.chattingDummyPlayerCount} / {channelInfo.MaxCount}";
        }

        public void OnLeaveChannelPlayer(ChannelInfo channelInfo)
        {
            // ä�ù濡 ���� ����

            // ���� ��
            mTextUserCount.text = $"{channelInfo.Players.Count + Lance.GameData.CommonData.chattingDummyPlayerCount} / {channelInfo.MaxCount}";
        }

        public void OnDeleteMessage(ChannelInfo channelInfo)
        {
            mChattingScrollView.totalItemCount = channelInfo.Messages.Count;
        }

        public void OnChangeTotalCount(int totalCount)
        {
            mChattingScrollView.totalItemCount = totalCount;
        }
        
        public void OnAddMessage(int totalCount)
        {
            OnChangeTotalCount(totalCount);

            RefreshIgnoreIsBottom();
        }

        void RefreshIgnoreIsBottom()
        {
            // ���� �� ���� ���� �ִ��� Ȯ���ϰ� �� ���� ���� �ִٸ� �ٷ� �Ʒ��� ��ũ�� ������
            if (mChattingScrollView.IsAllowIsBottomPos())
            {
                mChattingScrollView.ignoreIsBottom = false;
            }
            else
            {
                mChattingScrollView.ignoreIsBottom = true;
            }
        }
    }
}
