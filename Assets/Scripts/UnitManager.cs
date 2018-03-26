using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance;

    //gameObject
    public GameObject tileTestCorner;
    public GameObject tileTestEdges;

    //vector3
    public Vector3 originalRightClickTarget;
    public Vector3 newTemp;
    public Vector3 recalculatedTarget; 

    //lists
    public List<GameObject> movingUnits = new List<GameObject>();
    public List<GameObject> units = new List<GameObject>();
    public List<Vector3> occupiedNodes = new List<Vector3>();
    public List<Vector3> listOfVectors = new List<Vector3>();

    //bools   
    public bool foundClosestFreeNode; 

    //FSMs 
    public enum UnitMovement
    {
        rightClickTargetNode,
        calculateMoveArea, 
        createFormation,
        clearLists
    }
    public UnitMovement unitMovement; 

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
		//finds all unts at awake and adds toa list
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Unit").Length; i++)
        {
            units.Add(GameObject.FindGameObjectsWithTag("Unit")[i]);
        }
    }

    private void Update()
    {
        UnitMovementFSM();
    }

    private void UnitMovementFSM()
    {
        switch(unitMovement)
        {
            case UnitMovement.rightClickTargetNode:
                RightClickTargetNode();
                break;
            case UnitMovement.calculateMoveArea:
                CreateMoveableArea(originalRightClickTarget);
                break; 
            case UnitMovement.createFormation:
                FindTargetForSelectedUnits();
                break;
            case UnitMovement.clearLists:
                listOfVectors.Clear();
                unitMovement = UnitMovement.rightClickTargetNode; 
                break; 
        }
    }


    private void RightClickTargetNode()
    {
        if(SelectionManager.instance.currentlySelectedUnits.Count > 0)
        {			
            //if units selected, and if the right mouse button has been clicked
            if(Input.GetMouseButtonDown(1))
            {				
                //send out a ray
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
					
					//if the ray hits the ground
                    if (hit.collider.gameObject.name == "Ground")
                    {						
                        //take the vector3 of that hit location, find the node that corresponds to that vector3, get the node vector3, store it in tempTarget
                        originalRightClickTarget = Grid.instance.NodeFromWorldPoint(hit.point).nodeWorldPosition;

                        //remove all selected units from moving units if they are already in that list.
                        RemoveFromTargetsAndMoveingUnits();

                        //go to the calculateMoveArea state 
                        unitMovement = UnitMovement.calculateMoveArea;
                    }
                }
            }
        }
    }

	//puts units in a formation
    private void FindTargetForSelectedUnits()
    {
        List<GameObject> selectedUnits = SelectionManager.instance.currentlySelectedUnits;

		//loop through currently selected units
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            Vector3 newTarget = originalRightClickTarget;

			//if the node that the target cooresponds too is not walkable and the vector 3 of target is occupied, find another position to move to
            if(!Grid.instance.NodeFromWorldPoint(newTarget).walkable || occupiedNodes.Contains(newTarget))
            {
                newTarget = FindClosestAvailableNode(listOfVectors);
                occupiedNodes.Remove(selectedUnits[i].GetComponent<Unit>().target);
                occupiedNodes.Add(newTarget);
                selectedUnits[i].GetComponent<Unit>().target = newTarget;
                selectedUnits[i].GetComponent<Unit>().moveFSM = Unit.MoveFSM.findPosition;
            }
			//if walkable and target isnt occupied move to target
            else if(Grid.instance.NodeFromWorldPoint(newTarget).walkable && !occupiedNodes.Contains(newTarget))
            {
                occupiedNodes.Remove(selectedUnits[i].GetComponent<Unit>().target);
                occupiedNodes.Add(newTarget);
                selectedUnits[i].GetComponent<Unit>().target = newTarget;
                selectedUnits[i].GetComponent<Unit>().moveFSM = Unit.MoveFSM.findPosition;
            }
        }
        unitMovement = UnitMovement.clearLists; 
    }

	//create square of possible targets
    private void CreateMoveableArea(Vector3 startingPos)
    {		
		//vectors of nodes surrounding unit
        Vector3 topLeft = new Vector3();
        Vector3 bottomLeft = new Vector3();
        Vector3 topRight = new Vector3();
        Vector3 bottomRight = new Vector3();
        int cornerIncrementer = 1;
        int sideIncrement = 1;

        for (int i = 0; i < 4; i ++)
        {
			//
            topLeft.x = (startingPos.x - cornerIncrementer);
            topLeft.z = (startingPos.z + cornerIncrementer);
            topLeft.y = .01f; 
       
			//potetntial list of places that are available
            listOfVectors.Add(topLeft); 

            EdgeMaker(sideIncrement, topLeft, 1, 0);
            EdgeMaker(sideIncrement, topLeft, 0, -1);

            bottomLeft.x = (startingPos.x - cornerIncrementer);
            bottomLeft.z = (startingPos.z - cornerIncrementer);
            bottomLeft.y = .01f;
 
            listOfVectors.Add(bottomLeft);

            topRight.x = (startingPos.x + cornerIncrementer);
            topRight.z = (startingPos.z + cornerIncrementer);
            topRight.y = .01f;

            listOfVectors.Add(topRight);

            bottomRight.x = (startingPos.x + cornerIncrementer);
            bottomRight.z = (startingPos.z - cornerIncrementer);
            bottomRight.y = .01f;
      
            listOfVectors.Add(bottomRight);

            EdgeMaker(sideIncrement, bottomRight, -1, 0);
            EdgeMaker(sideIncrement, bottomRight, 0, 1);

            cornerIncrementer++;
            sideIncrement += 2;      
        }
        unitMovement = UnitMovement.createFormation; 
    }

	//
    private void EdgeMaker(int sideIncrementer, Vector3 corner, int x, int z)
    {
        for (int tl = 0; tl < sideIncrementer; tl++)
        {
            if (tl == 0)
            {
                Vector3 temp = corner;
                temp.x += x;
                temp.z += z;;
                newTemp = temp;
                listOfVectors.Add(newTemp); 
            }
            else
            {
                Vector3 thirdTemp = newTemp;
                thirdTemp.x += x;
                thirdTemp.z += z;
                newTemp = thirdTemp;
                listOfVectors.Add(newTemp);
            }
        }
    }

    private Vector3 FindClosestAvailableNode(List<Vector3> listOfVectors)
    {
        for (int i = 0; i < listOfVectors.Count; i++)
        {
            if (Grid.instance.NodeFromWorldPoint(listOfVectors[i]).walkable && !occupiedNodes.Contains(listOfVectors[i]))
            {
                return listOfVectors[i];
            }
        }
        return listOfVectors[0]; 
    }

	//remove unit from moving unit list
    private void RemoveFromTargetsAndMoveingUnits()
    {
        if(SelectionManager.instance.currentlySelectedUnits.Count > 0)
        {
            for(int i = 0; i < SelectionManager.instance.currentlySelectedUnits.Count; i++)
            {
                SelectionManager.instance.currentlySelectedUnits[i].GetComponent<Unit>().RemoveUnitFromUnitManagerMovingUnitsList(); 
            }
        }
    }
}
