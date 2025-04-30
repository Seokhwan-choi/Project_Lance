using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;


namespace Lance
{
    class SpriteLibraryManager
    {
        Dictionary<string, SpriteLibraryAsset> mLibraryDics;
        public void Init()
        {
            mLibraryDics = new Dictionary<string, SpriteLibraryAsset>();

            SpriteLibraryAsset[] librarys = Resources.LoadAll<SpriteLibraryAsset>("SpriteLibrarys");

            foreach(SpriteLibraryAsset library in librarys)
            {
                mLibraryDics.Add(library.name, library);
            }
        }

        public SpriteLibraryAsset GetAsset(string id)
        {
            return mLibraryDics.TryGet(id);
        }

        public SpriteLibraryAsset GetCostumeLibraryAsset(string libraryId)
        {
            return GetAsset(libraryId);
        }

        //public SpriteLibraryAsset GetBodyCostumeLibraryAssetsByCostumeId(string costumeId)
        //{

        //}

        //public ( body, SpriteLibraryAsset hand) 
        //{
        //    // 착용중인 코스튬이 있다면 해당 코스튬으로 변경해주자.
        //    var costumeLibraryAssetData = Lance.GameData.LibraryAssetData.TryGet(libraryId);
        //    if (costumeLibraryAssetData != null)
        //    {
        //        string costumeBodyLibraryAsset = costumeLibraryAssetData.libraryAsset;
        //        string costumeHandLibraryAsset = $"{costumeBodyLibraryAsset}_Hand";

        //        return (GetAsset(costumeBodyLibraryAsset), GetAsset(costumeHandLibraryAsset));
        //    }
        //    else
        //    {
        //        var libraryAssetData = Lance.GameData.LibraryAssetData.TryGet(Lance.GameData.CostumeCommonData.playerDefaultLibraryBodyAsset);

        //        string bodyLibraryAsset = libraryAssetData.libraryAsset;
        //        string handLibraryAsset = $"{bodyLibraryAsset}_Hand";

        //        return (GetAsset(bodyLibraryAsset), GetAsset(handLibraryAsset));
        //    }
        //}
    }
    
}