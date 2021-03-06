﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
	//is node walkable
	public bool walkable;
	//position of node in world
	public Vector3 worldPosition;

    //
    public int gridX;
	public int gridY;

	public int gCost;
	public int hCost;
	public Node parent;

	int heapIndex;


	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
	{
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	}

    //returns fcost, which is g + h
	public int fCost
	{
		get { return gCost + hCost; }
	}

	public int HeapIndex
	{
		get
		{
			return heapIndex;
		}
		set
		{
			heapIndex = value;
		}
	}

    //compares towo nodes f cost
	public int CompareTo(Node nodeToCompare)
	{
		int compare = fCost.CompareTo (nodeToCompare.fCost);
		if (compare == 0) 
		{
			compare = hCost.CompareTo (nodeToCompare.hCost);
		}
		return -compare;
	}
}
