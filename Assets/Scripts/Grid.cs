using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour 
{
	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	//defines area in world coords that grid will cover
	public Vector2 gridWorldSize;
	//space node covers
	public float nodeRadius;
	public Node[,] grid;

	//used for working out how many nodes fit into grid
	float nodeDiameter;
	int gridSizeX, gridSizeY;

	public static Grid instance;
	void Awake()
	{
		instance = this;
		nodeDiameter = nodeRadius * 2;
		//nodes that fit into world size x and y
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		CreateGrid ();
	}

	public int MaxSize
	{
		get{
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		//centre of world - left edge of world - bottom
		Vector3 bottomLeftOfWorld = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		//loop through all nodes to see if walkable
		for (int x = 0; x < gridSizeX; x++) 
		{
			for (int y = 0; y < gridSizeY; y++) 
			{
				//each point node will occupy in world
				Vector3 worldPoint = bottomLeftOfWorld + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				//collion check if return true walkable will be false
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
				//populate grid with nodes
				grid[x,y] = new Node(walkable,worldPoint, x, y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node> ();

		for (int x = -1; x <= 1; x++) 
		{
			for (int y = -1; y <= 1; y++) 
			{
                //if node is iteself continue
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

                //if inside of the grid
				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
                    //add node to neighbour
					neighbours.Add (grid [checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	//convert world position into grid coordinate
	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		//turns world coord into percentage of how far along grid it is located
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

		//clamped between 0 and 1
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

		return grid [x, y];
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1, gridWorldSize.y));

			if (grid != null && displayGridGizmos) 
			{

				foreach (Node n in grid) 
				{
					//set colour of gizmos. If collision then red
					Gizmos.color = (n.walkable) ? Color.white : Color.red;

					Gizmos.DrawCube (n.nodeWorldPosition, Vector3.one * (nodeDiameter - 0.1f));
				}
			}
	}
}
