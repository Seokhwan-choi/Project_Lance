using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class GuideAction_OpenMenu : GuideAction
    {
        public override void OnStart()
        {
            base.OnStart();

            mParent.SetActivHighlightObj(false);
            mParent.SetActiveTouchBlock(true);
            //mParent.SetTouchBlockAlpha(0f);

            if (Lance.Lobby.IsOpenedMenuUI() == false)
                Lance.Lobby.OpenMenuUI();
            else
                mWaitTime = 0f;
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            mWaitTime -= dt;
            if (mWaitTime <= 0f)
            {
                mIsFinish = true;
            }
        }

        public override void OnFinish()
        {
            base.OnFinish();

            mParent.SetActiveTouchBlock(false);
        }
    }
}


