using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using LitJson;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    // ������ ����ϴ� ��Ʈ
    public enum ChartType
    {
        forPost,
        //weaponChart,
        //itemChart
    }

    //===============================================================
    //  Post.Manager Ŭ������ GetPostList�� ���ϰ��� ���� �Ľ��ϴ� Ŭ����
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

            // ���� ���� ������
            if (posRewardsJson.Count > 0)
            {
                for (int itemNum = 0; itemNum < posRewardsJson.Count; itemNum++)
                {
                    PostReward reward = new PostReward(posRewardsJson[itemNum]);

                    mRewards.Add(reward);
                }
            }
        }

        // ���� ������ ���� �� ȣ��Ǵ� ��������Ʈ �Լ�
        public delegate void IsReceiveSuccessFunc(bool isSuccess);

        // [�ڳ�] ���� ���� �Լ�
        public void ReceiveReward(IsReceiveSuccessFunc isReceiveSuccessFunc)
        {
            SendQueue.Enqueue(Backend.UPost.ReceivePostItem, mPostType, inDate, callback => {
                bool isSuccess = false;
                try
                {
                    Debug.Log($"Backend.UPost.ReceivePostItem({mPostType}, {inDate}) : {callback}");

                    // ������ ���
                    if (callback.IsSuccess())
                    {
                        isSuccess = true;

                        RewardResult rewardResult = new RewardResult();

                        // �ش� ������ ������ �ִ� item�� Receive�Լ��� ȣ���Ͽ� ������ ȹ��
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

                            // ������ �� ��� �� �������� ����
                            Lance.BackEnd.UpdateAllAccountInfos();
                        }
                    }
                    else
                    {
                        // ���� ���� ���� �˾�
                        //StaticManager.UI.AlertUI.OpenErrorUIWithText("���� ���� ���� ����", "���� ���ɿ� �����߽��ϴ�.\n" + callback.ToString());
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
                        //������ �Ϸ�� ��� ���� ��Ͽ��� ����
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
    //  ������ ���� ���� Ŭ����
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
            // ����
            double itemCountTemp = 0;
            double.TryParse(rewardJson["itemCount"].ToString(), out itemCountTemp);
            mItemCount = itemCountTemp;

            // ��Ʈ�� �ִ� ������ ���� "item" �� �������
            // itemType, itemId Ȯ�� ����
            if (!Enum.TryParse<ItemType>(rewardJson["item"]["itemType"].ToString(), out var itemTypeTemp))
            {
                throw new Exception("�������� ���� itemType �Դϴ�.");
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