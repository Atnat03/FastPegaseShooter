using UnityEngine;

public abstract class PausePanel : MonoBehaviour
{
    public virtual void Init() { }
    
    public virtual void OnPause(bool pause) { }
}