using System;

namespace Lance
{
    static class StageRecordsUtil
    {
        public static string GetCurStageInfoToString(bool includeChapName = true)
        {
            int totalStage = Lance.Account.StageRecords.GetCurTotalStage();

            (StageDifficulty diff, int chapter, int stage) result = SplitTotalStage(totalStage);

            return ChangeStageInfoToString(result, includeChapName);
        }

        public static string GetBestStageInfoToString(bool includeChapName = true)
        {
            int totalStage = Lance.Account.StageRecords.GetBestTotalStage();

            (StageDifficulty diff, int chapter, int stage) result = SplitTotalStage(totalStage);

            return ChangeStageInfoToString(result, includeChapName);
        }

        public static bool IsCurrentStageData(StageData stageData)
        {
            int totalStage = Lance.Account.StageRecords.GetCurTotalStage();

            (StageDifficulty diff, int chapter, int stage) result = SplitTotalStage(totalStage);

            return stageData.diff == result.diff &&
                stageData.chapter == result.chapter &&
                stageData.stage == result.stage;
        }

        public static string ChangeStageInfoToString((StageDifficulty diff, int chapter, int stage) stageInfo, bool includeChapName = true)
        {
            string diffName = StringTableUtil.Get($"Name_{stageInfo.diff}");
            int chapter = stageInfo.chapter;
            int stage = stageInfo.stage;
            string chapterName = StringTableUtil.Get($"UIString_ChapterName_{chapter}");

            if (includeChapName)
            {
                return $"[{diffName}] {chapter}-{stage} {chapterName}";
            }
            else
            {
                return $"[{diffName}] {chapter}-{stage}";
            }
        }

        public static int CalcTotalStage(StageDifficulty diff, int chapter, int stage)
        {
            int maxStage = Lance.GameData.StageCommonData.maxStage;
            int maxChapter = Lance.GameData.StageCommonData.maxChapter;

            int diffStage = (int)diff * maxChapter * maxStage;
            int chapStage = (chapter - 1) * maxStage;

            return diffStage + chapStage + stage;
        }

        public static (StageDifficulty diff, int chapter, int stage) SplitTotalStage(int totalStage)
        {
            int maxStage = Lance.GameData.StageCommonData.maxStage;
            int maxChapter = Lance.GameData.StageCommonData.maxChapter;
            int diffTotalStage = maxChapter * maxStage;

            totalStage = Math.Min(totalStage, GetMaxTotalStage());

            // 총 스테이지 난이도, 챕터, 스테이지로 환산
            StageDifficulty calcDiff = (StageDifficulty)(((totalStage - 1) / diffTotalStage));
            int calcChapter = (((totalStage - 1) / maxStage) % maxChapter) + 1;
            int calcStage = ((totalStage - 1) % maxStage) + 1;

            return (calcDiff, calcChapter, calcStage);
        }

        public static int GetMaxTotalStage()
        {
            int maxDiff = (int)StageDifficulty.Count;
            int maxChapter = Lance.GameData.StageCommonData.maxChapter;
            int maxStage = Lance.GameData.StageCommonData.maxStage;

            return maxDiff * maxChapter * maxStage;
        }
    }
}
