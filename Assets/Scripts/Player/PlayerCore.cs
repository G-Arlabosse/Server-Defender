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

        CreateTower();
    }


    public void CreateTower()
    {

        int towerCost = 5; 

        Vector3 position = cameraManager.GetPositionFromMouse();

        //if (position == Vector3.zero) return;

        if (money < towerCost) return;

        bool success = playerCommunicant.CreateTower(new Vector3(3, 0, 3));

        if (success)
        {
            money -= towerCost;
        }

    }


}
