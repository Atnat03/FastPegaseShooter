using FishNet.Object;
using UnityEngine;

public class ArmBridgeAnimation : NetworkBehaviour
{
    [SerializeField] private GrenadeThrower _thrower;
    [SerializeField] private Animator _animator;
    [SerializeField] private MeshRenderer _ballInHand;
    
    public void StartThrow(Element e)
    {
        Color GetColor(Element e)
        {
            return e switch
            {
                Element.Fire => Color.red,
                Element.Electric => Color.yellow,
                Element.Ice => Color.blue,
                _ => Color.white
            };
        }
        
        _ballInHand.material.color = GetColor(e);
        
        _animator.SetTrigger("Throw");
    }

    public void Throw()
    {
        _thrower.ThrowGrenadeServerRpc();
    }
}
