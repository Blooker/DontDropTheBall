using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] private int mainCamInd;

    private Camera[] cameras;

    private void Awake() {
        UpdateCams();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Camera GetMainCam () {
        return cameras[mainCamInd];
    }

    public Camera[] GetAllCams () {
        return cameras;
    }

    private void UpdateCams() {
        cameras = transform.GetComponentsInChildren<Camera>();
    }
}
