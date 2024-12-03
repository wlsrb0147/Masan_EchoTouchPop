using UnityEngine;
using UnityEngine.EventSystems;

public class CleanObj : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] Parent parent;

    public void OnPointerClick(PointerEventData eventData)
    {
        parent.SetCleanOn();
    }
}
