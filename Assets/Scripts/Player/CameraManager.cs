using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class CameraManager : MonoBehaviour
{

    [SerializeField] private Camera cam;

    [SerializeField] private Vector2 INITIAL_CAMERA_POSITION = new Vector2(7,7);
    [SerializeField] private float INITIAL_CAMERA_HEIGHT = 4.0f;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();

            Assert.IsNotNull(cam);
        }

        cam.transform.position = new Vector3(INITIAL_CAMERA_POSITION.x, INITIAL_CAMERA_HEIGHT, INITIAL_CAMERA_POSITION.y);
    }



    private Vector2 getCursorPosition()
    {
        Vector2 MousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        return MousePos;
    }

    public Vector3 GetPositionFromMouse()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        //print(Input.mousePosition);
        Ray ray = cam.ScreenPointToRay(mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

}
