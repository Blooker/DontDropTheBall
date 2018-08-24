﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class PlayerInput : MonoBehaviour {
    [Header("Sticks")]
    [Range(0, 1)]
    [SerializeField] private float stickDeadzone;

    private PlayerController playerController;
    private bool playerIndexSet = false;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    PlayerIndex playerIndex;
    GamePadState state;
    GamePadState prevState;

#endif

    public void SetPlayer (PlayerController _playerController) {
        playerController = _playerController;
    }

    // Use this for initialization
    void Awake () {
        playerController = GetComponent<PlayerController>();
    }
	
	// Update is called once per frame
	void Update () {
        if (playerController == null)
            return;

        float horizMove = KeyInputHold(KeyCode.D, KeyCode.A);
        float vertMove = KeyInputHold(KeyCode.W, KeyCode.S);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (!playerIndexSet || !prevState.IsConnected) {
            for (int i = 0; i < 4; ++i) {
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                GamePadState testState = GamePad.GetState(testPlayerIndex);
                if (testState.IsConnected) {
                    Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
                    playerIndex = testPlayerIndex;
                    playerIndexSet = true;
                }
            }
        }

        prevState = state;
        state = GamePad.GetState(playerIndex);

        Vector2 leftStick = ApplyStickDeadzone(state.ThumbSticks.Left);
        Vector2 rightStick = ApplyStickDeadzone(state.ThumbSticks.Right);

        bool leftDPAD = state.DPad.Left == ButtonState.Pressed, rightDPAD = state.DPad.Right == ButtonState.Pressed;

        // If left stick movement is being picked up
        if (leftStick != Vector2.zero) {
            // Clicking the left stick toggles running
            playerController.Move(leftStick.x);
        } else if (leftDPAD || rightDPAD) {
            float dir = 0;
            if (leftDPAD) {
                dir -= 1;
            } 

            if (rightDPAD) {
                dir += 1;
            }

            playerController.Move(dir);
        }
        else {
            playerController.Move(horizMove);
        }

        bool jump = OnXInputButtonDown(prevState.Buttons.A, state.Buttons.A) || Input.GetKeyDown(KeyCode.Space);
        if (jump) {
            playerController.Jump();
        }

#else

        playerController.Move(horiz);
        if (Input.GetKeyDown(KeyCode.Space)){
            playerController.Jump();
        }

#endif


    }

    bool OnXInputButtonDown(ButtonState prevState, ButtonState state) {
        if (prevState == ButtonState.Released && state == ButtonState.Pressed) {
            return true;
        }

        return false;
    }

    float KeyInputHold(KeyCode posKey, KeyCode negKey) {
        float result = 0;

        if (Input.GetKey(posKey))
            result += 1;

        if (Input.GetKey(negKey))
            result -= 1;

        return result;
    }

    Vector2 ApplyStickDeadzone(GamePadThumbSticks.StickValue stick) {
        float stickX = stick.X;
        if (stickX > 0) {
            stickX = Mathf.Clamp(stickX, stickDeadzone, 1);
            stickX = ExtensionMethods.Map(stickX, stickDeadzone, 1, 0, 1);
        }
        else if (stickX < 0) {
            stickX = Mathf.Clamp(stickX, -1, -stickDeadzone);
            stickX = ExtensionMethods.Map(stickX, -1, -stickDeadzone, -1, 0);
        }


        float stickY = stick.Y;
        if (stickY > 0) {
            stickY = Mathf.Clamp(stickY, stickDeadzone, 1);
            stickY = ExtensionMethods.Map(stickY, stickDeadzone, 1, 0, 1);
        }
        else if (stickY < 0) {
            stickY = Mathf.Clamp(stickY, -1, -stickDeadzone);
            stickY = ExtensionMethods.Map(stickY, -1, -stickDeadzone, -1, 0);
        }

        return new Vector2(stickX, stickY);
    }
}