using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class TabButtonUI : MonoBehaviour 
    {
        int mTab;
        string mTabNameIndex;
        Image mImageFrame;
        TextMeshProUGUI mTextName;
        GameObject mLockObj;
        GameObject mRedDotObj;
        public int Tab => mTab;
        public void Init<T>(T tab, Action onButtonAction) where T : Enum
        {
            mTab = Convert.ToInt32(tab);

            mImageFrame = gameObject.FindComponent<Image>("Image_Frame");

            mTabNameIndex = $"{tab}";

            mTextName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            mTextName.text = StringTableUtil.Get($"TabName_{mTabNameIndex}");
            mLockObj = gameObject.FindGameObject("Image_Lock");
            mRedDotObj = gameObject.FindGameObject("RedDot");

            Button button = gameObject.GetComponentInChildren<Button>();
            button?.SetButtonAction(() =>
            {
                onButtonAction.Invoke();
            });

            SetActiveRedDot(false);
        }

        public void Localize()
        {
            mTextName.text = StringTableUtil.Get($"TabName_{mTabNameIndex}");
        }

        public void SetActiveFrame(bool isActive)
        {
            string prefix = isActive ? "Active" : "Inactive";
            string color = isActive ? "#FFFFFF" : "#6C6160";

            Color newColor;

            ColorUtility.TryParseHtmlString(color, out newColor);

            mImageFrame.sprite = Lance.Atlas.GetUISprite($"Lobby_Bottom_Tab_Button_{prefix}");
            mTextName.color = newColor;
        }

        public void SetLockButton(bool isLock)
        {
            mLockObj.SetActive(isLock);
        }

        public void SetActiveRedDot(bool isActive)
        {
            mRedDotObj.SetActive(isActive);
        }
    }
}