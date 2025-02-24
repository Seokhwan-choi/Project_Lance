using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;

namespace Lance
{
    class Lance
    {
        static public Account Account;
        static public BackendManager2 BackEnd;
        static public PopupManager PopupManager;
        static public SoundManager SoundManager;
        static public AtlasManager Atlas;
        static public AdManager AdManager;
        static public SpriteLibraryManager SpriteLibrary;
        static public LocalSave LocalSave;
        static public GameData GameData;
        static public GameObject TouchBlock;
        static public GameObject HideTouchBlock;
        static public IAPManager IAPManager;
        static public ObjectPool ObjectPool;
        static public ParticleManager ParticleManager;
        static public CameraManager CameraManager;
        static public GameManager GameManager;
        static public LobbyUI Lobby;
        static public FirebaseSetup Firebase;
        static public MainThreadDispatcher MainThreadDispatcher;

        //static public FontManager Font;

        static public void InitLogin(GameObject go, Action<bool> callBack)
        {
            MainThreadDispatcher = go.AddComponent<MainThreadDispatcher>();

            LocalSave = LocalSaveUtil.Load();
            LocalSave.Normalize();

            Atlas = new AtlasManager();
            Atlas.Init();

            AdManager = new AdManager();

            SpriteLibrary = new SpriteLibraryManager();
            SpriteLibrary.Init();

            PopupManager = new PopupManager();
            PopupManager.Init();

            TouchBlock = go.FindGameObject("TouchBlock");
            TouchBlock.SetActive(false);

            HideTouchBlock = go.FindGameObject("HideTouchBlock");
            HideTouchBlock.SetActive(false);

            SoundManager = go.AddComponent<SoundManager>();
            SoundManager.Init();

            BackEnd = go.FindComponent<BackendManager2>("BackendManager");
            BackEnd.Init(callBack);
        }

        static public void InitInGame(GameObject go)
        {
            IAPManager = new IAPManager();
            IAPManager.Init();

            CameraManager = go.AddComponent<CameraManager>();
            CameraManager.Init();

            ObjectPool = new ObjectPool();

            ParticleManager = go.AddComponent<ParticleManager>();
            ParticleManager.Init();

            var canvas = GameObject.Find("Main_Canvas");
            Lobby = canvas.GetOrAddComponent<LobbyUI>();
            Lobby.Init();

            GameManager = go.AddComponent<GameManager>();
            GameManager.Init();

            AdManager.Init();
        }
    }
}