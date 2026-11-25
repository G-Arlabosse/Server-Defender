using UnityEngine;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerCore playerCore;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (playerCore == null)
        {
            playerCore = GetComponent<PlayerCore>();
            Assert.IsNotNull(playerCore, "PlayerCore component is missing from the GameObject.");
        }
    }

    private void OnAddTower()
    {
        playerCore.CreateTower();
    }
}
