using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI tapToPlayText;
    [SerializeField] private GameManager gameManager;
    
    private int score;
    private static int highScore;
    float alpha = 0f;
    
    private void Start()
    {
        gameManager.OnCubeSpawned += SetScoreText;
        highScore = PlayerPrefs.GetInt("HighScore");
    }

    private void OnDestroy() => gameManager.OnCubeSpawned -= SetScoreText;

    private void SetScoreText()
    {
        score++;
        highScore = score > highScore ? score : highScore;
        var key = "HighScore";
        PlayerPrefs.SetInt(key,highScore);
        scoreText.text = $"{score}";
        tapToPlayText.text = "";
    }

    public void SetMenuText()
    {
        scoreText.text = $"HighScore:{highScore}";
        tapToPlayText.text = "Tap to Start";
        alpha += 0.7f * Time.deltaTime;
        scoreText.color=new Color(scoreText.color.g,scoreText.color.g,scoreText.color.b,alpha);
        tapToPlayText.color=new Color(tapToPlayText.color.g,tapToPlayText.color.g,tapToPlayText.color.b,alpha);
    }
}
