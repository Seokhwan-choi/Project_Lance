using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Lance
{

    class Popup_NicknameUI : PopupBase
    {
        TMP_InputField mNicknameInputField;
        double NicknameChangePrice => Lance.GameData.CommonData.nickNameChangePrice;
        public void Init(Action onConfirm)
        {
            bool isChangeNickname = Backend.UserNickName.IsValid();

            mNicknameInputField = gameObject.FindComponent<TMP_InputField>("InputField_Nickname");

            var button = gameObject.FindComponent<Button>("Button_Confirm");
            button.SetButtonAction(() => OnClickNicknameConfirmButton(onConfirm));
            button.SetActiveFrame(isChangeNickname ? Lance.Account.IsEnoughGem(NicknameChangePrice) : true);

            var confirmObj = gameObject.FindGameObject("Confirm");
            var changeObj = gameObject.FindGameObject("Change");

            confirmObj.SetActive(!isChangeNickname);
            changeObj.SetActive(isChangeNickname);

            if (isChangeNickname)
            {
                SetUpCloseAction();

                var textChangePrice = gameObject.FindComponent<TextMeshProUGUI>("Text_ChangePrice");
                textChangePrice.SetColor(UIUtil.GetEnoughTextColor(Lance.Account.IsEnoughGem(NicknameChangePrice)));
                textChangePrice.text = $"{NicknameChangePrice}";
            }
        }

        void OnClickNicknameConfirmButton(Action onConfirm)
        {
            string nickname = mNicknameInputField.text;

            if (string.IsNullOrEmpty(nickname))
            {
                // �ý��� �޼���
                UIUtil.ShowSystemErrorMessage("IsEmptyNickname");

                return;
            }

            // ��Ģ� �ɸ����� Ȯ�� ( �������� Ȯ�� )
            if (BadWordChecker.HaveBadWord(nickname))
            {
                // ���� �ܾ ���ԵǾ� ����
                UIUtil.ShowSystemErrorMessage("HaveBadWord");

                return;
            }

            bool isChangeNickname = Backend.UserNickName.IsValid();
            if (isChangeNickname)
            {
                if (Lance.Account.IsEnoughGem(NicknameChangePrice) == false)
                {
                    UIUtil.ShowSystemErrorMessage("IsNotEnoughGem");

                    return;
                }
            }

            //[�ڳ�] �г��� ������Ʈ �Լ�
            SendQueue.Enqueue(Backend.BMember.UpdateNickname, nickname, callback => {
                try
                {
                    if (callback.IsSuccess() == false)
                    {
                        if (callback.GetStatusCode() == "400")
                        {
                            if (callback.GetMessage().Contains("undefined nickname"))
                            {
                                // �г����� �Է����� �ʾ���
                                UIUtil.ShowSystemErrorMessage("IsEmptyNickname");
                            }
                            else if (callback.GetMessage().Contains("bad nickname is too long"))
                            {
                                // �г����� �ִ� 20�ڱ��� ����
                                UIUtil.ShowSystemErrorMessage("TooLongNickname");
                            }
                            else if (callback.GetMessage().Contains("bad beginning or end"))
                            {
                                // ��or�ڿ� ������ ���Ե� �г���
                                UIUtil.ShowSystemErrorMessage("ContainsEmptyNickname");
                            }
                            else
                            {
                                // �� �� ���� ����
                                UIUtil.ShowSystemDefaultErrorMessage();
                            }
                        }
                        else if (callback.GetStatusCode() == "409")
                        {
                            // �ߺ��� �г���
                            UIUtil.ShowSystemErrorMessage("IsDuplicatedNickname");
                        }
                    }
                    else
                    {
                        Close();

                        onConfirm?.Invoke();

                        Lance.Account.UseGem(NicknameChangePrice);
                        Lance.Account.JoustBattleInfo.SetNickName(Backend.UserNickName);
                        Lance.Lobby.UpdateGemUI();
                        Lance.Lobby.UpdadteUserInfo_Nickname();
                    }

                }
                catch (Exception e)
                {
                    // ������ �ý��� �޼��� �� �������
                    //UIUtil.ShowSystemDefaultErrorMessage();
                }
            });
        }
    }
}