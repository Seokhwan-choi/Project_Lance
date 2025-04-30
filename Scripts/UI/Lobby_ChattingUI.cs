using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEndChat;


namespace Lance
{
    class Lobby_ChattingUI : MonoBehaviour
    {
        const float ChattingCooltime = 5f;

        float mChattingCoolTime;
        TextMeshProUGUI mTextNickName;
        TextMeshProUGUI mTextChat;
        GameObject mLockObj;
        public void Init()
        {
            mTextNickName = gameObject.FindComponent<TextMeshProUGUI>("Text_Nickname");
            mTextChat = gameObject.FindComponent<TextMeshProUGUI>("Text_Chat");

            mLockObj = gameObject.FindGameObject("Lock");

            var button = GetComponent<Button>();
            button.SetButtonAction(() =>
            {
                if (ContentsLockUtil.IsLockContents(ContentsLockType.Rank))
                {
                    ContentsLockUtil.ShowContentsLockMessage(ContentsLockType.Rank);

                    SoundPlayer.PlayErrorSound();

                    return;
                }

                // 아직 필요없는 기능
                //if (mChattingCoolTime > 0)
                //{
                //    UIUtil.ShowSystemErrorMessage("CanNotOpenChattingInCoolTime");

                //    return;
                //}

                var popup = Lance.PopupManager.CreatePopup<Popup_ChattingUI>();

                popup.Init();

                mChattingCoolTime = ChattingCooltime;
            });

            var textLockReason = mLockObj.FindComponent<TextMeshProUGUI>("Text_LockReason");
            textLockReason.text = ContentsLockUtil.GetContentsLockMessage(ContentsLockType.Rank);

            RefreshContentsLockUI();

            RefreshChatMessage(Lance.BackEnd.ChattingManager.GetCurrentChannelLastMessageInfo());
        }

        public void Localize()
        {
            var textLockReason = mLockObj.FindComponent<TextMeshProUGUI>("Text_LockReason");
            textLockReason.text = ContentsLockUtil.GetContentsLockMessage(ContentsLockType.Rank);
        }

        public void OnUpdate(float dt)
        {
            if (mChattingCoolTime > 0)
            {
                mChattingCoolTime -= dt;

                //mImageChattingModal.fillAmount = mChattingCoolTime / ChattingCooltime;
            }
        }

        public void RefreshChatMessage(MessageInfo messageInfo)
        {
            if (messageInfo != null)
            {
                if (mTextNickName.gameObject.activeSelf == false)
                    mTextNickName.gameObject.SetActive(true);

                if (mTextChat.gameObject.activeSelf == false)
                    mTextChat.gameObject.SetActive(true);

                string message = string.Empty;
                int rank = 0;

                if (messageInfo.Message.Contains("\\rank"))
                {
                    string[] messages = messageInfo.Message.Split("\\rank");
                    if (messages.Length > 1)
                    {
                        message = messages[0];
                        rank = messages[1].ToIntSafe(0);
                    }
                    else
                    {
                        message = messageInfo.Message;
                    }
                }
                else
                {
                    message = messageInfo.Message;
                }

                string rankStr = string.Empty;
                string rankColor = string.Empty;
                if (rank > 0)
                {
                    StringParam param = new StringParam("rank", rank);

                    rankStr = StringTableUtil.Get("UIString_Rank", param);

                    if (rank == 1)
                    {
                        rankColor = "FFCD1A";
                    }
                    else if (rank == 2)
                    {
                        rankColor = "41FFFF";
                    }
                    else if (rank == 3)
                    {
                        rankColor = "EA8362";
                    }
                    else
                    {
                        rankColor = "CCBFB0"; // 9B8961
                    }
                }
                else
                {
                    rankStr = StringTableUtil.Get("UIString_NoneRank");
                    rankColor = "CCBFB0";
                }

                mTextNickName.text = $"{messageInfo.GamerName} [ {rankStr} ]";
                mTextNickName.SetColor(rankColor);
                mTextChat.text = message;
            }
            else
            {
                mTextNickName.gameObject.SetActive(false);
                mTextChat.gameObject.SetActive(false);
            }
        }

        public void RefreshContentsLockUI()
        {
            mLockObj.SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Rank));
        }
    }

}