using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Lance
{
    class CostumeTabNavBarUI : MonoBehaviour
    {
        Action<CostumeTab> mOnTabButton;
        List<CostumeTabButtonUI> mTabButtonUI;
        public void Init(Action<CostumeTab> onTabButton)
        {
            mOnTabButton = onTabButton;
            mTabButtonUI = new List<CostumeTabButtonUI>();

            for (int i = 0; i < (int)CostumeTab.Count; ++i)
            {
                CostumeTab tab = (CostumeTab)i;

                var tabButtonObj = gameObject.FindGameObject($"{tab}");

                var tabButtonUI = tabButtonObj.GetOrAddComponent<CostumeTabButtonUI>();

                tabButtonUI.Init(tab, () =>
                {
                    OnTabButton(tab);
                });

                mTabButtonUI.Add(tabButtonUI);
            }
        }

        void OnTabButton(CostumeTab tab)
        {
            mOnTabButton?.Invoke(tab);

            RefreshActiveFrame(tab);
        }

        public void RefreshRedDots()
        {
            foreach(var tabButtonUI in mTabButtonUI)
            {
                tabButtonUI.RefreshRedDot();
            }
        }

        public void RefreshActiveFrame(CostumeTab tab)
        {
            foreach (var tabButtonUI in mTabButtonUI)
            {
                tabButtonUI.RefreshActiveFrame(tabButtonUI.Tab == tab);
            }
        }

        public void SetActiveRedDots(bool active)
        {
            foreach (var tabButtonUI in mTabButtonUI)
            {
                tabButtonUI.SetActiveRedDot(active);
            }
        }
    }

    class CostumeTabButtonUI : MonoBehaviour
    {
        CostumeTab mTab;
        Image mImageFrame;
        GameObject mRedDotObj;
        public CostumeTab Tab => mTab;
        public void Init(CostumeTab tab, Action onTabButton)
        {
            mTab = tab;

            var button = GetComponent<Button>();
            button.SetButtonAction(() => onTabButton.Invoke());

            mImageFrame = GetComponent<Image>();

            mRedDotObj = gameObject.FindGameObject("RedDot");

            RefreshRedDot();
        }

        public void RefreshActiveFrame(bool isActive)
        {
            var orgColor = mImageFrame.color;

            var newColor = new Color(orgColor.r, orgColor.g, orgColor.b, isActive ? 1f : 0f);

            mImageFrame.color = newColor;
        }

        public void RefreshRedDot()
        {
            SetActiveRedDot(RedDotUtil.IsActiveRedDotByCostumeTab(mTab));
        }

        public void SetActiveRedDot(bool active)
        {
            mRedDotObj.SetActive(active);
        }
    }
}