using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_BuffUI : MonoBehaviour
    {
        List<LobbyBuffItemUI> mBuffItemUIList;
        public void Init()
        {
            mBuffItemUIList = new List<LobbyBuffItemUI>();

            int index = 0;
            foreach(BuffData data in Lance.GameData.BuffData.Values)
            {
                var buffItemObj = gameObject.FindGameObject($"Buff_{index + 1}");

                var buffItemUI = buffItemObj.GetOrAddComponent<LobbyBuffItemUI>();

                buffItemUI.Init(data);

                mBuffItemUIList.Add(buffItemUI);

                index++;
            }

            bool purchasedRemovedAD = Lance.Account.PackageShop.IsPurchasedRemoveAD() || Lance.IAPManager.IsPurchasedRemoveAd();

            OnUpdate(purchasedRemovedAD);
        }

        public void OnUpdate(bool purchasedRemovedAD)
        {
            foreach(var item in mBuffItemUIList)
            {
                item.OnUpdate(purchasedRemovedAD);
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void OnStartStage(StageData stageData)
        {
            SetActive(!stageData.type.IsJousting());
        }
    }

    class LobbyBuffItemUI : MonoBehaviour
    {
        BuffData mData;
        TextMeshProUGUI mTextBuffRemainTime;
        public void Init(BuffData buffData)
        {
            mData = buffData;

            Image imageBuffIcon = gameObject.FindComponent<Image>("Image_BuffIcon");
            imageBuffIcon.sprite = Lance.Atlas.GetBuffIcon(buffData.type);

            mTextBuffRemainTime = gameObject.FindComponent<TextMeshProUGUI>("Text_BuffRemainTime");
        }

        public void OnUpdate(bool purchasedRemovedAD)
        {
            if (Lance.Account.Buff.IsAlreadyActiveBuff(mData.id))
            {
                if (gameObject.activeSelf)
                {
                    if (purchasedRemovedAD)
                    {
                        if (mTextBuffRemainTime.gameObject.activeSelf)
                            mTextBuffRemainTime.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (mTextBuffRemainTime.gameObject.activeSelf == false)
                            mTextBuffRemainTime.gameObject.SetActive(true);

                        float remainTime = Lance.Account.Buff.GetBuffDurationTime(mData.id);
                        int remainMinute = Mathf.RoundToInt(remainTime / (float)TimeUtil.SecondsInMinute);

                        StringParam param = new StringParam("minute", remainMinute);

                        mTextBuffRemainTime.text = StringTableUtil.Get("UIString_TimeOfMinute", param);
                    }
                }
                else
                {
                    gameObject.SetActive(true);
                }
            }
            else
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
            }
        }
    }
}