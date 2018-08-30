using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettings : MonoBehaviour {

    [Range(0, 1)]
    [SerializeField] private float internalResScale = 0.5f;

    [SerializeField] private RenderTexture pixelRenderTexture;
    [SerializeField] private RawImage canvasImage;

    [SerializeField] private int frameRate;

    private Vector2 windowRes, internalRes;
    private EntityManager playerManager;

    private void Awake() {
        playerManager = GetComponent<EntityManager>();
        SetResolution(Screen.width, Screen.height);
    }

    // Use this for initialization
    void Start () {
        QualitySettings.vSyncCount = 0;
	}
	
	// Update is called once per frame
	void Update () {
        Application.targetFrameRate = frameRate;
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

        Camera[] allCams = playerManager.GetAllPlayerCams();

        for (int i = 0; i < allCams.Length; i++) {
            if (allCams[i].targetTexture != null) {
                allCams[i].targetTexture.Release();
            }
        }

        RenderTexture newTexture = new RenderTexture(Mathf.FloorToInt(width * internalResScale), Mathf.FloorToInt(height * internalResScale), 24, RenderTextureFormat.RGB111110Float);
        newTexture.useMipMap = false;
        newTexture.filterMode = FilterMode.Point;

        for (int i = 0; i < allCams.Length; i++) {
            allCams[i].targetTexture = newTexture;
        }

        canvasImage.texture = newTexture;
    }
}
