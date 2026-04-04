using System;
using FishNet.Object;
using MyPrint;
using UnityEngine;

public class ElementaryGrenade : MonoBehaviour
{
    [SerializeField] private MeshRenderer _model;
    
    private Element _element;
    private float _radius;
    private float _maxNumberTouch;
    private float _currentNumberTouch;
    private int _damage;
    private int _networkIdAttacker;
    
    [Header("Effect")]
    private ParticleSystem _particlesExplosion;
    
    Vector3 _lastPosition;
    bool _hasHit;
    RaycastHit hit;

    public void Initialize(Element element, int damage, float radius, int numberWallTouch, int netID)
    {
        _element = element;
        _radius = radius;
        _maxNumberTouch = numberWallTouch;
        _currentNumberTouch = 0;
        _damage = damage;
        _networkIdAttacker = netID;
        
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
        
        _model.material.color = GetColor(_element);
    }

    public void SetEffect(ParticleSystem explosion)
    {
        _particlesExplosion = explosion;
    }
    
    void FixedUpdate()
    {
        /*DetectCollision();
        _lastPosition = transform.position;*/
    }

    private void DetectCollision()
    {
        Vector3 direction = transform.position - _lastPosition;
        float distance = direction.magnitude;

        if (distance <= 0f) return;

        if (Physics.SphereCast(_lastPosition, 0.15f, direction.normalized, out hit,
                distance, ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore))
        {
            if (_hasHit) return;
            _hasHit = true;

            if (hit.collider.TryGetComponent(out IDamagable d))
            {
                Cons.Print("Grenade Collided with " + d.GetType().Name, ColorConsole.Grey);
                
                if(!hit.collider.CompareTag("Player"))
                    d.TakeDamage(_damage, _networkIdAttacker);
            }

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, _radius);

        foreach (Collider col in cols)
        {
            // /!\ a remplacer par la nouvelle interface pour les effets des ennemis
            
            if (col.TryGetComponent(out IDamagable d))
            {
                Cons.Print("Grenade Collided with " + d.GetType().Name, ColorConsole.Grey);
                
                if(!col.CompareTag("Player"))
                    d.TakeDamage(_damage, _networkIdAttacker);
            }
        }
        
        Instantiate(_particlesExplosion, transform.position + other.contacts[0].normal * 0.2f, Quaternion.identity);
        
        Destroy(gameObject);
    }
}
