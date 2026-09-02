using UnityEngine;

/*
 * GoalTrigger is a scoring zone at one end of the table.
 * In the starter, it directly tells the local GameManager who scored.
 * You will make this server-only so two clients cannot score twice.
 */

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] PaddleSide scoringSide;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
            gameManager.OnGoalScored(scoringSide);
    }
}