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
                            // ���� ������ �߻��ߴ�. 
                            // Url�� �ٷ� ��������
                            OpenUrl();

                            onFinishReview?.Invoke();
                        }
                        else
                        {
                            // �ξ� ���� ����!
                            var reviewOperation = review.LaunchReviewFlow(reviewInfo);

                            reviewOperation.Completed += (a) =>
                            {
                                onFinishReview?.Invoke();
                            };
                        }
                    }
                    else
                    {
                        // ���� ������ �߻��ߴ�. 
                        // Url�� �ٷ� ��������
                        OpenUrl();

                        onFinishReview?.Invoke();
                    }
                };
            }
            catch (Exception e)
            {
                // Url�� �ٷ� ��������
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