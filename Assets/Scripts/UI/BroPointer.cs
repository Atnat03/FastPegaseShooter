using System;
using FishNet;
using FishNet.Object;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public class BroPointer : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private RectTransform _pointerRectTransform;
	[SerializeField] private Camera _uiCamera;
	[SerializeField] private Camera _cam;
	[SerializeField] float _borderSize = 100;
	
	private Vector3 _targetPosition;
	private Transform _target = null;
	private Image _pointerImage;

	private int _myIndex = 0;
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		_pointerImage = _pointerRectTransform.GetComponent<Image>();
	}

	public override void OnStartClient()
	{
		_myIndex = Owner.ClientId == 0 ? 1 : 0;
		Cons.Print("StartClient "  + _myIndex, ColorConsole.Green);
	}
	
	public void SetTarget(Transform target)
	{
		_target = target;
		_targetPosition = target.position;
		
		Cons.Print("Set target " + _targetPosition, ColorConsole.Green);
	}

	private void LateUpdate()
	{
		if (_target == null)
		{
			if (InstanceFinder.ClientManager.Clients.Count != 2)
				return;

			Transform t;
			
			if(IsServerInitialized)
				t = InstanceFinder.ServerManager.Clients[_myIndex].FirstObject.transform;
			else
			{ 
				t = InstanceFinder.ClientManager.Clients[_myIndex].FirstObject.transform;
			}
			
			if (t != null)
			{
				SetTarget(t);
			}
		}
		
		Cons.Print("target "  + _target.name, ColorConsole.Green);
		
		Vector3 toPosition = _targetPosition;
		Vector3 fromPosition = _cam.transform.position;
		fromPosition.z = 0f;
		
		Vector3 dir = (toPosition - fromPosition).normalized;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		_pointerRectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
		
		Vector3 targetPositionScreenPoint = _cam.WorldToScreenPoint(_targetPosition);
		
		if (targetPositionScreenPoint.z < 0)
			targetPositionScreenPoint *= -1;
		
		bool isOffScreen = targetPositionScreenPoint.x <= _borderSize || targetPositionScreenPoint.x >= Screen.width - _borderSize
		                 || targetPositionScreenPoint.y <= _borderSize || targetPositionScreenPoint.y >= Screen.height - _borderSize;

		if (isOffScreen)
		{
			RotatePointer();
			Vector3 cappedTargetScreenPosition = targetPositionScreenPoint;
			if(cappedTargetScreenPosition.x <= _borderSize) cappedTargetScreenPosition.x = _borderSize;
			if(cappedTargetScreenPosition.x >= Screen.width - _borderSize) cappedTargetScreenPosition.x = Screen.width - _borderSize;
			if(cappedTargetScreenPosition.y <= _borderSize) cappedTargetScreenPosition.y = _borderSize;
			if(cappedTargetScreenPosition.y >= Screen.height - _borderSize) cappedTargetScreenPosition.y = Screen.height - _borderSize;
			
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_pointerRectTransform.parent as RectTransform,
				cappedTargetScreenPosition,
				_uiCamera,
				out localPoint
			);

			_pointerRectTransform.localPosition = localPoint;
			_pointerRectTransform.localPosition = new Vector3(_pointerRectTransform.localPosition.x, _pointerRectTransform.localPosition.y, 0);
		}
		else
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_pointerRectTransform.parent as RectTransform,
				targetPositionScreenPoint,
				_uiCamera,
				out localPoint
			);

			_pointerRectTransform.localPosition = localPoint;

			float yPos = _pointerRectTransform.localPosition.y + 100f;
			
			_pointerRectTransform.localPosition = new Vector3(_pointerRectTransform.localPosition.x, yPos, 0);
			
			_pointerRectTransform.localEulerAngles = Vector3.zero;
		}

		_target = null;
	}

	private void RotatePointer()
	{
		Vector3 toPosition = _targetPosition;
		Vector3 fromPosition = _cam.transform.position;
		fromPosition.z = 0f;
		Vector3 dir = (toPosition - fromPosition).normalized;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		_pointerRectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
	}

	#endregion
}
