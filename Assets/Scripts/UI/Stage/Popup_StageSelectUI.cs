using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Mosframe;

namespace Lance
{
    class Popup_StageSelectUI : PopupBase
    {
        StageDifficulty mSelectedDiff;
        int mSelectedChapter;

        Image mImageStageBackground;
        DynamicVScrollView mScrollView;
        TextMeshProUGUI mTextDiffName;
        List<ChapterSelectButtonUI> mChapterSelectButtonList;

        StageData[] mStageDatas;

        public void Init()
        {
            SetUpCloseAction();
            SetTitleText(StringTableUtil.Get("Title_StageSelect"));

            mScrollView = gameObject.FindComponent<DynamicVScrollView>("ScrollView");
            mScrollView.totalItemCount = Lance.GameData.StageCommonData.maxStage;

            mImageStageBackground = gameObject.FindComponent<Image>("StageBackground");

            mTextDiffName = gameObject.FindComponent<TextMeshProUGUI>("Text_DiffName");

            var buttonPrevDiff = gameObject.FindComponent<Button>("Button_Prev");
            buttonPrevDiff.SetButtonAction(() => ChangeDiff(isPrev: true));

            var buttonNextDiff = gameObject.FindComponent<Button>("Button_Next");
            buttonNextDiff.SetButtonAction(() => ChangeDiff(isPrev: false));

            mChapterSelectButtonList = new List<ChapterSelectButtonUI>();
            for(int i = 0; i < Lance.GameData.StageCommonData.maxChapter; ++i)
            {
                int chapter = i + 1;

                var buttonObj = gameObject.FindGameObject($"Button_ChapSelect_{chapter}");

                var buttonUI = buttonObj.GetOrAddComponent<ChapterSelectButtonUI>();
                buttonUI.Init(chapter, ChangeChapter);

                mChapterSelectButtonList.Add(buttonUI);
            }

            var buttonCurrentStage = gameObject.FindComponent<Button>("Button_BestStage");
            buttonCurrentStage.SetButtonAction(ChangeToBestStage);

            mSelectedDiff = Lance.Account.StageRecords.GetCurDifficulty();
            mSelectedChapter = Lance.Account.StageRecords.GetCurChapter();

            mStageDatas = DataUtil.GetStageDatas(mSelectedDiff, mSelectedChapter).ToArray();

            StartCoroutine(DelayedInit());
        }

        public override void Close(bool immediate = false, bool hideMotion = true)
        {
            base.Close(immediate, hideMotion);

            mStageDatas = null;
        }

        IEnumerator DelayedInit()
        {
            yield return null;

            Refresh();

            yield return null;

            mScrollView.scrollByItemIndex(Lance.Account.StageRecords.GetCurStage() - 1);
        }

        public StageData GetSelectedStageData(int index)
        {
            return mStageDatas[index];
        }

        void ChangeDiff(bool isPrev)
        {
            int newDiff = (int)mSelectedDiff + (isPrev ? -1 : 1);

            newDiff = Mathf.Clamp(newDiff, 0, (int)StageDifficulty.Count - 1);

            if ((int)mSelectedDiff == newDiff)
                return;

            mSelectedDiff = (StageDifficulty)newDiff;
            mStageDatas = null;
            mStageDatas = DataUtil.GetStageDatas(mSelectedDiff, mSelectedChapter).ToArray();

            mScrollView.scrollByItemIndex(0);

            Refresh();
        }

        public void OnStageSelect(StageData stageData)
        {
            Close();

            Lance.GameManager.ChangeStage(stageData);
        }

        void ChangeChapter(int chapter)
        {
            if (mSelectedChapter == chapter)
                return;

            mSelectedChapter = chapter;
            mStageDatas = null;
            mStageDatas = DataUtil.GetStageDatas(mSelectedDiff, mSelectedChapter).ToArray();

            mScrollView.scrollByItemIndex(0);

            Refresh();
        }

        void ChangeToBestStage()
        {
            mSelectedDiff = Lance.Account.StageRecords.GetBestDifficulty();
            mSelectedChapter = Lance.Account.StageRecords.GetBestChapter();
            mStageDatas = null;
            mStageDatas = DataUtil.GetStageDatas(mSelectedDiff, mSelectedChapter).ToArray();

            Refresh();

            int stage = Lance.Account.StageRecords.GetBestStage();

            stage = Mathf.Min(stage, StageRecordsUtil.GetMaxTotalStage());

            int index = stage - 1;

            mScrollView.scrollByItemIndex(index);
        }

        void Refresh()
        {
            // 선택한 난이도 이름
            string diffName = StringTableUtil.GetName($"{mSelectedDiff}");
            //StringParam param = new StringParam("diffName", diffName);
            mTextDiffName.text = diffName;

            mImageStageBackground.sprite = Lance.Atlas.GetUISprite($"Image_Stage_Thumbnail_{mSelectedChapter:00}");

            // 선택한 챕터
            foreach (var button in mChapterSelectButtonList)
            {
                button.SetActiveFrame(button.Chapter == mSelectedChapter);
            }

            mScrollView.refresh();
        }
    }
}