using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseSettings : MonoBehaviour {

    [SerializeField] private GameObject mouseSpherePfb;
    [SerializeField] private float sphereResScalar;
    [SerializeField] private Color mouseColor;

    private GameObject mouseSphere;
    private Vector2 cursorScreenPos;
    private Vector3 cursorWorldPos;
    private VideoSettings videoSettings;

    private void Awake() {
        videoSettings = GetComponent<VideoSettings>();
    }

    // Use this for initialization
    void Start () {
        SpawnCursor();
    }
	
	// Update is called once per frame
	void Update () {
        mouseSphere.transform.position = cursorWorldPos;
    }

    public Vector3 GetCursorWorldPoint () {
        return cursorWorldPos;
    }

    public void SetCursorPos (Vector2 mousePos, float playerPosZ) {
        cursorScreenPos = mousePos;
        CalcCursorWorldPoint();
    }

    void CalcCursorWorldPoint () {
        float resScale = videoSettings.GetInternalResScale();
        Camera cam = videoSettings.GetPixelRenderCam();
        cursorWorldPos = cam.ScreenToWorldPoint(new Vector3(cursorScreenPos.x * resScale, cursorScreenPos.y * resScale, -cam.transform.position.z));
        cursorWorldPos.z = 0;
    }

    void SpawnCursor() {
        mouseSphere = Instantiate(mouseSpherePfb);
        SetCursorSize();
    }

    void SetCursorSize () {
        mouseSphere.transform.localScale = Vector3.one * (1/videoSettings.GetWindowRes().y) * sphereResScalar;
    }

    void SetCursorColor () {
        //mouseImage.color = mouseColor;
    }
}
