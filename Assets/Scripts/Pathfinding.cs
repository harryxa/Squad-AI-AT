using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour 
{

	Grid grid;
	PathManager requestManager;

	void Awake()
	{
		requestManager = GetComponent<PathManager> ();
		grid = GetComponent<Grid>();
	}

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
	{
		StartCoroutine(FindPath(startPos, targetPos));
	}

	IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
	{
		Stopwatch sw = new Stopwatch ();
		sw.Start();

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		Node startNode = grid.NodeFromWorldPoint (startPos);
		Node targetNode = grid.NodeFromWorldPoint (targetPos);

		if (startNode.walkable && targetNode.walkable)
        {
            //use heap instead of list
			Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node> (); //hashset similur to dictionary
			openSet.Add (startNode);


			while (openSet.Count > 0) {               
				Node currentNode = openSet.removeFirst (); //current node is equal...
				closedSet.Add (currentNode);

				if (currentNode == targetNode) {
					sw.Stop ();
					print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;

					break;
				}

                //loop through all neighbours
				foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    //check if neighbour is not walkable or in closed list, skip ahead to next neighbour
                    if (!neighbour.walkable || closedSet.Contains (neighbour))
                    {
						continue;
					}

                    //distance between current node and neighbour
					int NewMovementCostToNeighbour = currentNode.gCost + GetDistance (currentNode, neighbour);

                    //if distance between node and neighbour is less than the neighbours g cost, or not in the open list
					if (NewMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour))
                    {
                        //set g cost and h cost of neighbour
						neighbour.gCost = NewMovementCostToNeighbour;
						neighbour.hCost = GetDistance (neighbour, targetNode);
						neighbour.parent = currentNode;

                        //check if neighbour is not in the open set, if not add it.
						if (!openSet.Contains (neighbour))
							openSet.Add (neighbour);
						else
							openSet.UpdateItem (neighbour);
					}
				}
			}
		}
		yield return null;  //execution of coroutine will end untill the next frame

        if (pathSuccess)
        {
			waypoints = RetracePath (startNode, targetNode);
		}
		requestManager.FinishedProcessingPath (waypoints, pathSuccess);
	}

    //retrace steps to get the path from start to end
	Vector3[] RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node> ();
		Node currentNode = endNode;


		while(currentNode != startNode)
		{
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}
		Vector3[] waypoints = simplifyPath (path);

		Array.Reverse (waypoints);

		return waypoints;

	}

	Vector3[] simplifyPath(List<Node> path)
	{
		List<Vector3> waypoints = new List<Vector3> ();
		Vector2 directionOld = Vector2.zero;

		for (int i = 1; i < path.Count; i++) {
			Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX, path[i-1].gridY - path[i].gridY);
			if (directionNew != directionOld) {
				waypoints.Add (path [i].worldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray ();
	}

    //get distance between any two given nodes
	int GetDistance(Node nodeA, Node nodeB)
	{
		int distX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs (nodeA.gridY - nodeB.gridY);

		if (distX > distY)
			return 14 * distY + 10 * (distX - distY);
		return 14 * distX + 10 * (distY - distX);
	}
}
