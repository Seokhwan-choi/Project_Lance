using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;
using System;


namespace Lance
{
    public class Notice : AccountBase
    {
        List<NoticeItem> mNoticeList;
        public void GetNoticeList(Action onFinish)
        {
            mNoticeList = new List<NoticeItem>();

            Lance.TouchBlock.SetActive(true);

            SendQueue.Enqueue(Backend.Notice.NoticeList, 10, (bro) =>
            {
                if (bro.IsSuccess())
                {
                    JsonData jsonList = bro.FlattenRows();
                    for (int i = 0; i < jsonList.Count; i++)
                    {
                        NoticeItem notice = new NoticeItem(jsonList[i]);

                        mNoticeList.Add(notice);
                    }

                    onFinish?.Invoke();
                }

                Lance.TouchBlock.SetActive(false);
            });
        }

        public List<NoticeItem> GetNoticeList()
        {
            return mNoticeList;
        }
    }

    public class NoticeItem
    {
        string mTitle;
        string mContents;
        DateTime mPostingDate;
        string mInDate;
        string mUuid;
        bool mIsPublic;
        string mAuthor;

        string mImageKey;
        string mLinkUrl;
        string mLinkButtonName;

        public NoticeItem(JsonData gameDataJson)
        {
            mTitle = gameDataJson["title"].ToString();
            mContents = gameDataJson["content"].ToString();
            mPostingDate = DateTime.Parse(gameDataJson["postingDate"].ToString());
            mInDate = gameDataJson["inDate"].ToString();
            mUuid = gameDataJson["uuid"].ToString();
            mIsPublic = gameDataJson["isPublic"].ToString() == "y" ? true : false;
            mAuthor = gameDataJson["author"].ToString();

            if (gameDataJson.ContainsKey("imageKey"))
            {
                mImageKey = "http://upload-console.thebackend.io" + gameDataJson["imageKey"].ToString();
            }
            if (gameDataJson.ContainsKey("linkUrl"))
            {
                mLinkUrl = gameDataJson["linkUrl"].ToString();
            }
            if (gameDataJson.ContainsKey("linkButtonName"))
            {
                mLinkButtonName = gameDataJson["linkButtonName"].ToString();
            }
        }

        public string GetTitle()
        {
            return mTitle;
        }

        public string GetContents()
        {
            return mContents;
        }

        public string GetPostingDateStr()
        {
            return mPostingDate.ToString("yyyy.MM.dd");
        }
    }
}