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
                // FirebaseApp�� ���� ������ �����ϰ� �����մϴ�.
                // where app is a Firebase.FirebaseApp property of your application class.
                // ���⼭ app�� ���ø����̼� Ŭ������ Firebase.FirebaseApp �Ӽ��Դϴ�.
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    Lance.MainThreadDispatcher.QueueOnMainThread(() =>
                    {
                        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                        Crashlytics.IsCrashlyticsCollectionEnabled = true;
                        Debug.Log("Firebase initialized successfully on main thread");

                        // Firebase �ʱ�ȭ
                        FirebaseApp app = FirebaseApp.DefaultInstance;
                        // FCM ��ū ��������
                        GetToken();

                        mInit = true;

                        // Set a flag here to indicate whether Firebase is ready to use by your app.
                        // �ۿ��� Firebase�� ����� �غ� �Ǿ����� ���θ� ��Ÿ���� �÷��׸� ���⿡ �����մϴ�
                    });
                }
                else
                {
                    // Firebase Unity SDK is not safe to use here.
                    // Firebase Unity SDK�� ���⼭ ����ϱ⿡ �������� �ʽ��ϴ�.
                    Debug.Log($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        void GetToken()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;

            // �ش� task�� �ܺξ������ �۵��մϴ�.
            // ���� GameObject.Instantiate ���� ����Ƽ �Լ��� UnityEngine.UI�� ����� ���, ���ܰ� �߻��մϴ�.
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