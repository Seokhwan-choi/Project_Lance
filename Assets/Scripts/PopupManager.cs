using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class PopupManager
    {
        Transform mParent;
        List<PopupBase> mPopupList;
        public int Count => mPopupList.Count;
        public Transform Parent => mParent;
        public void Init()
        {
            mParent = GameObject.Find("PopupList").transform;

            mPopupList = new List<PopupBase>();
        }

        public T CreatePopup<T>(string name = null, bool showMotion = true) where T : PopupBase
        {
            if (name.IsValid() == false)
                name = typeof(T).Name;

            // »ý¼º
            T popup = Util.InstantiateUI($"Popup/{name}", mParent).GetOrAddComponent<T>();

            popup.SetUp(this);

            mPopupList.Add(popup);

            if (showMotion)
                popup.PlayShowMotion();

            return popup;
        }

        public void RemovePopup(PopupBase popup)
        {
            mPopupList.Remove(popup);
        }

        public T GetPopup<T>() where T : PopupBase
        {
            for (int i = mPopupList.Count - 1; i >= 0; i--)
            {
                T popup = mPopupList[i] as T;
                if (popup != null)
                    return popup;
            }

            return null;
        }

        public void OnBackButton(bool immediate = false)
        {
            for (int i = mPopupList.Count - 1; i >= 0; i--)
            {
                PopupBase popup = mPopupList[i];
                if (popup.IsVisible)
                {
                    popup.OnBackButton(immediate: immediate);
                    return;
                }
            }
        }
    }
}