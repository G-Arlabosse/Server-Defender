using UnityEngine;

public class PerlinWorms : MonoBehaviour
{
    //https://github.com/SunnyValleyStudio/Perlin-Worms-Unity-tutorial/blob/main/Scripts/NoiseHelper.cs

    private Vector2 currentDirection;
    private Vector2 currentPosition;
    private Vector2 convergancePoint;
    public bool moveToConvergancepoint = false;
    [Range(0.5f, 0.9f)]
    public float weight = 0.6f;

}
