﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenIcon : MonoBehaviour {

    // TEST
    [SerializeField] private Vector2 t_screenPos;

    [SerializeField] private CameraController camController;
    [SerializeField] private EntityManager entityManager;

    [SerializeField] private float minScale = 0.5f, scaleMaxDist;

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

            SetRotAndSize(ballPos);

            Vector3 worldPoint = camController.GetMainCam().ScreenToWorldPoint(new Vector3(x, y, zDiff));
            transform.position = worldPoint;

            pointerGfx.SetActive(true);
        } else
        {
            pointerGfx.SetActive(false);
        }
    }

    private void SetRotAndSize(Vector3 ballPos) {

        float rot = 0, size = 1;

        if (ballPos.x < diagonalThreshold.x)
        {
            if (ballPos.y < diagonalThreshold.y)
            {
                rot = 135;
                size = Vector3.Distance(ballPos, Vector3.zero);
            } else if (ballPos.y > (Screen.height*0.5f) - diagonalThreshold.y)
            {
                rot = 45;
                size = Vector3.Distance(ballPos, new Vector3(0, 1));
            } else
            {
                rot = 90;
                size = -ballPos.x;
            }
        } else if (ballPos.x > (Screen.width*0.5f) - diagonalThreshold.x)
        {
            if (ballPos.y < diagonalThreshold.y)
            {
                rot = 225;
                size = Vector3.Distance(ballPos, new Vector3(1, 0));
            }
            else if (ballPos.y > (Screen.height * 0.5f) - diagonalThreshold.y)
            {
                rot = 315;
                size = Vector3.Distance(ballPos, Vector3.one);
            }
            else
            {
                rot = 270;
                size = ballPos.x - (Screen.width * 0.5f);
            }   
        } else
        {
            if (ballPos.y < diagonalThreshold.y)
            {
                rot = 180;
                size = -ballPos.y;
            }
            else if (ballPos.y > (Screen.height * 0.5f) - diagonalThreshold.y)
            {
                rot = 0;
                size = ballPos.y - (Screen.height * 0.5f);
            }
        }

        pointerGfx.transform.rotation = Quaternion.Euler(0, 0, rot);
        Debug.Log(size);
        pointerGfx.transform.localScale = Vector3.one * ExtensionMethods.Map(Mathf.Clamp(size, 0, scaleMaxDist), 0, scaleMaxDist, 1, minScale);
    }
}
