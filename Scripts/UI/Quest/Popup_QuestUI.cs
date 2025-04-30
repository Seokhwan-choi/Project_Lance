using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lance
{
    class Popup_QuestUI : PopupBase
    {
        QuestTabUIManager mTabUIManager;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Quest"));

            StartCoroutine(DelayedInit());
        }

        IEnumerator DelayedInit()
        {
            var loadingObj = gameObject.FindGameObject("Loading");
            loadingObj.SetActive(true);

            mTabUIManager = new QuestTabUIManager();

            yield return mTabUIManager.InitAsync(gameObject);

            loadingObj.SetActive(false);
        }

        public void Refresh()
        {
            mTabUIManager.Refresh();
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }
    }
}