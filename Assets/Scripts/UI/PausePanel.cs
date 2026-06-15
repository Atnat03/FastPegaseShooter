using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PausePanel : MonoBusListener
{
    public List<RectTransform> buttons = new List<RectTransform>();
    private Coroutine buttonsCoroutine;
    private Dictionary<RectTransform, Vector3> buttonPositions = new Dictionary<RectTransform, Vector3>();

    public virtual void Init()
    {
        foreach (RectTransform button in buttons)
        {
            buttonPositions.Add(button, button.localPosition);
        }
    }

    public virtual void OnPause(bool pause)
    {
    }

    public virtual void OnPanelSelected()
    {
        buttonsCoroutine = StartCoroutine(ButtonsArrivalCoroutine());
    }

    public virtual void OnPanelDeselected()
    {
        StopCoroutine(buttonsCoroutine);
    }

    IEnumerator ButtonsArrivalCoroutine()
    {
        foreach (RectTransform button in buttons)
        {
            button.localPosition += new Vector3(-1000, 0, 0);
        }
        foreach (RectTransform button in buttons)
        {
            StartCoroutine(ButtonArrivalCoroutine(button, button.localPosition  ,  buttonPositions[button]));
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator ButtonArrivalCoroutine(RectTransform button,Vector3 startpos, Vector3 endpos)
    {
        button.localPosition = startpos;
        while (Vector3.Distance(button.localPosition, endpos) > 0.01f)
        {
            button.localPosition = Vector3.Lerp(button.localPosition, endpos, Time.deltaTime * 5);
            yield return new WaitForFixedUpdate();
        }
    }
}