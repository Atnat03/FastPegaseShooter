using System;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public TerrainType type;
    public Vector3 worldPosition;
    public int gridX, gridY;
    public int gCost;
    public int hCost;
    public Node parent;
    
    public int fCost => gCost + hCost;
    
    public Node(bool _wakable, Vector3 worldPos, int X, int Y, TerrainType type)
    {
        walkable = _wakable;
        worldPosition = worldPos;
        gridX = X;
        gridY = Y;
        this.type = type;
    }
}

public class GridManager : MonoBehaviour
{
	[SerializeField] LayerMask unWalkableMask;
	[SerializeField] LayerMask waterMask;
	[SerializeField] Vector2   gridWorldSize;
	[SerializeField] float     nodeRadius;
	Node[,]                    grid;
	int                        gridSizeX, gridSizeY;
	float                      nodeDiameter;

	public List<Node> path;

	void Awake()
	{
		nodeDiameter = nodeRadius * 2f;

		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		CreateGrid();
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

		if (grid != null)
		{
			foreach (Node node in grid)
			{
				Gizmos.color = node.walkable ? Color.white : Color.red;

				switch (node.type)
				{
					case Water:
						Gizmos.color = Color.cyan;
						break;
					case Route:
						Gizmos.color = Color.grey;
						break;
					case Boue:
						Gizmos.color = Color.black;
						break;
					case Herbe:
						Gizmos.color = Color.green;
						break;
					
				}
				
				if (path != null && path.Contains(node))
				{
					Gizmos.color = Color.blue;
				}

				Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
			}
		}
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];

		Vector3 worldBottomLeft = transform.position - Vector3.right   * (gridWorldSize.x * 0.5f - nodeRadius)
													 - Vector3.forward * (gridWorldSize.y * 0.5f - nodeRadius);

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right   * (x * nodeDiameter)
													 + Vector3.forward * (y * nodeDiameter);

				bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unWalkableMask);

				TerrainType type = new Herbe();
				
				RaycastHit hit;

				if (Physics.Raycast(worldPoint + Vector3.up*2, Vector3.down, out hit, 1000))
				{
					type = hit.collider.gameObject.GetComponent<TerrainType>();
				}
				
				grid[x, y] = new Node(walkable, worldPoint, x, y, type);
			}
		}
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
				{
					continue;
				}

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPosition(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

		return grid[x, y];
	}
}
