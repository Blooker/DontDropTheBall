using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseSettings : MonoBehaviour {

    [SerializeField] private Sprite mouseSprite;
    [SerializeField] private Image mouseImage;
    [SerializeField] private Color mouseColor;

    private Vector2 cursorPos;
    private VideoSettings videoSettings;

    private void Awake() {
        videoSettings = GetComponent<VideoSettings>();
    }

    // Use this for initialization
    void Start () {
        SetCursorSprite();
        SetCursorColor();
    }
	
	// Update is called once per frame
	void Update () {
        mouseImage.rectTransform.position = cursorPos;
    }

    public Vector3 GetCursorWorldPoint (float playerPosZ) {
        float resScale = videoSettings.GetInternalResScale();
        Camera cam = videoSettings.GetPixelRenderCam();
        Vector3 result = cam.ScreenToWorldPoint(new Vector3(cursorPos.x * resScale, cursorPos.y * resScale, playerPosZ-cam.transform.position.z));
        result.z = playerPosZ;

        return result;
    }

    public void SetCursorPos (Vector2 mousePos) {
        cursorPos = mousePos;
    }

    void SetCursorSprite () {
        mouseImage.sprite = mouseSprite;
    }

    void SetCursorColor () {
        mouseImage.color = mouseColor;
    }
}
