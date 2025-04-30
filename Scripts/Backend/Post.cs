using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Reflection;
using BackEnd;
using LitJson;
using System.Linq;

namespace Lance
{
    public class Post : AccountBase
    {
        Dictionary<string, PostItem> mPostItems = new();

        DateTime mRankPostUpdateTime;
        DateTime mAdminPostUpdateTime;
        public IEnumerable<PostItem> GetPostItems(PostType postType)
        {
            foreach (var postItem in mPostItems.Values)
                if (postItem.GetPostType() == postType)
                    yield return postItem;
        }

        public int GetPostItemCount(PostType postType)
        {
            int totalCount = 0;

            foreach(var postItem in mPostItems.Values)
            {
                if (postItem.GetPostType() == postType)
                {
                    totalCount++;
                }
            }

            return totalCount;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            foreach(var postItem in mPostItems.Values)
            {
                postItem.RandomizeKey();
            }
        }

        // 로딩씬에서 호출되는 함수
        public override void BackendLoad(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            mAdminPostUpdateTime = DateTime.MinValue;

            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            // 관리자 우편 불러오기
            GetPostList(PostType.Admin, (isSuccess, errorInfo) => {
                afterBackendLoadFunc.Invoke(isSuccess, className, funcName, errorInfo);
            });
        }

        public void BackendLoadForRank(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            mRankPostUpdateTime = DateTime.MinValue;

            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            // 관리자 우편 불러오기
            GetPostList(PostType.Rank, (isSuccess, errorInfo) => {
                afterBackendLoadFunc.Invoke(isSuccess, className, funcName, errorInfo);
            });
        }

        public void BackendLoadForCoupon(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string className = GetType().Name;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            // 관리자 우편 불러오기
            GetPostList(PostType.Coupon, (isSuccess, errorInfo) => {
                afterBackendLoadFunc.Invoke(isSuccess, className, funcName, errorInfo);
            });
        }

        public delegate void AfterGetPostFunc(bool isSuccess, string errorInfo);

        // 우편 리스트 불러오는 함수
        public void GetPostList(PostType postType, AfterGetPostFunc afterPostLoadingFunc)
        {
            bool isSuccess = false;
            string errorInfo = string.Empty;

            // 10분이 지나지 않았을 경우에는 캐싱된 값을 리턴
            if (postType == PostType.Rank)
            {
                if ((TimeUtil.UtcNow - mRankPostUpdateTime).Minutes < 10)
                {
                    afterPostLoadingFunc(true, string.Empty);
                    return;
                }
            }
            else if (postType == PostType.Admin)
            {
                if ((TimeUtil.UtcNow - mAdminPostUpdateTime).Minutes < 10)
                {
                    afterPostLoadingFunc(true, string.Empty);
                    return;
                }
            }

            //[뒤끝] 우편 목록 불러오기 함수
            SendQueue.Enqueue(Backend.UPost.GetPostList, postType, callback => {
                try
                {
                    Debug.Log($"Backend.UPost.GetPostList({postType}) : {callback}");

                    if (callback.IsSuccess() == false)
                    {
                        throw new Exception(callback.ToString());
                    }

                    // 랭킹 우편 시간 최근시간으로 갱신
                    if (postType == PostType.Rank)
                        mRankPostUpdateTime = TimeUtil.UtcNow;
                    else if (postType == PostType.Admin)
                        mAdminPostUpdateTime = TimeUtil.UtcNow;

                    JsonData postListJson = callback.GetReturnValuetoJSON()["postList"];

                    for (int i = 0; i < postListJson.Count; i++)
                    {
                        if (mPostItems.ContainsKey(postListJson[i]["inDate"].ToString()))
                        {
                            //새로 불러온 우편에 아직 안받은 우편의 데이터가 있을 경우 패스
                        }
                        else
                        {
                            // 새로운 아이템 등록
                            PostItem postItem = new PostItem(postType, postListJson[i]);

                            mPostItems.Add(postItem.inDate, postItem);
                        }
                    }
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    afterPostLoadingFunc.Invoke(isSuccess, errorInfo);
                }
            });
        }

        public void RemovePost(string inDate)
        {
            mPostItems.Remove(inDate);
        }

        public void AllRemovePost(PostType postType)
        {
            List<PostItem> removeList = new List<PostItem>();

            foreach(PostItem postItem in mPostItems.Values)
            {
                if (postItem.GetPostType() == postType)
                {
                    removeList.Add(postItem);
                }
            }

            foreach(var remove in removeList)
            {
                mPostItems.Remove(remove.inDate);
            }
        }

        public void ReceiveAllPost(PostType postType, Action<List<RewardResult>> onSuccess)
        {
            string errorInfo = string.Empty;
            List<RewardResult> reults = new List<RewardResult>();

            // [뒤끝] 해당 타입 우편 모두 수령
            SendQueue.Enqueue(Backend.UPost.ReceivePostItemAll, postType, callback => 
            {
                try
                {
                    Debug.Log($"Backend.UPost.ReceivePostItemAll({postType}) : {callback}");

                    if (callback.IsSuccess() == false)
                    {
                        throw new Exception(callback.ToString());
                    }

                    JsonData postItems = callback.GetReturnValuetoJSON()["postItems"];

                    foreach (JsonData postItem in postItems)
                    {
                        for (int j = 0; j < postItem.Count; ++j)
                        {
                            RewardResult rewardResult = RewardResultUtil.CreatePostRewardResult(postItem[j]);

                            reults.Add(rewardResult);
                        }
                    }

                    AllRemovePost(postType);

                    if (reults.Count > 0)
                        onSuccess.Invoke(reults);
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
            });
        }

        public bool AnyCanReceiveReward()
        {
            foreach(var postItem in mPostItems.Values)
            {
                if (postItem.CanReceiveReward())
                    return true;
            }

            return false;
        }

        public bool AnyCanReceiveReward(PostType postType)
        {
            IEnumerable<PostItem> postItems = mPostItems.Values.Where(x => x.PostType == postType);
            foreach (var postItem in postItems)
            {
                if (postItem.CanReceiveReward())
                    return true;
            }

            return false;
        }
    }
}