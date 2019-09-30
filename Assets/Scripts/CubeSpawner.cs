using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDirection
{
    X,
    Z,
}
public class CubeSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovingCube cubePrefab;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MoveDirection moveDirection;
    
    public void SpawnCube()
    {
        var movingCube = Instantiate(cubePrefab);

        if (MovingCube.LastCube != null && MovingCube.LastCube!=gameManager.StartCube)
        {
            float x = moveDirection == MoveDirection.X?
                transform.position.x:MovingCube.LastCube.transform.position.x;
            
            float z = moveDirection == MoveDirection.Z?
                transform.position.z:MovingCube.LastCube.transform.position.z;
            
            movingCube.transform.position = new Vector3(x,
                MovingCube.LastCube.transform.position.y + cubePrefab.transform.localScale.y, z);
        }
        else
            movingCube.transform.position = transform.position;

        movingCube.MoveDirection = moveDirection;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color=Color.green;
        Gizmos.DrawWireCube(transform.position,cubePrefab.transform.localScale);
    }
}
