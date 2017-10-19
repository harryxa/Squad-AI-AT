﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour 
{
//	public Transform seeker, target;
	Grid grid;
	PathManager requestManager;

	void Awake()
	{
		requestManager = GetComponent<PathManager> ();
		grid = GetComponent<Grid>();
	}

	//void Update()
	//{
	//	if (Input.GetButtonDown ("Jump")) 
	//	{
	//		FindPath (seeker.position, target.position);
	//	}
	//}

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

		if (startNode.walkable && targetNode.walkable) {
			Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node> ();
			openSet.Add (startNode);

			while (openSet.Count > 0) {
				Node currentNode = openSet.removeFirst ();
				closedSet.Add (currentNode);

				if (currentNode == targetNode) {
					sw.Stop ();
					print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;

					break;
				}

				foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
					if (!neighbour.walkable || closedSet.Contains (neighbour)) {
						continue;
					}

					int NewMovementCostToNeighbour = currentNode.gCost + GetDistance (currentNode, neighbour);

					if (NewMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour)) {
						neighbour.gCost = NewMovementCostToNeighbour;
						neighbour.hCost = GetDistance (neighbour, targetNode);
						neighbour.parent = currentNode;

						if (!openSet.Contains (neighbour))
							openSet.Add (neighbour);
						else
							openSet.UpdateItem (neighbour);
					}
				}
			}
		}
		yield return null;	//execution of coroutine will end untill the next frame
		if (pathSuccess) {
			waypoints = RetracePath (startNode, targetNode);
		}
		requestManager.FinishedProcessingPath (waypoints, pathSuccess);
	}

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

	int GetDistance(Node nodeA, Node nodeB)
	{
		int distX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs (nodeA.gridY - nodeB.gridY);

		if (distX > distY)
			return 14 * distY + 10 * (distX - distY);
		return 14 * distX + 10 * (distY - distX);
	}
}
