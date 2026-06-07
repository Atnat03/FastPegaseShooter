using UnityEngine;

public class InputMappingPanelBehaviour : PausePanel
{
    public override void OnPause(bool isPause)
    {
        gameObject.SetActive(false);
    }
    
    public void QuitPanel() => gameObject.SetActive(false);
}
