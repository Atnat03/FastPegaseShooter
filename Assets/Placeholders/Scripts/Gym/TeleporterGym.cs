using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeleporterGym : MonoBehaviour
{
    [SerializeField] private Transform p_zoneToTp;
    [SerializeField] private TextMeshProUGUI p_display;

    private void Awake()
    {
        if(p_zoneToTp != null) p_display.text = p_zoneToTp.gameObject.name;
    }
        
    private void OnTriggerEnter(Collider other)
    {
        other.transform.parent.transform.parent.position = p_zoneToTp.position;
    }
}
