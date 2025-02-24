using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd.Leaderboard;


namespace Lance
{
    // ���ھ� �� : E9CFAA
    // 4�� ~ �� : 9B8961
    // �� �̸� : 2A2220
    // ������ : ȭ��Ʈ
    class JoustingTopRankingItemUI : MonoBehaviour
    {
        int mRank;
        Image mImagePortrait;
        Image mImagePortraitFrame;
        TextMeshProUGUI mTextNickName;
        public int Rank => mRank;
        public void Init(int rank)
        {
            mRank = rank;

            // �ʻ�ȭ
            mImagePortrait = gameObject.FindComponent<Image>("Image_Portrait");
            // �ʻ�ȭ ������
            mImagePortraitFrame = gameObject.FindComponent<Image>("Image_Portrait_Frame");
            // �г���
            mTextNickName = gameObject.FindComponent<TextMeshProUGUI>("Text_Nickname");

            var levelObj = gameObject.FindGameObject("Level");
            levelObj.SetActive(false);
        }

        public void Refresh(UserLeaderboardItem rankItem)
        {
            gameObject.SetActive(true);

            if (rankItem != null)
            {
                string extraInfo = rankItem.extraData;

                if (extraInfo.IsValid())
                {
                    if (extraInfo.Contains("\\split"))
                    {
                        var splitResult = extraInfo.Split("\\split");

                        string portrait = splitResult[0];
                        string portraitFrame = splitResult[1];

                        mImagePortrait.sprite = GetPortrait(portrait);
                        mImagePortraitFrame.sprite = GetPortraitFrame(portraitFrame);
                    }
                    else
                    {
                        mImagePortrait.sprite = GetPortrait(extraInfo);
                        mImagePortraitFrame.sprite = GetPortraitFrame(string.Empty);
                    }
                }
                else
                {
                    mImagePortrait.sprite = GetPortrait(string.Empty);
                    mImagePortraitFrame.sprite = GetPortraitFrame(string.Empty);
                }
            }
            else
            {
                // ���� ������ ����
                mImagePortrait.gameObject.SetActive(false);
            }

            if (rankItem != null)
            {
                mTextNickName.text = rankItem.nickname;
            }
            else
            {
                mTextNickName.text = "";
            }
        }
        
        Sprite GetPortrait(string portrait)
        {
            if (portrait.IsValid())
            {
                var costumeData = DataUtil.GetCostumeData(portrait);

                return Lance.Atlas.GetPlayerSprite(costumeData.uiSprite);
            }
            else
            {
                return Lance.Atlas.GetPlayerSprite(Lance.GameData.CostumeCommonData.playerDefaultUISprite);
            }
        }

        Sprite GetPortraitFrame(string frame)
        {
            if (frame.IsValid())
            {
                var achievementData = Lance.GameData.AchievementData.TryGet(frame);

                return Lance.Atlas.GetUISprite(achievementData.uiSprite);
            }
            else
            {
                return Lance.Atlas.GetUISprite(Lance.GameData.AchievementCommonData.defaultFrame);
            }
        }
    }
}