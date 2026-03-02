using System.Linq;
using UnityEngine;

namespace GunDecorator
{
    public class NoiseModuleModule : GunModule, INoiseModule
    {
        public void ApplyNoise()
        {
            Debug.Log("OMG ça se disperse !!");
        }
    }
}