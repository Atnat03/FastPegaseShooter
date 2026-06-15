using UnityEngine;

public class MainPausePanelBehaviour : PausePanel
{
    public override void Init()
    {
        base.Init();
        gameObject.SetActive(false);
    }
    
    public override void OnPause(bool isPause)
    {
        gameObject.SetActive(isPause);
        if(isPause) OnPanelSelected();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
