using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRegion : MonoBehaviour {

    [SerializeField] private Vector2 cameraMaxBounds;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, transform.localScale);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, cameraMaxBounds);
    }
}
