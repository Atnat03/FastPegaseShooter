using UnityEngine;

public class InputMappingPanelBehaviour : PausePanel
{
    public override void Init()
    { 
        base.Init();
        gameObject.SetActive(false);
    }

    public override void OnPause(bool isPause)
    {
        gameObject.SetActive(false);
    }
    
    public void QuitPanel() => gameObject.SetActive(false);
}
