using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum MatchPhase
{
    Waiting,
    Playing,
    GameOver
}

/*
 * GameManager owns the local match rules: scoring, win checks, and ball resets.
 * In the starter project, everything happens in one Unity player. This is
 * the script you will convert so the server owns shared game state and the
 * score is synchronized to every client.
 */

public class GameManager : NetworkBehaviour
{
    [SerializeField] Transform ball;
    [SerializeField] float startSpeed = 3f;
    [SerializeField] Vector3 startPosition = new(0f, 0.25f, 0f);
    [SerializeField] TextMeshProUGUI leftPlayerScoreText;
    [SerializeField] TextMeshProUGUI rightPlayerScoreText;
    [SerializeField] GameObject matchOverPanel;
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] Button restartButton;

    readonly NetworkVariable<int> _leftPlayerScore = new();
    readonly NetworkVariable<int> _rightPlayerScore = new();
    readonly NetworkVariable<MatchPhase> _matchPhase = new(MatchPhase.Waiting);
    readonly NetworkVariable<PaddleSide> _winner = new(PaddleSide.Left);

    public static event EventHandler OnMatchOver;
    public static event EventHandler OnMatchStarted;
    
    const int ScoreToWin = 5;

    public override void OnNetworkSpawn()
    {
        _leftPlayerScore.OnValueChanged += OnScoreChanged;
        _rightPlayerScore.OnValueChanged += OnScoreChanged;
        _matchPhase.OnValueChanged += OnMatchPhaseChanged;
        _winner.OnValueChanged += OnWinnerChanged;
        
        UpdateScore();
        UpdateMatchUI();
        //StartGame();
    }
    
    public override void OnNetworkDespawn()
    {
        _leftPlayerScore.OnValueChanged -= OnScoreChanged;
        _rightPlayerScore.OnValueChanged -= OnScoreChanged;
        _matchPhase.OnValueChanged -= OnMatchPhaseChanged;
        _winner.OnValueChanged -= OnWinnerChanged;
    }

    void OnScoreChanged(int oldValue, int newValue)
    {
        UpdateScore();
    }

    void OnMatchPhaseChanged(MatchPhase oldValue, MatchPhase newValue)
    {
        UpdateMatchUI();

        if (newValue == MatchPhase.GameOver)
            OnMatchOver?.Invoke(this, EventArgs.Empty);
        else if (newValue == MatchPhase.Playing)
            OnMatchStarted?.Invoke(this, EventArgs.Empty);
    }

    void OnWinnerChanged(PaddleSide oldValue, PaddleSide newValue)
    {
        UpdateMatchUI();
    }

    public void StartGame()
    {
        if (!IsServer) return;
        if (NetworkManager.ConnectedClients.Count < 2) return;

        _leftPlayerScore.Value = 0;
        _rightPlayerScore.Value = 0;
        _matchPhase.Value = MatchPhase.Playing;
        
        float direction = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        ResetBall(direction);
    }

    public void RestartGame()
    {
        if (!IsServer) return;

        StartGame();
    }

    public void OnGoalScored(PaddleSide scoringSide)
    {
        if (!IsServer) return;
        if (_matchPhase.Value != MatchPhase.Playing) return;
        
        // If the ball entered a goal area, increment the score, check for win, and reset the ball
        
        if (scoringSide == PaddleSide.Left)
        {
            _leftPlayerScore.Value++;
            Debug.Log($"Left player scored: {_leftPlayerScore.Value}");

            if (_leftPlayerScore.Value == ScoreToWin)
            {
                Debug.Log("Left player wins!");
                EndMatch(PaddleSide.Left);
            }
            else
                ResetBall(1f);
        }
        else if (scoringSide == PaddleSide.Right)
        {
            _rightPlayerScore.Value++;
            Debug.Log($"Right player scored: {_rightPlayerScore.Value}");

            if (_rightPlayerScore.Value == ScoreToWin)
            {
                Debug.Log("Right player wins!");
                EndMatch(PaddleSide.Right);
            }
            else
                ResetBall(-1f);
        }

    }

    void UpdateScore()
    {
        rightPlayerScoreText.text = _rightPlayerScore.Value.ToString();
        leftPlayerScoreText.text = _leftPlayerScore.Value.ToString();
    }

    void UpdateMatchUI()
    {
        bool matchOver = _matchPhase.Value == MatchPhase.GameOver;

        matchOverPanel.SetActive(matchOver);
        restartButton.gameObject.SetActive(IsHost && matchOver);

        if (matchOver)
            winnerText.text = $"{_winner.Value} Player Wins!";
    }
    
    public void ShowConnectionMessage(string message)
    {
        matchOverPanel.SetActive(true);
        winnerText.text = message;
        winnerText.color = Color.red;
        restartButton.gameObject.SetActive(false);
        Invoke(nameof(CloseGame), 3f);
    }
    
    void CloseGame()
    {
        UnityEditor.EditorApplication.Exit(0);
    }

    void EndMatch(PaddleSide winner)
    {
        _winner.Value = winner;
        _matchPhase.Value = MatchPhase.GameOver;

        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
    }

    void ResetBall(float directionSign)
    {
        // Start the ball within 20 degrees off-center toward direction indicated by directionSign
        directionSign = Mathf.Sign(directionSign);
        Vector3 newVelocity = new Vector3(directionSign, 0f, 0f) * startSpeed;
        newVelocity = Quaternion.Euler(0f, UnityEngine.Random.Range(-20f, 20f), 0f) * newVelocity;

        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.position = startPosition;
        ballRigidbody.linearVelocity = newVelocity;
        ballRigidbody.angularVelocity = Vector3.zero;
    }
}
