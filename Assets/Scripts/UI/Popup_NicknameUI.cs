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
                // 시스템 메세지
                UIUtil.ShowSystemErrorMessage("IsEmptyNickname");

                return;
            }

            // 금칙어에 걸리는지 확인 ( 기초적인 확인 )
            if (BadWordChecker.HaveBadWord(nickname))
            {
                // 나쁜 단어가 포함되어 있음
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

            //[뒤끝] 닉네임 업데이트 함수
            SendQueue.Enqueue(Backend.BMember.UpdateNickname, nickname, callback => {
                try
                {
                    if (callback.IsSuccess() == false)
                    {
                        if (callback.GetStatusCode() == "400")
                        {
                            if (callback.GetMessage().Contains("undefined nickname"))
                            {
                                // 닉네임을 입력하지 않았음
                                UIUtil.ShowSystemErrorMessage("IsEmptyNickname");
                            }
                            else if (callback.GetMessage().Contains("bad nickname is too long"))
                            {
                                // 닉네임은 최대 20자까지 가능
                                UIUtil.ShowSystemErrorMessage("TooLongNickname");
                            }
                            else if (callback.GetMessage().Contains("bad beginning or end"))
                            {
                                // 앞or뒤에 공백이 포함된 닉네임
                                UIUtil.ShowSystemErrorMessage("ContainsEmptyNickname");
                            }
                            else
                            {
                                // 알 수 없는 오류
                                UIUtil.ShowSystemDefaultErrorMessage();
                            }
                        }
                        else if (callback.GetStatusCode() == "409")
                        {
                            // 중복된 닉네임
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
                    // 위에서 시스템 메세지 다 뱉어줬음
                    //UIUtil.ShowSystemDefaultErrorMessage();
                }
            });
        }
    }
}