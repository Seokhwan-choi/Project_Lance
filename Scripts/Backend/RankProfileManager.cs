using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using BackEnd;
using LitJson;

namespace Lance
{
    public delegate void GetRankProfileFunc(bool isSuccess, RankProfile rankProfile);

    class RankProfileManager
    {
        DateTime mMyRankProfileUpdateTime;
        PlayerPreview mRankProfileCostumePreview;
        Dictionary<string, RankProfileInfo> mRankProfileInfos = new();

        public void Init()
        {
            mRankProfileCostumePreview = GameObject.Find("RankProfileCostumePreview").GetOrAddComponent<PlayerPreview>();
            mRankProfileCostumePreview.Init();

            UpdateMyRankProfile();
        }
        
        public void UpdateMyRankProfile()
        {
            //// 15분이 지나지 않았을 경우에는 캐싱된 값 리턴
            //if ((TimeUtil.UtcNow - mMyRankProfileUpdateTime).Minutes < 15)
            //    return;

            InternalUpdateMyRankProfile();
        }

        void InternalUpdateMyRankProfile()
        {
            // 착용 장비
            Lance.Account.RankProfile.SetEquipments(Lance.Account.GetEquippedEquipments().ToArray());
            // 착용 악세사리
            Lance.Account.RankProfile.SetAccessorys(Lance.Account.GetEquippedAccessorys().ToArray());
            // 착용 신수
            Lance.Account.RankProfile.SetPet(Lance.Account.Pet.GetEquippedInst());
            // 착용 코스튬
            Lance.Account.RankProfile.SetCostumes(Lance.Account.Costume.GetEquippedCostumeIds());
            // 국가코드
            Lance.Account.RankProfile.SetCountryCode(Lance.Account.CountryCode);
            // 레벨
            Lance.Account.RankProfile.SetLevel(Lance.Account.ExpLevel.GetLevel());
            // 전투력
            Lance.Account.RankProfile.SetPowerLevel(Lance.Account.UserInfo.GetBestPowerLevel());
            // 스탯
            Dictionary<StatType, double> stats = new Dictionary<StatType, double>();
            for (int i = 0; i < (int)StatType.Count; ++i)
            {
                StatType statType = (StatType)i;
                if (statType.IsShowingUserInfo())
                {
                    stats.Add(statType, StatCalculator.GetPlayerStatValue(statType));
                }
            }
            Lance.Account.RankProfile.SetStats(stats);

            // 레벨 랭킹
            var levelLeaderoboard = Lance.Account.Leaderboard.GetLeaderboardInfo(Lance.Account.ExpLevel.GetTableName());
            levelLeaderoboard.GetMyRank((isSuccess, levelItem) =>
            {
                if (levelItem != null)
                    Lance.Account.RankProfile.SetLevelRank(levelItem.rank.ToIntSafe(0));

                // 전투력 랭킹
                var powerLevelLeaderoboard = Lance.Account.Leaderboard.GetLeaderboardInfo(Lance.Account.UserInfo.GetTableName());
                powerLevelLeaderoboard.GetMyRank((isSuccess, powerLevelItem) =>
                {
                    if (powerLevelItem != null)
                        Lance.Account.RankProfile.SetPowerLevelRank(powerLevelItem.rank.ToIntSafe(0));

                    // 스테이지 랭킹
                    var stageLeaderoboard = Lance.Account.Leaderboard.GetLeaderboardInfo(Lance.Account.StageRecords.GetTableName());
                    stageLeaderoboard.GetMyRank((isSuccess, stageItem) =>
                    {
                        if (stageItem != null)
                            Lance.Account.RankProfile.SetStageRank(stageItem.rank.ToIntSafe(0));

                        Lance.BackEnd.UpdateAllAccountInfos();

                    });
                });
            }); 
        }

        public void GetRankProfile(string nickName, GetRankProfileFunc getRankProfileFunc)
        {
            if (mRankProfileInfos.ContainsKey(nickName))
            {
                var rankProfileInfo = mRankProfileInfos.TryGet(nickName);

                rankProfileInfo.GetRankProfile(getRankProfileFunc);
            }
            else
            {
                var rankProfileInfo = new RankProfileInfo();
                rankProfileInfo.Init(nickName);

                rankProfileInfo.GetRankProfile(getRankProfileFunc);
            }
        }

        public void SetActiveRankProfile(bool isActive)
        {
            mRankProfileCostumePreview.SetActive(isActive);
        }

        public void RefreshRankProfilePreview(string[] costumes)
        {
            mRankProfileCostumePreview.Refresh(costumes);
        }

        public RenderTexture GetRankProfilePreviewRenderTexture()
        {
            return mRankProfileCostumePreview.GetRenderTexture();
        }
    }

    class RankProfileInfo
    {
        string mNickname;
        RankProfile mRankProfile;
        DateTime mUpdateTime;

        public void Init(string nickname)
        {
            mNickname = nickname;
            mUpdateTime = TimeUtil.UtcNow;
        }

        public void GetRankProfile(GetRankProfileFunc getRankProfileFunc)
        {
            // 15분이 지나지 않았을 경우에는 캐싱된 값 리턴
            if ((TimeUtil.UtcNow - mUpdateTime).Minutes < 15 && mRankProfile != null)
            {
                getRankProfileFunc(true, mRankProfile);
                return;
            }

            // 닉네임을 검색하여 해당 유저의 owner_inDate 알아내기
            SendQueue.Enqueue(Backend.Social.GetUserInfoByNickName, mNickname, callback =>
            {
                if (callback.IsSuccess())
                {
                    string otherOwnerIndate = callback.GetReturnValuetoJSON()["row"]["inDate"].ToString();

                    // inDate를 가지고 RankProfile 테이블 정보를 읽어오자
                    SendQueue.Enqueue(Backend.PlayerData.GetOtherData, Lance.Account.RankProfile.GetTableName(), otherOwnerIndate, 1, callback =>
                    {
                        if (callback.IsSuccess() && callback.FlattenRows().Count <= 0)
                        {
                            getRankProfileFunc.Invoke(false, null);

                            return;
                        }

                        mUpdateTime = TimeUtil.UtcNow;

                        JsonData gameDataListJson = callback.FlattenRows();

                        mRankProfile = new RankProfile();

                        mRankProfile.ExternalSetServerDataToLocal(gameDataListJson[0]);

                        getRankProfileFunc.Invoke(true, mRankProfile);

                    });
                }
                else
                {
                    getRankProfileFunc.Invoke(false, null);
                }
            });
        }
    }
}