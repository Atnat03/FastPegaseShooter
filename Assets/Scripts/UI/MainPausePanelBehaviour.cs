using UnityEngine;

public class MainPausePanelBehaviour : PausePanel
{
    public override void Init()
    {
        gameObject.SetActive(false);
    }
    
    public override void OnPause(bool isPause)
    {
        gameObject.SetActive(isPause);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
