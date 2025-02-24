using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace Lance
{
    class TabNavBarUIManager<T> where T : Enum
    {
        List<TabButtonUI> mTabButtonUIList;
        public void Init(GameObject go, Func<T, int> onButtonAction) 
        {
            mTabButtonUIList = new List<TabButtonUI>();

            Array tabs = Enum.GetValues(typeof(T));
            int lastIndex = tabs.Length - 1;
            int index = 0;

            foreach(T tab in tabs)
            {
                if (index == lastIndex)
                    break;

                GameObject tabButtonObj = go.FindGameObject($"{tab}");
                TabButtonUI tabButtonUI = tabButtonObj.GetOrAddComponent<TabButtonUI>();
                tabButtonUI.Init(tab, () =>
                {
                    int tabInt = onButtonAction.Invoke(tab);

                    RefreshActiveFrame(tabInt);
                });

                mTabButtonUIList.Add(tabButtonUI);

                index++;
            }
        }

        public List<TabButtonUI> GetTabButtonUIList()
        {
            return mTabButtonUIList;
        }

        public void Localize()
        {
            foreach (var tabButtonUI in mTabButtonUIList)
            {
                tabButtonUI.Localize();
            }
        }

        public void RefreshActiveFrame(T tab)
        {
            int targetTab = Convert.ToInt32(tab);

            foreach (var tabButtonUI in mTabButtonUIList)
            {
                tabButtonUI.SetActiveFrame(tabButtonUI.Tab == targetTab);
            }
        }

        public void SetActiveTab(T tab, bool isActive)
        {
            int targetTab = Convert.ToInt32(tab);

            foreach (var tabButtonUI in mTabButtonUIList)
            {
                if (tabButtonUI.Tab == targetTab)
                {
                    tabButtonUI.gameObject.SetActive(isActive);

                    break;
                }
            }
        }

        public void RefreshActiveFrame(int targetTab)
        {
            foreach (var tabButtonUI in mTabButtonUIList)
            {
                tabButtonUI.SetActiveFrame(tabButtonUI.Tab == targetTab);
            }
        }

        public void SetActiveRedDot(T tab, bool isActive)
        {
            int targetTab = Convert.ToInt32(tab);

            foreach (var tabButtonUI in mTabButtonUIList)
            {
                if (tabButtonUI.Tab == targetTab)
                {
                    tabButtonUI.SetActiveRedDot(isActive);

                    break;
                }
            }
        }
    }
}