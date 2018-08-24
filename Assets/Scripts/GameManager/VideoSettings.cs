using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettings : MonoBehaviour {

    [SerializeField] private RenderTexture pixelRenderTexture;
    [SerializeField] private Camera pixelRenderCam;
    [SerializeField] private RawImage canvasImage;

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        
        SetResolution(Screen.width, Screen.height);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void SetResolution (int width, int height) {
        if (pixelRenderCam.targetTexture != null) {
            pixelRenderCam.targetTexture.Release();
        }

        RenderTexture newTexture = new RenderTexture(width / 2, height / 2, 24, RenderTextureFormat.RGB111110Float);
        newTexture.useMipMap = false;
        newTexture.filterMode = FilterMode.Point;

        pixelRenderCam.targetTexture = newTexture;
        canvasImage.texture = newTexture;

        Screen.SetResolution(width, height, Screen.fullScreen);
    }
}
