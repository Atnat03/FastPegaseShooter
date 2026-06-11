using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ElevatorTillingManager : MonoBusListener
{
    [SerializeField, Tooltip("First element in the list should be the walls player exits from")]
    private List<ElevatorTilling>  _elevatorTillings = new List<ElevatorTilling>();
    
    [SerializeField] private float _speed = 10;

    private bool _isRunning;
    private bool _shouldStopWhenAligned;

    void Awake()
    {
        foreach (ElevatorTilling elevatorTilling in _elevatorTillings)
        {
            elevatorTilling.Initialise();
        }
        
        ListenToEvent<OnDapEvent>(ODE =>
        {
            StopScrolling();
        });
    }

    public void StartScrolling()
    {
        if(_isRunning) return;
        
        _isRunning = true;
        _elevatorTillings[0].p_onTileMovingUp += POnTileMovingUp;
        Scroll();
    }

    void StopScrolling()
    {
        _shouldStopWhenAligned = true;
    }

    private void POnTileMovingUp()
    {
        if (_shouldStopWhenAligned)
        {
            _isRunning = false;
            _shouldStopWhenAligned = false;
        }
    }

    async void Scroll()
    {
        while (_isRunning && Application.isPlaying)
        {
            foreach (ElevatorTilling elevatorTilling in _elevatorTillings)
            {
                elevatorTilling.MoveTiles(_speed);
            }
            await Task.Yield();
        }
    }
}
