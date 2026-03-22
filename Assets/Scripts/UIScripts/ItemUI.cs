using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI cantidadText;

    public void Setup(ItemData item, int cantidad)
    {
        icon.sprite = item.icon;
        icon.enabled = true;

        cantidadText.text = cantidad > 1 ? cantidad.ToString() : "";
    }
}