# Pong Starter

This project starts as a local two-player Pong game. The goal is to keep the scene and scripts easy to understand before adding multiplayer.

The Netcode package is already installed, but the game does not depend on networking yet. You will convert this project toward the multiplayer solution by deciding which logic should run on the owner, which logic should run on the server, and which state needs to be synchronized.

## Local controls

- Left paddle: W / S
- Right paddle: Up Arrow / Down Arrow

## Suggested multiplayer path

1. Add the Network Manager and transport.
2. Convert the scripts that need network lifecycle hooks from `MonoBehaviour` to `NetworkBehaviour`.
3. Turn `Player.prefab` into the player prefab spawned by Netcode.
4. Set each spawned paddle's side when it joins.
5. Let the owning client read paddle input.
6. Keep ball physics, goals, scoring, and match start server-authoritative.
7. Synchronize score so every client sees the same match state.
