using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySFX(SfxType.Click);
    }
}