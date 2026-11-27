using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyManagerbis : MonoBehaviour
{

    private List<Enemybis> activeEnemies = new List<Enemybis>();
    private Camera mainCam;
    public GameObject enemyPrefab;
    public FlowManager flowField;
    public InputAction spawnMelee;
    public InputAction spawnRanged;
    public InputAction spawnFlying;
    public InputAction clearAction;

    void Start()
    {
        mainCam = Camera.main;
        spawnMelee.Enable();
        spawnRanged.Enable();
        spawnFlying.Enable();
        clearAction.Enable();
    }

    void Update()
    {
        // N
        if (spawnMelee.IsPressed())
        {
            SpawnEnemy(EnemyType.Melee);
        }
        // B
        if (spawnRanged.IsPressed())
        {
            SpawnEnemy(EnemyType.Ranged);
        }
        // V
        if (spawnFlying.IsPressed())
        {
            SpawnEnemy(EnemyType.Flying);
        }
        // C
        if (clearAction.WasPressedThisFrame())
        {
            ClearAllEnemies();
        }
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Vector3 moveDir = flowField.GetMoveDirection(enemy.transform.position, enemy.type);
                enemy.Move(moveDir);
            }
        }
    }

    void OnDestroy()
    {
        spawnMelee.Disable();
        spawnRanged.Disable();
        spawnFlying.Disable();
        clearAction.Disable();
    }

    void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();
    }

    void SpawnEnemy(EnemyType type)
    {
        float height = 2f * mainCam.orthographicSize;
        float width = height * mainCam.aspect;
        float x = 0f, y = 0f;
        int edge = Random.Range(0, 4);

        if (edge == 0) //Haut
        {
            x = Random.Range(-width / 2f, width / 2f);
            y = height / 2f;
        }
        else if (edge == 1) // Droite
        {
            x = width / 2f;
            y = Random.Range(-height / 2f, height / 2f);
        }
        else if (edge == 2) //Gauche
        {
            x = Random.Range(-width / 2f, width / 2f);
            y = -height / 2f;
        }
        else //Bas
        {
            x = -width / 2f;
            y = Random.Range(-height / 2f, height / 2f);
        }

        Vector3 spawnPos = mainCam.transform.position + new Vector3(x, y, 10f);
        spawnPos.z = 0;
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemybis enemyScript = newEnemy.GetComponent<Enemybis>();
        enemyScript.type = type;
        //BWAH
        SpriteRenderer sr = newEnemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (type == EnemyType.Flying) sr.color = Color.cyan;
            if (type == EnemyType.Ranged) sr.color = Color.green;
            if (type == EnemyType.Melee) sr.color = Color.red;
        }

        activeEnemies.Add(newEnemy.GetComponent<Enemybis>());
    }
}