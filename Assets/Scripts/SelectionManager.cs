using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
	//instance
    public static SelectionManager instance; 

    //floats
    public float boxWidth;
    public float boxHeight;
    public float boxTop;
    public float boxLeft;

    //vector2
    public Vector2 boxStart;
    public Vector2 boxFinish;
    public Vector2 mouseDragStartPosition;

    //vector3
    public Vector3 currentMousePoint;
    public Vector3 mouseDownPoint;

    //gui
    public GUIStyle mouseDragSkin;

    //list and arrays
    public List<GameObject> currentlySelectedUnits = new List<GameObject>();

    //bool
    public bool mouseDragging;
    //gameobjects

    public GameObject selectedUnit;

    //FSM 
    public enum SelectFSM
    {
        clickOrDrag, 
        clickSelect,
        clickDeselect     
    }
    public SelectFSM selectFSM; 

	//METHODS

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        SelectUnitsFSM();
    }

    private void OnGUI()
    {
        if (mouseDragging)
            GUI.Box(new Rect(boxLeft, boxTop, boxWidth, boxHeight), "", mouseDragSkin);
    }

    private void SelectUnitsFSM()
    {
        switch(selectFSM)
        {
            case SelectFSM.clickOrDrag:
                ClickOrDrag(); 
                break;
            case SelectFSM.clickSelect:
                SelectSingleUnit(); 
                break;
            case SelectFSM.clickDeselect:
                DeselectAll();
                break; 
        }
    }

	//unit selection and deselection
    private void ClickOrDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

		//if ray hits something
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {			
            currentMousePoint = hit.point;

			//if left mouse and no lft shift
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
            {
                mouseDownPoint = hit.point; 
                mouseDragStartPosition = Input.mousePosition;

				//click to select a single unit, goes into SelectSingleUnit() method
                if (hit.collider.gameObject.tag == "Unit")
                {
                    selectedUnit = hit.collider.gameObject; 
                    selectFSM = SelectFSM.clickSelect; 
                }
				//click ground to deselect units, goes into DeselectAll() method
                else if (hit.collider.gameObject.tag == "Ground")
                    selectFSM = SelectFSM.clickDeselect;
            }
			//holding shift, click to select units or click selected units to deselect
            else if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
            {
                if (hit.collider.gameObject.tag == "Unit" && !currentlySelectedUnits.Contains(hit.collider.gameObject))
                    AddToCurrentlySelectedUnits(hit.collider.gameObject);
				
                else if (hit.collider.gameObject.tag == "Unit" && currentlySelectedUnits.Contains(hit.collider.gameObject))
                    RemoveFromCurrentlySelectedUnits(hit.collider.gameObject); 
            }
			//draw drag box and select units in box
            else if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftShift))
            {
                if (UserDraggingByPosition(mouseDragStartPosition, Input.mousePosition))
                {
                    mouseDragging = true;
                    DrawDragBox();
                    SelectUnitsInDrag();
                }
            }
            else if (Input.GetMouseButtonUp(0) && !Input.GetKey(KeyCode.LeftShift))
            {
                mouseDragging = false;
            }
        }
    }

	//select single unit then set the state back to click or drag
    private void SelectSingleUnit()
    {
        if(selectedUnit != null)
        {		
			//remove any units on the list	
            if (currentlySelectedUnits.Count > 0)
            {				
                for (int i = 0; i < currentlySelectedUnits.Count; i++)
                {
                    currentlySelectedUnits.Remove(currentlySelectedUnits[i]);
                }
            }
			//add the unit to the selected list and return to original selectFSM, click or drag
            else if (currentlySelectedUnits.Count == 0)
            {
                AddToCurrentlySelectedUnits(selectedUnit);
                selectFSM = SelectFSM.clickOrDrag; 
            }
        }
        else
        {
            Debug.Log("Select Single Unit Problems"); 
        }
    }

    private void DrawDragBox()
    {
        boxWidth = Camera.main.WorldToScreenPoint(mouseDownPoint).x - Camera.main.WorldToScreenPoint(currentMousePoint).x;
        boxHeight = Camera.main.WorldToScreenPoint(mouseDownPoint).y - Camera.main.WorldToScreenPoint(currentMousePoint).y;
        boxLeft = Input.mousePosition.x; 
        boxTop = (Screen.height - Input.mousePosition.y) - boxHeight; //need to invert y and y as GUI space has 0,0 at top left, but Screen space has 0,0 at bottom left.

        if (boxWidth > 0 && boxHeight < 0f)
            boxStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        else if (boxWidth > 0 && boxHeight > 0f)
            boxStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y + boxHeight);
        else if (boxWidth < 0 && boxHeight < 0f)
            boxStart = new Vector2(Input.mousePosition.x + boxWidth, Input.mousePosition.y);
        else if (boxWidth < 0 && boxHeight > 0f)
            boxStart = new Vector2(Input.mousePosition.x + boxWidth, Input.mousePosition.y + boxHeight);

        boxFinish = new Vector2(boxStart.x + Mathf.Abs(boxWidth), boxStart.y - Mathf.Abs(boxHeight));
    }

    private bool UserDraggingByPosition(Vector2 dragStartPoint, Vector2 newPoint)
    {
        if ((newPoint.x > dragStartPoint.x || newPoint.x < dragStartPoint.x) || (newPoint.y > dragStartPoint.y || newPoint.y < dragStartPoint.y))
            return true;
        else
            return false;
    }

    private void SelectUnitsInDrag()
    {
		//loop through al units in the game
        for (int i = 0; i < UnitManager.instance.units.Count; i++)
        {
                Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(UnitManager.instance.units[i].transform.position);
				
				//if units within box add to list, else remove them. 
                if (unitScreenPosition.x < boxFinish.x && unitScreenPosition.y > boxFinish.y && unitScreenPosition.x > boxStart.x && unitScreenPosition.y < boxStart.y)
                    AddToCurrentlySelectedUnits(UnitManager.instance.units[i]);
                else
                {
                    RemoveFromCurrentlySelectedUnits(UnitManager.instance.units[i]);
                }
        }
    }

	//add unit to list
    private void AddToCurrentlySelectedUnits(GameObject unitToAdd)
    {
        if(!currentlySelectedUnits.Contains(unitToAdd))
        {
            currentlySelectedUnits.Add(unitToAdd);
       }
    }

	//remove unit from list
    private void RemoveFromCurrentlySelectedUnits(GameObject unitToRemove)
    {
        if(currentlySelectedUnits.Count > 0)
        {
            currentlySelectedUnits.Remove(unitToRemove);
        }
    }

	//remove all units from the list
    private void DeselectAll()
    {
        if(currentlySelectedUnits.Count > 0)
        {
            for(int i = 0; i < currentlySelectedUnits.Count; i++)
            {
                currentlySelectedUnits.Remove(currentlySelectedUnits[i]);
            }
        }
        else if(currentlySelectedUnits.Count == 0)
        {
            selectFSM = SelectFSM.clickOrDrag; 
        }
    }
}
