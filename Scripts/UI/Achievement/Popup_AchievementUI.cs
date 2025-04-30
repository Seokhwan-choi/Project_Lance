using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

namespace Lance
{
    class Popup_AchievementUI : PopupBase
    {
        List<AchievementItemUI> mAchievementItemUIList;

        string mSelectedId;
        TextMeshProUGUI mTextSelectedName;
        TextMeshProUGUI mTextSelectedDesc;
        TextMeshProUGUI mTextSelectedOwnStatValue;

        Button mButtonEquip;
        Button mButtonReceive;
        GameObject mRedDot;
        public void Init()
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_Achievement"));

            var content = gameObject.FindGameObject("Content");
            content.AllChildObjectOff();

            mAchievementItemUIList = new List<AchievementItemUI>();

            foreach(var data in Lance.GameData.AchievementData.Values)
            {
                var uiObj = Util.InstantiateUI("AchievementItemUI", content.transform);

                var ui = uiObj.GetOrAddComponent<AchievementItemUI>();
                ui.Init(data.id, OnSelect);

                mAchievementItemUIList.Add(ui);
            }

            mTextSelectedName = gameObject.FindComponent<TextMeshProUGUI>("Text_Name");
            mTextSelectedDesc = gameObject.FindComponent<TextMeshProUGUI>("Text_Desc");
            mTextSelectedOwnStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_OwnValue");

            mButtonEquip = gameObject.FindComponent<Button>("Button_Equip");
            mButtonEquip.SetButtonAction(() =>
            {
                if (Lance.GameManager.EquipAchievement(mSelectedId))
                {
                    Refresh();
                }
            });

            mButtonReceive = gameObject.FindComponent<Button>("Button_Receive");
            mRedDot = mButtonReceive.gameObject.FindGameObject("RedDot");
            mButtonReceive.SetButtonAction(() =>
            {
                if (Lance.GameManager.ReceiveAchievement(mSelectedId))
                {
                    Refresh();
                }
            });

            var equippedAchievement = Lance.Account.Achievement.GetEquippedAchievement();
            if (equippedAchievement.IsValid())
            {
                OnSelect(equippedAchievement);
            }
            else
            {
                OnSelect(Lance.GameData.AchievementData.Keys.First());
            }
        }

        void OnSelect(string id)
        {
            if (mSelectedId == id)
                return;

            mSelectedId = id;

            Refresh();
        }

        void Refresh()
        {
            foreach (var ui in mAchievementItemUIList)
            {
                ui.SetSelected(mSelectedId == ui.Id);
                ui.Refresh();
            }

            mRedDot.SetActive(Lance.Account.Achievement.CanReceive(mSelectedId));

            mTextSelectedName.text = StringTableUtil.GetName(mSelectedId);

            var data = Lance.GameData.AchievementData.TryGet(mSelectedId);
            if (data != null)
            {
                if (data.hideDesc)
                {
                    if (Lance.Account.Achievement.IsCompleteAchievement(mSelectedId) == false)
                    {
                        mTextSelectedDesc.text = "???";
                    }
                    else
                    {
                        SetDesc();
                    }
                }
                else
                {
                    SetDesc();
                }

                void SetDesc()
                {
                    bool isQuest = data.quest.IsValid();

                    if (isQuest)
                    {
                        var questData = Lance.GameData.AchievementQuestData.TryGet(data.quest);

                        StringParam descParam = new StringParam("stackCount", Lance.Account.Achievement.GetStackedQuestCount(mSelectedId));

                        descParam.AddParam("requireCount", questData.requireCount);

                        mTextSelectedDesc.text = StringTableUtil.GetDesc(mSelectedId, descParam);
                    }
                    else
                    {
                        mTextSelectedDesc.text = StringTableUtil.GetDesc(mSelectedId);
                    }
                }

                if (Lance.Account.Achievement.IsReceivedAchievement(mSelectedId))
                {
                    mButtonReceive.gameObject.SetActive(false);
                    mButtonEquip.gameObject.SetActive(true);
                    mButtonEquip.SetActiveFrame(Lance.Account.Achievement.CanEquip(mSelectedId));
                }
                else
                {
                    mButtonEquip.gameObject.SetActive(false);
                    mButtonReceive.gameObject.SetActive(true);
                    mButtonReceive.SetActiveFrame(Lance.Account.Achievement.CanReceive(mSelectedId));
                }

                if (data.rewardStat.IsValid())
                {
                    var ownStatData = Lance.GameData.AchievementStatData.TryGet(data.rewardStat);
                    if (ownStatData != null)
                    {
                        string statName = StringTableUtil.Get($"Name_{ownStatData.valueType}");
                        string statValue = ownStatData.valueType.IsPercentType() ? $"{ownStatData.statValue * 100f:F2}%" :
                            ownStatData.valueType.IsNoneAlphaType() ? $"{ownStatData.statValue:F2}" : $"{ownStatData.statValue.ToAlphaString()}";

                        mTextSelectedOwnStatValue.text = $"{statName} {statValue}";
                    }
                }
                else
                {
                    mTextSelectedOwnStatValue.text = string.Empty;
                }
            }
        }
    }

    class AchievementItemUI : MonoBehaviour
    {
        string mId;
        Image mImageFrame;
        Image mImageSelected;
        GameObject mEquippedObj;
        Image mImageModal;
        GameObject mRedDot;
        public string Id => mId;
        public void Init(string id, Action<string> onSelect)
        {
            mId = id;

            mImageFrame = gameObject.FindComponent<Image>("Image_Frame");
            var buttonOnSelect = mImageFrame.GetComponent<Button>();
            buttonOnSelect.SetButtonAction(() => onSelect(id));

            var imageFrameIcon = gameObject.FindComponent<Image>("Image_FrameIcon");
            var achievementData = Lance.GameData.AchievementData.TryGet(id);
            imageFrameIcon.sprite = Lance.Atlas.GetUISprite(achievementData.uiSprite);

            mImageSelected = gameObject.FindComponent<Image>("Image_Selected");
            mEquippedObj = gameObject.FindGameObject("Equipped");

            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
            mRedDot = gameObject.FindGameObject("RedDot");

            Refresh();
        }

        public void SetSelected(bool selected)
        {
            mImageSelected.gameObject.SetActive(selected);
            mImageFrame.sprite = Lance.Atlas.GetUISprite(selected ? "Frame_Costume_Active" : "Frame_Costume_Inactive");
        }

        public void Refresh()
        {
            mImageModal.gameObject.SetActive(Lance.Account.Achievement.IsCompleteAchievement(mId) == false);
            mEquippedObj.SetActive(Lance.Account.Achievement.IsEquippedAchievement(mId));
            mRedDot.SetActive(Lance.Account.Achievement.CanReceive(mId));
        }
    }
}