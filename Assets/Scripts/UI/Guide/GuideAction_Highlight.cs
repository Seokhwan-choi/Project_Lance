using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class GuideAction_Highlight : GuideAction
    {
        float mDelayShowMessage;
        public override void OnStart()
        {
            base.OnStart();

            mParent.SetHighlightModalAlpha(1f);
            mParent.SetActiveTouchBlock(true);
            mParent.ShowHighlight(mData.type, () => mIsFinish = true);

            mDelayShowMessage = 0.5f;
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            if (mDelayShowMessage > 0)
            {
                mDelayShowMessage -= dt;
                if (mDelayShowMessage <= 0)
                {
                    if (mData.showMessage.IsValid())
                    {
                        mParent.SetHighlightModalAlpha(0f);

                        Lance.Lobby.StartCharacterMessage(mData.showMessage);
                    }
                }
            }
        }

        public override void OnFinish()
        {
            base.OnFinish();

            mParent.SetActivHighlightObj(false);
            mParent.SetActiveTouchBlock(false);
        }
    }
}