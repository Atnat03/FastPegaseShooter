using System;
using UnityEngine;

public class EnergyCuveViewer : MonoBehaviour
{
    [SerializeField] private EnergyCuve _energyCuve;

    private void Awake()
    {
        _energyCuve.OnDeath += OnDeath;
    }

    private void OnDeath()
    {
        gameObject.SetActive(false);
    }
}
