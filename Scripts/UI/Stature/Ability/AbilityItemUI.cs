using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;


namespace Lance
{
    static class AbilitySprite
    {
        public static Sprite HorLine_Gray => Lance.Atlas.GetUISprite("TreeLine_Horizontal_Gray");
        public static Sprite HorLine_Light => Lance.Atlas.GetUISprite("TreeLine_Horizontal_Light");
        public static Sprite VerLine_Gray => Lance.Atlas.GetUISprite("TreeLine_Vertical_Gray");
        public static Sprite VerLine_Light => Lance.Atlas.GetUISprite("TreeLine_Vertical_Light");
    }

    class AbilityItemUI : MonoBehaviour
    {
        int mStep;
        Image mImageUpLine;
        Image mImageDownLine;
        Dictionary<string, AbilitySlotUI> mSlotDics;
        public void Init(int step, Action<string> onButton)
        {
            mStep = step;

            mImageUpLine = gameObject.FindComponent<Image>("UpLine");
            mImageDownLine = gameObject.FindComponent<Image>("DownLine");

            var slotListObj = gameObject.FindGameObject("SlotList");

            slotListObj.AllChildObjectOff();

            mSlotDics = new Dictionary<string, AbilitySlotUI>();

            foreach(AbilityData data in DataUtil.GetAbilityDatasByStep(step))
            {
                var abilitySlotObj = Util.InstantiateUI("AbilitySlotUI", slotListObj.transform);

                var abilitySlotUI = abilitySlotObj.GetOrAddComponent<AbilitySlotUI>();

                abilitySlotUI.Init(data, onButton);

                mSlotDics.Add(data.id, abilitySlotUI);
            }

            Refresh();
        }

        public AbilitySlotUI GetSlot(string id)
        {
            return mSlotDics.TryGet(id);
        }

        public string GetFirstAbilityId()
        {
            return mSlotDics.Keys.FirstOrDefault();
        }

        public void Localize()
        {
            Refresh();
        }

        public void Refresh()
        {
            bool isFirstStep = mStep - 1 <= 0;
            bool isLastStep = mStep == DataUtil.GetAbilityMaxStep();

            mImageUpLine.gameObject.SetActive(isFirstStep == false);
            mImageDownLine.gameObject.SetActive(isLastStep == false);

            mImageUpLine.sprite = AllMeetRequirements() ? AbilitySprite.HorLine_Light : AbilitySprite.HorLine_Gray;
            mImageDownLine.sprite = AllIsMaxLevel() ? AbilitySprite.HorLine_Light : AbilitySprite.HorLine_Gray;

            foreach(var slot in mSlotDics.Values)
            {
                slot.Refresh();
            }
        }

        public void SetActiveSelected(string id)
        {
            foreach (var slot in mSlotDics.Values)
            {
                slot.SetActiveSelected(slot.Id == id);
            }
        }

        bool AllMeetRequirements()
        {
            foreach(var ability in mSlotDics.Values)
            {
                if (ability.IsMeetRequirements() == false)
                    return false;
            }

            return true;
        }

        bool AllIsMaxLevel()
        {
            foreach (var ability in mSlotDics.Values)
            {
                if (ability.IsMaxLevel() == false)
                    return false;
            }

            return true;
        }
    }

    class AbilitySlotUI : MonoBehaviour
    {
        AbilityData mData;
        GameObject mRedDotObj;
        GameObject mSelectedObj;
        Image mImageUpLine;
        Image mImageDownLine;
        Image mImageModal;
        TextMeshProUGUI mTextLevel;
        public string Id => mData.id;
        int Level => Lance.Account.Ability.GetAbilityLevel(mData.id);
        public void Init(AbilityData data, Action<string> onButton)
        {
            mData = data;

            var button = GetComponent<Button>();
            button.SetButtonAction(() => onButton.Invoke(mData.id));

            mSelectedObj = gameObject.FindGameObject("Selected");
            mRedDotObj = gameObject.FindGameObject("RedDot");

            mImageUpLine = gameObject.FindComponent<Image>("UpLine");
            mImageDownLine = gameObject.FindComponent<Image>("DownLine");
            mImageModal = gameObject.FindComponent<Image>("Image_Modal");
            var imageIcon = gameObject.FindComponent<Image>("Image_Icon");
            imageIcon.sprite = Lance.Atlas.GetUISprite(data.sprite);

            mTextLevel = gameObject.FindComponent<TextMeshProUGUI>("Text_Level");
        }

        public void SetActiveSelected(bool isActive)
        {
            mSelectedObj.SetActive(isActive);
        }

        public void Refresh()
        {
            if (mData == null)
                return;

            mImageUpLine.gameObject.SetActive(mData.requireAbilitys.IsValid());
            mImageDownLine.gameObject.SetActive(DataUtil.IsMaxStepAbility(mData.id) == false);
            mImageModal.gameObject.SetActive(Level <= 0);

            if (IsMeetRequirements())
            {
                mImageUpLine.sprite = AbilitySprite.VerLine_Light;
            }
            else
            {
                mImageUpLine.sprite = AbilitySprite.VerLine_Gray;
            }

            if (IsMaxLevel())
            {
                mImageDownLine.sprite = AbilitySprite.VerLine_Light;
            }
            else
            {
                mImageDownLine.sprite = AbilitySprite.VerLine_Gray;
            }

            mTextLevel.text = $"{Level} / {mData.maxLevel}";

            mRedDotObj.SetActive(Lance.Account.CanLevelUpAbility(mData.id));
        }

        public bool IsMeetRequirements()
        {
            return Lance.Account.Ability.IsMeetRequirements(mData.id);
        }

        public bool IsMaxLevel()
        {
            return mData.maxLevel == Level;
        }
    }
}