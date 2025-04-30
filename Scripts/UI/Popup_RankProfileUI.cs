using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

namespace Lance
{
    class Popup_RankProfileUI : PopupBase
    {
        public void Init(string nickname, ulong index, string tag)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_UserInfo"));

            var buttonReport = gameObject.FindComponent<Button>("Button_Report");
            buttonReport.SetButtonAction(() =>
            {
                string title = StringTableUtil.Get("Title_Confirm");
                string desc = StringTableUtil.Get("Desc_ConfirmReport");

                UIUtil.ShowConfirmPopup(title, desc, () => { Lance.BackEnd.OnSendReport(index, tag);  }, null);
            });

            var textNickname = gameObject.FindComponent<TextMeshProUGUI>("Text_Nickname");
            textNickname.text = nickname;

            var loadingObj = gameObject.FindGameObject("Loading");
            loadingObj.SetActive(true);

            var preview = gameObject.FindComponent<RawImage>("Preview");

            Lance.GameManager.SetActiveRankProfilePreview(true);

            preview.texture = Lance.GameManager.GetRankProfilePreviewRenderTexture();

            Lance.GameManager.GetRankProfile(nickname, (isSuccess, rankProfile) =>
            {
                loadingObj.SetActive(false);

                if (isSuccess)
                {
                    if (rankProfile != null)
                    {
                        // ±¹°¡
                        var imageCountry = gameObject.FindComponent<Image>("Image_Country");
                        imageCountry.sprite = Lance.Atlas.GetCountry(rankProfile.GetCountryCode());

                        var textLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_LevelValue");
                        textLevel.text = $"{rankProfile.GetLevel()}";

                        var textPowerLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_PowerLevel");
                        textPowerLevel.text = rankProfile.GetPowerLevel().ToAlphaString();

                        var content = gameObject.FindGameObject("Content");

                        content.AllChildObjectOff();

                        var statInfoList = new List<RankProfile_StatInfoUI>();

                        // ½ºÅÈ
                        foreach (var statInfo in rankProfile.GetStats())
                        {
                            if (statInfo.GetStatType().IsShowingUserInfo())
                            {
                                var statInfoUIObj = Util.InstantiateUI("UserStatInfoUI", content.transform);

                                var statInfoUI = statInfoUIObj.GetOrAddComponent<RankProfile_StatInfoUI>();

                                statInfoUI.Init(statInfo.GetStatType(), statInfo.GetStatValue());
                            }
                        }

                        // Àåºñ
                        foreach (var equipment in rankProfile.GetEquipments())
                        {
                            var data = DataUtil.GetEquipmentData(equipment.GetId());

                            var equippedItemObj = gameObject.FindGameObject($"EquippedItem_{data.type}");

                            var equippedItemUI = equippedItemObj.GetOrAddComponent<RankProfile_EquipmentEquippedItemUI>();
                            equippedItemUI.Init(equipment);
                        }

                        // ½Å¼ö
                        var petInfo = rankProfile.GetPetInfo();
                        if (petInfo != null)
                        {
                            var equippedPetItemObj = gameObject.FindGameObject("EquippedItem_Pet");

                            var petItemUI = equippedPetItemObj.GetOrAddComponent<RankProfile_EquippedPetItemUI>();
                            petItemUI.Init(petInfo);
                        }

                        // Àå½Å±¸

                        Dictionary<ItemType, int> slotIndexs = new Dictionary<ItemType, int>();

                        foreach (var accessory in rankProfile.GetAccessoryInfos())
                        {
                            var data = DataUtil.GetAccessoryData(accessory.GetId());

                            if (slotIndexs.ContainsKey(data.type))
                            {
                                slotIndexs[data.type]++;
                            }
                            else
                            {
                                slotIndexs.Add(data.type, 0);
                            }

                            int slotIndex = slotIndexs.TryGet(data.type);

                            var equippedItemObj = gameObject.FindGameObject($"EquippedItem_{data.type}_{slotIndex}");

                            var equippedItemUI = equippedItemObj.GetOrAddComponent<RankProfile_AccessoryEquippedItemUI>();
                            equippedItemUI.Init(accessory);
                        }

                        // ÄÚ½ºÆ¬
                        var costumes = rankProfile.GetCostumes();
                        if (costumes != null)
                        {
                            
                            Lance.GameManager.RefreshRankProfilePreview(costumes);

                            
                        }

                        // ·©Å·
                        var rankInfosObj = gameObject.FindGameObject("RankInfos");
                        // ·¹º§ ·©Å·
                        var levelRankInfoObj = rankInfosObj.FindGameObject("Level");
                        var levelRankInfo = levelRankInfoObj.GetOrAddComponent<RankProfile_RankInfoUI>();
                        levelRankInfo.Init(rankProfile.GetLevelRank());
                        // ÀüÅõ·Â ·©Å·
                        var powerLevelRankInfoObj = rankInfosObj.FindGameObject("PowerLevel");
                        var powerLevelRankInfo = powerLevelRankInfoObj.GetOrAddComponent<RankProfile_RankInfoUI>();
                        powerLevelRankInfo.Init(rankProfile.GetPowerLevelRank());
                        // ½ºÅ×ÀÌÁö ·©Å·
                        var stageRankInfoObj = rankInfosObj.FindGameObject("Stage");
                        var stageRankInfo = stageRankInfoObj.GetOrAddComponent<RankProfile_RankInfoUI>();
                        stageRankInfo.Init(rankProfile.GetStageRank());
                    }
                }
            });
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            base.Close(immediate, hideMotion);

            Lance.GameManager.SetActiveRankProfilePreview(false);
        }
    }

    class RankProfile_RankInfoUI : MonoBehaviour
    {
        public void Init(int rank)
        {
            var top3Obj = gameObject.FindGameObject("Top3");
            var otherRankObj = gameObject.FindGameObject("OtherRank");

            if (rank <= 3 && rank > 0)
            {
                top3Obj.SetActive(true);
                otherRankObj.SetActive(false);

                var imageTop3 = top3Obj.GetComponent<Image>();
                imageTop3.sprite = Lance.Atlas.GetRankIcon(rank);
            }
            else
            {
                top3Obj.SetActive(false);
                otherRankObj.SetActive(true);

                StringParam param = new StringParam("rank", rank);

                var textRank = otherRankObj.FindComponent<TextMeshProUGUI>("Text_Rank");
                textRank.text = rank != 0 ? StringTableUtil.Get("UIString_Rank", param) : StringTableUtil.Get("UIString_NoneRank");
            }
        }
    }

    class RankProfile_StatInfoUI : MonoBehaviour
    {
        public void Init(StatType statType, double statValue)
        {
            if (statType == StatType.AtkSpeed)
                statValue = Math.Min(Lance.GameData.PlayerStatMaxData.atkSpeedMax, statValue);
            else if (statType == StatType.MoveSpeed)
                statValue = Math.Min(Lance.GameData.PlayerStatMaxData.moveSpeedMax, statValue);

            TextMeshProUGUI textStatName = gameObject.FindComponent<TextMeshProUGUI>("Text_StatName");
            textStatName.text = StringTableUtil.Get($"Name_{statType}");

            TextMeshProUGUI textStatValue = gameObject.FindComponent<TextMeshProUGUI>("Text_StatValue");
            textStatValue.text = statType.IsPercentType() ? $"{statValue * 100f:F2}%" :
                statType.IsNoneAlphaType() ? $"{statValue:F2}" : $"{statValue.ToAlphaString(showDp: ((statType == StatType.Atk || statType == StatType.Hp) ? ShowDecimalPoint.GoldTrain : ShowDecimalPoint.Default))}";
        }
    }

    class RankProfile_EquippedPetItemUI : MonoBehaviour
    {
        public void Init(RankProfile_PetInfo petInfo)
        {
            if (petInfo != null)
            {
                var emptyObj = gameObject.FindGameObject("Empty");
                emptyObj.SetActive(false);

                var itemObj = gameObject.FindGameObject("PetItemUI");
                itemObj.SetActive(true);

                var itemUI = itemObj.GetOrAddComponent<RankProfile_PetItemUI>();
                itemUI.Init(petInfo.GetId(), petInfo.GetLevel(), petInfo.GetStep());
            }
        }
    }

    class RankProfile_AccessoryEquippedItemUI : MonoBehaviour
    {
        public void Init(RankProfile_AccessoryInfo accessoryInfo)
        {
            if (accessoryInfo != null)
            {
                var emptyObj = gameObject.FindGameObject("Empty");
                emptyObj.gameObject.SetActive(false);

                var itemObj = gameObject.FindGameObject("AccessoryItemUI");
                itemObj.SetActive(true);

                var accessoryItemUI = itemObj.GetOrAddComponent<RankProfile_AccessoryItemUI>();
                accessoryItemUI.Init(accessoryInfo.GetId(), accessoryInfo.GetLevel(), accessoryInfo.GetReforgeStep());
            }
        }
    }

    class RankProfile_EquipmentEquippedItemUI : MonoBehaviour
    {
        public void Init(RankProfile_EquipmentInfo equipmentInfo)
        {
            if (equipmentInfo != null)
            {
                var emptyObj = gameObject.FindGameObject("Empty");
                emptyObj.gameObject.SetActive(false);

                var itemObj = gameObject.FindGameObject("EquipmentItemUI");
                itemObj.SetActive(true);

                var equipmentItemUI = itemObj.GetOrAddComponent<RankProfile_EquipmentItemUI>();
                equipmentItemUI.Init(equipmentInfo.GetId(), equipmentInfo.GetLevel(), equipmentInfo.GetReforgeStep());
            }
        }
    }
}