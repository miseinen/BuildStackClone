using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public enum GameState
{
    StartGame,
    SpawnCube,
    CropCube,
}
public class GameManager : MonoBehaviour
{
    [SerializeField] private MovingCube startCube;
    [SerializeField] private CubeSpawner[] cubeSpawners;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ScoreText scoreText;

    private GameState gameState=GameState.SpawnCube;
    private CubeSpawner currentSpawner;
    private int spawnerIndex;
    private Color color;
    private static float rColorStep=1f;
    private static float gColorStep;
    private static float bColorStep;
    private float yCameraPosition;
    public MovingCube StartCube => startCube;

    public  Action OnCubeSpawned;

    private void Start() => gameState = GameState.StartGame;

    void Update()
    {
        switch (gameState)
        {
            case GameState.StartGame:
                scoreText.SetMenuText();
                break;
            
            case GameState.SpawnCube:
                if (!MovingCube.StopSpawnCube)
                {
                    spawnerIndex = UnityEngine.Random.Range(0, cubeSpawners.Length);
                    currentSpawner = cubeSpawners[spawnerIndex];
                    currentSpawner.SpawnCube();
                    CameraMove();
                    gameState = GameState.CropCube; 
                }
                break;
            
            case GameState.CropCube:
                break;
        }

        if (Input.GetMouseButtonDown(0) && gameState == GameState.StartGame)
            gameState = GameState.SpawnCube;
        
        if (Input.GetMouseButtonDown(0) && gameState==GameState.CropCube)
        {
            if (MovingCube.CurrentCube != null)
            {
                MovingCube.CurrentCube.Stop();
                OnCubeSpawned?.Invoke();
                gameState = GameState.SpawnCube;
            }
        }
    }

    private void CameraMove()
    {
        var cameraPosition = mainCamera.transform.position;
        cameraPosition.y = 2f+MovingCube.CurrentCube.transform.position.y;
        mainCamera.transform.position = cameraPosition;
    }

    public  Color GetGradientColor()
    {
        var step = 0.25f;

        if (rColorStep >= 1f && gColorStep < 1f && bColorStep<=0f)
            gColorStep += step;
        
        else if (rColorStep > 0f && gColorStep >= 1f && bColorStep<=0f)
            rColorStep -= step;
        
        else if (rColorStep <= 0f && gColorStep>=1f &&bColorStep < 1f)
            bColorStep += step;
        
        else if (rColorStep<=0f && gColorStep > 0f && bColorStep >= 1f)
            gColorStep -= step;
        
        else if (rColorStep < 1f && gColorStep <= 0f && bColorStep>=1f)
            rColorStep += step;
        
        else
            bColorStep -= step;
        
        color=new Color(rColorStep,gColorStep,bColorStep,1f);
        return color;
    }

    public void SetFinalCameraPosition()
    {
        var position = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(position.x, 1.83f, position.z);
        if (MovingCube.LastCube.transform.position.y < 2f)
            mainCamera.GetComponent<Camera>().orthographicSize = 2f;
        else
            mainCamera.GetComponent<Camera>().orthographicSize = 2f*MovingCube.LastCube.transform.position.y;
        
    }
}
