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
    
    [Header("Settings")]
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
        var maxForwardPosition = position - lastCubePos - 0.5f * lastCubeScale - 0.5f * scale;
        var maxBackPosition = position - lastCubePos + 0.5f * lastCubeScale + 0.5f * scale;
        
        switch (moveForward)
        {
            case true:
                if (maxForwardPosition < 0.1f)
                    transform.position += moveSpeed * Time.deltaTime * vectorDirection;
                else
                    moveForward = false;
                break;
            
            case false:
                if (maxBackPosition > -0.1f)
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

        if (Math.Abs(hangover) < 0.06f) //some fix for more comfortable game
            hangover = 0;
        
        if (hangover == 0)
            Handheld.Vibrate();

        float maxForwardEdge;
        float maxBackEdge;
                          
        switch (MoveDirection)
        {
            case MoveDirection.X:
                maxForwardEdge = transform.position.x - LastCube.transform.position.x - 
                              0.5f * LastCube.transform.localScale.x - 0.5f * transform.localScale.x;
                maxBackEdge = transform.position.x - LastCube.transform.position.x + 
                              0.5f * LastCube.transform.localScale.x  + 0.5f * transform.localScale.x;
                break;
            case MoveDirection.Z:
                maxForwardEdge = transform.position.z - LastCube.transform.position.z - 
                              0.5f * LastCube.transform.localScale.z - 0.5f * transform.localScale.z;
                maxBackEdge = transform.position.z - LastCube.transform.position.z + 
                              0.5f * LastCube.transform.localScale.z  + 0.5f * transform.localScale.z;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        float max = MoveDirection == MoveDirection.Z?LastCube.transform.localScale.z:LastCube.transform.localScale.x;
            
        if (Math.Abs(hangover)-max >= 0.1f || maxForwardEdge>-0.1f|| maxBackEdge<0.1f)
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
                    FallingBlock(transform.position.z, newSize);
                break;
            
            case MoveDirection.Z:
                newSize=LastCube.transform.localScale.z-Mathf.Abs(hangover);
                newPosition = LastCube.transform.position.z + hangover * 0.5f;
                transform.localScale=new Vector3(transform.localScale.x, transform.localScale.y,newSize);
                transform.position = new Vector3(transform.position.x, transform.position.y, newPosition);
                if (newSize < LastCube.transform.localScale.z)
                    FallingBlock(transform.position.z, newSize);
                break;
        }
    }

    private void FallingBlock(float position, float newSize)
    {
        float fallingBlockSize = Mathf.Abs(hangover);
        float cubeEdge = position + newSize * 0.5f * direction;
        float fallingBlockPosition = cubeEdge + fallingBlockSize * 0.5f*direction;

        SpawnDropCube(fallingBlockPosition, fallingBlockSize);
    }
    
    private void SpawnDropCube(float fallingBlockPosition, float fallingBlockSize)
    {
        var fallingCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fallingCube.GetComponent<Renderer>().material.color = color;
        
        if (MoveDirection == MoveDirection.Z)
        {
            fallingCube.transform.localScale=new Vector3(transform.localScale.x, transform.localScale.y, fallingBlockSize);
            fallingCube.transform.position=new Vector3(transform.position.x, transform.position.y,fallingBlockPosition ); 
        }
        else
        {
            fallingCube.transform.localScale=new Vector3(fallingBlockSize,transform.localScale.y, transform.localScale.z);
            fallingCube.transform.position=new Vector3(fallingBlockPosition,transform.position.y, transform.position.z); 
        }
        
        fallingCube.AddComponent<Rigidbody>();
        Destroy(fallingCube.gameObject, 1f);
    }
}
