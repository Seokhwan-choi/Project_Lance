using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Lance
{
    class PostItemUI : MonoBehaviour
    {
        PostItem mItem;
        Action mOnReceive;
        public void Init(PostItem item, Action onReceive)
        {
            mItem = item;
            mOnReceive = onReceive;

            TextMeshProUGUI textTitle = gameObject.FindComponent<TextMeshProUGUI>("Text_Title");
            textTitle.text = item.title;

            TextMeshProUGUI textExpireTime = gameObject.FindComponent<TextMeshProUGUI>("Text_ExpireTime");

            StringParam expireParam = new StringParam("expireTime", item.expirationDate);

            textExpireTime.text = StringTableUtil.Get("UIString_ExpireTime", expireParam);

            var rewards = gameObject.FindGameObject("Rewards");

            rewards.AllChildObjectOff();

            foreach (var postReward in mItem.GetRewards())
            {
                var itemInfos = postReward.GetItemInfos();
                foreach(var itemInfo in itemInfos)
                {
                    var postRewardSlotObj = Util.InstantiateUI("PostRewardSlotUI", rewards.transform);

                    var postRewardSlotUI = postRewardSlotObj.GetOrAddComponent<ItemSlotUI>();

                    postRewardSlotUI.Init(itemInfo);
                }
            }

            var buttonReceive = gameObject.FindComponent<Button>("Button_Receive");
            buttonReceive.SetButtonAction(OnReceiveButton);
        }

        void OnReceiveButton()
        {
            try
            {
                // PostItem 객체에서 우편 받기 함수 수령후 결과값 전송
                mItem.ReceiveReward((isSuccess) => {
                    if (isSuccess)
                    {
                        // 우편을 지워주자
                        mOnReceive.Invoke();
                    }
                });
            }
            catch (Exception e)
            {
                // 뭔가 잘 못되었다
                UIUtil.ShowSystemDefaultErrorMessage();
            }
        }
    }

}
