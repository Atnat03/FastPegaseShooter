using UnityEditor;
using UnityEngine;

namespace  GunDecorator
{
    public abstract class GunModule : MonoBehaviour
    {
        protected GunController _gunController;

        [SerializeField] private string moduleName = "";
        [SerializeField] private Color moduleColor = Color.clear;

        public string ModuleName => moduleName;
        public Color ModuleColor => moduleColor;

        public virtual void Initialize(GunController gun)
        {
            _gunController = gun;
        }
    }

    
}
