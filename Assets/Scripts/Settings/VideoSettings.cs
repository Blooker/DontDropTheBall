using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettings : MonoBehaviour {

    [Range(0, 1)]
    [SerializeField] private float internalResScale = 0.5f;

    [SerializeField] private RenderTexture pixelRenderTexture;
    [SerializeField] private Camera pixelRenderCam;
    [SerializeField] private Camera[] otherCams;
    [SerializeField] private RawImage canvasImage;

    [SerializeField] private int frameRate;

    private Vector2 windowRes, internalRes;

	// Use this for initialization
	void Start () {
        QualitySettings.vSyncCount = 0;
       
        //Application.targetFrameRate = 60;

        SetResolution(Screen.width, Screen.height);
	}
	
	// Update is called once per frame
	void Update () {
        Application.targetFrameRate = frameRate;
    }

    public Camera GetPixelRenderCam () {
        return pixelRenderCam;
    }

    public Vector2 GetWindowRes() {
        return windowRes;
    }

    public float GetInternalResScale () {
        return internalResScale;
    }

    void SetResolution (int width, int height) {
        windowRes = new Vector2(width, height);
        Screen.SetResolution(width, height, Screen.fullScreen);

        if (pixelRenderCam.targetTexture != null) {
            pixelRenderCam.targetTexture.Release();
        }

        RenderTexture newTexture = new RenderTexture(Mathf.FloorToInt(width * internalResScale), Mathf.FloorToInt(height * internalResScale), 24, RenderTextureFormat.RGB111110Float);
        newTexture.useMipMap = false;
        newTexture.filterMode = FilterMode.Point;

        pixelRenderCam.targetTexture = newTexture;
        for (int i = 0; i < otherCams.Length; i++) {
            otherCams[i].targetTexture = newTexture;
        }

        canvasImage.texture = newTexture;
    }
}
