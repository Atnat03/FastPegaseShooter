using UnityEngine;

public class EventBusInitialiser : MonoBehaviour
{
    public static EventBusInitialiser instance;
    public EventBus Bus;
    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
        //Bus Creation
        Bus = new EventBus();

        //ShootingService shootingService = new ShootingService(Bus);
    }
}
