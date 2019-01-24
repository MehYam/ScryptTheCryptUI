using UnityEngine;
using UnityEngine.UI;

public class Nameplate : MonoBehaviour
{
    public HealthBar HealthBar { get { return GetComponentInChildren<HealthBar>(); } }
    public Text Name { get { return GetComponentInChildren<Text>(); } }
}
