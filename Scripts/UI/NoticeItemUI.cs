using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Lance
{
    class NoticeItemUI : MonoBehaviour
    {
        public void Init(NoticeItem noticeItem)
        {
            var textTitle = gameObject.FindComponent<TextMeshProUGUI>("Text_Title");
            textTitle.text = noticeItem.GetTitle();

            var textPostingDate = gameObject.FindComponent<TextMeshProUGUI>("Text_PostingDate");
            textPostingDate.text = noticeItem.GetPostingDateStr();

            var button = GetComponent<Button>();
            button.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_NoticeUI>();

                popup.Init(noticeItem);
            });
        }
    }
}