using UnityEngine;
using UnityEngine.Events;

public class MouseClickController : MonoBehaviour
{
    public Vector3 clickPosition;

    private Vector3 lastClicked;

    public UnityEvent<Vector3> OnClick;
    
    void Update() { 
        // Get the mouse click position in world space 
        if (Input.GetMouseButtonDown(0)) { 
            Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition ); 
            if (Physics.Raycast( mouseRay, out RaycastHit hitInfo )) { 
                Vector3 clickWorldPosition = hitInfo.point; 
                Debug.Log(clickWorldPosition);

                lastClicked = clickWorldPosition;

                OnClick.Invoke(lastClicked);
            } 
        }

        if (lastClicked != new Vector3(0,0,0))
        {
            Debug.DrawLine(transform.position, lastClicked, Color.yellow);
            DebugExtension.DebugWireSphere(lastClicked, Color.blue);
        }
    } 
}
