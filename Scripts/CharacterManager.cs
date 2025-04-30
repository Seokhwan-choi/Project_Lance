using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;


namespace Lance
{
    class CharacterManager
    {
        Player mPlayer;
        List<Monster> mMonsterList;
        public Player Player => mPlayer;
        public void Init()
        {
            mMonsterList = new List<Monster>();
        }

        public void OnUpdate(float dt)
        {
            foreach (var monster in mMonsterList)
            {
                // 플레이어가 움직일 때, 몬스터들이 플레이어 쪽으로 다가온다.
                if (mPlayer.IsMoving)
                {
                    // 즉, 플레이어의 이동속도로 몬스터들이 다가오면 된다.
                    monster.Physics.MoveTo(dt, mPlayer.GetCurMoveSpeed());
                }

                monster.OnUpdate(dt);
            }

            mPlayer?.OnUpdate(dt);
        }

        public void OnLateUpdate()
        {
            //foreach (var monster in mMonsterList)
            //{
            //    monster.OnLateUpdate();
            //}

            //mPlayer?.OnLateUpdate();
        }

        public bool IsAllMonsterDeath()
        {
            foreach(var monster in mMonsterList)
            {
                if (monster.IsAlive)
                    return false;
            }

            return true;
        }

        public void ClearAllCharacters()
        {
            if (mPlayer != null)
            {
                mPlayer.OnRelease();

                Lance.ObjectPool.ReleaseObject(mPlayer.gameObject);
            }

            ClearAllMonsters();
        }

        public void ClearAllMonsters()
        {
            foreach (var monster in mMonsterList)
            {
                if (monster != null)
                {
                    monster.OnRelease();

                    Lance.ObjectPool.ReleaseObject(monster.gameObject);
                }
            }

            mMonsterList.Clear();
        }

        public Boss GetBoss()
        {
            if (mMonsterList != null && mMonsterList.Count > 0)
            {
                foreach(var monster in mMonsterList)
                {
                    if (monster is Boss)
                        return monster as Boss;
                }
            }

            return null;
        }

        public JoustOpponent FindJoustOpponent()
        {
            if (mMonsterList != null && mMonsterList.Count > 0)
            {
                foreach (var monster in mMonsterList)
                {
                    if (monster is JoustOpponent joustOpponent)
                        return joustOpponent;
                }
            }

            return null;
        }

        public Monster FindFrontMostMonster()
        {
            float frontMostPosX = 0;
            Monster frontMostMonster = null;

            if (mMonsterList != null && mMonsterList.Count > 0)
            {
                foreach (var monster in mMonsterList)
                {
                    if (monster.IsDeath) // 죽은 몬스터라면 무시
                        continue;

                    float newPosX = monster.GetPosition().x;

                    if ( frontMostPosX > newPosX )
                    {
                        frontMostPosX = newPosX;
                        frontMostMonster = monster;
                    }
                }
            }

            return frontMostMonster;
        }

        public IEnumerable<Monster> GetAllMonsters()
        {
            if (mMonsterList != null && mMonsterList.Count > 0)
            {
                foreach (var monster in mMonsterList)
                {
                    if (monster.IsDeath)
                        continue;

                    yield return monster;
                }    
            }
        }

        public IEnumerable<Character> GetOpponents(bool isPlayer)
        {
            if (isPlayer)
            {
                return mMonsterList;
            }
            else
            {
                return Enumerable.Repeat(mPlayer, 1);
            }
        }

        public void OnStartStage(StageData stageData)
        {
            ClearAllCharacters();
            SpawnPlayer();
            SpawnMonsters(stageData);
        }

        public void OnStartJousting(string[] opponentCostumes)
        {
            ClearAllCharacters();
            // 마상 전투 전용 플레이어
            SpawnJoustPlayer();
            // 마상 전투 전용 몬스터 ( 상대 플레이어 )
            SpawnJoustOpponent(opponentCostumes);
        }

        void SpawnJoustPlayer()
        {
            var pcObj = Lance.ObjectPool.AcquireObject("JoustPlayer", Lance.GameManager.CharacterParent);
            mPlayer = pcObj.GetOrAddComponent<JoustPlayer>();
            mPlayer.Init();
            mPlayer.UpdateCostumes(Lance.Account.Costume.GetEquippedCostumeIds(), isJousting:true);
            
            Lance.CameraManager.SetFollowInfo(new FollowInfo()
            {
                FollowType = FollowType.None,
                FollowTm = pcObj.transform
            });
        }

        void SpawnJoustOpponent(string[] opponentCostumes)
        {
            var opponentObj = Lance.ObjectPool.AcquireObject("JoustOpponent", Lance.GameManager.CharacterParent);
            JoustOpponent opponent = opponentObj.GetOrAddComponent<JoustOpponent>();
            opponent.Init();
            opponent.UpdateCostumes(opponentCostumes);

            mMonsterList.Add(opponent);
        }

        void SpawnPlayer()
        {
            // PC 생성
            var pcObj = Lance.ObjectPool.AcquireObject("Player", Lance.GameManager.CharacterParent);
            mPlayer = pcObj.GetOrAddComponent<Player>();
            mPlayer.Init();
            mPlayer.UpdateCostumes(Lance.Account.Costume.GetEquippedCostumeIds(), isJousting:false);
            mPlayer.ResetSkillCoolTime();

            Lance.CameraManager.SetFollowInfo(new FollowInfo()
            {
                FollowType = FollowType.None,
                FollowTm = pcObj.transform
            });
        }

        void SpawnMonsters(StageData stageData)
        {
            MonsterData monsterData = DataUtil.GetRandomStageMonster(stageData);
            if (monsterData != null)
            {
                SpawnMonsters(monsterData, stageData);
            }
        }

        public void SpawnMonsters(MonsterData data, StageData stageData)
        {
            string monsterPrefab = data.prefab.IsValid() ? data.prefab : "Monster";
            string reward = stageData.monsterDropReward;
            int level = stageData.monsterLevel;
            int monsterCount = stageData.atOnceSpawn ? stageData.monsterLimitCount : 
                stageData.type == StageType.Growth ? 
                Lance.GameData.StageCommonData.growthDungeonSpawnMonsterCount : 
                stageData.type == StageType.Accessory ?
                Lance.GameData.StageCommonData.demonicRealmAccessorySpawnMonsterCount :
                Lance.GameData.StageCommonData.spawnMonsterCount;

            bool spawnTreasureGoblin = false;
            if (stageData.type.IsNormal())
                spawnTreasureGoblin = Util.Dice(Lance.GameData.StageCommonData.treasureGoblinProb);

            for (int i = 0; i <= monsterCount; ++i)
            {
                if (i == monsterCount && spawnTreasureGoblin)
                {
                    MonsterData treasureGoblinData = Lance.GameData.MonsterData.TryGet(Lance.GameData.StageCommonData.treasureGoblin);

                    SpawnMonster("Monster", treasureGoblinData, i, Lance.GameData.StageCommonData.treasureGoblinRewardBonusValue);
                }
                else if (i != monsterCount)
                {
                    SpawnMonster(monsterPrefab, data, i);
                }
            }

            void SpawnMonster(string monsterPrefab, MonsterData data, int index, float rewardBonusValue = 1f)
            {
                GameObject monsterObj = Lance.ObjectPool.AcquireObject(monsterPrefab, Lance.GameManager.CharacterParent);

                Monster monster = monsterObj.GetOrAddComponent<Monster>();
                monster.SetMonsterInfo(data, stageData.type, stageData.diff, stageData.chapter, stageData.stage, level + (stageData.increaseLevel * index), reward, rewardBonusValue);

                float bodySize = Mathf.Max(0.5f, monster.GetBodySize());

                string[] posStr = data.pos.SplitByDelim();
                if (posStr.Length > 1)
                {
                    float x, y;
                    float.TryParse(posStr[0], out x);
                    float.TryParse(posStr[1], out y);

                    //data.bodySize
                    monster.SetPosition(new Vector2(x + (index * bodySize), y));
                }

                monster.CreateHpGaugeUI();

                mMonsterList.Add(monster);
            }
        }

        public Boss SpawnBoss(MonsterData data, StageType stageType, StageDifficulty diff, int chapter, int stage, string reward, int level)
        {
            string bossPrefab = data.prefab.IsValid() ? data.prefab : "Boss";

            var bossObj = Lance.ObjectPool.AcquireObject(bossPrefab, Lance.GameManager.CharacterParent);
            Boss boss = data.type == MonsterType.boss ? bossObj.GetOrAddComponent<Boss>() :
                data.type == MonsterType.raidBoss ? bossObj.GetOrAddComponent<RaidBoss>() :
                data.type == MonsterType.ancientBoss ? bossObj.GetOrAddComponent<AncientBoss>() :
                bossObj.GetOrAddComponent<PetBoss>();


            boss.SetMonsterInfo(data, stageType, diff, chapter, stage, level, reward);

            Vector2 pos = new Vector2();

            if (data.pos.IsValid())
            {
                string[] posStr = data.pos.SplitByDelim();
                if (posStr.Length > 1)
                {
                    float x, y;
                    float.TryParse(posStr[0], out x);
                    float.TryParse(posStr[1], out y);

                    pos = new Vector2(x, y);
                }
            }

            boss.SetPosition(pos);

            mMonsterList.Add(boss);

            return boss;
        }
    }
}