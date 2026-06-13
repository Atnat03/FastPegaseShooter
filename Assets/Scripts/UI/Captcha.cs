using System;
using System.Collections;
using System.Collections.Generic;
using GameKit.Dependencies.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Captcha : MonoBehaviour
{
    [SerializeField] private Sprite[] _spriteList;
    [SerializeField] private Button[] _buttons;

    [SerializeField] private int[] _goodAnwser;

    [SerializeField] private GameObject goodNotif;
    [SerializeField] private GameObject wrongNotif;
    [SerializeField] MenuManager _menuManager;

    private List<Button> _buttonList;
    private List<int> _awnserList;

    private Coroutine _wrongTry;

    private void Awake()
    {
        _buttonList = new List<Button>(_buttons);
        _buttonList.Shuffle();

        _awnserList = new List<int>();

        for (int i = 0; i < _buttonList.Count; i++)
        {
            int index = i;

            _buttonList[i].GetComponent<Image>().sprite = _spriteList[i];
            
            _buttonList[i].GetComponent<Outline>().enabled = false;
            
            _buttonList[i].onClick.AddListener(() => SelectCaptcha(index));
        }
        
        wrongNotif.SetActive(false);
        goodNotif.SetActive(false);
    }

    void SelectCaptcha(int id)
    {
        Button btn = _buttonList[id];
        Outline outline = btn.GetComponent<Outline>();

        bool isSelected = outline.enabled;
        outline.enabled = !isSelected;
        
        if (!_awnserList.Contains(id))
        {
            _awnserList.Add(id);
        }
        else
        {
            _awnserList.Remove(id);
        }
    }

    public void CheckCatcha()
    {
        if (IsGood())
        {
            Debug.Log("Captcha réussi !");

            StartCoroutine(Sucess());
        }
        else
        {
            Debug.Log("Captcha échoué !");
            
            if(_wrongTry == null)
                _wrongTry = StartCoroutine(Wrong());
        }
        
        ResetUI();
    }

    void ResetUI()
    {
        _awnserList.Clear();

        foreach (var btn in _buttonList)
        {
            btn.GetComponent<Outline>().enabled = false;
        }
    }
    
    bool IsGood()
    {
        if (_awnserList.Count != _goodAnwser.Length)
            return false;

        foreach (int value in _goodAnwser)
        {
            if (!_awnserList.Contains(value))
                return false;
        }

        return true;
    }

    IEnumerator Wrong()
    {
        yield return new WaitForSeconds(0.5f);
        
        wrongNotif.SetActive(true);
        
        _menuManager._sound.PlaySound("Wrong");
        
        yield return new WaitForSeconds(0.5f);
        
        wrongNotif.SetActive(false);
    }

    IEnumerator Sucess()
    {
        yield return new WaitForSeconds(0.5f);
        
        goodNotif.SetActive(true);
        
        _menuManager._sound.PlaySound("Selected");
        
        yield return new WaitForSeconds(0.5f);
        
        SceneManager.LoadScene(_menuManager.indexSceneGame);
    }
}