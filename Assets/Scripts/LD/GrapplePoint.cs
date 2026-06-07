using System;
using System.Collections;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [SerializeField]private GameObject _canvas;
    
    [HideInInspector]public Transform p_playerTransform;
    [HideInInspector]public bool p_mustShowCanvas = false;
     public Transform p_targetTransform;
     public float detectableDistance = 50f;
     public float grappleSpeed = 50f;
     public float endGrappleImpulseForce = 50;

     private float elapsedTime = 0;

     public ParticleSystem[] grappleParticulesLookedAt;
     private bool particulesPlaying = false;

    void Start()
    {
        p_playerTransform = Camera.main?.transform;
        _canvas.SetActive(false);
    }
    
    void Update()
    {
        _canvas.SetActive(p_mustShowCanvas);
        if (p_mustShowCanvas)
        {
            //elapsedTime += Time.deltaTime;
            
            if(p_playerTransform!=null) 
                _canvas.transform.LookAt(p_playerTransform);


            if (!particulesPlaying)
            {
                foreach (ParticleSystem particule in grappleParticulesLookedAt)
                {
                    particule.gameObject.SetActive(true);
                    particule.Play();
                }

                particulesPlaying = true;
            }

            /*if (elapsedTime >= .1 && !p_mustShowCanvas)
            {
                _canvas.SetActive(false);
                p_mustShowCanvas = false;
                elapsedTime = 0;
            }*/
        }

        else if(particulesPlaying)
        {
            foreach (ParticleSystem particule in grappleParticulesLookedAt)
            {
                particule.Stop();
                particule.gameObject.SetActive(false);

            }
            particulesPlaying = false;
            
        }
    }
    
}
