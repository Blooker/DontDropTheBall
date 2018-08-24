using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour {

    [SerializeField] private Transform modelParent;
    [SerializeField] private bool facingRight = true;

    [Header("Jumping")]
    [SerializeField] private GameObject jumpSmokePfb;
    [SerializeField] private Transform jumpSmokeOrigin;
    [SerializeField] private float jumpSmokeLength;

    [Header("Wall Sliding")]
    [SerializeField] private GameObject wallSmokePfb;
    [SerializeField] private Transform leftWallSmokeOrigin, rightWallSmokeOrigin;

    private ParticleSystem jumpSmoke;
    private float jumpSmokeTimer = 0;

    private ParticleSystem wallSmoke;
    private GameObject lastWall;
    private Transform currentWallSmokeOrigin;

	// Use this for initialization
	void Start () {
        if (!facingRight)
            LookLeft();
	}

    void Update() {
        if (wallSmoke != null)
            wallSmoke.transform.position = currentWallSmokeOrigin.transform.position;

        if (jumpSmokeTimer > 0)
            jumpSmokeTimer -= Time.deltaTime;

        if (jumpSmoke != null) {
            jumpSmoke.transform.position = jumpSmokeOrigin.transform.position;

            if (jumpSmokeTimer <= 0) {
                jumpSmoke.Stop();
                Destroy(jumpSmoke.gameObject, 1f);
            }
        }
    }

    public void LookLeft () {
        facingRight = false;
        Vector3 localModelScale = modelParent.localScale;
        localModelScale.x = -1;
        modelParent.localScale = localModelScale;
    }

    public void LookRight () {
        facingRight = true;
        Vector3 localModelScale = modelParent.localScale;
        localModelScale.x = 1;
        modelParent.localScale = localModelScale;
    }

    public void SpawnJumpSmoke () {
        jumpSmoke = Instantiate(jumpSmokePfb, jumpSmokeOrigin.position, Quaternion.identity).GetComponent<ParticleSystem>();
        jumpSmokeTimer = jumpSmokeLength;
    }

    public void StartWallSlide (bool wallOnRight, GameObject wall) {
        if (!ReferenceEquals(wall, lastWall)) {
            SpawnWallSmoke(wallOnRight);
            lastWall = wall;
        }
    }

    public void EndWallSlide () {
        wallSmoke.Stop();
        Destroy(wallSmoke.gameObject, 1f);

        wallSmoke = null;
        lastWall = null;
    }

    public bool IsFacingRight () {
        return facingRight;
    }

    void SpawnWallSmoke(bool wallOnRight) {
        currentWallSmokeOrigin = wallOnRight ? rightWallSmokeOrigin : leftWallSmokeOrigin;
        wallSmoke = Instantiate(wallSmokePfb, currentWallSmokeOrigin.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
    }
}
