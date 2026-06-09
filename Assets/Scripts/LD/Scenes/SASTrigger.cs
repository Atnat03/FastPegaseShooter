using UnityEngine;

public class SASTrigger : NeedTwoPlayerBehaviour
{
    public GameObject sasDoor;
    
    protected override void OnTwoPlayerFunction()
    { 
        sasDoor.SetActive(true);
    }
}
