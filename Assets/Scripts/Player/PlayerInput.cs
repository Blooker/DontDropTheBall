using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class PlayerInput : MonoBehaviour {
    [Header("Sticks")]
    [Range(0, 1)]
    [SerializeField] private float leftStickDeadzone, rightStickDeadzone;

    private PlayerController playerController;
    private MouseSettings mouseSettings;
    private bool playerIndexSet = false;

    private bool mouseAiming, stickAiming;

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
        mouseSettings = GetComponent<MouseSettings>();
    }

    private void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        mouseSettings.SetCursorPos(Input.mousePosition, 0);
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

        //Vector2 leftStick = ApplyStickDeadzone(state.ThumbSticks.Left, leftStickDeadzone);
        Vector2 leftStick = new Vector2(Input.GetAxisRaw("LeftStickX"), Input.GetAxisRaw("LeftStickY"));
        Vector2 rightStick = new Vector2(Input.GetAxisRaw("RightStickX"), -Input.GetAxisRaw("RightStickY"));
        Vector2 playerToMouse = mouseSettings.GetCursorWorldPoint() - playerController.transform.position;

        bool leftDPAD = state.DPad.Left == ButtonState.Pressed, rightDPAD = state.DPad.Right == ButtonState.Pressed;
        bool upDPAD = state.DPad.Up == ButtonState.Pressed, downDPAD = state.DPad.Down == ButtonState.Pressed;

        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
            mouseSettings.SetCursorPos(Input.mousePosition, playerController.transform.position.z);
            mouseSettings.SetVisible(true);
        }

        if (Input.GetMouseButtonDown(1)) {
            playerController.Dash(playerToMouse.x, playerToMouse.y);
            mouseSettings.SetVisible(true);
        }

        // If left stick movement is being picked up
        if (leftStick.magnitude >= 0.35f) {
            playerController.Move(leftStick.x);

            if (OnXInputButtonDown(prevState.Buttons.LeftShoulder, state.Buttons.LeftShoulder)) {
                playerController.Dash(leftStick.x, leftStick.y);
            }

            mouseSettings.SetVisible(false);
        } else if (leftDPAD || rightDPAD || upDPAD || downDPAD) {
            float xDir = 0;
            if (leftDPAD) {
                xDir -= 1;
            } 
            if (rightDPAD) {
                xDir += 1;
            }

            float yDir = 0;
            if(upDPAD) {
                yDir += 1;
            }
            if(downDPAD) {
                yDir -= 1;
            }

            playerController.Move(xDir);

            if (OnXInputButtonDown(prevState.Buttons.LeftShoulder, state.Buttons.LeftShoulder)) {
                playerController.Dash(xDir, yDir);
            }

            mouseSettings.SetVisible(false);
        }
        else if (horizMove != 0) {
            playerController.Move(horizMove);

            mouseSettings.SetVisible(true);
        } else {
            playerController.Move(0);
        }

        if (Input.GetMouseButton(0)) {
            playerController.Aim(playerToMouse.x, playerToMouse.y);
            mouseAiming = true;

            mouseSettings.SetVisible(true);
        } else if (rightStick.magnitude >= 0.5f) {
            playerController.Aim(rightStick.x, rightStick.y);
            stickAiming = true;

            mouseSettings.SetVisible(false);
        }

        if (Input.GetMouseButtonUp(0) && mouseAiming) {
            playerController.Attack();
            mouseAiming = false;
        } else if (rightStick.magnitude < 0.5f && stickAiming) {
            playerController.Attack();
            stickAiming = false;
        }


        bool jumpKey = Input.GetKeyDown(KeyCode.Space);
        if (jumpKey) {
            mouseSettings.SetVisible(true);
        }

        bool jumpPad = OnXInputButtonDown(prevState.Buttons.A, state.Buttons.A);
        if (jumpPad) {
            mouseSettings.SetVisible(false);
        }

        if (jumpKey || jumpPad) {
            playerController.StartJump();
        }


        bool jumpCancelKey = Input.GetKeyDown(KeyCode.LeftShift);
        if (jumpCancelKey) {
            mouseSettings.SetVisible(true);
        }

        bool jumpCancelPad = OnXInputButtonDown(prevState.Buttons.RightShoulder, state.Buttons.RightShoulder);
        if (jumpCancelPad) {
            mouseSettings.SetVisible(false);
        }

        if (jumpCancelKey || jumpCancelPad) {
            playerController.EndJump();
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

    bool OnXInputButtonUp(ButtonState prevState, ButtonState state) {
        if (prevState == ButtonState.Pressed && state == ButtonState.Released) {
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

    Vector2 ApplyStickDeadzone (GamePadThumbSticks.StickValue stick, float stickDeadzone) {
        return ApplyStickDeadzone(new Vector2(stick.X, stick.Y), stickDeadzone);
    }

    Vector2 ApplyStickDeadzone(Vector2 stick, float stickDeadzone) {
        float stickX = stick.x;
        if (stickX > 0) {
            stickX = Mathf.Clamp(stickX, stickDeadzone, 1);
            stickX = ExtensionMethods.Map(stickX, stickDeadzone, 1, 0, 1);
        }
        else if (stickX < 0) {
            stickX = Mathf.Clamp(stickX, -1, -stickDeadzone);
            stickX = ExtensionMethods.Map(stickX, -1, -stickDeadzone, -1, 0);
        }


        float stickY = stick.y;
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