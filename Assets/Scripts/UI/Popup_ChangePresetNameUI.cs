using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Lance
{

    class Popup_ChangePresetNameUI : PopupBase
    {
        int mPreset;
        TMP_InputField mPresetNameInputField;
        public void Init(int preset, Action onConfirm)
        {
            mPreset = preset;

            var info = Lance.Account.Preset.GetPresetInfo(preset);

            var textPlaceholder = gameObject.FindComponent<TextMeshProUGUI>("Text_Placeholder");
            textPlaceholder.text = info.GetPresetName();

            SetTitleText(StringTableUtil.Get("Title_ChangePresetName"));

            SetUpCloseAction();

            mPresetNameInputField = gameObject.FindComponent<TMP_InputField>("InputField_PresetName");

            var button = gameObject.FindComponent<Button>("Button_Confirm");
            button.SetButtonAction(() => OnClickPresetNameConfirmButton(onConfirm));
        }

        void OnClickPresetNameConfirmButton(Action onConfirm)
        {
            var presetInfo = Lance.Account.Preset.GetPresetInfo(mPreset);
            if (presetInfo == null || presetInfo.IsUnlocked() == false)
            {
                UIUtil.ShowSystemErrorMessage("IsLockedPreset");

                return;
            }

            string presetName = mPresetNameInputField.text;

            if (string.IsNullOrEmpty(presetName))
            {
                // 시스템 메세지
                UIUtil.ShowSystemErrorMessage("IsEmptyPresetName");

                return;
            }

            // 금칙어에 걸리는지 확인 ( 기초적인 확인 )
            if (BadWordChecker.HaveBadWord(presetName))
            {
                // 나쁜 단어가 포함되어 있음
                UIUtil.ShowSystemErrorMessage("HaveBadWord");

                return;
            }

            // 최대 13자까지 가능 => 한글 기준 약 52바이트
            int bytecount = System.Text.Encoding.Default.GetByteCount(presetName);
            if (bytecount > 52)
            {
                UIUtil.ShowSystemErrorMessage("TooLongPresetName");

                return;
            }

            Lance.Account.ChangePresetName(mPreset, presetName);

            Close();

            onConfirm?.Invoke();
        }
    }
}