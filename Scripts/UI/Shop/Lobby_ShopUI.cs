using System.Collections;

namespace Lance
{
    class Lobby_ShopUI : LobbyTabUI
    {
        ShopTabUIManager mTabUIManager;
        public override void Init(LobbyTab tab)
        {
            base.Init(tab);

            mTabUIManager = new ShopTabUIManager();
            mTabUIManager.Init(gameObject);
        }

        public override void InitFinished()
        {
            base.InitFinished();

            mTabUIManager.InitFinished();
        }

        public override void Localize()
        {
            mTabUIManager.Localize();
        }

        public override void OnEnter()
        {
            mTabUIManager.Refresh();
        }

        public override void OnUpdate()
        {
            mTabUIManager.OnUpdate();
        }

        public override void RefreshRedDots()
        {
            mTabUIManager.RefrehRedDots();
        }
    }
}