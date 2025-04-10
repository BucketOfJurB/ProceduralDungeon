using UnityEngine;
using UnityEngine.Events;

public class ClickToMove : MonoBehaviour
{
    private Vector3 clickPosition;
    private Ray ray;
    private float rayLength;

    public UnityEvent<Vector3> OnMouseClick;

    void Update()
    {
        // Get the mouse click position in world space
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
            {
                Vector3 clickWorldPosition = hitInfo.point;
                Debug.Log(clickWorldPosition);


                clickPosition = clickWorldPosition;
                ray = mouseRay;
                rayLength = hitInfo.distance;

                OnMouseClick.Invoke(clickWorldPosition);
            }
        }

        Debug.DrawLine(transform.position, clickPosition, Color.blue);
        DebugExtension.DebugWireSphere(clickPosition);
    }

}
