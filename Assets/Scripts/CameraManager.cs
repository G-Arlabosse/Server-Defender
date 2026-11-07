using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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

        transform.position = new Vector3(INITIAL_CAMERA_POSITION.x, INITIAL_CAMERA_HEIGHT, INITIAL_CAMERA_POSITION.y);
    }



    public Vector2 getCursorPosition()
    {
        print(Input.mousePosition);
        Vector2 MousePos = cam.ScreenToWorldPoint(Input.mousePosition);


        return MousePos;
    }

    private void Update()
    {
        print(Input.mousePosition);
    }
}
