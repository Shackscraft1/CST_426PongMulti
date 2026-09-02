using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Paddle owns local paddle movement and the ball bounce when the ball hits it.
 * In this starter, both paddles live in the scene and read local keyboard input.
 * In the multiplayer solution, this same responsibility splits by authority:
 * the owning client reads input, while the server handles shared ball physics.
 *
 * LeftX / RightX are magic world positions, not a real placement system.
 * Move the scene around and these numbers are silently wrong. We keep them so
 * the lesson stays on ownership instead of spawn-point wiring. The same
 * shortcut can stay in the multiplayer version: a Netcode-spawned prefab
 * cannot drag in the scene's goals.
 *
 * Doing this properly would mean storing the positions on GameManager,
 * searching the scene for the goals (easy to get wrong), or flipping the
 * prefab X if the table is centered at 0.
 */

public class Paddle : NetworkBehaviour
{
    [SerializeField] PaddleSide side;
    [SerializeField] float minTravelZ;
    [SerializeField] float maxTravelZ;
    [SerializeField] float speed;
    [SerializeField] float collisionBallSpeedUp = 1.5f;
    
    // Local two-player needs separate keys per paddle. InputSystem_Actions
    // already has a Player/Paddle axis (W/S) for the one-owner step.
    [SerializeField] Key moveUpKey = Key.W;
    [SerializeField] Key moveDownKey = Key.S;

    // Demo shortcut — see the class header note on LeftX / RightX.
    const float LeftX = -7.5f;
    const float RightX = 7.5f;



    public override void OnNetworkSpawn()
    {
        side = GetSide();
        ApplySidePosition();
    }

    private PaddleSide GetSide()
    {
        return OwnerClientId == NetworkManager.ServerClientId ? PaddleSide.Left : PaddleSide.Right;
    }

    void ApplySidePosition()
    {
        float x = side == PaddleSide.Left ? LeftX : RightX;
        Vector3 paddlePos = transform.position;
        paddlePos.x = x;
        transform.position = paddlePos;
    }

    void Update()
    {
        if (!IsOwner) return;
        
        float direction = 0f;
        if (Keyboard.current[moveUpKey].isPressed) direction += 1f;
        if (Keyboard.current[moveDownKey].isPressed) direction -= 1f;

        Vector3 newPosition = transform.position + new Vector3(0f, 0f, direction) * speed * Time.deltaTime;
        newPosition.z = Mathf.Clamp(newPosition.z, minTravelZ, maxTravelZ);

        transform.position = newPosition;
    }

    void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return;
        
        // Get world-space bounds
        var paddleBounds = GetComponent<BoxCollider>().bounds;

        float paddleCenterZ = paddleBounds.center.z;
        float paddleHalfHeight = paddleBounds.extents.z;
        float hitZ = other.GetContact(0).point.z;

        // Get a parameterized value roughly in the -1 to 1 range for where the ball hits
        float normalizedHit = (hitZ - paddleCenterZ) / paddleHalfHeight;

        // Cap it so that it stay within range (happens when hitting the corner of the paddle)
        float bounceDirection = Mathf.Clamp(normalizedHit, -1f, 1f);

        // Ideally we would use linearVelocity here.  Unfortunately, it is 0-length during the collision
        Vector3 currentVelocity = other.relativeVelocity;

        // The flipped sign will change the velocity direction appropriately for both paddles
        float newSign = -Mathf.Sign(currentVelocity.x);

        // Change the velocity between -60 to 60 degrees based on where it hit the paddle
        float newSpeed = currentVelocity.magnitude * collisionBallSpeedUp;
        float newAngle = 60f * bounceDirection * Mathf.Deg2Rad;

        // Calculate new velocity vector - using trig and scaled by new speed
        Vector3 newVelocity = new Vector3(newSign * Mathf.Cos(newAngle), 0f, Mathf.Sin(newAngle)) * newSpeed;
        other.rigidbody.linearVelocity = newVelocity;
    }
}