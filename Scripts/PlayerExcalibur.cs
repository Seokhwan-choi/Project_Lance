using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;


namespace Lance
{
    class PlayerExcalibur
    {
        Player mParent;
        GameObject mExcaliburObj;
        public Transform Tm => mExcaliburObj.transform;
        public Player Parent => mParent;
        public void Init(Player parent)
        {
            mParent = parent;

            // 엑스칼리버 오브젝트 생성
            mExcaliburObj = Lance.ObjectPool.AcquireObject("Excalibur", parent.transform);
            mExcaliburObj.transform.localPosition = new Vector2(-0.55f, 0.5f);

            Refresh();
        }

        public void Refresh()
        {
            bool isAllArtifactMaxLevel = Lance.Account.AncientArtifact.IsAllArtifactMaxLevel();
            if (isAllArtifactMaxLevel)
            {
                if (mExcaliburObj.activeSelf == false)
                    mExcaliburObj.SetActive(true);
            }
            else
            {
                if (mExcaliburObj.activeSelf)
                    mExcaliburObj.SetActive(false);
            }
        }

        public void SetActive(bool active)
        {
            mExcaliburObj.SetActive(active);
        }

        public void OnRelease()
        {
            if (mExcaliburObj != null)
            {
                Lance.ObjectPool.ReleaseObject(mExcaliburObj);
                mExcaliburObj = null;
            }
        }
    }
}