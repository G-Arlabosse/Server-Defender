using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerbis : MonoBehaviour
{
    public FlowManager flowManager;
    public InputAction placeWallAction;
    public InputAction placeTargetAction;

    void Start()
    {
        placeWallAction.Enable();
        placeTargetAction.Enable();
    }

    void Update()
    {
        // clic gauche
        if (placeWallAction.WasPressedThisFrame())
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
            mouseWorldPos.z = 0;
            flowManager.AddWall(mouseWorldPos);
        }
        // Clic droit
        if (placeTargetAction.WasPressedThisFrame())
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
            mouseWorldPos.z = 0;
            flowManager.AddTarget(mouseWorldPos);
        }
    }

    void OnDestroy()
    {
        placeWallAction.Disable();
        placeTargetAction.Disable();
    }
}