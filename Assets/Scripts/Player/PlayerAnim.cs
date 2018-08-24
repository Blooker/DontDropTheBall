using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour {

    [SerializeField] private Transform modelParent;
    [SerializeField] private bool facingRight = true;

    [Header("Wall Sliding")]
    [SerializeField] private GameObject wallSmokePfb;
    [SerializeField] private Transform leftWallSmokeOrigin, rightWallSmokeOrigin;

    private ParticleSystem wallSmoke;
    private GameObject lastWall;
    private Transform currentSmokeOrigin;

	// Use this for initialization
	void Start () {
        if (!facingRight)
            LookLeft();
	}

    void Update() {
        if (wallSmoke != null)
            wallSmoke.transform.position = currentSmokeOrigin.transform.position;
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

    public void StartWallSlide (bool wallOnRight, GameObject wall) {
        if (!ReferenceEquals(wall, lastWall)) {
            GenerateWallSmoke(wallOnRight);
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

    void GenerateWallSmoke(bool wallOnRight) {
        currentSmokeOrigin = wallOnRight ? rightWallSmokeOrigin : leftWallSmokeOrigin;
        wallSmoke = Instantiate(wallSmokePfb, currentSmokeOrigin.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
    }
}
