using BackEndChat;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mosframe;
using DG.Tweening;

namespace Lance
{
    class ChattingManager : IChatClientListener
    {
        const string GroupName = "korea";
        const string LobbyName = "lobby";

        ChatClient mChatClient;
        string mCurrentChannelGroup;
        string mCurrentChannelName;
        ulong mCurrentChannelNumber;
        Dictionary<string, Dictionary<string, Dictionary<ulong, ChannelInfo>>> mChannelList = new();
        public void Init()
        {
            mChatClient = new ChatClient(this, GetAvatar());

            mChatClient.SendJoinOpenChannel(GroupName, LobbyName);
        }

        public string GetAvatar()
        {
            string bodyCostume = Lance.Account.Costume.GetEquippedCostumeId(CostumeType.Body);

            string achievement = Lance.Account.Achievement.GetEquippedAchievement();
            if (achievement.IsValid())
            {
                AchievementData data = Lance.GameData.AchievementData.TryGet(achievement);

                achievement = data.key;
            }
            
            string countryCode = Lance.Account.CountryCode;

            return $"{bodyCostume}\\s{achievement}\\s{countryCode}";
        }

        public void SendLeaveChannel()
        {
            mInLeaveChattingChannel = true;

            mChatClient?.SendLeaveChannel(mCurrentChannelGroup, mCurrentChannelName, mCurrentChannelNumber);
        }

        public void SendChatMessage(string sendMessage)
        {
            if (mChatClient == null) 
                return;

            if (mCurrentChannelName.IsValid() == false)
                return;

            if (!mChannelList.ContainsKey(mCurrentChannelGroup)) 
                return;

            if (!mChannelList[mCurrentChannelGroup].ContainsKey(mCurrentChannelName)) 
                return;

            if (!mChannelList[mCurrentChannelGroup][mCurrentChannelName].ContainsKey(mCurrentChannelNumber))
                return;

            ChannelInfo channelInfo = mChannelList[mCurrentChannelGroup][mCurrentChannelName][mCurrentChannelNumber];
            if (channelInfo == null) 
                return;

            mChatClient.SendChatMessage(channelInfo.ChannelGroup, channelInfo.ChannelName, channelInfo.ChannelNumber, sendMessage);
        }

        public void OnChatMessage(MessageInfo messageInfo)
        {
            AddMessage(messageInfo);
        }

        //public void OnChangeNickName()
        //{

        //}

        //public void OnChangeCostume()
        //{
        //    //mChatClient.SendLeaveChannel
        //}

        public void OnChattingPopup(Action<ChannelInfo> refreshUI)
        {
            var channelInfo = GetCurrenctChannelInfo();
            if (channelInfo != null)
            {
                refreshUI?.Invoke(channelInfo);
            }
            else
            {
                mChatClient.SendJoinOpenChannel(GroupName, LobbyName);
            }
        }

        public void OnWhisperMessage(WhisperMessageInfo messageInfo)
        {
            // 귓속말은 구현안함, 인터페이스 구현 비워둠
        }

        public ChannelInfo GetCurrenctChannelInfo()
        {
            if (mCurrentChannelGroup.IsValid() && mCurrentChannelName.IsValid())
            {
                if (mChannelList.ContainsKey(mCurrentChannelGroup) && mChannelList[mCurrentChannelGroup].ContainsKey(mCurrentChannelName))
                {
                    ChannelInfo channelInfo = mChannelList[mCurrentChannelGroup][mCurrentChannelName][mCurrentChannelNumber];

                    return channelInfo;
                }
            }

            return null;
        }

        public MessageInfo GetCurrentChannelLastMessageInfo()
        {
            var messageInfos = GetCurrentChannelMessageInfos();

            return messageInfos?.LastOrDefault();
        }

        public List<MessageInfo> GetCurrentChannelMessageInfos()
        {
            if (mCurrentChannelGroup.IsValid() && mCurrentChannelName.IsValid())
            {
                ChannelInfo channelInfo = mChannelList[mCurrentChannelGroup][mCurrentChannelName][mCurrentChannelNumber];

                return channelInfo?.Messages;
            }
            else
            {
                return null;
            }
        }

        // 채널 입장
        public void OnJoinChannel(ChannelInfo channelInfo)
        {
            if (mChannelList.ContainsKey(channelInfo.ChannelGroup))
            {
                if (mChannelList[channelInfo.ChannelGroup].ContainsKey(channelInfo.ChannelName))
                {
                    if (mChannelList[channelInfo.ChannelGroup][channelInfo.ChannelName].ContainsKey(channelInfo.ChannelNumber)) 
                        return;
                }
            }

            if (!mChannelList.ContainsKey(channelInfo.ChannelGroup))
            {
                mChannelList.Add(channelInfo.ChannelGroup, new Dictionary<string, Dictionary<ulong, ChannelInfo>>());
                mChannelList[channelInfo.ChannelGroup].Add(channelInfo.ChannelName, new Dictionary<ulong, ChannelInfo>());
            }
            else
            {
                if (!mChannelList[channelInfo.ChannelGroup].ContainsKey(channelInfo.ChannelName))
                {
                    mChannelList[channelInfo.ChannelGroup].Add(channelInfo.ChannelName, new Dictionary<ulong, ChannelInfo>());
                }
            }

            mChannelList[channelInfo.ChannelGroup][channelInfo.ChannelName].Add(channelInfo.ChannelNumber, channelInfo);

            if (mChannelList.Count == 1)
            {
                mCurrentChannelGroup = channelInfo.ChannelGroup;
                mCurrentChannelName = channelInfo.ChannelName;
                mCurrentChannelNumber = channelInfo.ChannelNumber;
            }

            var popup = Lance.PopupManager.GetPopup<Popup_ChattingUI>();
            popup?.OnJoinChannel(channelInfo);

            mInJoinChattingChannel = false;

            if (Lance.Lobby != null && Lance.Lobby.IsInit)
                Lance.Lobby.RefreshChattingUI(GetCurrentChannelLastMessageInfo());
        }

        public void OnLeaveChannel(ChannelInfo channelInfo)
        {
            if (!mChannelList.ContainsKey(channelInfo.ChannelGroup)) 
                return;

            if (!mChannelList[channelInfo.ChannelGroup].ContainsKey(channelInfo.ChannelName)) 
                return;

            if (!mChannelList[channelInfo.ChannelGroup][channelInfo.ChannelName].ContainsKey(channelInfo.ChannelNumber)) 
                return;

            mChannelList[channelInfo.ChannelGroup][channelInfo.ChannelName].Remove(channelInfo.ChannelNumber);

            if (mChannelList[channelInfo.ChannelGroup][channelInfo.ChannelName].Count == 0)
            {
                mChannelList[channelInfo.ChannelGroup].Remove(channelInfo.ChannelName);
            }

            if (mChannelList[channelInfo.ChannelGroup].Count == 0)
            {
                mChannelList.Remove(channelInfo.ChannelGroup);
            }

            mInLeaveChattingChannel = false;
        }

        // 유저가 채팅 채널에 입장함
        public void OnJoinChannelPlayer(string channelGroup, string channelName, UInt64 channelNumber, string gamerName, string avatar)
        {
            if (!mChannelList.ContainsKey(channelGroup)) 
                return;

            if (!mChannelList[channelGroup].ContainsKey(channelName)) 
                return;

            if (!mChannelList[channelGroup][channelName].ContainsKey(channelNumber)) 
                return;

            ChannelInfo channelInfo = mChannelList[channelGroup][channelName][channelNumber];
            if (channelInfo == null) 
                return;

            if (channelInfo.Players.ContainsKey(gamerName)) 
                return;

            PlayerInfo playerInfo = new PlayerInfo()
            {
                GamerName = gamerName,
                Avatar = avatar
            };

            channelInfo.Players.Add(gamerName, playerInfo);

            if (channelInfo.ChannelGroup == mCurrentChannelGroup &&
                channelInfo.ChannelName == mCurrentChannelName &&
                channelInfo.ChannelNumber == mCurrentChannelNumber)
            {
                var popup = Lance.PopupManager.GetPopup<Popup_ChattingUI>();
                popup?.OnJoinChannelPlayer(channelInfo);
            }
        }

        // 유저가 채팅 채널을 떠남
        public void OnLeaveChannelPlayer(string channelGroup, string channelName, UInt64 channelNumber, string gamerName, string avatar)
        {
            if (!mChannelList.ContainsKey(channelGroup)) 
                return;

            if (!mChannelList[channelGroup].ContainsKey(channelName)) 
                return;

            if (!mChannelList[channelGroup][channelName].ContainsKey(channelNumber)) 
                return;

            ChannelInfo channelInfo = mChannelList[channelGroup][channelName][channelNumber];
            if (channelInfo == null) return;

            if (!channelInfo.Players.ContainsKey(gamerName)) return;

            channelInfo.Players.Remove(gamerName);

            if (channelInfo.ChannelGroup == mCurrentChannelGroup &&
                channelInfo.ChannelName == mCurrentChannelName &&
                channelInfo.ChannelNumber == mCurrentChannelNumber)
            {
                var popup = Lance.PopupManager.GetPopup<Popup_ChattingUI>();
                popup?.OnLeaveChannelPlayer(channelInfo);
            }
        }

        public void OnHideMessage(MessageInfo messageInfo)
        {
            if (!mChannelList.ContainsKey(messageInfo.ChannelGroup)) 
                return;

            if (!mChannelList[messageInfo.ChannelGroup].ContainsKey(messageInfo.ChannelName)) 
                return;

            if (!mChannelList[messageInfo.ChannelGroup][messageInfo.ChannelName].ContainsKey(messageInfo.ChannelNumber)) 
                return;

            ChannelInfo channelInfo = mChannelList[messageInfo.ChannelGroup][messageInfo.ChannelName][messageInfo.ChannelNumber];
            if (channelInfo == null) 
                return;

            foreach (var message in channelInfo.Messages)
            {
                if (message.Index == messageInfo.Index && message.Tag == messageInfo.Tag)
                {
                    message.Message = messageInfo.Message;
                    break;
                }
            }

            if (mCurrentChannelGroup == messageInfo.ChannelGroup && 
                mCurrentChannelName == messageInfo.ChannelName && 
                mCurrentChannelNumber == messageInfo.ChannelNumber)
            {
                var popup = Lance.PopupManager.GetPopup<Popup_ChattingUI>();
                popup?.Refresh();
            }
        }

        public void OnDeleteMessage(MessageInfo messageInfo)
        {
            if (!mChannelList.ContainsKey(messageInfo.ChannelGroup)) 
                return;

            if (!mChannelList[messageInfo.ChannelGroup].ContainsKey(messageInfo.ChannelName)) 
                return;

            if (!mChannelList[messageInfo.ChannelGroup][messageInfo.ChannelName].ContainsKey(messageInfo.ChannelNumber)) 
                return;

            ChannelInfo channelInfo = mChannelList[messageInfo.ChannelGroup][messageInfo.ChannelName][messageInfo.ChannelNumber];
            if (channelInfo == null) 
                return;

            foreach (var message in channelInfo.Messages)
            {
                if (message.Index == messageInfo.Index && message.Tag == messageInfo.Tag)
                {
                    channelInfo.Messages.Remove(message);
                    break;
                }
            }

            if (mCurrentChannelGroup == messageInfo.ChannelGroup && 
                mCurrentChannelName == messageInfo.ChannelName && 
                mCurrentChannelNumber == messageInfo.ChannelNumber)
            {
                var popup = Lance.PopupManager.GetPopup<Popup_ChattingUI>();
                popup?.OnDeleteMessage(channelInfo);
            }
        }

        // 신고 완료
        public void OnSuccess(SUCCESS_MESSAGE success, object param)
        {
            switch (success)
            {
                case SUCCESS_MESSAGE.REPORT:
                    {
                        UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("SuccessReport"));
                    }
                    break;
            }
        }

        public void OnError(ERROR_MESSAGE error, object param)
        {
            if (error == ERROR_MESSAGE.NOT_MY_REPORT ||
                error == ERROR_MESSAGE.TOO_MANY_REPORT)
            {
                if (error == ERROR_MESSAGE.NOT_MY_REPORT)
                {
                    UIUtil.ShowSystemErrorMessage("NotMyReport");
                }
                else
                {
                    UIUtil.ShowSystemErrorMessage("TooManyReport");
                }

                return;
            }
            else
            {
                MessageInfo messageInfo = new MessageInfo
                {
                    Index = 0,
                    MessageType = MESSAGE_TYPE.SYSTEM_MESSAGE,
                    Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Tag = ""
                };

                if (error == ERROR_MESSAGE.CHAT_BAN)
                {
                    ErrorMessageChatBanParam errorMessageChatBanParam = (ErrorMessageChatBanParam)param;
                    if (errorMessageChatBanParam == null) return;

                    var banTime = DateTime.Now.AddSeconds(errorMessageChatBanParam.RemainSeconds);

                    messageInfo.Message = error.ToString() + " : " + banTime.ToString("yyyy-MM-dd HH:mm:ss") + " 까지";
                }
                else if (error == ERROR_MESSAGE.CHANNEL_FULL ||
                    error == ERROR_MESSAGE.INVALID_PASSWORD ||
                    error == ERROR_MESSAGE.ALREADY_CREATED_CHANNEL ||
                    error == ERROR_MESSAGE.CHANNEL_GROUP_TOO_SHORT ||
                    error == ERROR_MESSAGE.CHANNEL_GROUP_TOO_LONG ||
                    error == ERROR_MESSAGE.CHANNEL_NAME_TOO_SHORT ||
                    error == ERROR_MESSAGE.CHANNEL_NAME_TOO_LONG ||
                    error == ERROR_MESSAGE.DUPLICATE_CHANNEL_GROUP ||
                    error == ERROR_MESSAGE.PASSWORD_TOO_LONG ||
                    error == ERROR_MESSAGE.CHANNEL_GROUP_FILTERED ||
                    error == ERROR_MESSAGE.CHANNEL_NAME_FILTERED)
                {
                    ErrorMessageChannelParam errorMessageChannelParam = (ErrorMessageChannelParam)param;
                    if (errorMessageChannelParam == null) return;

                    messageInfo.Message = error.ToString() + " : " + errorMessageChannelParam.ChannelGroup + " / " + errorMessageChannelParam.ChannelName + " / " + errorMessageChannelParam.ChannelNumber;
                }
                else
                {
                    messageInfo.Message = error.ToString();
                }

                AddMessage(messageInfo);
            }

            mInReEntryChattingChannel = false;
            mInLeaveChattingChannel = false;
            mInJoinChattingChannel = false;

            //Debug.LogError(messageInfo.Message);
        }

        void AddMessage(MessageInfo messageInfo)
        {
            if (mChannelList.ContainsKey(mCurrentChannelGroup))
            {
                if (mChannelList[mCurrentChannelGroup].ContainsKey(mCurrentChannelName))
                {
                    if (mChannelList[mCurrentChannelGroup][mCurrentChannelName].ContainsKey(mCurrentChannelNumber))
                    {
                        ChannelInfo channelInfo = mChannelList[mCurrentChannelGroup][mCurrentChannelName][mCurrentChannelNumber];
                        if (channelInfo != null)
                        {
                            messageInfo.ChannelGroup = channelInfo.ChannelGroup;
                            messageInfo.ChannelName = channelInfo.ChannelName;
                            messageInfo.ChannelNumber = channelInfo.ChannelNumber;

                            channelInfo.Messages.Add(messageInfo);

                            var popup = Lance.PopupManager.GetPopup<Popup_ChattingUI>();
                            if (popup != null)
                            {
                                popup?.OnAddMessage(channelInfo.Messages.Count);
                            }

                            if (Lance.Lobby != null && Lance.Lobby.IsInit)
                                Lance.Lobby.RefreshChattingUI(GetCurrentChannelLastMessageInfo());
                        }
                    }
                }
            }
        }

        //public void SetNewMessage(bool isNew)
        //{
        //    mIsNewMessage = isNew;
        //}

        public void SendReportChat(ulong index, string tag, string keyword, string reason)
        {
            mChatClient?.SendReportChatMessage(index, tag, keyword, reason);
        }

        public void OnUpate()
        {
            mChatClient?.Update();
        }

        public void OnObjectDestory()
        {
            mChatClient?.Dispose();
        }

        public void OnAppQuit()
        {
            mChatClient?.Dispose();
        }

        // 채팅방 재입장
        public void ReEntryChattinChannel()
        {
            if (mInReEntryChattingChannel)
                return;

            Lance.GameManager.StartCoroutine(ReEntryChattingChannel());
        }

        bool mInReEntryChattingChannel;
        bool mInLeaveChattingChannel;
        bool mInJoinChattingChannel;

        IEnumerator ReEntryChattingChannel()
        {
            mChatClient?.Dispose();

            mChatClient = new ChatClient(this, GetAvatar());

            mInReEntryChattingChannel = true;

            yield return new WaitForSeconds(2f);

            //// 현재 채널을 떠난다.
            //SendLeaveChannel();

            mInReEntryChattingChannel = false;

            //while (mInLeaveChattingChannel)
            //{
            //    yield return null;
            //}

            //mInJoinChattingChannel = true;

            //// 채널을 다시 입장한다.
            //mChatClient.SendJoinOpenChannel(GroupName, LobbyName);

            //while (mInJoinChattingChannel)
            //{
            //    yield return null;
            //}
        }
    }
}

