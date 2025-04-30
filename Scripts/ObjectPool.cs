using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class ObjectPool
    {
        ObjectPoolManager mObjPool;
        ObjectPoolManager mUIPool;

        public ObjectPool()
        {
            mObjPool = new ObjectPoolManager((new GameObject("ObjectPool")).transform);
            mUIPool = new ObjectPoolManager(GameObject.Find("UIObjectPool").transform);
        }

        public GameObject AcquireObject(string prefabPath, Transform parent = null)
        {
            return mObjPool.Acquire(prefabPath, parent);
        }

        public void ReleaseObject(GameObject go)
        {
            mObjPool.Release(go);
        }

        public GameObject AcquireUI(string prefabPath, RectTransform parent = null)
        {
            if (parent == null)
                parent = GameObject.Find("Main_Canvas")?.GetComponent<RectTransform>() ?? null;

            return mUIPool.Acquire("Prefabs/UI/" + prefabPath, parent);
        }

        public void ReleaseUI(GameObject go)
        {
            mUIPool.Release(go);
        }

        public void Clear(bool obj = true, bool ui = true)
        {
            if (obj)
                mObjPool.DestroyAllObjects();

            if (ui)
                mUIPool.DestroyAllObjects();

            Resources.UnloadUnusedAssets();
        }
    }
}