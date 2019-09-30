using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MovingCube : MonoBehaviour
{
    public static MovingCube CurrentCube { get; private set; }
    public static MovingCube LastCube { get; private set; }
    public  MoveDirection MoveDirection { get;  set; }
    public static bool StopSpawnCube { get; private set; }
    
    [SerializeField] private float moveSpeed=1f;

    private GameManager gameManager;
    private Color color;
    private bool moveForward=true;
    private float hangover;
    private float direction;

    private void OnEnable()
    {
        gameManager = FindObjectOfType<GameManager>();
        StopSpawnCube = false;
        
        if (LastCube == null)
            LastCube = gameManager.StartCube;
        
        CurrentCube = this;
        
        color = gameManager.GetGradientColor();
        GetComponent<Renderer>().material.color = color;
        
        transform.localScale = new Vector3(LastCube.transform.localScale.x,transform.localScale.y,
            LastCube.transform.localScale.z);
    }
    void Update()
    {
        switch (MoveDirection)
        {
            case MoveDirection.X:
                ChangedMoveDirection(Vector3.right, transform.position.x, transform.localScale.x,
                    LastCube.transform.localScale.x,LastCube.transform.position.x);
                break;
            
            case MoveDirection.Z:
                ChangedMoveDirection(Vector3.forward, transform.position.z, transform.localScale.z,
                    LastCube.transform.localScale.z, LastCube.transform.position.z);
                break;
        }
    }

    private void ChangedMoveDirection(Vector3 vectorDirection, float position,
        float scale, float lastCubeScale,  float lastCubePos)
    {
        var difference1 = position - lastCubePos - 0.5f * lastCubeScale - 0.5f * scale;
        var difference2 = position - lastCubePos + 0.5f * lastCubeScale + 0.5f * scale;
        
        switch (moveForward)
        {
            case true:
                if (difference1 < 0.1f)
                    transform.position += moveSpeed * Time.deltaTime * vectorDirection;
                else
                    moveForward = false;
                break;
            
            case false:
                if (difference2 > -0.1f)
                    transform.position -= moveSpeed * Time.deltaTime * vectorDirection;
                else
                    moveForward = true;
                break;
        }
    }

    internal void Stop()
    {
        moveSpeed = 0f;

        hangover = GetHangover();
        
        direction = hangover > 0 ? 1f : -1f;

        if (Math.Abs(hangover) < 0.06f)
            hangover = 0;
        
        if (hangover == 0)
            Handheld.Vibrate();

        float difference1;
        float difference2;
                          
        switch (MoveDirection)
        {
            case MoveDirection.X:
                difference1 = transform.position.x - LastCube.transform.position.x - 
                              0.5f * LastCube.transform.localScale.x - 0.5f * transform.localScale.x;
                difference2 = transform.position.x - LastCube.transform.position.x + 
                              0.5f * LastCube.transform.localScale.x  + 0.5f * transform.localScale.x;
                break;
            case MoveDirection.Z:
                difference1 = transform.position.z - LastCube.transform.position.z - 
                              0.5f * LastCube.transform.localScale.z - 0.5f * transform.localScale.z;
                difference2 = transform.position.z - LastCube.transform.position.z + 
                              0.5f * LastCube.transform.localScale.z  + 0.5f * transform.localScale.z;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        float max = MoveDirection == MoveDirection.Z?LastCube.transform.localScale.z:LastCube.transform.localScale.x;
            
        if (Math.Abs(hangover)-max >= 0.1f || difference1>-0.1f|| difference2<0.1f)
        {
            StopSpawnCube = true;
            CurrentCube.gameObject.AddComponent<Rigidbody>();
            gameManager.SetFinalCameraPosition();
            StartCoroutine(ReloadScene());
        }
        else
            SplitCube();

        LastCube = this;
    }

    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private float GetHangover()
    {
        if(MoveDirection==MoveDirection.Z)
            return transform.position.z - LastCube.transform.position.z;
        return transform.position.x - LastCube.transform.position.x;
    }

    private void SplitCube()
    {
        float newSize;
        float newPosition;
        
        switch (MoveDirection)
        {
            case MoveDirection.X:
                newSize = LastCube.transform.localScale.x-Mathf.Abs(hangover);
                newPosition = LastCube.transform.position.x + hangover * 0.5f;
                transform.localScale=new Vector3(newSize, transform.localScale.y,transform.localScale.z);
                transform.position = new Vector3(newPosition, transform.position.y, transform.position.z);
                if (newSize < LastCube.transform.localScale.x)
                    FallingBlockSize(transform.position.z, newSize);
                break;
            
            case MoveDirection.Z:
                newSize=LastCube.transform.localScale.z-Mathf.Abs(hangover);
                newPosition = LastCube.transform.position.z + hangover * 0.5f;
                transform.localScale=new Vector3(transform.localScale.x, transform.localScale.y,newSize);
                transform.position = new Vector3(transform.position.x, transform.position.y, newPosition);
                if (newSize < LastCube.transform.localScale.z)
                    FallingBlockSize(transform.position.z, newSize);
                break;
        }
    }

    private void FallingBlockSize(float position, float newSize)
    {
        float fallingBlockSize = Mathf.Abs(hangover);
        float cubeEdge = position + newSize * 0.5f * direction;
        float fallingBlockPosition = cubeEdge + fallingBlockSize * 0.5f*direction;

        SpawnDropCube(fallingBlockPosition, fallingBlockSize);
    }
    
    private void SpawnDropCube(float fallingBlockPosition, float fallingBlockSize)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<Renderer>().material.color = color;
        if (MoveDirection == MoveDirection.Z)
        {
            cube.transform.localScale=new Vector3(transform.localScale.x, transform.localScale.y, fallingBlockSize);
            cube.transform.position=new Vector3(transform.position.x, transform.position.y,fallingBlockPosition ); 
        }
        else
        {
            cube.transform.localScale=new Vector3(fallingBlockSize,transform.localScale.y, transform.localScale.z);
            cube.transform.position=new Vector3(fallingBlockPosition,transform.position.y, transform.position.z); 
        }
        
        cube.AddComponent<Rigidbody>();
        Destroy(cube.gameObject, 1f);
    }
}
