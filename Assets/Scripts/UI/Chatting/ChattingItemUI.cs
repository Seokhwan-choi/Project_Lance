using System;
using Mosframe;
using TMPro;
using BackEndChat;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class ChattingItemUI : MonoBehaviour, IDynamicScrollViewItem
    {
        string mNickname;
        GameObject mUserObj;
        TextMeshProUGUI mTextUserName;
        TextMeshProUGUI mTextUserChat;
        Image mImageUserPortrait;
        Image mImageUserPortraitFrame;
        Image mImageUserCountry;

        GameObject mMyObj;
        TextMeshProUGUI mTextMyName;
        TextMeshProUGUI mTextMyChat;
        Image mImageMyPortrait;
        Image mImageMyPortraitFrame;
        Image mImageMyCountry;

        GameObject mSystemObj;
        TextMeshProUGUI mTextSystemChat;

        bool mInit;
        ulong mIndex;
        string mTag;

        public void Init()
        {
            if (mInit)
                return;

            mInit = true;

            var button = GetComponent<Button>();
            button.SetButtonAction(() => 
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_RankProfileUI>();
                popup.Init(mNickname, mIndex, mTag);
            });

            mUserObj = gameObject.FindGameObject("UserChat");
            mTextUserName = mUserObj.FindComponent<TextMeshProUGUI>("Text_Name");
            mTextUserChat = mUserObj.FindComponent<TextMeshProUGUI>("Text_Chat");
            mImageUserPortrait = mUserObj.FindComponent<Image>("Image_Portrait");
            mImageUserPortraitFrame = mUserObj.FindComponent<Image>("Image_Portrait_Frame");
            mImageUserCountry = mUserObj.FindComponent<Image>("Image_UserCountry");
            var userLevel = mUserObj.FindGameObject("Level");
            userLevel.SetActive(false);

            mMyObj = gameObject.FindGameObject("MyChat");
            mTextMyName = mMyObj.FindComponent<TextMeshProUGUI>("Text_Name");
            mTextMyChat = mMyObj.FindComponent<TextMeshProUGUI>("Text_Chat");
            mImageMyPortrait = mMyObj.FindComponent<Image>("Image_Portrait");
            mImageMyPortraitFrame = mMyObj.FindComponent<Image>("Image_Portrait_Frame");
            mImageMyCountry = mMyObj.FindComponent<Image>("Image_MyCountry");
            var myLevel = mMyObj.FindGameObject("Level");
            myLevel.SetActive(false);

            mSystemObj = gameObject.FindGameObject("SystemChat");
            mTextSystemChat = mSystemObj.FindComponent<TextMeshProUGUI>("Text_Chat");
        }

        public void OnUpdateItem(int index)
        {
            var messageInfos = Lance.BackEnd.GetCurrentChannelMessageInfos();
            if (messageInfos == null || messageInfos.Count <= index)
                return;

            MessageInfo messageInfo = messageInfos[index];

            mIndex = messageInfo.Index;
            mTag = messageInfo.Tag;

            BackEnd.Backend.BackendChatSettings settings = BackEnd.Backend.GetBackendChatSettings();

            bool isMy = settings.nickname == messageInfo.GamerName;
            bool isSysetm = messageInfo.MessageType == MESSAGE_TYPE.SYSTEM_MESSAGE;

            if (isSysetm)
            {
                mSystemObj.SetActive(true);
                mUserObj.SetActive(false);
                mMyObj.SetActive(false);

                mTextSystemChat.text = messageInfo.Message;
            }
            else
            {
                mSystemObj.SetActive(false);
                mUserObj.SetActive(!isMy);
                mMyObj.SetActive(isMy);

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
                        rankColor = "9B8961";
                    }
                }
                else
                {
                    rankStr = StringTableUtil.Get("UIString_NoneRank");
                    rankColor = "9B8961";
                }

                if (isMy)
                {
                    mNickname = messageInfo.GamerName;
                    mTextMyName.text = $"{messageInfo.GamerName} [ {rankStr} ]";
                    mTextMyName.SetColor(rankColor);
                    mTextMyChat.text = message;

                    if (messageInfo.Avatar.IsValid())
                    {
                        if (messageInfo.Avatar.Contains("\\s"))
                        {
                            var splitResult = messageInfo.Avatar.Split("\\s");

                            var costume = splitResult.Length > 0 ? splitResult[0] : string.Empty;
                            var achievement = splitResult.Length > 1 ? splitResult[1] : string.Empty;
                            var countryCode = splitResult.Length > 2 ? splitResult[2] : string.Empty;

                            var costumeData = Lance.GameData.BodyCostumeData.TryGet(costume);
                            if (costumeData != null)
                            {
                                mImageMyPortrait.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
                            }
                            else
                            {
                                mImageMyPortrait.sprite = Lance.Atlas.GetPlayerSprite(Lance.GameData.CostumeCommonData.playerDefaultUISprite);
                            }

                            var achievementData = DataUtil.GetAchievementDataByKey(achievement);
                            if (achievementData != null)
                            {
                                mImageMyPortraitFrame.sprite = Lance.Atlas.GetUISprite(achievementData.uiSprite);
                            }
                            else
                            {
                                mImageMyPortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                            }

                            if (countryCode.IsValid())
                            {
                                mImageMyCountry.gameObject.SetActive(true);
                                mImageMyCountry.sprite = Lance.Atlas.GetCountry(countryCode);
                            }
                            else
                            {
                                mImageMyCountry.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            var costumeData = Lance.GameData.BodyCostumeData.TryGet(messageInfo.Avatar);
                            if (costumeData != null)
                            {
                                mImageMyPortrait.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
                                mImageMyPortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                            }
                            else
                            {
                                mImageMyPortrait.sprite = Lance.Atlas.GetPlayerSprite(Lance.GameData.CostumeCommonData.playerDefaultUISprite);
                                mImageMyPortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                            }

                            mImageMyCountry.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        mImageMyPortrait.sprite = Lance.Atlas.GetPlayerSprite(Lance.GameData.CostumeCommonData.playerDefaultUISprite);
                        mImageMyPortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                        mImageMyCountry.gameObject.SetActive(false);
                    }
                }
                else
                {
                    mNickname = messageInfo.GamerName;
                    mTextUserName.text = $"{messageInfo.GamerName} [ {rankStr} ]";
                    mTextUserName.SetColor(rankColor);
                    mTextUserChat.text = message;

                    if (messageInfo.Avatar.IsValid())
                    {
                        if (messageInfo.Avatar.Contains("\\s"))
                        {
                            var splitResult = messageInfo.Avatar.Split("\\s");

                            var costume = splitResult.Length > 0 ? splitResult[0] : string.Empty;
                            var achievement = splitResult.Length > 1 ? splitResult[1] : string.Empty;
                            var countryCode = splitResult.Length > 2 ? splitResult[2] : string.Empty;

                            var costumeData = Lance.GameData.BodyCostumeData.TryGet(costume);
                            if (costumeData != null)
                            {
                                mImageUserPortrait.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
                            }
                            else
                            {
                                mImageUserPortrait.sprite = Lance.Atlas.GetPlayerSprite(Lance.GameData.CostumeCommonData.playerDefaultUISprite);
                            }

                            var achievementData = DataUtil.GetAchievementDataByKey(achievement);
                            if (achievementData != null)
                            {
                                mImageUserPortraitFrame.sprite = Lance.Atlas.GetUISprite(achievementData.uiSprite);
                            }
                            else
                            {
                                mImageUserPortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                            }

                            if (countryCode.IsValid())
                            {
                                mImageUserCountry.gameObject.SetActive(true);
                                mImageUserCountry.sprite = Lance.Atlas.GetCountry(countryCode);
                            }
                            else
                            {
                                mImageUserCountry.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            var costumeData = Lance.GameData.BodyCostumeData.TryGet(messageInfo.Avatar);
                            if (costumeData != null)
                            {
                                mImageUserPortrait.sprite = Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
                                mImageUserPortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                            }
                            else
                            {
                                mImageUserPortrait.sprite = Lance.Atlas.GetPlayerSprite(Lance.GameData.CostumeCommonData.playerDefaultUISprite);
                                mImageUserPortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                            }

                            mImageUserCountry.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        mImageUserPortrait.sprite = Lance.Atlas.GetPlayerSprite(Lance.GameData.CostumeCommonData.playerDefaultUISprite);
                        mImageUserPortraitFrame.sprite = Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
                        mImageUserCountry.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}


