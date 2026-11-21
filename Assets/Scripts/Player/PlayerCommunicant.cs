using UnityEngine;
using UnityEngine.Assertions;

public class PlayerCommunicant : MonoBehaviour
{
    [SerializeField] private PlayerCore playerCore;

    [SerializeField] private MapCommunicant mapCommunicant;


    private void Start()
    {
        if (playerCore == null)
        {
            playerCore = GetComponent<PlayerCore>();

            Assert.IsNotNull(playerCore, "PlayerCore component is missing from the GameObject.");
        }
    }

    public bool CreateTower(Vector3 position)
    {
        // Can't create a tower if the MapCommunicant is not assigned
        if (mapCommunicant == null) return false;


        return mapCommunicant.CreateTower(position);
    }



}
