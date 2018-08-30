using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour {

    [SerializeField] private GameObject playerPfb, ballPfb;
    [SerializeField] private CameraController playerCam;
    [SerializeField] private Vector2 playerStartPos, ballStartPos;

    private PlayerController player;
    private BallController ball;

    private PlayerInput playerInput;

    void Awake() {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start() {
        RespawnPlayer();
        RespawnBall();
    }

    // Update is called once per frame
    void Update() {
        // Player
        if (player == null)
            return;

        if (player.transform.position.y < -15.4) {
            RespawnPlayer();
        }

        //Ball
        if (ball == null)
            return;

        if (ball.transform.position.y < -15.4) {
            RespawnPlayer();
        }
    }

    public Camera GetMainPlayerCam () {
        return playerCam.GetMainCam();
    }

    public Camera[] GetAllPlayerCams() {
        return playerCam.GetAllCams();
    }

    // Kills the player if they exist, then instantiates a new one
    public void RespawnPlayer() {
        if (player != null)
            DestroyPlayer ();

        player = Instantiate(playerPfb, playerStartPos, Quaternion.identity).GetComponent<PlayerController>();
        playerInput.SetPlayer(player);
    }

    // Destroys player and safely removes all references to it
    public void DestroyPlayer () {
        playerInput.SetPlayer(null);
        Destroy(player.gameObject);
    }

    // Kills the player if they exist, then instantiates a new one
    public void RespawnBall() {
        if (ball != null)
            DestroyBall();

        ball = Instantiate(ballPfb, ballStartPos, Quaternion.identity).GetComponent<BallController>();
    }

    // Destroys player and safely removes all references to it
    public void DestroyBall () {
        Destroy(ball.gameObject);
    }

}
