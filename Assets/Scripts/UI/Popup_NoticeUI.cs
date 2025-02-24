using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Lance
{
    class Popup_NoticeUI : PopupBase
    {
        public void Init(NoticeItem noticeItem)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Notice"));

            StartCoroutine(DelayedInit(noticeItem));
        }

        IEnumerator DelayedInit(NoticeItem noticeItem)
        {
            var textTitle = gameObject.FindComponent<TextMeshProUGUI>("Text_NoticeTitle");
            textTitle.text = noticeItem.GetTitle();

            var textPostingDate = gameObject.FindComponent<TextMeshProUGUI>("Text_PostingDate");
            textPostingDate.text = noticeItem.GetPostingDateStr();

            var textContents = gameObject.FindComponent<TextMeshProUGUI>("Text_Contents");
            textContents.text = noticeItem.GetContents();

            yield return null;

            // 스크롤 뷰 맨위로
            var scrollView = gameObject.FindComponent<ScrollRect>("Scroll View");
            scrollView.normalizedPosition = new Vector2(0f, 1f);
        }
    }
}