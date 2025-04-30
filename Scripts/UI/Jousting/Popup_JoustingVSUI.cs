using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Lance
{
    class Popup_JoustingVSUI : PopupBase
    {
        float mDuration;
        public void Init(JoustBattleInfo myBattleInfo, JoustBattleInfo opponentBattleInfo)
        {
            var textMyNickname = gameObject.FindComponent<TextMeshProUGUI>("Text_MyNickname");
            textMyNickname.text = myBattleInfo.GetNickName();

            Lance.GameManager.SetActiveJoustingPlayerPreview(true);
            Lance.GameManager.RefreshJoustingPlayerPreview(myBattleInfo.GetCostumes());
            var myPreview = gameObject.FindComponent<RawImage>("MyPreivew");
            myPreview.texture = Lance.GameManager.GetJoustingPlayerPreivewRenderTexture();

            var textOpponentNickname = gameObject.FindComponent<TextMeshProUGUI>("Text_OpponentNickname");
            textOpponentNickname.text = opponentBattleInfo.GetNickName();

            Lance.GameManager.SetActiveJoustingOpponentPreview(true);
            Lance.GameManager.RefreshJoustingOpponentPlayerPreview(opponentBattleInfo.GetCostumes());
            var opponentPreview = gameObject.FindComponent<RawImage>("OpponentPreview");
            opponentPreview.texture = Lance.GameManager.GetJoustingOpponentPreviewRenderTexture();

            mDuration = 2f;
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            base.Close(immediate, hideMotion);

            Lance.GameManager.SetActiveJoustingPlayerPreview(false);
            Lance.GameManager.SetActiveJoustingOpponentPreview(false);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (mDuration > 0)
            {
                if (mDuration >= 1.75f)
                {
                    mDuration -= dt;
                    if (mDuration < 1.75f)
                    {
                        SoundPlayer.PlayJoustingReady();
                    }
                }
                else
                {
                    mDuration -= dt;
                    if (mDuration <= 0f)
                    {
                        Close();
                    }
                }
            }
        }
    }
}