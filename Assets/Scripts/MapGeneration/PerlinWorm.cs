using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinWorm : MonoBehaviour
{
    //https://github.com/SunnyValleyStudio/Perlin-Worms-Unity-tutorial/blob/main/Scripts/NoiseHelper.cs

    private Vector2 currentDirection;
    private Vector2 currentPosition;
    private Vector2 convergancePoint;
    private readonly Vector2[,] perlinGradient;

    public bool moveToConvergancepoint = false;

    [UnityEngine.Range(0.5f, 0.9f)]
    public float weight = 0.6f;

    /// <summary>
    /// Constructor for worm without convergence point
    /// </summary>
    /// <param name="grad"></param>
    /// <param name="startPosition"></param>
    public PerlinWorm(Vector2[,] grad, Vector2 startPosition)
    {
        this.currentPosition = startPosition;
        moveToConvergancepoint = false;
        this.perlinGradient = grad;
        currentDirection = Vector2.zero;
    }

    /// <summary>
    /// Constructor for worm with convergence point
    /// </summary>
    /// <param name="grad"></param>
    /// <param name="startPosition"></param>
    /// <param name="convergancePoint"></param>
    public PerlinWorm(Vector2[,] grad, Vector2 startPosition, Vector2 convergancePoint)
    {
        this.currentPosition = startPosition;
        this.convergancePoint = convergancePoint;
        moveToConvergancepoint = true;
        this.perlinGradient = grad;
        currentDirection = Vector2.zero;
    }

    /// <summary>
    /// Return the tangent of the gradient normalized
    /// </summary>
    /// <returns></returns>
    private Vector2 GetPerlinDirection()
    {
        Vector2 grad = perlinGradient[(int)currentPosition.x, (int)currentPosition.y];
        Vector2 dir = new Vector2(-grad.y, grad.x);

        currentDirection = dir.normalized;
        return currentDirection;
    }

    /// <summary>
    /// Move towards convergance point modulated by weight
    /// </summary>
    /// <returns></returns>
    public Vector2 MoveTowardsConvergancePoint()
    {
        Vector2 direction = GetPerlinDirection();
        Vector2 directionToConvergancePoint = (currentPosition - convergancePoint).normalized;
        currentPosition += (direction * (1 - weight) + directionToConvergancePoint * weight).normalized;
        return currentPosition;

    }

    /// <summary>
    /// Move according to gradient direction
    /// </summary>
    /// <returns></returns>
    public Vector2 Move()
    {
        Vector2 direction = GetPerlinDirection();
        currentPosition += direction;
        return currentPosition;
    }

    /// <summary>
    /// Iterate the worm for length iteration and return the resulting points
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public List<Vector2> MoveLength(int length)
    {
        List<Vector2> points = new List<Vector2>();
        for(int i = 0; i < length; i++)
        {
            if (moveToConvergancepoint)
            {
                var point = MoveTowardsConvergancePoint();
                points.Add(point);
                if (Vector2.Distance(point, convergancePoint) < 1)
                {
                    break;
                }
            } else
            {
                var point = Move();
                points.Add(point);
            }
        }

        //making sure to reach convergance point
        if (moveToConvergancepoint)
        {
            while (Vector2.Distance(this.convergancePoint, currentPosition) > 1)
            {
                weight = 0.9f;
                var result = MoveTowardsConvergancePoint();
                points.Add(result);
                if (Vector2.Distance(this.convergancePoint, result) < 1)
                {
                    break;
                }
            }
        }

        return points;
    }






}
