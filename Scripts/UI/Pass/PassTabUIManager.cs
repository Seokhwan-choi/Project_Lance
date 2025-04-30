using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;

namespace Lance
{
    enum PassTab
    {
        Level,
        Stage,
        Spawn,
        PlayTime,

        Count
    }

    class PassTabUIManager
    {
        PassTab mCurTab;
        TabNavBarUIManager<PassTab> mNavBarUI;

        GameObject mGameObject;
        Image mImagePassTitle;
        Image mImageCharacter;
        TextMeshProUGUI mTextDesc1;
        List<PassTabUI> mPassTabUIList;

        public IEnumerator AsyncInit(GameObject go)
        {
            mGameObject = go;
            mImagePassTitle = go.FindComponent<Image>("Image_Pass_Title");
            mImagePassTitle.gameObject.SetActive(false);
            mImageCharacter = go.FindComponent<Image>("Image_Banner_Character");
            mTextDesc1 = go.FindComponent<TextMeshProUGUI>("Text_Desc_1");

            mNavBarUI = new TabNavBarUIManager<PassTab>();
            mNavBarUI.Init(go.FindGameObject("NavBar"), OnChangeTabButton);
            mNavBarUI.RefreshActiveFrame(PassTab.Level);

            var loading = go.FindGameObject("Loading");
            loading.SetActive(true);

            mPassTabUIList = new List<PassTabUI>();

            yield return InitPassTab<Pass_LevelTabUI>(PassTab.Level);

            yield return InitPassTab<Pass_StageTabUI>(PassTab.Stage);

            yield return InitPassTab<Pass_SpawnTabUI>(PassTab.Spawn);

            yield return InitPassTab<Pass_PlayTimeTabUI>(PassTab.PlayTime);

            yield return new WaitForSeconds(0.2f);

            RefreshRedDots();

            ShowTab(PassTab.Level);

            loading.SetActive(false);
        }

        public void OnUpdate()
        {
            // 현재 탭만 
            foreach (PassTabUI tab in mPassTabUIList)
            {
                if (mCurTab == tab.Tab)
                {
                    tab.OnUpdate();
                }
            }
        }

        public void OnRelease()
        {
            foreach(var passTab in mPassTabUIList)
            {
                passTab.OnRelease();
            }
        }

        IEnumerator InitPassTab<T>(PassTab tab) where T : PassTabUI
        {
            GameObject tabObj = mGameObject.Find($"Tab_{tab}", true);

            Debug.Assert(tabObj != null, $"{tab}의 PassTabUI가 없다.");

            T tabUI = tabObj.GetOrAddComponent<T>();

            tabObj.SetActive(true);

            yield return null;

            tabUI.Init(this, tab);

            tabUI.SetVisible(false);

            mPassTabUIList.Add(tabUI);

            yield return new WaitForSeconds(0.2f);
        }

        public T GetTab<T>() where T : PassTabUI
        {
            return mPassTabUIList.FirstOrDefault(x => x is T) as T;
        }

        public void Refresh()
        {
            var curTab = mPassTabUIList[(int)mCurTab];

            curTab.Refresh();
        }

        public void Localize()
        {
            //foreach (var tab in mLobbyTabUIList)
            //{
            //    tab.Localize();
            //}
        }

        int OnChangeTabButton(PassTab tab)
        {
            ChangeTab(tab);

            return (int)mCurTab;
        }

        public void ChangeTab(string tabName)
        {
            if (Enum.TryParse(tabName, out PassTab result))
            {
                ChangeTab(result);
            }
        }

        public bool ChangeTab(PassTab tab)
        {
            if (mCurTab == tab)
                return false;

            HideTab(mCurTab);

            mCurTab = tab;

            ShowTab(mCurTab);

            return true;
        }

        void ShowTab(PassTab tab)
        {
            PassTabUI showTab = mPassTabUIList[(int)tab];

            showTab.SetVisible(true);

            showTab.OnEnter();

            ChangeBanner(mCurTab.ChangeToPassType());
        }

        void ChangeBanner(PassType passType)
        {
            string passTitle = Lance.LocalSave.LangCode == LangCode.KR ? $"Pass_Title_{passType}" : $"Pass_Title_{passType}_Eng";

            mImagePassTitle.gameObject.SetActive(true);
            mImagePassTitle.sprite = Lance.Atlas.GetUISprite(passTitle);

            mImageCharacter.sprite = Lance.Atlas.GetUISprite($"Image_Pass_Title_{passType}");
            mTextDesc1.text = StringTableUtil.GetDesc($"Pass_{passType}");
        }

        void HideTab(PassTab tab)
        {
            PassTabUI hideTab = mPassTabUIList[(int)tab];

            hideTab.OnLeave();

            hideTab.SetVisible(false);
        }

        public void RefreshRedDots()
        {
            for(int i = 0; i < (int)PassTab.Count; ++i)
            {
                PassTab tab = (PassTab)i;
                PassType type = tab.ChangeToPassType();

                mNavBarUI.SetActiveRedDot(tab, Lance.Account.Pass.AnyCanReceiveReward(type));
            }

            foreach(var tab in mPassTabUIList)
            {
                tab.RefreshRedDots();
            }
        }
    }

    static class PassTabExt
    {
        public static PassType ChangeToPassType(this PassTab tab)
        {
            switch(tab)
            {
                case PassTab.Level:
                    return PassType.Level;
                case PassTab.Stage:
                    return PassType.Stage;
                case PassTab.Spawn:
                    return PassType.Spawn;
                case PassTab.PlayTime:
                default:
                    return PassType.PlayTime;
            }
        }
    }
}
