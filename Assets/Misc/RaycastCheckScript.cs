using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastTest : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{gameObject.name} clicked!");
    }
}