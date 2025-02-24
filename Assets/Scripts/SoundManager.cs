using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace Lance
{
    class SoundManager : MonoBehaviour
    {
        const string DefaultLobbyBGM = "Lobby_Main_01";
        const int SECount = 30;
        const float DefaultmBgmDelay = 0.5f;

        Dictionary<string, AudioClip> mAudioList;

        public AudioSource mBGAudioPlayer;
        public AudioSource[] mSFXAudioPlayer;
        public void Init()
        {
            mAudioList = new Dictionary<string, AudioClip>();

            Object[] audioSource = Resources.LoadAll("Sounds");

            for (int i = 0; i < audioSource.Length; ++i)
            {
                mAudioList.Add(audioSource[i].name, audioSource[i] as AudioClip);
            }

            //오디오소스 컴포넌트 호출
            mBGAudioPlayer = gameObject.AddComponent<AudioSource>();
            mSFXAudioPlayer = new AudioSource[SECount];
            for (int i = 0; i < SECount; ++i)
            {
                mSFXAudioPlayer[i] = gameObject.AddComponent<AudioSource>();
            }

            PlayBg(DefaultLobbyBGM);

            ApplyLocalSave();
        }

        void ApplyLocalSave()
        {
            SetBGSoundActive(SaveBitFlags.BGMSound.IsOn());
            SetEffectSoundActive(SaveBitFlags.SFXSound.IsOn());
        }

        public void PlayDefaultLobbyBg()
        {
            PlayBg(DefaultLobbyBGM);
        }

        public void PlayBg(string name, float volume = 0.5f, float delay = DefaultmBgmDelay)
        {
            if (mAudioList.TryGetValue(name, out var clip))
            {
                mBGAudioPlayer.clip = clip;
            }

            mBGAudioPlayer.loop = true;

            mBGAudioPlayer.DORewind();
            mBGAudioPlayer.DOFade(0f, delay * 0.5f)
                .OnComplete(() =>
                {
                    mBGAudioPlayer.DORewind();
                    mBGAudioPlayer.DOFade(SaveBitFlags.BGMSound.IsOn() ? volume : 0f, delay * 0.5f);
                    mBGAudioPlayer.Play();
                })
                .SetId("BGMManager");
        }

        public void PauseBG()
        {
            if (mBGAudioPlayer.isPlaying)
            {
                mBGAudioPlayer.Pause();
            }
        }

        public void SetBGVolume(float volume)
        {
            mBGAudioPlayer.volume = volume;
        }

        public void UnPauseBG()
        {
            if (!mBGAudioPlayer.isPlaying)
            {
                mBGAudioPlayer.UnPause();
            }
        }

        public void SetBGSoundActive(bool on)
        {
            if (on)
                SetBGVolume(0.5f);
            else
                SetBGVolume(0f);
        }

        public void SetEffectSoundActive(bool on)
        {
            for (int i = 0; i < mSFXAudioPlayer.Length; ++i)
            {
                mSFXAudioPlayer[i].volume = on ? 1f : 0f;
            }
        }

        public AudioSource PlaySE(string name, float volume = 1f)
        {
            for (int i = 0; i < mSFXAudioPlayer.Length; ++i)
            {
                if (mSFXAudioPlayer[i].isPlaying) continue;
                if (mAudioList.TryGetValue(name, out var clip))
                {
                    mSFXAudioPlayer[i].clip = clip;
                    mSFXAudioPlayer[i].volume = SaveBitFlags.SFXSound.IsOn() ? volume : 0f;
                    mSFXAudioPlayer[i].Play();
                    return mSFXAudioPlayer[i];
                }
            }

            return null;
        }

        public void PauseSEAll()
        {
            for (int i = 0; i < mSFXAudioPlayer.Length; ++i)
            {
                if (mSFXAudioPlayer[i].isPlaying)
                {
                    mSFXAudioPlayer[i].Pause();
                }
            }
        }

        public void OnChangeBitFlag()
        {
            ApplyLocalSave();
        }

        public void OnRelease()
        {
            for (int i = 0; i < mSFXAudioPlayer.Length; ++i)
            {
                if (mSFXAudioPlayer[i].isPlaying)
                {
                    mSFXAudioPlayer[i].Stop();
                }
            }

            if (mBGAudioPlayer.isPlaying)
                mBGAudioPlayer.Stop();
        }
    }

    static class SoundPlayer
    {
        //======================================
        // Sound Effect
        //======================================

        public static void PlayUpgradeManaHeartCharge()
        {
            PlaySE("UpgradeManaHeartCharge");
        }

        public static void PlayUpgradeManaHeartFinish()
        {
            PlaySE("EvolutionNova");
        }

        public static void PlayUpgradeManaHeartInst()
        {
            PlaySE("UpgradeManaHearInst");
        }

        public static void PlayReforge(int step)
        {
            PlaySE($"Reforge_Step{step}");
        }

        public static void PlayPetFeed()
        {
            PlaySE("PetFeed");
        }

        public static void PlayEvolutionCharge()
        {
            PlaySE($"EvolutionCharge");
        }

        public static void PlayEvolutionNova()
        {
            PlaySE($"EvolutionNova", 2f);
        }

        public static void PlayRage(ElementalType type)
        {
            PlaySE($"Rage_RaidBoss{type}");
        }

        public static void PlayRageFinish(ElementalType type)
        {
            PlaySE($"RageFinish_RaidBoss{type}");
        }

        public static void PlayCharacterJump()
        {
            PlaySE("CharacterJump");
        }

        public static void PlayCharacterAppear()
        {
            PlaySE("CharacterAppear");
        }

        public static void PlayMessageSound()
        {
            PlaySE("MessageSound");
        }

        public static void PlayPlayerLevelUp()
        {
            PlaySE("PlayerLevelUp");
        }

        public static void PlayUpdateRaidBestScore()
        {
            PlaySE("UpdateRaidBestScore");
        }

        public static void PlayLimitBreakReady()
        {
            PlaySE("LimitBreakReady");
        }

        public static void PlayLimitBreakActive()
        {
            PlaySE("LimitBreakActive");
        }

        public static void PlayEquipmentEquip()
        {
            PlaySE("EquipmentEquip");
        }

        public static void PlayEquipmentCombine()
        {
            PlaySE("EquipmentCombine");
        }

        public static void PlayEquipmentLeveUp()
        {
            PlaySE("EquipmentLevelUp");
        }

        public static void PlayHeartExplostion()
        {
            PlaySE("HeartExplostion");
        }

        public static void PlaySkillLevelUp()
        {
            PlaySE("SkillLevelUp");
        }

        public static void PlayGloryOrbUpgrade(JoustGloryOrbType type)
        {
            switch (type)
            {
                case JoustGloryOrbType.GloryOrb_1:
                case JoustGloryOrbType.GloryOrb_2:
                    PlaySE("UpgradeGloryOrb_1");
                    break;
                case JoustGloryOrbType.GloryOrb_3:
                case JoustGloryOrbType.GloryOrb_4:
                    PlaySE("UpgradeGloryOrb_2");
                    break;
                case JoustGloryOrbType.GloryOrb_5:
                case JoustGloryOrbType.GloryOrb_6:
                    PlaySE("UpgradeGloryOrb_3");
                    break;
                case JoustGloryOrbType.GloryOrb_7:
                default:
                    PlaySE("UpgradeGloryOrb_4");
                    break;
            }
        }

        public static void PlayLevelUp()
        {
            PlaySE("LevelUp");
        }

        public static void PlayShowReward(float volume = 1f)
        {
            PlaySE("ShowReward", volume);
        }

        public static void PlayMessageShowSound(float volume)
        {
            PlaySE("MessageSound", volume);
        }

        public static void PlayQuestClear()
        {
            PlaySE("QuestClear");
        }

        public static void PlayDropStamp()
        {
            PlaySE("DropStamp");
        }

        public static void PlayAttendanceCheck()
        {
            PlaySE("AttendanceCheck");
        }

        public static void PlayUIButtonTouchSound()
        {
            PlaySE("ButtonTouch", 0.5f);
        }

        public static void PlaySpawnItem(Grade grade)
        {
            if (grade == Grade.A)
            {
                PlaySE("Spawn_Grade_A");
            }
            else if (grade == Grade.S)
            {
                PlaySE("Spawn_Grade_S");
            }
            else if (grade == Grade.SS)
            {
                PlaySE("Spawn_Grade_SS");
            }
            else
            {
                PlaySE("ItemDrop");
            }
        }

        public static AudioSource PlayScratchLotto()
        {
            return PlaySE("ScratchLotto");
        }

        public static void PlayErrorSound()
        {
            PlaySE("Error");
        }

        public static void PlayBuffActive()
        {
            PlaySE("Postion");
        }

        public static void PlaySkillUnEquip()
        {
            PlaySE("SkillEquip");
        }

        public static void PlaySkillEquip()
        {
            PlaySE("SkillEquip");
        }

        public static void PlayPetEquip()
        {
            PlaySE("SkillEquip");
        }

        public static void PlayShowPopup()
        {
            PlaySE("ShowPopup");
        }

        public static void PlayHidePopup()
        {
            PlaySE("HidePopup");
        }

        public static void PlayAttack(Character parent, int index)
        {
            string prefix = parent.IsPlayer ? "Player" : 
                parent.IsRaidBoss ? $"RaidBoss{parent.Stat.ElementalType}" :
                parent.IsBoss ? "Boss" : "Monster";

            string atkName = $"Attack_{prefix}_{index}";

            PlaySE(atkName);
        }

        public static void PlayHit(Character attacker, Character defender)
        {
            if (attacker is RaidBoss)
            {
                PlaySE("Hit_Player_ByRaidBoss", 0.25f);
            }
            else
            {
                string prefix = defender.IsPlayer ? "Player" : "Monster";

                PlaySE($"Hit_{prefix}", 0.25f);
            }
        }

        public static void PlayHitShield()
        {
            PlaySE("Hit_Shield", 0.25f);
        }

        public static AudioSource PlaySkillAttack(string id)
        {
            return PlaySE($"{id}_Attack");
        }

        public static AudioSource PlaySkillReady(string id)
        {
            return PlaySE($"{id}_Ready");
        }

        public static void PlayArtifactLevelUpFail()
        {
            PlaySE("ArtifactLevelUp_Fail");
        }

        public static void PlayArtifactLevelUpSuccess()
        {
            PlaySE("ArtifactLevelUp_Success");
        }
        public static void PlayStartGame()
        {
            PlaySE("StartGame");
        }

        public static void PlayBossAppear()
        {
            PlaySE("BossAppear", 0.5f);
        }

        public static void PlayGameOver()
        {
            PlaySE("GameOver");
        }

        public static void PlayActiveCentralEssence()
        {
            PlaySE("ActiveCentralEssence");
        }

        public static void PlayJoustingReady()
        {
            PlaySE("Jousting_Ready", 1f);
        }

        public static void PlayImpactJousting(JoustingAttackType selectAtkType)
        {
            PlaySE($"Impact_Jousting_{selectAtkType}", 1f);
        }

        public static void PlayJoustingAtkReady()
        {
            PlaySE("Jousting_AtkReady");
        }

        public static void PlayJoustingWin()
        {
            PlaySE("Jousting_Win");
        }

        public static void PlayJoustingLose()
        {
            PlaySE("Jousting_Lose");
        }

        public static void PlayJoustingStartCheer()
        {
            PlaySE("JoustingStart_Cheer");
        }

        public static void PlayJoustingImpactCheer()
        {
            PlaySE("JoustingImpact_Cheer", 1f);
        }

        // ===========================================
        // BGM
        // ===========================================

        public static void PlayChapterBGM(int chapter)
        {
            PlayBG($"Chapter_{chapter}");
        }

        public static void PlayDungeonBGM(string id, StageType stageType)
        {
            PlayBG($"Dungeon_{stageType}");
        }

        public static void PlayDemonicRealmBGM(string id, StageType stageType)
        {
            // DemonicRealm_Accessory
            PlayBG($"DemonicRealm_{stageType}");
        }

        public static void PlayJoustingBGM()
        {
            PlayBG("Jousting");
        }

        public static void PlayLimitBreakBGM()
        {
            PlayBG("LimitBreak");
        }

        static AudioSource PlaySE(string name, float volume = 0.75f)
        {
            return Lance.SoundManager.PlaySE(name, volume);
        }
        
        static void PlayBG(string name, float volume = 0.5f)
        {
            Lance.SoundManager.PlayBg(name, volume);
        }
    }
}

