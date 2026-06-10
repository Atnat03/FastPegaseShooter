using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ElevatorTilling : MonoBehaviour
{
   [SerializeField] private List<ElevatorTile> _elevatorTiles = new();
   
   [SerializeField] private Transform _startPoint;
   [SerializeField] private Transform _endPoint;
   [SerializeField] private int _initialFloor;
   

   private Vector3[] _initialOffset;
   private Vector3 _scrollingDirection;

   [Header("----- Debug -----")]
   [SerializeField] private float _debugSize = 0.1f;

   public Action p_onTileMovingUp;
   

   public void Initialise()
   {
      _scrollingDirection = (_endPoint.position - _startPoint.position).normalized;
      _initialOffset = new Vector3[_elevatorTiles.Count];
      
      for(int i = 0; i < _elevatorTiles.Count; i++)
      {
         Vector3 pos = _elevatorTiles[i].transform.position;
         float axisProgress = Vector3.Dot(pos - _startPoint.position, _scrollingDirection);
         _initialOffset[i] = pos - (_startPoint.position + _scrollingDirection * axisProgress);
         
         _elevatorTiles[i].ChangeFloor(_initialFloor+i);
      }
   }

   public void MoveTiles(float speed)
   {
      for(int i = 0; i < _elevatorTiles.Count; i++)
      {
         _elevatorTiles[i].transform.position += _scrollingDirection * Time.deltaTime * speed;
         float dot = Vector3.Dot(_elevatorTiles[i].transform.position - _endPoint.position, _scrollingDirection);
         if (dot > 0f)
         {
            _elevatorTiles[i].transform.position = _startPoint.position + _scrollingDirection * dot + _initialOffset[i];
            _elevatorTiles[i].ChangeFloor(_elevatorTiles.Count);
            p_onTileMovingUp?.Invoke();
         }
      }
   }

   private void OnDrawGizmos()
   {
      Gizmos.color = Color.green;
      Gizmos.DrawSphere(_startPoint.position, _debugSize);
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(_endPoint.position, _debugSize);
   }
}
