using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class Lobby_StageInfoUI : MonoBehaviour
    {
        Lobby_NormalStageInfoUI mNormalStageInfo;
        Lobby_DungeonStageInfoUI mDungeonStageInfo;
        Lobby_DemonicRealmStageInfoUI mDemonicRealmStageInfo;
        Lobby_JoustingStageInfoUI mJoustingStageInfo;
        public void Init()
        {
            var normalObj = gameObject.FindGameObject("Normal");
            mNormalStageInfo = normalObj.GetOrAddComponent<Lobby_NormalStageInfoUI>();
            mNormalStageInfo.Init();

            var dungeonObj = gameObject.FindGameObject("Dungeon");
            mDungeonStageInfo = dungeonObj.GetOrAddComponent<Lobby_DungeonStageInfoUI>();
            mDungeonStageInfo.Init();

            var demonicRealmObj = gameObject.FindGameObject("DemonicRealm");
            mDemonicRealmStageInfo = demonicRealmObj.GetOrAddComponent<Lobby_DemonicRealmStageInfoUI>();
            mDemonicRealmStageInfo.Init();

            var joustingObj = gameObject.FindGameObject("Jousting");
            mJoustingStageInfo = joustingObj.GetOrAddComponent<Lobby_JoustingStageInfoUI>();
            mJoustingStageInfo.Init();
        }

        public void Localize()
        {
            mNormalStageInfo.Localize();
        }

        public void Refresh()
        {
            mNormalStageInfo.Refresh();
            mDungeonStageInfo.Refresh();
            mDemonicRealmStageInfo.Refresh();
        }

        public void RefreshContentsLockUI()
        {
            mNormalStageInfo.RefreshContentsLockUI();
        }

        public void UpdateMonsterKillCount()
        {
            mNormalStageInfo.UpdateMonsterKillCount();
            mDungeonStageInfo.UpdateScore();
            mDemonicRealmStageInfo.UpdateScore();
        }

        public void UpdateBossHp()
        {
            mNormalStageInfo.UpdateBossHp();
            mDungeonStageInfo.UpdateScore();
            mDemonicRealmStageInfo.UpdateScore();
        }

        public void UpdateTimer()
        {
            mNormalStageInfo.UpdateBossTime();
            mDungeonStageInfo.UpdateTimer();
            mDemonicRealmStageInfo.UpdateTimer();
        }

        public void RefreshJoustingInfo(JoustBattleInfo myBattleInfo, JoustBattleInfo opponentBattleInfo)
        {
            mJoustingStageInfo.RefreshInfos(myBattleInfo, opponentBattleInfo);
        }

        public void UpdateJoustingDistance()
        {
            mJoustingStageInfo.UpdateDistance();
        }

        public void OnStartStage(StageData stageData)
        {
            bool isDungeon = stageData.type.IsDungeon();
            bool isDemonicRealm = stageData.type.IsDemonicRealm();
            bool isJousting = stageData.type.IsJousting();

            mNormalStageInfo.SetActive(!isDungeon && !isDemonicRealm && !isJousting);
            mDungeonStageInfo.SetActive(isDungeon);
            mDemonicRealmStageInfo.SetActive(isDemonicRealm);
            mJoustingStageInfo.SetActive(isJousting);

            if (isDungeon)
            {
                mDungeonStageInfo.Refresh();
            }
            else if (isDemonicRealm)
            {
                mDemonicRealmStageInfo.Refresh();
            }
            else
            {
                mNormalStageInfo.Refresh();
            }
        }
    }
}