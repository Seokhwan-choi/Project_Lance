using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Lance
{
    class Lobby_JoustingModeUI : MonoBehaviour
    {
        JoustingAttackType mSelectedAtkType;
        List<JoustingAttackSelectItemUI> mAttackSelectItemList;
        bool mIsActive;
        public void Init()
        {
            mAttackSelectItemList = new List<JoustingAttackSelectItemUI>();

            for(int i = 0; i < (int)JoustingAttackType.Count; ++i)
            {
                JoustingAttackType type = (JoustingAttackType)i;

                var selectItemObj = gameObject.FindGameObject($"SelectAttackItemUI_{type}");
                var selectItemUI = selectItemObj.GetOrAddComponent<JoustingAttackSelectItemUI>();
                selectItemUI.Init(type, (selectType) =>
                {
                    OnSelectAtkType(selectType);

                    Lance.GameManager.StageManager.OnSelectJoustingAttackType(selectType);
                });

                mAttackSelectItemList.Add(selectItemUI);
            }

            var buttonExit = gameObject.FindComponent<Button>("Button_Giveup");
            buttonExit.SetButtonAction(OnButtonGiveup);

            gameObject.SetActive(false);
        }

        public void Localize()
        {
            if (mStageData != null)
            {

            }
        }

        StageData mStageData;
        public void OnStartStage(StageData stageData)
        {
            mStageData = stageData;
            if (stageData.type.IsJousting())
            {
                mIsActive = true;

                gameObject.SetActive(mIsActive);

                mSelectedAtkType = JoustingAttackType.None;

                foreach (var itemUI in mAttackSelectItemList)
                {
                    itemUI.SetActiveSelected(false);
                    itemUI.SetActiveModal(false);
                }
            }
            else
            {
                mIsActive = false;

                gameObject.SetActive(mIsActive);
            }
        }

        void OnButtonGiveup()
        {
            Lance.GameManager.StageManager.OnGiveupJoustingButton();
        }

        public void OnSelectAtkType(JoustingAttackType selectType)
        {
            if (mSelectedAtkType != JoustingAttackType.None)
                return;

            mSelectedAtkType = selectType;

            foreach (var itemUI in mAttackSelectItemList)
            {
                itemUI.SetActiveSelected(itemUI.Type == selectType);
                itemUI.SetActiveModal(itemUI.Type != selectType);
            }
        }
    }

    class JoustingAttackSelectItemUI : MonoBehaviour
    {
        JoustingAttackType mType;
        Image mImageModal;
        GameObject mSelectedObj;

        public JoustingAttackType Type => mType;
        public void Init(JoustingAttackType type, Action<JoustingAttackType> onSelect)
        {
            mType = type;
            mSelectedObj = gameObject.FindGameObject("Selected");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");

            var buttonSelect = gameObject.FindComponent<Button>("Button_Select");
            buttonSelect.SetButtonAction(() => onSelect?.Invoke(type));

            Refresh();
        }

        public void Refresh()
        {
            SetActiveSelected(false);
            SetActiveModal(false);
        }

        public void SetActiveSelected(bool active)
        {
            mSelectedObj.SetActive(active);
        }

        public void SetActiveModal(bool acitve)
        {
            mImageModal.gameObject.SetActive(acitve);
        }
    }
}