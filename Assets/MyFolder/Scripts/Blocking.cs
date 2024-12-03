using UnityEngine;

public class Blocking : MonoBehaviour
{
    [SerializeField] private bool isTop;

    private void Awake()
    {
        transform.position = isTop ? new Vector3(transform.position.x, transform.position.y - JsonSaver.instance.settings.blockTop, transform.position.z) : new Vector3(transform.position.x, transform.position.y + JsonSaver.instance.settings.blockBottom, transform.position.z);
    }
}
