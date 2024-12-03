using UnityEngine;

public class SetBackgroundPos : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(JsonSaver.instance.settings.backgroundPos);
        transform.position = new Vector3(transform.position.x + JsonSaver.instance.settings.backgroundPos.x, transform.position.y + JsonSaver.instance.settings.backgroundPos.y, transform.position.z);
    }
}
