using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseSettings : MonoBehaviour {

    [SerializeField] private GameObject mouseSpherePfb, mouseCollider;
    [SerializeField] private float sphereResScalar;
    [SerializeField] private Color mouseColor;
    [SerializeField] private LayerMask mouseColLayer;

    private GameObject mouseSphere;
    private Vector2 cursorScreenPos;
    private Vector3 cursorWorldPos, mouseColOffset;
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
        if (mouseSphere == null)
            return;

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

        if (mouseColOffset == Vector3.zero) {
            mouseColOffset = mouseCollider.transform.position - videoSettings.GetPixelRenderCam().transform.position;
        }

        mouseCollider.transform.position = cam.transform.position + mouseColOffset;

        Ray ray = cam.ScreenPointToRay(new Vector3(cursorScreenPos.x * resScale, cursorScreenPos.y * resScale));
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mouseColLayer)) {
            cursorWorldPos = hit.point;
        }
    }

    void SpawnCursor() {
        mouseSphere = Instantiate(mouseSpherePfb);
        SetCursorSize();
    }

    public void SetVisible (bool _visible) {
        if (mouseSphere == null)
            return;

        mouseSphere.SetActive(_visible);
    }

    void SetCursorSize () {
        if (mouseSphere == null)
            return;

        mouseSphere.transform.localScale = Vector3.one * (1/videoSettings.GetWindowRes().y) * sphereResScalar;
    }

    void SetCursorColor () {
        if (mouseSphere == null)
            return;

        //mouseImage.color = mouseColor;
    }
}
