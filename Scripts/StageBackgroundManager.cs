using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class StageBackgroundManager
    {
        StageManager mParent;
        GameObject mWindlinesSpeedy;
        GameObject mBossBreakThrough;
        BackgroundScroller[] mBackgrounds;
        public void Init(StageManager parent)
        {
            mParent = parent;

            var stageBackground = GameObject.Find("Stage_Background");

            mBackgrounds = stageBackground.GetComponentsInChildren<BackgroundScroller>();

            for(int i = 0; i < mBackgrounds.Length; ++i)
            {
                mBackgrounds[i].Init();
            }

            mWindlinesSpeedy = stageBackground.FindGameObject("WindlinesSpeedy");

            mBossBreakThrough = stageBackground.FindGameObject("BossBreakThrough");

            SetActiveWindlines(false);
            SetActiveBossBreakThrough(false);
        }

        public void SetActiveWindlines(bool isActive)
        {
            mWindlinesSpeedy.SetActive(isActive);
        }

        public void SetActiveBossBreakThrough(bool isActive)
        {
            mBossBreakThrough.SetActive(isActive);
        }

        public void OnUpdate(float dt)
        {
            for (int i = 0; i < mBackgrounds.Length; ++i)
            {
                mBackgrounds[i].OnUpdate(dt);
            }
        }

        public void PlayKnockback()
        {
            for (int i = 0; i < mBackgrounds.Length; ++i)
            {
                mBackgrounds[i].PlayKnockback();
            }
        }

        public void ChangeBackground(StageType stageType, int chapter)
        {
            for (int i = 0; i < mBackgrounds.Length; ++i)
            {
                mBackgrounds[i].ChangeBackground(stageType, chapter);
            }
        }

        public void SetPlayerMoving(bool move)
        {
            SetPause(!move);
        }

        void SetPause(bool pause)
        {
            for (int i = 0; i < mBackgrounds.Length; ++i)
            {
                mBackgrounds[i].SetPause(pause);
            }
        }

        public void SetAdjSpeed(float value)
        {
            for (int i = 0; i < mBackgrounds.Length; ++i)
            {
                mBackgrounds[i].SetAdjSpeedValue(value);
            }
        }
    }
}