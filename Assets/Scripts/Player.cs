using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


namespace Lance
{
    class Player : Character
    {
        PlayerPet mPet;
        PlayerExcalibur mExcalibur;
        SkillManager mSkillManager;
        Dictionary<string, GameObject> mFXList;

        protected float mCurMoveSpeed;
        protected SpriteRenderer mWeaponRenderer;
        public SkillManager SkillManager => mSkillManager;
        public bool IsMoving => mActionManager.CurrAction is Action_Move;
        public bool IsKnockback => mActionManager.CurrAction is Action_Knockback;
        public override void Init()
        {
            mFXList = new Dictionary<string, GameObject>();

            mAnim = new PlayerAnim();
            mAnim.Init(this);

            mPhysics = new CharacterPhysics();
            mPhysics.Init(this);

            mActionManager = new ActionManager();
            mActionManager.Init(this);

            mSkillManager = new SkillManager();
            mSkillManager.Init(this);

            mPet = new PlayerPet();
            mPet.Init(this);

            mExcalibur = new PlayerExcalibur();
            mExcalibur.Init(this);

            mStat = new CharacterStat();
            mStat.InitPlayer(this);

            mHpGaugebarUI = UIUtil.CreateHpGaugebarUI(this);

            mActionManager.PlayAction(ActionType.Move);

            mWeaponRenderer = gameObject.FindComponent<SpriteRenderer>("Weapon");

            SetPosition(new Vector2(-1.15f, 0.85f));
        }

        public void ResetSkillCoolTime()
        {
            mSkillManager.ResetCoolTimes();
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mSkillManager?.RandomizeKey();
            mPet?.RandomizeKey();
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            mPet?.OnUpdate(dt);
            mSkillManager?.OnUpdate(dt);

            Lance.Lobby?.UpdateSkillCastUI(this);

            if (IsMoving)
            {
                if (mCurMoveSpeed < GetMoveSpeed())
                {
                    mCurMoveSpeed += (dt * Const.Acceleration);

                    mAnim.SetMoveSpeed(mCurMoveSpeed / GetMoveSpeed());
                }
                else
                {
                    if (mCurMoveSpeed != GetMoveSpeed())
                    {
                        mCurMoveSpeed = GetMoveSpeed();

                        mAnim.SetMoveSpeed(1f);

                        Lance.GameManager.StageManager.SetActiveWindlines(true);
                    }
                }
            }
            else
            {
                mCurMoveSpeed = 0f;
            }
        }

        public float GetCurMoveSpeed()
        {
            return mCurMoveSpeed;
        }

        public void UpdatePlayerStat()
        {
            mStat.UpdatePlayerStat();

            mPet?.Refresh();
            mExcalibur?.Refresh();
        }

        public void UpdateCostumes(string[] costumes, bool isJousting)
        {
            string bodyLibraryAsset = isJousting ? Lance.GameData.CostumeCommonData.playerDefaultJoustingBodyLibraryAsset :Lance.GameData.CostumeCommonData.playerDefaultLibraryBodyAsset;
            string handLibraryAsset = Lance.GameData.CostumeCommonData.playerDefaultLibraryHandAsset;

            if (costumes.Length > (int)CostumeType.Body)
            {
                string bodyCostume = costumes[(int)CostumeType.Body];
                if (bodyCostume.IsValid())
                {
                    var bodyCostumeData = Lance.GameData.BodyCostumeData.TryGet(bodyCostume);
                    if (bodyCostumeData != null)
                    {
                        bodyLibraryAsset = isJousting ? bodyCostumeData.joustingLibraryAsset : bodyCostumeData.libraryAsset;
                        handLibraryAsset = bodyCostumeData.handLibraryAsset;
                    }
                }
            }

            mSpriteLibraryAssetData = Lance.GameData.LibraryAssetData.TryGet(bodyLibraryAsset);

            string etcLibrearyAsset = Lance.GameData.CostumeCommonData.playerDefaultLibraryEtcAsset;
            int orderInLayer = 0;

            if (costumes.Length > (int)CostumeType.Etc)
            {
                string etcCostume = costumes[(int)CostumeType.Etc];
                if (etcCostume.IsValid())
                {
                    var etcCostumeData = Lance.GameData.EtcCostumeData.TryGet(etcCostume);
                    if (etcCostumeData != null)
                    {
                        etcLibrearyAsset = isJousting ? etcCostumeData.joustingLibraryAsset : etcCostumeData.libraryAsset;
                        orderInLayer = etcCostumeData.orderInLayer;
                    }
                }
            }
            
            
            mAnim.SetBodyLibraryAsset(mSpriteLibraryAssetData.libraryAsset);
            mAnim.SetHandLibraryAsset(handLibraryAsset);
            mAnim.SetEtcLibraryAsset(etcLibrearyAsset, orderInLayer);

            string weaponSprite = Lance.GameData.CostumeCommonData.playerDefaultWeapon;

            if (costumes.Length > (int)CostumeType.Body)
            {
                string weaponCostume = costumes[(int)CostumeType.Weapon];
                if (weaponCostume.IsValid())
                {
                    var weaponCostumeData = Lance.GameData.WeaponCostumeData.TryGet(weaponCostume);
                    if (weaponCostumeData != null)
                    {
                        weaponSprite = weaponCostumeData.sprite;
                    }
                }
            }

            mWeaponRenderer.sprite = Lance.Atlas.GetPlayerSprite(weaponSprite);
        }

        public override void OnRelease()
        {
            base.OnRelease();

            mPet?.OnRelease();
            mExcalibur?.OnRelease();
            mSkillManager?.OnRelease();
            mHpGaugebarUI?.OnRelease();

            if (mFXList != null)
            {
                foreach (var skillFX in mFXList.Values)
                {
                    foreach (var ps in skillFX.GetComponentsInChildren<ParticleSystem>())
                    {
                        ps.Stop();
                    }

                    Lance.ObjectPool.ReleaseObject(skillFX);
                }

                mFXList.Clear();
            }
        }

        public override void OnDamage(DamageInst inst)
        {
            if (IsDeath)
                return;

            // 데미지 입음
            mStat.OnDamage(inst.Damage);

            UIUtil.ShowDamageText(this, inst.Damage, inst.IsCritical, inst.IsSuperCritical);

            mHpGaugebarUI.UpdateGaugebar();

            mAnim.PlayHit();

            SoundPlayer.PlayHit(inst.Attacker, inst.Defender);

            // 데미지를 입어서 죽었다면 게임 매니저한테 알려주자
            if (mStat.IsDeath)
            {
                OnDeath();
            }

            inst = null;
        }

        public override void OnDeath()
        {
            Lance.GameManager.OnPlayerDeath();

            Lance.GameManager.StageManager.SetPlayerMoving(false);

            mAnim.PlayDeath();
        }

        public void OnStartBossSpawnMotion()
        {
            mSkillManager?.OnRelease();
            mSkillManager?.ResetCoolTimes();

            mPet?.OnStartBossSpawnMotion();

            //mActionManager.PlayAction(ActionType.Idle, true);
        }

        public void OnFinishBossSpawnMotion()
        {
            mStat.RefillHp();
        }

        public override bool AnyInAttackRangeOpponent()
        {
            // 현재 착용중인 스킬에 대해서도 확인해야 함
            var result = Lance.GameManager.StageManager.AnyInAttackRangeOpponent(this, mStat.AtkRange);
            // 일단은 기본 공격에 대해서만 체크
            return result.any;
        }

        public override float GetBodySize()
        {
            return 0.5f;
        }

        public void OnChangePet()
        {
            mPet?.OnChangePet();
        }

        public void PlayFX(string id, string fxPrefab)
        {
            if (mFXList != null)
            {
                if (mFXList.ContainsKey(id) == false)
                {
                    var fxObj = Lance.ObjectPool.AcquireObject($"FX/{fxPrefab}", transform);

                    foreach (var ps in fxObj.GetComponentsInChildren<ParticleSystem>())
                    {
                        ps.time = 0f;
                        ps.Play();
                    }

                    mFXList.Add(id, fxObj);
                }
            }
            
        }

        public void ReleaseFX(string id)
        {
            if (mFXList != null)
            {
                var fxObj = mFXList.TryGet(id);
                if (fxObj != null)
                {
                    foreach (var ps in fxObj.GetComponentsInChildren<ParticleSystem>())
                    {
                        ps.Stop();
                    }

                    Lance.ObjectPool.ReleaseObject(fxObj);

                    mFXList.Remove(id);
                }
            }
        }

        public double GatherPetPassiveSkillValue(StatType type, DamageInst inst = null)
        {
            return mPet.SkillManager.GatherPassiveSkillValue(type, inst);
        }
    }
}