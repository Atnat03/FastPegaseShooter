using UnityEngine;

namespace Tuto
{
    public class TriggerInTuto : MonoBusListener
    {
        public bool isTuto = true;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerVisuelBridge player))
            {
                player.transform.root.TryGetComponent(out PlayerTuto tuto);
                {
                    if (isTuto)
                    {
                        tuto.EnterInTuto();
                    }
                    else
                    {
                        tuto.ExitTuto();
                    }
                }
            }
        }
    }
}