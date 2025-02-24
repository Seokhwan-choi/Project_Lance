using System.Linq;
using Mosframe;


namespace Lance
{
    class Popup_RaidDungeonRewardInfoUI : PopupBase
    {
        RaidRewardData[] mRaidRewardDatas;
        DynamicVScrollView mScrollView;
        public void Init(ElementalType type)
        {
            SetUpCloseAction();

            SetTitleText(StringTableUtil.Get("Title_RaidBossReward"));

            mRaidRewardDatas = DataUtil.GetRaidRewardDatas(type).ToArray();

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            mScrollView.totalItemCount = mRaidRewardDatas.Length;
        }

        public RaidRewardData GetRaidRewardData(int index)
        {
            if (mRaidRewardDatas.Length <= index || index < 0)
                return null;

            return mRaidRewardDatas[index];
        }
    }
}