using System;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Gameplay.Grid; // IGridObject için

namespace PuzzleGame.Gameplay.Managers
{
    /// <summary>
    /// Object Pooling sistemi - tekrar kullanılabilir GameObject yönetimi
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        [Serializable]
        public class PoolItem
        {
            public string key;         // Örn: "Cube"
            public GameObject prefab;  // Havuza alınacak prefab
            public int initialSize = 10;
        }

        public static PoolManager Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private List<PoolItem> poolItems = new();

        private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new();
        private readonly Dictionary<Type, string> typeToKey = new();

        //  CACHED COMPONENT REFERENCES - Performance Optimization
        private readonly Dictionary<GameObject, Collider> colliderCache = new();

        private readonly HashSet<MonoBehaviour> activeObjects = new();
        private Transform container;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            container = new GameObject("Pooled Objects").transform;
            container.parent = transform;
            container.gameObject.hideFlags = HideFlags.HideInHierarchy;

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var item in poolItems)
            {
                if (item.prefab == null || string.IsNullOrWhiteSpace(item.key))
                {
                    Debug.LogWarning($"[PoolManager] Pool item is invalid: {item.key}");
                    continue;
                }

                var queue = new Queue<GameObject>();
                for (int i = 0; i < item.initialSize; i++)
                {
                    var obj = Instantiate(item.prefab, container);
                    obj.name = item.prefab.name;
                    obj.SetActive(false);

                    //  CACHE COLLIDER COMPONENT AT INITIALIZATION
                    CacheColliderComponent(obj);

                    queue.Enqueue(obj);
                }

                poolDictionary[item.key] = queue;

                var mono = item.prefab.GetComponent<MonoBehaviour>();
                if (mono != null)
                {
                    typeToKey[mono.GetType()] = item.key;
                }
            }
        }

        //  CACHE COLLIDER COMPONENT FOR PERFORMANCE
        private void CacheColliderComponent(GameObject obj)
        {
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                colliderCache[obj] = collider;
            }
        }

        public T Get<T>() where T : MonoBehaviour
        {
            if (!typeToKey.TryGetValue(typeof(T), out string key))
            {
                Debug.LogError($"[PoolManager] No pool registered for type {typeof(T)}");
                return null;
            }

            if (!poolDictionary.TryGetValue(key, out var queue))
            {
                Debug.LogError($"[PoolManager] Pool not found for key {key}");
                return null;
            }

            GameObject obj;
            if (queue.Count > 0)
            {
                obj = queue.Dequeue();
            }
            else
            {
                //  Burada havuz taşmış oluyor
                Debug.LogWarning($"[PoolManager] Pool '{key}' boşaldı, yeni Instantiate yapılıyor!");
        
                var prefab = GetPrefabByKey(key);
                if (prefab == null) return null;
                obj = Instantiate(prefab, container);
                obj.name = prefab.name;

                // ✅ CACHE NEW OBJECT'S COLLIDER
                CacheColliderComponent(obj);
            }

            obj.SetActive(true);

            // ✅ ENABLE COLLIDER USING CACHED REFERENCE
            if (colliderCache.TryGetValue(obj, out var collider))
            {
                collider.enabled = true;
            }

            var component = obj.GetComponent<T>();
            activeObjects.Add(component);
            return component;
        }

        public void Return<T>(T obj) where T : MonoBehaviour
        {
            if (obj == null) return;

            if (!typeToKey.TryGetValue(typeof(T), out string key))
            {
                Debug.LogError($"[PoolManager] No pool registered for type {typeof(T)}");
                return;
            }

            if (!poolDictionary.TryGetValue(key, out var queue))
            {
                Debug.LogError($"[PoolManager] Pool not found for key {key}");
                return;
            }

            // 🔹 Grid'den çıkar
            if (obj is IGridObject gridObj)
            {
                GridManager.Instance?.UnregisterObject(gridObj);
            }

            // ✅ DISABLE COLLIDER USING CACHED REFERENCE - No more TryGetComponent!
            if (colliderCache.TryGetValue(obj.gameObject, out var collider))
            {
                collider.enabled = false;
            }

            obj.gameObject.SetActive(false);
            queue.Enqueue(obj.gameObject);

            activeObjects.Remove(obj);
        }

        public void ReturnAll<T>() where T : MonoBehaviour
        {
            if (!typeToKey.TryGetValue(typeof(T), out string key)) return;
            if (!poolDictionary.ContainsKey(key)) return;

            var toReturn = new List<T>(activeObjects.Count);
            foreach (var comp in activeObjects)
            {
                if (comp is T t) toReturn.Add(t);
            }

            foreach (var obj in toReturn)
            {
                Return(obj);
            }
        }

        private GameObject GetPrefabByKey(string key)
        {
            foreach (var item in poolItems)
            {
                if (item.key == key)
                    return item.prefab;
            }
            return null;
        }

        //  CLEAR CACHE WHEN OBJECTS ARE DESTROYED
        public void ClearCache()
        {
            colliderCache.Clear();
        }

        private void OnDestroy()
        {
            ClearCache();
        }
    }
}