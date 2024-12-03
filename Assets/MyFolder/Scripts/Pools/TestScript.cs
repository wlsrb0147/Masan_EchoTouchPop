using UnityEngine;

public class TestScript : MonoBehaviour
{
    private PoolManager _poolManager;

    private void Awake()
    {
        _poolManager = PoolManager.Instance;
    }

    public void CreatePool()
    {
        
        _poolManager.Get();
    }
}
