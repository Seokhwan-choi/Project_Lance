using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Messaging;
using Firebase.Extensions;
using System;
using Firebase;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Concurrent;


namespace Lance
{
    // MainThreadDispatcher.cs
    class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();

        public void Update()
        {
            while (_executionQueue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        public void QueueOnMainThread(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _executionQueue.Enqueue(action);
        }

        private void OnDisable()
        {
            _executionQueue.Clear();
        }
    }

    class FirebaseSetup
    {
        bool mInit;
        ObscuredString mDeviceToken;
        public void Init()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                // Create and hold a reference to your FirebaseApp,
                // FirebaseApp에 대한 참조를 생성하고 보관합니다.
                // where app is a Firebase.FirebaseApp property of your application class.
                // 여기서 app은 애플리케이션 클래스의 Firebase.FirebaseApp 속성입니다.
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    Lance.MainThreadDispatcher.QueueOnMainThread(() =>
                    {
                        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                        Crashlytics.IsCrashlyticsCollectionEnabled = true;
                        Debug.Log("Firebase initialized successfully on main thread");

                        // Firebase 초기화
                        FirebaseApp app = FirebaseApp.DefaultInstance;
                        // FCM 토큰 가져오기
                        GetToken();

                        mInit = true;

                        // Set a flag here to indicate whether Firebase is ready to use by your app.
                        // 앱에서 Firebase를 사용할 준비가 되었는지 여부를 나타내는 플래그를 여기에 설정합니다
                    });
                }
                else
                {
                    // Firebase Unity SDK is not safe to use here.
                    // Firebase Unity SDK는 여기서 사용하기에 안전하지 않습니다.
                    Debug.Log($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        void GetToken()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;

            // 해당 task는 외부쓰레드로 작동합니다.
            // 만약 GameObject.Instantiate 같은 유니티 함수나 UnityEngine.UI를 사용할 경우, 예외가 발생합니다.
            FirebaseMessaging.GetTokenAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    mDeviceToken = task.Result;

                    //Debug.LogError(mDeviceToken.ToString());
                    //Debug.LogError(Backend.Android.GetDeviceToken());
                }
                else
                {
                    Debug.Log("Failed to get FCM token");
                }
            });
        }

        public void SetPush(bool on, Action<bool> onFinish, bool ignoreTouchBlock = false)
        {
            if (on)
            {
                if (mDeviceToken.IsValid())
                {
                    Lance.MainThreadDispatcher.QueueOnMainThread(() =>
                    {
                        SendQueue.Enqueue(Backend.Android.PutDeviceToken, mDeviceToken.ToString(), (callback) =>
                        {
                            if (ignoreTouchBlock == false)
                                Lance.TouchBlock.SetActive(true);

                            if (callback.IsSuccess())
                            {
                                Lance.Account.UserInfo.SetPushAllow(true);

                                UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("IsAllowPush"));
                            }
                            else
                            {
                                UIUtil.ShowSystemDefaultErrorMessage();
                            }

                            if (ignoreTouchBlock == false)
                                Lance.TouchBlock.SetActive(false);

                            onFinish?.Invoke(callback.IsSuccess());
                        });
                    });
                }
                else
                {
                    onFinish?.Invoke(false);
                }
            }
            else
            {
                Lance.MainThreadDispatcher.QueueOnMainThread(() =>
                {
                    SendQueue.Enqueue(Backend.Android.DeleteDeviceToken, (callback) =>
                    {
                        if (ignoreTouchBlock == false)
                            Lance.TouchBlock.SetActive(true);

                        if (callback.IsSuccess())
                        {
                            Lance.Account.UserInfo.SetPushAllow(false);

                            UIUtil.ShowSystemMessage(StringTableUtil.GetSystemMessage("IsNotAllowPush"));
                        }
                        else
                        {
                            UIUtil.ShowSystemDefaultErrorMessage();
                        }

                        if (ignoreTouchBlock == false)
                            Lance.TouchBlock.SetActive(false);

                        onFinish?.Invoke(callback.IsSuccess());
                    });
                });
            }
        }

        void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {

        }

        public void LogEvent(string name, string paramName, int paramValue)
        {
            if (mInit == false)
                return;

            Lance.MainThreadDispatcher.QueueOnMainThread(() =>
            {
                FirebaseAnalytics.LogEvent(name, paramName, paramName);
            });
        }

        public void LogEvent(string name)
        {
            if (mInit == false)
                return;

            Lance.MainThreadDispatcher.QueueOnMainThread(() =>
            {
                FirebaseAnalytics.LogEvent(name);
            });
        }
    }
}