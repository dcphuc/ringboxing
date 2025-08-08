using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defines : MonoBehaviour
{
    public const string PLAYER_TAG = "Player";
    public const string ENEMY_TAG = "Enemy";
    public const int MAX_ALLIES_PER_ENEMY = 1;
}

public enum CharacterState
{
    Idle,
    Walking,
    Attacking,
    Dead
}

public enum AnimationVariables
{
    IsVictory,
    IsWalking,
    IsKidneyPunchLeft,
    IsKidneyPunchRight,
    IsStomachHit,
    IsKidneyHit,
    IsJumpingOver,
    IsKnockedOut,
    IsStomachPunch
}
