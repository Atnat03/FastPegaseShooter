using System;
using UnityEngine;

public enum PlayerType
{
    Human,
    FisherMan,
}


public class Seeker : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] float speed = 1;
    [SerializeField] public PlayerType playerType;

    void Update()
    {
        if (gridManager.path != null)
        {
            if (gridManager.path.Count > 0)
            {
                transform.position = Vector3.MoveTowards(transform.position, gridManager.path[0].worldPosition, speed * Time.deltaTime);
            }
        }
    }

    public bool CheckPlayerType(PlayerType playerType)
    {
        foreach (PlayerType t in Enum.GetValues(typeof(PlayerType)))
        {
            if(t == playerType)
                return true;
        }
        
        return false;
    }
}