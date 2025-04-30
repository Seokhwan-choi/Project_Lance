using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Popup_NoticeListUI : PopupBase
    {
        public void Init(List<NoticeItem> noticeList)
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_Notice"));

            StartCoroutine(DelayedInit(noticeList));
        }

        IEnumerator DelayedInit(List<NoticeItem> noticeList)
        {
            var noticeListObj = gameObject.FindGameObject("NoticeList");

            noticeListObj.AllChildObjectOff();

            foreach (var notice in noticeList)
            {
                GameObject noticeItemObj = Util.InstantiateUI("NoticeItemUI", noticeListObj.transform);

                NoticeItemUI noticeItemUI = noticeItemObj.GetOrAddComponent<NoticeItemUI>();
                noticeItemUI.Init(notice);
            }

            yield return null;

            // 스크롤 뷰 맨위로
            var scrollView = gameObject.FindComponent<ScrollRect>("Scroll View");
            scrollView.normalizedPosition = new Vector2(0f, 1f);
        }
    }
}