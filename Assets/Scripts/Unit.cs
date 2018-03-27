using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour
{
    public Vector2 screenPos;
    public bool onScreen = false;
    public bool selected = false;

	public Vector3 target;
	float speed = 15f;
	public Vector3[] path;
	int targetIndex;
   
    private int walkSpeedId;

    public bool stopMoving;
    private Grid grid;   

    public Vector3 currentWaypoint; 

    public enum MoveFSM
    {
        empty,
        findPosition,
        recalculatePath,
        moveToTarget
    }

    public MoveFSM moveFSM; 

	void Start()
    {       
        grid = GameObject.FindGameObjectWithTag("A*").GetComponent<Grid>();       
    }

    void Update()
    { 
		MoveStates();
    }

    public void MoveStates()
    {
        switch (moveFSM)
        {
            case MoveFSM.empty:
                break; 

            case MoveFSM.findPosition:
                {
                    RemoveUnitFromUnitManagerMovingUnitsList();
                    PathManager.RequestPath(transform.position, target, OnPathFound);
                    moveFSM = MoveFSM.empty; 
                }
                break;

            case MoveFSM.recalculatePath:
                {
                    Node targetNode = grid.NodeFromWorldPoint(target);
                    if (targetNode.walkable == false)
                    {
                        stopMoving = false;
                        FindClosestWalkableNode(targetNode);
                        moveFSM = MoveFSM.empty;
                    }
                    else if (targetNode.walkable == true)
                    {
                        stopMoving = false;
                        PathManager.RequestPath(transform.position, target, OnPathFound);
                        moveFSM = MoveFSM.empty;
                    }
                }
                break;

            case MoveFSM.moveToTarget:
                MoveToTarget(); 
                break;
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
		if (pathSuccessful)
        {
			path = newPath;
			targetIndex = 0;

            UnitManager.instance.movingUnits.Add(this.gameObject);
            StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
            moveFSM = MoveFSM.empty; 
		}
	}

    private void FindClosestWalkableNode(Node originalNode)
    {
        Node comparisonNode = grid.grid[0, 0];
        Node incrementedNode = originalNode;
        for (int x = 0; x < incrementedNode.gridX; x++)
        {
            incrementedNode = grid.grid[incrementedNode.gridX - 1, incrementedNode.gridY];

            if (incrementedNode.walkable == true)
            {
                comparisonNode = incrementedNode;
                target = comparisonNode.nodeWorldPosition;
                PathManager.RequestPath(transform.position, target, OnPathFound);
                moveFSM = MoveFSM.empty;
                break;
            }
        }

    }

    public void MoveToTarget()
    {
        if(transform.position != target)
        {
            transform.rotation = Quaternion.LookRotation(target - transform.position);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

	IEnumerator FollowPath()
    {
        currentWaypoint = path[0];
		while (true)
        {
			if (transform.position == currentWaypoint)
            {
				targetIndex ++;
				if (targetIndex >= path.Length)
                {
                    moveFSM = MoveFSM.moveToTarget; 
                    yield break;
				} 
                currentWaypoint = path[targetIndex];           
            }
            stopMoving = false;           
            transform.rotation = Quaternion.LookRotation(currentWaypoint - transform.position); 
            transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			yield return null;

		}
	} 

    public void OnDrawGizmos()
    {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}


    public void RemoveUnitFromUnitManagerMovingUnitsList()
    {
        if (UnitManager.instance.movingUnits.Count > 0)
        {
            for (int i = 0; i < UnitManager.instance.movingUnits.Count; i++)
            {
                if (this.gameObject == UnitManager.instance.movingUnits[i])
                {
                    UnitManager.instance.movingUnits.Remove(UnitManager.instance.movingUnits[i]);
                }
            }
        }
    }
}
