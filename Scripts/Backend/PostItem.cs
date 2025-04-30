using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using LitJson;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    // 우편에서 사용하는 차트
    public enum ChartType
    {
        forPost,
        //weaponChart,
        //itemChart
    }

    //===============================================================
    //  Post.Manager 클래스의 GetPostList의 리턴값에 대한 파싱하는 클래스
    //===============================================================
    public class PostItem
    {
        PostType mPostType;
        List<PostReward> mRewards;

        public readonly string title;
        public readonly string content;
        public readonly DateTime expirationDate;
        public readonly string inDate;
        public PostType PostType => mPostType;
        public PostItem(PostType postType, JsonData postListJson)
        {
            mPostType = postType;
            mRewards = new List<PostReward>();

            expirationDate = DateTime.Parse(postListJson["expirationDate"].ToString());
            content = postListJson["content"].ToString();
            inDate = postListJson["inDate"].ToString();
            title = postListJson["title"].ToString();

            var posRewardsJson = postListJson["items"];

            // 우편 보상 데이터
            if (posRewardsJson.Count > 0)
            {
                for (int itemNum = 0; itemNum < posRewardsJson.Count; itemNum++)
                {
                    PostReward reward = new PostReward(posRewardsJson[itemNum]);

                    mRewards.Add(reward);
                }
            }
        }

        // 우편 보상을 받은 후 호출되는 델리게이트 함수
        public delegate void IsReceiveSuccessFunc(bool isSuccess);

        // [뒤끝] 우편 수령 함수
        public void ReceiveReward(IsReceiveSuccessFunc isReceiveSuccessFunc)
        {
            SendQueue.Enqueue(Backend.UPost.ReceivePostItem, mPostType, inDate, callback => {
                bool isSuccess = false;
                try
                {
                    Debug.Log($"Backend.UPost.ReceivePostItem({mPostType}, {inDate}) : {callback}");

                    // 수령할 경우
                    if (callback.IsSuccess())
                    {
                        isSuccess = true;

                        RewardResult rewardResult = new RewardResult();

                        // 해당 우편이 가지고 있는 item의 Receive함수를 호출하여 보상을 획득
                        foreach (PostReward reward in mRewards)
                        {
                            var itemInfos = reward.GetItemInfos();

                            foreach(var itemInfo in itemInfos)
                            {
                                rewardResult = rewardResult.AddReward(itemInfo);
                            }
                        }

                        if (rewardResult.IsEmpty() == false)
                        {
                            Lance.GameManager.GiveReward(rewardResult, ShowRewardType.Popup);

                            // 보상을 다 얻고 난 다음에는 저장
                            Lance.BackEnd.UpdateAllAccountInfos();
                        }
                    }
                    else
                    {
                        // 수령 실패 오류 팝업
                        //StaticManager.UI.AlertUI.OpenErrorUIWithText("우편 수령 실패 에러", "우편 수령에 실패했습니다.\n" + callback.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                finally
                {
                    if (isSuccess)
                    {
                        //수령이 완료될 경우 우편 목록에서 제거
                        Lance.Account.Post.RemovePost(inDate);
                    }

                    isReceiveSuccessFunc(isSuccess);
                }
            });
        }

        public PostType GetPostType()
        {
            return mPostType;
        }

        public List<PostReward> GetRewards()
        {
            return mRewards;
        }

        public void RandomizeKey()
        {
            foreach(var reward in mRewards)
            {
                reward.RandomizeKey();
            }
        }

        public bool CanReceiveReward()
        {
            return mRewards.Count > 0;
        }
    }

    //===============================================================
    //  우편의 보상에 대한 클래스
    //===============================================================
    public class PostReward
    {
        ObscuredString mItemId;
        ObscuredInt mItemTypeInt;
        ObscuredDouble mItemCount;
        ItemType ItemType => (ItemType)(int)mItemTypeInt;
        public double GetItemCount()
        {
            return mItemCount;
        }

        public ItemType GetItemType()
        {
            return (ItemType)(int)mItemTypeInt;
        }

        public void RandomizeKey()
        {
            mItemId?.RandomizeCryptoKey();
            mItemTypeInt.RandomizeCryptoKey();
            mItemCount.RandomizeCryptoKey();
        }

        public PostReward(JsonData rewardJson)
        {
            // 수량
            double itemCountTemp = 0;
            double.TryParse(rewardJson["itemCount"].ToString(), out itemCountTemp);
            mItemCount = itemCountTemp;

            // 차트에 있는 아이템 정보 "item" 에 들어있음
            // itemType, itemId 확인 가능
            if (!Enum.TryParse<ItemType>(rewardJson["item"]["itemType"].ToString(), out var itemTypeTemp))
            {
                throw new Exception("지정되지 않은 itemType 입니다.");
            }

            mItemTypeInt = (int)itemTypeTemp;

            mItemId = rewardJson["item"]["itemId"].ToString();
        }

        public ItemInfo[] GetItemInfos()
        {
            if (ItemType == ItemType.Reward)
            {
                var rewardResult = Lance.GameManager.RewardDataChangeToRewardResult(mItemId);

                return rewardResult.Split().ToArray();
            }
            else
            {
                return new ItemInfo[] { ItemInfoUtil.CreateItemInfo(ItemType, mItemId, mItemCount) };
            }
        }
    }
}