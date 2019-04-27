using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenIcon : MonoBehaviour {

    // TEST
    [SerializeField] private Vector2 t_screenPos;

    [SerializeField] private CameraController camController;
    [SerializeField] private EntityManager entityManager;

    [SerializeField] private GameObject pointerGfx;

    [SerializeField] private Vector2 diagonalThreshold;

    Vector3 ballPos;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!entityManager.GetBallController())
            return;

        /*Vector3*/ ballPos = camController.GetMainCam().WorldToScreenPoint(entityManager.GetBallController().transform.position);
        //ballPos = camController.GetMainCam().WorldToScreenPoint(ballPos);

        if (ballPos.x >= Screen.width*0.5f || ballPos.x <= 0 || ballPos.y >= Screen.height*0.5f || ballPos.y <= 0)
        {
            float zDiff = Mathf.Abs(camController.GetMainCam().transform.position.z);

            float x = Mathf.Clamp(ballPos.x, 1, (Screen.width * 0.5f) - 1);
            float y = Mathf.Clamp(ballPos.y, 1, (Screen.height * 0.5f) - 1);

            SetRot(ballPos);

            Vector3 worldPoint = camController.GetMainCam().ScreenToWorldPoint(new Vector3(x, y, zDiff));
            transform.position = worldPoint;

            pointerGfx.SetActive(true);
        } else
        {
            pointerGfx.SetActive(false);
        }
    }

    private void SetRot(Vector3 ballPos) {

        if (ballPos.x < diagonalThreshold.x)
        {
            if (ballPos.y < diagonalThreshold.y)
            {
                pointerGfx.transform.rotation = Quaternion.Euler(0, 0, 135);
            } else if (ballPos.y > (Screen.height*0.5f) - diagonalThreshold.y)
            {
                pointerGfx.transform.rotation = Quaternion.Euler(0, 0, 45);
            } else
            {
                pointerGfx.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        } else if (ballPos.x > (Screen.width*0.5f) - diagonalThreshold.x)
        {
            if (ballPos.y < diagonalThreshold.y)
            {
                pointerGfx.transform.rotation = Quaternion.Euler(0, 0, 225);
            }
            else if (ballPos.y > (Screen.height * 0.5f) - diagonalThreshold.y)
            {
                pointerGfx.transform.rotation = Quaternion.Euler(0, 0, 315);
            }
            else
            {
                pointerGfx.transform.rotation = Quaternion.Euler(0, 0, 270);
            }   
        } else
        {
            if (ballPos.y < diagonalThreshold.y)
            {
                pointerGfx.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else if (ballPos.y > (Screen.height * 0.5f) - diagonalThreshold.y)
            {
                pointerGfx.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}
