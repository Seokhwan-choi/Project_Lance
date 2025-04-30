using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace Lance
{
    class Lobby_SkillAutoCastUI : MonoBehaviour
    {
        Button mButtonAutoCast;
        Image mImageAutoCastFrame;
        Image mImageAutoCast;

        Tweener mAutoCastTween;
        public void Init()
        {
            mButtonAutoCast = gameObject.FindComponent<Button>("Button_AutoCastSkill");
            mButtonAutoCast.SetButtonAction(OnAutoCastSkillButton);

            mImageAutoCastFrame = mButtonAutoCast.GetComponent<Image>();
            mImageAutoCast = gameObject.FindComponent<Image>("Image_Auto");

            RefreshAutoCastUI();
        }

        public void RefreshContentsLockUI()
        {
            SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.AutoCast) == false);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        void OnAutoCastSkillButton()
        {
            if (ContentsLockUtil.IsLockContents(ContentsLockType.AutoCast))
                return;

            SaveBitFlags.SkillAutoCast.Toggle();

            RefreshAutoCastUI();

            if (SaveBitFlags.SkillAutoCast.IsOn())
                Lance.GameManager.CheckQuest(QuestType.ActiveAutoCastSkill, 1);
        }

        void RefreshAutoCastUI()
        {
            if (SaveBitFlags.SkillAutoCast.IsOn())
            {
                var tm = mImageAutoCast.GetComponent<RectTransform>();

                mAutoCastTween = tm.DOLocalRotate(Vector3.forward * 360f, 1f, RotateMode.FastBeyond360)
                    .SetUpdate(isIndependentUpdate: true)
                    .SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
                mAutoCastTween.timeScale = 1f;

                mImageAutoCastFrame.sprite = Lance.Atlas.GetUISprite("Frame_Auto_Active");
                mImageAutoCast.sprite = Lance.Atlas.GetUISprite("Icon_Auto_Active");
            }
            else
            {
                mAutoCastTween.Rewind();
                mAutoCastTween.Pause();

                mImageAutoCastFrame.sprite = Lance.Atlas.GetUISprite("Frame_Auto_Inactive");
                mImageAutoCast.sprite = Lance.Atlas.GetUISprite("Icon_Auto_Inactive");
            }
        }

        public void OnStartStage(StageData stageData)
        {
            SetActive(ContentsLockUtil.IsLockContents(ContentsLockType.AutoCast) == false && !stageData.type.IsJousting());
        }
    }
}