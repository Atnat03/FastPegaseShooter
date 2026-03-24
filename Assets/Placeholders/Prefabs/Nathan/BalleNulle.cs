using UnityEngine;

public class BalleNulle : MonoBehaviour
{
    public float speed = 40f;
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        VieNulle enemy = other.GetComponent<VieNulle>();

        Debug.Log("please" + other.name);
        if (enemy != null)
        {
            Debug.Log("mmh");
            enemy.TakeDamage(1);
        }

        Destroy(gameObject);
    }
}
