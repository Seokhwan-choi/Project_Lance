using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Play.Review;
using System;

namespace Lance
{
    class InAppReviewManager
    {
        //public void RequestReview()
        //{
        //    if (IsSatisfiedReviewRequest())
        //    {
        //        string title = StringTableUtil.Get("Title_RequestReview");
        //        string desc = StringTableUtil.GetDesc("RequestReview");

        //        MonsterLandUtil.ShowConfirmPopup(title, desc, LaunchReview, FinishReview, ignoreModalTouch: true);
        //    }
        //}

        public void LaunchReview(Action onFinishReview)
        {
            try
            {
                var review = new Google.Play.Review.ReviewManager();

                var playReviewInfoAsyncOperation = review.RequestReviewFlow();

                playReviewInfoAsyncOperation.Completed += (infoAsync) =>
                {
                    if (infoAsync.Error == ReviewErrorCode.NoError)
                    {
                        var reviewInfo = infoAsync.GetResult();
                        if (reviewInfo == null)
                        {
                            // 뭔가 오류가 발생했다. 
                            // Url로 바로 보내주자
                            OpenUrl();

                            onFinishReview?.Invoke();
                        }
                        else
                        {
                            // 인앱 리뷰 실행!
                            var reviewOperation = review.LaunchReviewFlow(reviewInfo);

                            reviewOperation.Completed += (a) =>
                            {
                                onFinishReview?.Invoke();
                            };
                        }
                    }
                    else
                    {
                        // 뭔가 오류가 발생했다. 
                        // Url로 바로 보내주자
                        OpenUrl();

                        onFinishReview?.Invoke();
                    }
                };
            }
            catch (Exception e)
            {
                // Url로 바로 보내주자
                OpenUrl();

                onFinishReview?.Invoke();

                Debug.LogError(e.Message);
            }
        }

        void OpenUrl()
        {
            Application.OpenURL(Lance.GameData.CommonData.playStoreUrl);
        }
    }
}