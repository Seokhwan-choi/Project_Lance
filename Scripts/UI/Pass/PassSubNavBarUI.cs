using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using DG.Tweening;

namespace Lance
{
    class PassSubNavBarUI
    {
        GameObject mNavBarObj;
        Image mImageButtonFrame;
        PassSubTabUIManager mParent;

        List<SubTabNavBarButton> mNavBarButtonList;
        public void Init(PassSubTabUIManager parent)
        {
            mParent = parent;

            mNavBarObj = mParent.Parent.gameObject.FindGameObject("SubTabNavBarButtonList");
            mNavBarObj.AllChildObjectOff();

            mImageButtonFrame = mParent.Parent.gameObject.FindComponent<Image>("Image_NavBarButtonFrame");

            mNavBarButtonList = new List<SubTabNavBarButton>();

            PassType passType = mParent.Tab.ChangeToPassType();

            var datas = DataUtil.GetPassDatas(passType).ToArray();
            for(int i = 0; i < datas.Length; ++i)
            {
                int index = i;
                PassData data = datas[i];
                string id = data.id;

                GameObject subTabNavObj = Lance.ObjectPool.AcquireUI("PassSubTabNavButton", mNavBarObj.GetComponent<RectTransform>());

                SubTabNavBarButton navBarButton = subTabNavObj.GetOrAddComponent<SubTabNavBarButton>();
                navBarButton.Init(id, index, OnChangeSubTab);

                mNavBarButtonList.Add(navBarButton);
            }

            var rectTm = mNavBarObj.GetComponent<RectTransform>();
            if ( rectTm != null )
            {
                var horizontalLayoutGroup = rectTm.GetComponent<HorizontalLayoutGroup>();

                float spacing = horizontalLayoutGroup.spacing;
                float totalWidth = rectTm.rect.width - (spacing * (datas.Length - 1));
                float subTabButtonWidth = totalWidth / datas.Length;

                var frameRectTm = mImageButtonFrame.GetComponent<RectTransform>();
                frameRectTm.sizeDelta = new Vector2(subTabButtonWidth, frameRectTm.sizeDelta.y);
            }
        }

        public void OnRelease()
        {
            foreach(var navBar in mNavBarButtonList)
            {
                navBar.OnRelease();

                Lance.ObjectPool.ReleaseUI(navBar.gameObject);
            }

            mNavBarButtonList = null;
        }

        void OnChangeSubTab(string id)
        {
            mParent.ChangeSubTab(id);

            Refresh();
        }

        public void Refresh()
        {
            var targetRectTm = GetNavBarButton(mParent.SelectedId).GetComponent<RectTransform>();

            mImageButtonFrame.rectTransform
                        .DOAnchorPosX(targetRectTm.anchoredPosition.x, 0.25f)
                        .SetAutoKill(false);
        }

        SubTabNavBarButton GetNavBarButton(string id)
        {
            foreach(SubTabNavBarButton navBarButton in mNavBarButtonList)
            {
                if (id == navBarButton.Id)
                    return navBarButton;
            }

            return null;
        }

        public void RefreshRedDots()
        {
            foreach(var navBarButton in mNavBarButtonList)
            {
                navBarButton.RefreshRedDot();
            }
        }
    }

    class SubTabNavBarButton : MonoBehaviour
    {
        GameObject mRedDotObj;
        string mId;
        public string Id => mId;
        public void Init(string id, int index, Action<string> onButton)
        {
            mId = id;
            var button = gameObject.GetComponent<Button>();
            button.SetButtonAction(() =>
            {
                onButton?.Invoke(id);
            });

            var textTab = gameObject.FindComponent<TextMeshProUGUI>("Text_Tab");
            textTab.text = $"{index + 1}";

            mRedDotObj = gameObject.FindGameObject("RedDot");
        }

        public void OnRelease()
        {
            mRedDotObj = null;
            mId = null;
        }

        public void RefreshRedDot()
        {
            mRedDotObj.SetActive(Lance.Account.Pass.AnyCanReceiveReward(mId));
        }
    }
}


