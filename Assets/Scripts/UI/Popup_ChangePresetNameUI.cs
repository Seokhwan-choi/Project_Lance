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
                // �ý��� �޼���
                UIUtil.ShowSystemErrorMessage("IsEmptyPresetName");

                return;
            }

            // ��Ģ� �ɸ����� Ȯ�� ( �������� Ȯ�� )
            if (BadWordChecker.HaveBadWord(presetName))
            {
                // ���� �ܾ ���ԵǾ� ����
                UIUtil.ShowSystemErrorMessage("HaveBadWord");

                return;
            }

            // �ִ� 13�ڱ��� ���� => �ѱ� ���� �� 52����Ʈ
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