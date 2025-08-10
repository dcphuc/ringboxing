using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Joystick joystick;

    public Action<Vector2> OnMovement;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (joystick != null)
        {
            Vector2 direction = joystick.Direction;
            OnMovement?.Invoke(direction);
        }


    }


}
