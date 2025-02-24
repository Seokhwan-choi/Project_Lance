using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lance
{
    class ParticleManager : MonoBehaviour
    {
        List<GameObject> mParticles;
        List<GameObject> mUIParticles;

        public void Init()
        {
            mParticles = new List<GameObject>();
            mUIParticles = new List<GameObject>();
        }

        public GameObject Aquire(string name, Transform parent = null)
        {
            var particle = Lance.ObjectPool.AcquireObject($"FX/{name}", parent);

            mParticles.Add(particle);

            return particle;
        }

        public GameObject AquireUI(string name, RectTransform anchor, Vector3? anchoredPos = null)
        {
            var particle = Lance.ObjectPool.AcquireUI($"FX/{name}", anchor);

            var ps = particle.GetComponent<ParticleSystem>();

            ps.Play();

            if (anchoredPos != null)
            {
                var psTm = ps.GetComponent<RectTransform>();

                psTm.anchoredPosition = anchoredPos.Value;
            }

            mUIParticles.Add(particle);

            return particle;
        }

        public GameObject Aquire(string name, Transform parent = null, Vector3? pos = null)
        {
            var particle = Aquire(name, parent);

            if (pos != null)
            {
                particle.transform.localPosition = pos.Value;
            }

            return particle;
        }


        void Release(GameObject go)
        {
            if (IsValid(go))
            {
                mParticles.Remove(go);

                Lance.ObjectPool.ReleaseObject(go);
            }
        }

        public void ReleaseUI(GameObject go)
        {
            if (IsValidUI(go))
            {
                mUIParticles.Remove(go);

                Lance.ObjectPool.ReleaseUI(go);
            }
        }

        bool IsValid(GameObject go)
        {
            return go != null &&
                   mParticles.Contains(go);
        }

        bool IsValidUI(GameObject go)
        {
            return go != null &&
                   mUIParticles.Contains(go);
        }


        void Update()
        {
            for (int i = 0; i < mParticles.Count; ++i)
            {
                var particle = mParticles[i];
                if (IsValid(particle) && AllIsStopped(particle))
                {
                    Release(particle);

                    i--;
                }
            }

            for (int i = 0; i < mUIParticles.Count; ++i)
            {
                var particleUI = mUIParticles[i];
                if (IsValidUI(particleUI) && AllIsStopped(particleUI))
                {
                    ReleaseUI(particleUI);

                    i--;
                }
            }

            bool AllIsStopped(GameObject particleObj)
            {
                foreach(var ps in particleObj.GetComponentsInChildren<ParticleSystem>())
                {
                    if (ps.isStopped == false)
                        return false;
                }

                return true;
            }
        }
    }
}