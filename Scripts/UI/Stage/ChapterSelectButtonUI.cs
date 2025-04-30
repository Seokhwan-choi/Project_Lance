using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Lance
{
    class ChapterSelectButtonUI : MonoBehaviour
    {
        int mChapter;
        Button mButtonChapterSelect;
        TextMeshProUGUI mTextChapterNum;
        public int Chapter => mChapter;
        public void Init(int chapter, Action<int> onButtonAction)
        {
            mChapter = chapter;
            mTextChapterNum = gameObject.FindComponent<TextMeshProUGUI>("Text_Chapter");

            mButtonChapterSelect = GetComponent<Button>();
            mButtonChapterSelect.SetButtonAction(() => onButtonAction.Invoke(chapter));
        }

        public void SetActiveFrame(bool isActive)
        {
            mButtonChapterSelect.SetActiveFrame(isActive);

            StringParam param = new StringParam("chapter", mChapter);

            mTextChapterNum.text = StringTableUtil.Get("UIString_Chapter", param);
        }
    }
}


