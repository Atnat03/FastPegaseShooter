using System.Collections.Generic;
using UnityEngine;

public class Asta : MonoBehaviour
{
	[SerializeField] Transform seeker;
	[SerializeField] Transform target;

	[SerializeField] GridManager gridManager;
	[SerializeField] private Seeker player;

	void Update()
	{
		FindPath(seeker.position, target.position);
	}

	void FindPath(Vector3 startPos, Vector3 endPos)
	{
		Node startNode = gridManager.NodeFromWorldPosition(startPos - gridManager.transform.position);
		Node endNode   = gridManager.NodeFromWorldPosition(endPos - gridManager.transform.position);

		List<Node>    openNodes   = new();
		HashSet<Node> closedNodes = new();

		openNodes.Add(startNode);

		while (openNodes.Count > 0)
		{
			Node currentNode = GetLowestFCostNode(openNodes);

			openNodes.Remove(currentNode);
			closedNodes.Add(currentNode);

			if (currentNode == endNode)
			{
				RetracePath(startNode, endNode);
				return;
			}

			foreach (Node neighbour in gridManager.GetNeighbours(currentNode))
			{
				if (!neighbour.walkable || closedNodes.Contains(neighbour))
				{
					continue;
				}

				int newCost = currentNode.gCost + GetDistance(currentNode, neighbour);

				if (newCost < neighbour.gCost || !openNodes.Contains(neighbour))
				{
					newCost = currentNode.type.ApplyCost(newCost, player.playerType);
					
					neighbour.gCost  = newCost;
					
					neighbour.hCost  = GetDistance(neighbour, endNode);
					neighbour.parent = currentNode;

					if (!openNodes.Contains(neighbour))
					{
						openNodes.Add(neighbour);
					}
				}
			}
		}
	}

	int GetDistance(Node nodeA, Node nodeB)
	{
		const int diagonalCost = 14;
		const int straightCost = 10;

		int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		return distX > distY
			? diagonalCost * distY + straightCost * (distX - distY)
			: diagonalCost * distX + straightCost * (distY - distX);
	}

	void RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new();

		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		path.Reverse();
		gridManager.path = path;
	}

	Node GetLowestFCostNode(List<Node> nodes)
	{
		Node bestNode = nodes[0];

		foreach (Node node in nodes)
		{
			if (node.fCost < bestNode.fCost || (node.fCost == bestNode.fCost && node.hCost < bestNode.hCost))
			{
				bestNode = node;
			}
		}

		return bestNode;
	}
}
