using UnityEngine;
using UnityEngine.UI;

public class GrenadeThrowerView : MonoBehaviour
{
    private GrenadeThrower _thrower;
    
    [Header("UI")]
    [SerializeField] private Image _imageCooldown;
    
    //Fire
    [SerializeField] private ParticleSystem _impactParticlesPositive;
    //Ice
    [SerializeField] private ParticleSystem _impactParticlesNegative;
    
    private void UpdateCooldown(float ratio)
    {
        _imageCooldown.fillAmount = 1 - ratio;
    }
    
    private void ThrowGrenade(ElementaryGrenade grenade)
    {
        ParticleSystem GetExplosion(MagneticCharge e)
        {
            return e switch
            {
                MagneticCharge.Positive => _impactParticlesPositive,
                MagneticCharge.Negative => _impactParticlesNegative,
            };
        }
        
        grenade.SetEffect(GetExplosion(_thrower.MagneticCharge));
    }
    
    void OnEnable()
    {
        _thrower = GetComponent<GrenadeThrower>();
        
        _thrower.OnCooldownUpdate += UpdateCooldown;
        _thrower.OnThrow += ThrowGrenade;
    }

}
