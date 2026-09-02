/*
 * PaddleSide gives readable names to the two sides of the Pong table.
 * Paddles use it for placement, and goals use it to tell GameManager which
 * player should receive a point.
 */

public enum PaddleSide : byte
{
    Left = 0,
    Right = 1
}
