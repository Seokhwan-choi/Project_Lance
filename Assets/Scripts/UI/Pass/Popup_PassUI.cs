using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class Popup_PassUI : PopupBase
    {
        PassTabUIManager mTabUIManager;
        public void Init()
        {
            SetUpCloseAction();

            Lance.Account.UpdatePassRewardValues();

            //mTabUIManager = new PassTabUIManager();
           // mTabUIManager.AsyncInit(gameObject);

            StartCoroutine(DelayedInit());
        }

        IEnumerator DelayedInit()
        {
            mTabUIManager = new PassTabUIManager();

            yield return mTabUIManager.AsyncInit(gameObject);
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            base.Close();

            mTabUIManager.OnRelease();

            mTabUIManager = null;
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefreshRedDots();
        }
    }
}