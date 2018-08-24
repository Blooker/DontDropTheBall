using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour {

    [SerializeField] private Transform modelParent;
    [SerializeField] private bool facingRight = true;

	// Use this for initialization
	void Start () {
        if (!facingRight)
            LookLeft();
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

    public bool IsFacingRight () {
        return facingRight;
    }
}
