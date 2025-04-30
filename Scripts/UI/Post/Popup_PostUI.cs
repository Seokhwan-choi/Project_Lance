using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

namespace Lance
{
    class Popup_PostUI : PopupBase
    {
        PostTabUIManager mTabUIManager;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Post"));

            StartCoroutine(GetPostList());
        }

        IEnumerator GetPostList()
        {
            var loading = gameObject.FindGameObject("Loading");
            loading.SetActive(true);

            bool processing = true;

            // 한번에 호출하는것도없고 대박이다 진짜
            Lance.Account.Post.GetPostList(PostType.Admin, (callback, errorInfo) => 
            {
                // 성공 유무 상관없이 계속 호출
                Lance.Account.Post.GetPostList(PostType.Rank, (callback, errorInfo) =>
                {
                    Lance.Account.Post.GetPostList(PostType.Coupon, (callback, errorInfo) =>
                    {
                        processing = false;
                    });
                });
            });

            while(processing)
            {
                yield return null;
            }

            loading.SetActive(false);

            mTabUIManager = new PostTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }
    }
}