using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class CameraManager : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private GameObject pivot;
    [SerializeField] private GameObject tileHighlight;

    [SerializeField] private Vector2 INITIAL_CAMERA_POSITION = new Vector2(7,7);
    [SerializeField] private float INITIAL_CAMERA_HEIGHT = 4.0f;

    private Vector2 screenSize;
    private Vector2 cameraMovement = Vector2.zero;
    private float cameraRotation = 0f;
    private float cameraSpeed = 20.0f;
    private float cameraRotationSpeed = 50.0f;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();

            Assert.IsNotNull(cam);
        }


        cam.transform.position = new Vector3(INITIAL_CAMERA_POSITION.x, INITIAL_CAMERA_HEIGHT, INITIAL_CAMERA_POSITION.y);
        screenSize = new Vector2(cam.pixelWidth, cam.pixelHeight);
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

    public void Update()
    {

        if (cameraMovement != Vector2.zero)
        {
            transform.position += new Vector3(cameraMovement.x, 0, cameraMovement.y) * Time.deltaTime * cameraSpeed;
        }
        if (cameraRotation != 0f)
        {
            Vector3 pivotRotation = pivot.transform.rotation.eulerAngles;
            pivotRotation.x = Mathf.Clamp(pivotRotation.x - cameraRotation * Time.deltaTime * cameraRotationSpeed, 25f, 90f);
            pivot.transform.rotation = Quaternion.Euler(pivotRotation);
        }

        Vector2 mousePos = Input.mousePosition;
        
        //Size of the screen in world units
        

        if (mousePos.x < 0.1f * screenSize.x)
        {
            transform.position += new Vector3(-0.1f, 0, 0);
        }
        else if (mousePos.x > 0.9f * screenSize.x)
        {
            transform.position += new Vector3(0.1f, 0, 0);
        }

        if (mousePos.y < 0.1f * screenSize.y)
        {
            transform.position += new Vector3(0, 0, -0.1f);
        }
        else if (mousePos.y > 0.9f * screenSize.y)
        {
            transform.position += new Vector3(0, 0, 0.1f);
        }

        // update the position of the tile highlight to match the mouse position
        if (tileHighlight != null)
        {
            Vector3 mouseWorldPos = GetPositionFromMouse();

            if (mouseWorldPos == Vector3.zero) 
                tileHighlight.transform.position = new Vector3(0, -1f, 0);
            else
                tileHighlight.transform.position = TilesManager.getCenterPosition(mouseWorldPos) + new Vector3(0, 1f, 0); 
        }

    }

    private void OnMovement(InputValue value)
    {
        if (value == null) {
            cameraMovement = Vector2.zero;
            return;
        }
        
        Vector2 movement = value.Get<Vector2>();
        cameraMovement = movement;
    }

    private void OnRotation(InputValue value)
    {
        if (value == null) {
            cameraRotation = 0f;
            return;
        }
        
        float rotation = value.Get<float>();
        cameraRotation = rotation;
    }

}
