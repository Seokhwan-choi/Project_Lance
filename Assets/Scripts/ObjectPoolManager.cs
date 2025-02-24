using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Lance
{
    // ���� �� �� ������ �ʿ���
    class ObjectPoolManager
    {
        public class PoolEntry
        {
            public GameObject Prefab;
            public Stack<GameObject> Items = new Stack<GameObject>();
        }

        int mUniqueId;
        // ������Ʈ Ǯ
        Dictionary<string, PoolEntry> mPathToPool;
        Dictionary<GameObject, PoolEntry> mActiveObjects;

        Transform mContainer;

        public ObjectPoolManager(Transform container)
        {
            mUniqueId = 100;
            mContainer = container;

            mPathToPool = new Dictionary<string, PoolEntry>();
            mActiveObjects = new Dictionary<GameObject, PoolEntry>();
        }

        public GameObject Acquire(string prefabPath, Transform parent = null)
        {
            mPathToPool.TryGetValue(prefabPath, out PoolEntry pool);
            if (pool == null)
            {
                GameObject prefabObj = Resources.Load(prefabPath) as GameObject;
                if (prefabObj == null)
                    return null;

                pool = new PoolEntry() { Prefab = prefabObj };

                mPathToPool.Add(prefabPath, pool);
            }

            return Acquire(pool, parent);
        }

        public GameObject Acquire(PoolEntry pool, Transform parent = null)
        {
            Debug.Assert(pool != null);

            GameObject go;

            if (pool.Items.Count > 0)
            {
                go = pool.Items.Pop();
            }
            else
            {
                go = Util.Instantiate(pool.Prefab, parent);
                go.name = $"{pool.Prefab.name}_{mUniqueId++}";
            }

            go.transform.SetParent(parent, false);

            go.SetActive(true);

            mActiveObjects[go] = pool;

            return go;
        }

        public void Release(GameObject go)
        {
            mActiveObjects.TryGetValue(go, out PoolEntry pool);
            if (pool != null)
            {
                // �ݳ�
                go.transform.SetParent(mContainer, false);

                // ��Ȱ��ȭ
                go.SetActive(false);

                pool.Items.Push(go);
                mActiveObjects.Remove(go);
            }
            else
            {
                Debug.LogError($"Ȱ��ȭ �ȵ� ������Ʈ�� Release�Ϸ��� �õ���. ({go.name})");
            }
        }

        public void DestroyAllObjects()
        {
            foreach (PoolEntry pool in mPathToPool.Values)
            {
                foreach (var go in pool.Items)
                {
                    go.Destroy();
                }

                pool.Items.Clear();
            }

            foreach (var item in mActiveObjects)
            {
                item.Key.Destroy();
            }

            mPathToPool.Clear();
            mActiveObjects.Clear();
        }
    }
}