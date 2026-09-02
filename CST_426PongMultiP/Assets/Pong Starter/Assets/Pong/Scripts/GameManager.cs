using TMPro;
using Unity.Netcode;
using UnityEngine;


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

    readonly NetworkVariable<int> _leftPlayerScore = new();
    readonly NetworkVariable<int> _rightPlayerScore = new();
    
    const int ScoreToWin = 11;

    public override void OnNetworkSpawn()
    {
        _leftPlayerScore.OnValueChanged += OnScoreChanged;
        _rightPlayerScore.OnValueChanged += OnScoreChanged;
        
        UpdateScore();
        //StartGame();
    }
    
    public override void OnNetworkDespawn()
    {
        _leftPlayerScore.OnValueChanged -= OnScoreChanged;
        _rightPlayerScore.OnValueChanged -= OnScoreChanged;
    }

    void OnScoreChanged(int oldValue, int newValue)
    {
        UpdateScore();
    }

    public void StartGame()
    {
        if (!IsServer) return;
        
        float direction = Random.value < 0.5f ? -1f : 1f;
        ResetBall(direction);
    }

    public void OnGoalScored(PaddleSide scoringSide)
    {
        if (!IsServer) return;
        
        // If the ball entered a goal area, increment the score, check for win, and reset the ball
        
        if (scoringSide == PaddleSide.Left)
        {
            _leftPlayerScore.Value++;
            Debug.Log($"Left player scored: {_leftPlayerScore.Value}");

            if (_leftPlayerScore.Value == ScoreToWin)
                Debug.Log("Left player wins!");
            else
                ResetBall(1f);
        }
        else if (scoringSide == PaddleSide.Right)
        {
            _rightPlayerScore.Value++;
            Debug.Log($"Right player scored: {_rightPlayerScore.Value}");

            if (_rightPlayerScore.Value == ScoreToWin)
                Debug.Log("Right player wins!");
            else
                ResetBall(-1f);
        }

    }

    void UpdateScore()
    {
        rightPlayerScoreText.text = _rightPlayerScore.Value.ToString();
        leftPlayerScoreText.text = _leftPlayerScore.Value.ToString();
    }

    void ResetBall(float directionSign)
    {
        // Start the ball within 20 degrees off-center toward direction indicated by directionSign
        directionSign = Mathf.Sign(directionSign);
        Vector3 newVelocity = new Vector3(directionSign, 0f, 0f) * startSpeed;
        newVelocity = Quaternion.Euler(0f, Random.Range(-20f, 20f), 0f) * newVelocity;

        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.position = startPosition;
        ballRigidbody.linearVelocity = newVelocity;
        ballRigidbody.angularVelocity = Vector3.zero;
    }
}