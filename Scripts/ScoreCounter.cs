using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class ScoreCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    public static ScoreCounter Instance { get; private set; }
    private int _score;
    public int Score
    {
        get => _score;
        set
        {
            if (_score == value) return;
            _score = value;
            scoreText.SetText($"Score={_score}");
        }
    }
    private void Awake() => Instance = this;
   
}
