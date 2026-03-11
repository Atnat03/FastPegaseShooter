using TMPro;
using UnityEngine;

public class MovementSpeedGym : MonoBehaviour
{
    private float s_chrono; 
   [SerializeField] private TextMeshProUGUI p_display;

    private void Start()
    {
        s_chrono = 0;    
    }

    private void Update()
    {
        if (p_display != null & s_chrono != 0) p_display.text = s_chrono.ToString();
        if (s_chrono == 0) p_display.text = "";
    }
    private void OnTriggerEnter(Collider other)
    {
        s_chrono = 0;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other != null)
        {
            s_chrono += Time.deltaTime;
            Mathf.Round(s_chrono);
        }
    }
}
