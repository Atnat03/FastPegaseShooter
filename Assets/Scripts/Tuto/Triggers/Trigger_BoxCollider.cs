using System;
using UnityEngine;

namespace Tuto.Triggers
{
    public enum TriggerBoxMode
    {
        BothAtSameTime,
        RedOnly,
        BlueOnly,
        EitherHasPassed,
    }

    [Serializable]
    public class Trigger_BoxCollider : BaseTrigger
    {
        public override string DisplayName => "Trigger Box";

        public int proxyIndex = 0;
        public TriggerBoxMode mode = TriggerBoxMode.BothAtSameTime;

        private TriggerBoxBridge _bridge;

        private bool _redPassed;
        private bool _bluePassed;

        public void InjectProxy(TriggerBoxBridge bridge)
        {
            _bridge = bridge;
        }

        public override void Initialize(TutoManager tuto)
        {
            if (_bridge == null)
            {
                return;
            }
            _bridge.OnPlayerEntered += HandlePlayerEntered;
            _bridge.OnPlayerExited  += HandlePlayerExited;
        }

        public override void Dispose()
        {
            if (_bridge == null) return;
            _bridge.OnPlayerEntered -= HandlePlayerEntered;
            _bridge.OnPlayerExited  -= HandlePlayerExited;
        }

        private void HandlePlayerEntered(PlayerSide side)
        {
            if (side == PlayerSide.Red)  _redPassed  = true;
            if (side == PlayerSide.Blue) _bluePassed = true;
            Evaluate(side, isInside: true);
        }

        private void HandlePlayerExited(PlayerSide side) => Evaluate(side, isInside: false);

        private void Evaluate(PlayerSide side, bool isInside)
        {
            switch (mode)
            {
                case TriggerBoxMode.RedOnly         when side == PlayerSide.Red  && isInside:
                case TriggerBoxMode.BlueOnly        when side == PlayerSide.Blue && isInside:
                case TriggerBoxMode.EitherHasPassed when _redPassed && _bluePassed:
                    OnActivated?.Invoke();
                    break;

                case TriggerBoxMode.BothAtSameTime:
                    if (_bridge.IsRedInside && _bridge.IsBlueInside)
                        OnActivated?.Invoke();
                    break;
            }
        }
    }
}