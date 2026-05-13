using UnityEngine;

public abstract class PausePanel : MonoBusListener
{
    public virtual void Init() { }
    
    public virtual void OnPause(bool pause) { }
}