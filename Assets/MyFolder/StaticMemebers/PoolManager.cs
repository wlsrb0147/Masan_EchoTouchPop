using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance {get; private set;}
    
    // pool data
    private ObjectPool<GameObject> _pool;
    private int _defaultPoolSize;
    private int _maxSpawnPerWave;
    [SerializeField] private GameObject poolPrefab;
    [SerializeField] private Transform parent;
    private readonly List<GameObject> _pooledObjects = new ();

    private float _pos;
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _pool = new ObjectPool<GameObject>(
            createFunc: OnCreate,
            actionOnGet: ActionOnGet,
            actionOnRelease: ActionOnRelease,
            collectionCheck: true,
            defaultCapacity: _defaultPoolSize
            );

        _pos = 540f - JsonSaver.instance.settings.blockTop;
    }
    
    private GameObject OnCreate()
    {
        GameObject obj = Instantiate(poolPrefab,parent);
        Debug.Log("Created");
        return obj;
    }
    
    private void ActionOnGet(GameObject obj)
    {
        Debug.Log("ActionOnGet");
        _pooledObjects.Add(obj);
        float spawnPosX = Random.Range(-960+80, 960-80);
        Vector2 spawnPos = new Vector2(spawnPosX, _pos+75);
        RectTransform trs = obj.GetComponent<RectTransform>();
        trs.anchoredPosition = spawnPos;
        obj.SetActive(true);
    }
    
    private void ActionOnRelease(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void Get()
    {
        Debug.Log("Get");
        _pool.Get();
    }

    public void ReleaseAndRemoveList(GameObject obj)
    {
        _pooledObjects.Remove(obj);
        _pool.Release(obj);
    }

    public void ReturnAllPooledObjects()
    {
        foreach (var obj in _pooledObjects)
        {
            if (obj.activeInHierarchy)
            {
                _pool.Release(obj);
            }
        }
        
        _pooledObjects.Clear();
    }
}
