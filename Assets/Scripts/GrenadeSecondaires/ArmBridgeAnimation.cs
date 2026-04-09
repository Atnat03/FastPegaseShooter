using FishNet.Object;
using UnityEngine;

public class ArmBridgeAnimation : NetworkBehaviour
{
    [SerializeField] private GrenadeThrower _thrower;
    [SerializeField] private Animator _animator;
    [SerializeField] private MeshRenderer _ballInHand;
    
    public void StartThrow(MagneticCharge e)
    {
        Color GetColor(MagneticCharge e)
        {
            return e switch
            {
                MagneticCharge.Positive => Color.red,
                MagneticCharge.Negative => Color.blue,
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
