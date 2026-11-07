using System;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class TestTilesManager : MonoBehaviour
{

    [SerializeField] private TilesManager tilesManager;


   private void Start()
   {
      Vector2Int squareCornerTop = new Vector2Int(-5,-5);
      Vector2Int squareCornerBottom = new Vector2Int(5,5);
   }



}
