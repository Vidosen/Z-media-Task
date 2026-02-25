using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZMediaTask.Presentation.Services
{
    public sealed class VfxPool : MonoBehaviour
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;

            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }

            for (var i = 0; i < count; i++)
            {
                var instance = Instantiate(prefab, transform);
                instance.SetActive(false);
                queue.Enqueue(instance);
            }
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }

            GameObject instance;
            if (queue.Count > 0)
            {
                instance = queue.Dequeue();
                instance.transform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                instance = Instantiate(prefab, position, rotation, transform);
            }

            instance.SetActive(true);
            return instance;
        }

        public void Return(GameObject instance)
        {
            if (instance == null) return;

            instance.SetActive(false);
        }

        public void ReturnAfterDelay(GameObject prefab, GameObject instance, float delay)
        {
            if (instance == null) return;

            StartCoroutine(ReturnAfterDelayCoroutine(prefab, instance, delay));
        }

        public void ClearAll()
        {
            foreach (var pair in _pools)
            {
                while (pair.Value.Count > 0)
                {
                    var instance = pair.Value.Dequeue();
                    if (instance != null)
                    {
                        Destroy(instance);
                    }
                }
            }

            _pools.Clear();
        }

        private IEnumerator ReturnAfterDelayCoroutine(GameObject prefab, GameObject instance, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (instance == null) yield break;

            instance.SetActive(false);

            if (prefab != null && _pools.TryGetValue(prefab, out var queue))
            {
                queue.Enqueue(instance);
            }
        }

        private void OnDestroy()
        {
            ClearAll();
        }
    }
}
