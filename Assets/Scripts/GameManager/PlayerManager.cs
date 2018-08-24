using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector2 startPos;

    private PlayerController player;

    private PlayerInput playerInput;

    void Awake() {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start() {
        RespawnPlayer();
    }

    // Update is called once per frame
    void Update() {
        if (player == null)
            return;

        if (player.transform.position.y < -15.4) {
            RespawnPlayer();
        }
    }

    // Kills the player if they exist, then instantiates a new one
    public void RespawnPlayer() {
        if (player != null)
            DestroyPlayer ();

        player = Instantiate(playerPrefab, startPos, Quaternion.identity).GetComponent<PlayerController>();
        playerInput.SetPlayer(player);
    }

    // Destroys player and safely removes all references to it
    public void DestroyPlayer () {
        playerInput.SetPlayer(null);
        Destroy(player);
    }

}
