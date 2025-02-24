using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

namespace Lance
{
    class Post_RankTabUI : PostTabUI
    {
        public override void Init(PostTabUIManager parent, PostTab tab)
        {
            base.Init(parent, tab);

            var contentObj = gameObject.FindGameObject("Content");

            contentObj.AllChildObjectOff();

            mItemUIDics.Clear();

            AddPostItemUIs(PostType.Rank, contentObj.transform);
        }
    }
}
