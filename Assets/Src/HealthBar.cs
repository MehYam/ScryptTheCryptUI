using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] GameObject FGImage;

    public float Percent
    {
        set
        {
            var image = FGImage.GetComponent<Image>();
            image.fillAmount = Mathf.Clamp(value, 0, 1);
        }
    }
}
