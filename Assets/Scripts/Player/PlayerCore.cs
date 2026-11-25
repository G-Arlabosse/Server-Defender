using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class PlayerCore : MonoBehaviour
{

    [SerializeField] private PlayerCommunicant playerCommunicant;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraManager cameraManager;

    private int money = 200;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerCommunicant == null)
        {
            playerCommunicant = GetComponent<PlayerCommunicant>();
            Assert.IsNotNull(playerCommunicant, "PlayerCommunicant component is missing from the GameObject.");
        }
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            Assert.IsNotNull(playerController, "PlayerController component is missing from the GameObject.");
        }

    }


    public void CreateTower()
    {

        int towerCost = 5; 

        Vector3 position = cameraManager.GetPositionFromMouse();

        if (position == Vector3.zero) 
        {
            Debug.LogWarning("Invalid tower position.");
            return;
        }

        if (money < towerCost) 
            return;

        bool success = playerCommunicant.CreateTower(position);

        if (success)
        {
            money -= towerCost;
        }

    }


}
