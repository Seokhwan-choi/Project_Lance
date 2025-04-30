using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_SkipBattleUI : MonoBehaviour
    {
        Button mButtonSkipBattle;
        GameObject mRedDot;
        public void Init()
        {
            mButtonSkipBattle = gameObject.FindComponent<Button>("Button_SkipBattle");
            mButtonSkipBattle.SetButtonAction(() =>
            {
                var popup = Lance.PopupManager.CreatePopup<Popup_SkipBattleUI>();

                popup.Init();
            });

            mRedDot = gameObject.FindGameObject("RedDot");

            Refresh();
        }

        public void Refresh()
        {
            int skipBattleStackedCount = Lance.Account.StageRecords.GetSkipBattleStackedCount();
            int price = DataUtil.GetSkipBattlePrice(skipBattleStackedCount);

            bool isFree = price == 0;

            mRedDot.SetActive(isFree);
        }

        public void RefreshContentsLockUI()
        {
            SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.Spawn_ArtifactTab) == false);
        }

        public void OnStatStage(StageData stageData)
        {
            SetActive(stageData.type.IsNormal() && ContentsLockUtil.IsLockContents(ContentsLockType.Spawn_ArtifactTab) == false);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}