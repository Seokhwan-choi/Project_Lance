using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class GuideAction_JustWaitTime : GuideAction
    {
        public override void OnStart()
        {
            base.OnStart();

            mParent.SetActivHighlightObj(false);
            mParent.SetActiveTouchBlock(true);

            if (mData.showMessage.IsValid())
            {
                Lance.Lobby.StartCharacterMessage(mData.showMessage);
            }
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            mWaitTime -= dt;
            if (mWaitTime <= 0f)
            {
                mIsFinish = true;
            }
            else
            {
                //if (Lance.GameManager.IsActiveTouchBlock() == false)
                //    Lance.GameManager.StartTouchBlock(mWaitTime);
            }
        }

        public override void OnFinish()
        {
            base.OnFinish();

            mParent.SetActiveTouchBlock(false);
        }
    }
}


