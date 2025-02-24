using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Popup_NewEventUI : PopupBase
    {
        NewEventTabUIManager mTabUIManager;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Event"));

            StartCoroutine(DelayedInit());
        }

        IEnumerator DelayedInit()
        {
            var loadingObj = gameObject.FindGameObject("Loading");
            loadingObj.SetActive(true);

            mTabUIManager = new NewEventTabUIManager();

            yield return mTabUIManager.DelayedInit(gameObject);

            RefreshRedDots();

            loadingObj.SetActive(false);
        }

        public void Refresh()
        {
            mTabUIManager.Refresh();
        }

        public void PlayQuestItemPlayMotion()
        {

        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }
    }
}