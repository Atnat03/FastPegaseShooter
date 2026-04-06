using UnityEngine;
using UnityEngine.UI;

public class GrenadeThrowerView : MonoBehaviour
{
    private GrenadeThrower _thrower;
    
    [Header("UI")]
    [SerializeField] private Image _imageCooldown;
    
    //Fire
    [SerializeField] private ParticleSystem _impactParticlesFire;
    //Electik
    [SerializeField] private ParticleSystem _impactParticlesElectic;
    //Ice
    [SerializeField] private ParticleSystem _impactParticlesIce;
    
    private void UpdateCooldown(float ratio)
    {
        _imageCooldown.fillAmount = 1 - ratio;
    }
    
    private void ThrowGrenade(ElementaryGrenade grenade)
    {
        ParticleSystem GetExplosion(Element e)
        {
            return e switch
            {
                Element.Fire => _impactParticlesFire,
                Element.Electric => _impactParticlesElectic,
                Element.Ice => _impactParticlesIce,
            };
        }
        
        grenade.SetEffect(GetExplosion(_thrower.Element));
    }
    
    void OnEnable()
    {
        _thrower = GetComponent<GrenadeThrower>();
        
        _thrower.OnCooldownUpdate += UpdateCooldown;
        _thrower.OnThrow += ThrowGrenade;
    }

}
