using System;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public struct UIColorFromPlayer
{
	public Material playerMaterial;
	[Header("Guns")]
	public Material mitrailleteMaterial;
	public Material lanceGrenadeMaterial;
	public Material shotgunMaterial;
	[Header("UI")] 
	public Color contourScreenColor;
	public Color backgroundInfoColor;
	public Color contourInfoColor;
	public Color gunStatColor;
	public Color textColor;
}

[Serializable]
public struct UISelectGun
{
	public Sprite gunName;
	public string textInfo;
	public Material coinMaterial;
	[Range(1, 3)] public int numberArrowDamage;
	[Range(1, 3)] public int numberArrowFireRate;
}

public class PlayerSelectGun : NetworkBusListener
{
	#region Variables

	[SerializeField] private GunSwitching _gun;
	[SerializeField] private FPSController _fps;
	
	[Header("View")]
	[SerializeField] private Canvas _canvas;
	[SerializeField] private GameObject _uiInput;
	[SerializeField] private GameObject _uiMain;

	[Header("UI Data")] 
	[SerializeField] private UIColorFromPlayer[] _colorPlayer;
	[SerializeField] private UISelectGun[] uiGunData;

	[Header("UI References")]
	[SerializeField] private Button goNextButton;
	[SerializeField] private Button goPreviousButton;
	[SerializeField] private Image contourTopImage;
	[SerializeField] private Image contourBotImage;
	[SerializeField] private Image backgroundInfoImage;
	[SerializeField] private Image contourInfoImage;
	[SerializeField] private Image[] arrowDamageImageList;
	[SerializeField] private Image[] arrowFireRateImageList;
	[SerializeField] private Image gunStatContourImage;
	[SerializeField] private Image gunInfoTextContourImage;
	[SerializeField] private TextMeshProUGUI textInfoText;
	[SerializeField] private Image gunNameImage;
	[SerializeField] private SkinnedMeshRenderer playerMesh;
	[SerializeField] private GameObject[] gunMeshList;
	[SerializeField] private MeshRenderer coinGunMesh;
	[SerializeField] private Image equipButton;
	[SerializeField] private Color[] equipButtonColors;
	
	private int _newIndexGun = 0;
	private int equipedGun = 0;

	private PlayerInput _playerInput;
	
	#endregion

	#region Fonctions
	
	public override void OnStartNetwork()
	{
		ListenToEvent<OnAllPlayerAtBorne>(OnShowUI);
		ListenToEvent<OnAllPlayerCanSelectGun>(CanSelectGun);
		
		goNextButton.onClick.AddListener(SelectNextGun);
		goPreviousButton.onClick.AddListener(SelectPreviousGun);
		
		_playerInput = GetComponent<PlayerInput>();

		if (_playerInput != null)
		{
			_playerInput.actions["Escape"].performed += ClosePannel;
		}
	}

	public override void OnStopNetwork()
	{
		if (_playerInput != null)
		{
			_playerInput.actions["Escape"].performed -= ClosePannel;
		}
	}

	private void ClosePannel(InputAction.CallbackContext obj)
	{
		if(_uiMain.activeSelf)
			FinishSelection();
	}

	private void OnShowUI(OnAllPlayerAtBorne data)
	{
		if (!IsOwner) return;
	
		InvokeEvent(new OnGunSelectionStateChanged
		{
			IsOpen = true
		});
		
		InvokeEvent(new OnOpenBorne{p_playerPositive = _gun.IsPositive});
		InvokeEvent(new PlayUISound{keySound = "OpenBorne"});
		
		_gun.DesactivateAllMainGun();
		_fps.IsFreeze = true;
		
		CursorManager.instance.PushState(CursorState.UI, _fps);

		_canvas.sortingOrder = 10;
		
		_newIndexGun = _gun.CurrentMainGunIndex;
		equipedGun = _newIndexGun;

		UpdateUI_Color();
		UpdateUI_Gun();
		
		_uiMain.SetActive(true);
		_uiInput.SetActive(false);
	}

	private void CanSelectGun(OnAllPlayerCanSelectGun data)
	{
		if (!IsOwner) return;
    
		_uiInput.SetActive(data.p_open);
	}

	void SelectNextGun()
	{
		int id = (_newIndexGun + 1) % 3;

		ChangeGun(id);
	}
	
	void SelectPreviousGun()
	{
		int id = (_newIndexGun + 2) % 3;

		ChangeGun(id);
	}
	
	void ChangeGun(int id)
	{
		if (!IsOwner) return;
		
		_newIndexGun = id;
		
		UpdateUI_Gun();
	}

	public void FinishSelection()
	{
		if (!IsOwner) return;
		
		_uiMain.SetActive(false);
		_gun.ChangeCurrentGun_Main_ServerRpc(equipedGun);
		_fps.IsFreeze = false;
		
		InvokeEvent(new OnGunSelectionStateChanged
		{
			IsOpen = false
		});
		
		InvokeEvent(new PlayUISound{keySound = "Quit"});
		
		_canvas.sortingOrder = 1;
		
		CursorManager.instance.PopState(_fps);
	}

	public void EquipedGun()
	{
		equipedGun = _newIndexGun;
		equipButton.color = equipButtonColors[1];
	}
	
	private void UpdateUI_Gun()
	{
		UISelectGun dataGun = uiGunData[_newIndexGun];

		equipButton.color = equipedGun == _newIndexGun ? equipButtonColors[1] : equipButtonColors[0];

		for (int i = 0; i < gunMeshList.Length; i++)
		{
			if (i == _newIndexGun)
			{
				gunMeshList[i].gameObject.SetActive(true);
			}
			else
			{
				gunMeshList[i].gameObject.SetActive(false);
			}
		}
		
		gunNameImage.sprite = dataGun.gunName;
		textInfoText.text = dataGun.textInfo;
		coinGunMesh.material = dataGun.coinMaterial;

		for (int i = 0; i < arrowDamageImageList.Length; i++)
		{
			if (i <= dataGun.numberArrowDamage)
			{
				arrowDamageImageList[i].gameObject.SetActive(true);
			}
			else
			{
				arrowDamageImageList[i].gameObject.SetActive(false);
			}
		}
		
		for (int i = 0; i < arrowFireRateImageList.Length; i++)
		{
			if (i <= dataGun.numberArrowFireRate)
			{
				arrowFireRateImageList[i].gameObject.SetActive(true);
			}
			else
			{
				arrowFireRateImageList[i].gameObject.SetActive(false);
			}
		}
	}

	private void UpdateUI_Color()
	{
		UIColorFromPlayer colorData = _gun.IsPositive ? _colorPlayer[0] : _colorPlayer[1];
		
		playerMesh.material = colorData.playerMaterial;

		MeshRenderer[] m_mesh = gunMeshList[1].GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer m in m_mesh)
			m.material = colorData.lanceGrenadeMaterial;
		
		gunMeshList[0].GetComponent<MeshRenderer>().material = colorData.mitrailleteMaterial;
		gunMeshList[2].GetComponent<MeshRenderer>().material = colorData.shotgunMaterial;
		
		contourTopImage.color = colorData.contourScreenColor;
		contourBotImage.color = colorData.contourScreenColor;
		backgroundInfoImage.color = colorData.backgroundInfoColor;
		gunStatContourImage.color = colorData.gunStatColor;
		contourInfoImage.color = colorData.contourInfoColor;
		gunInfoTextContourImage.color = colorData.contourInfoColor;
		textInfoText.color = colorData.textColor;
	}
	
	#endregion
}

public struct OnGunSelectionStateChanged
{
	public bool IsOpen;
}