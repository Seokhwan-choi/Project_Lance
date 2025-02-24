using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;


namespace Lance
{
    class PlayerPet
    {
        Player mParent;
        PetSkillManager mSkillManager;
        GameObject mPetObj;
        SpriteLibrary mSpriteLibrary;
        public PetSkillManager SkillManager => mSkillManager;
        public Transform Tm => mPetObj.transform;
        public Player Parent => mParent;
        public void Init(Player parent)
        {
            mParent = parent;

            // Pet 오브젝트 생성
            mPetObj = Lance.ObjectPool.AcquireObject("Pet", parent.transform);
            mPetObj.transform.localPosition = new Vector2(-0.3f, 0.9f);

            mSpriteLibrary = mPetObj.GetComponentInChildren<SpriteLibrary>();

            mSkillManager = new PetSkillManager();
            mSkillManager.Init(this, parent);

            Refresh();
        }

        public void RandomizeKey()
        {
            mSkillManager.RandomizeKey();
        }

        public void OnUpdate(float dt)
        {
            mSkillManager.OnUpdate(dt);
        }

        public void OnChangePet()
        {
            mSkillManager.OnChangePet();
        }

        public void Refresh()
        {
            // 현재 착용중인 펫의 속성, 단계를 확인해서 이미지를 교체해주자
            PetInst petInst = Lance.Account.Pet.GetEquippedInst();
            if (petInst != null)
            {
                if (mPetObj.activeSelf == false)
                    mPetObj.SetActive(true);

                ElementalType type = petInst.GetElementalType();
                int step = petInst.GetStep();

                SpriteLibraryAsset libraryAsset = Lance.SpriteLibrary.GetAsset($"Pet{type}_{step}");

                mSpriteLibrary.spriteLibraryAsset = libraryAsset;
            }
            else
            {
                mPetObj.SetActive(false);
            }
        }

        public void SetActive(bool active)
        {
            mPetObj.SetActive(active);
        }

        public void OnRelease()
        {
            if (mPetObj != null)
            {
                Lance.ObjectPool.ReleaseObject(mPetObj);
                mPetObj = null;
                mSpriteLibrary = null;
            }

            mSkillManager?.OnRelease();
        }

        public void OnStartBossSpawnMotion()
        {
            mSkillManager?.OnRelease();
            mSkillManager.ResetCoolTimes();
        }
    }
}