using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour {

    [SerializeField] private Transform modelParent;
    [SerializeField] private bool facingRight = true;

    [Header("Jumping")]
    [SerializeField] private GameObject jumpSmokePfb;
    [SerializeField] private Transform jumpSmokeOrigin;
    [SerializeField] private float jumpSmokeLength;

    [Header("Wall Sliding")]
    [SerializeField] private GameObject wallSmokePfb;
    [SerializeField] private Transform leftWallSmokeOrigin, rightWallSmokeOrigin;

    [Header("Dash")]
    [SerializeField] private float bodyFlashSpeedAir;
    [SerializeField] private float bodyFlashSpeedLand;
    [SerializeField] private Vector2 bodyFlashMinMax;
    [SerializeField] private int maxLandBodyFlashes;

    [Header("Aim")]
    [SerializeField] private float aimDistFromPlayer;
    [SerializeField] private float aimLength, aimSphereSize;
    [SerializeField] private GameObject aimArrowPfb, aimSpherePfb;
    [SerializeField] private int numSpheres;
    [SerializeField] private Vector2 aimSphereMinMax;

    private Material bodyMatReg, bodyMatStencil;

    private ParticleSystem jumpSmoke;
    private float jumpSmokeTimer = 0;

    private ParticleSystem wallSmoke;
    private GameObject lastWall;
    private Transform currentWallSmokeOrigin;

    private bool outOfDashes = false;
    private float bodyFlashTimer = -1;
    private float bodyFlashSpeed, bodyFlashMin, bodyFlashMax;
    private int landFlashCount = 0;
    private bool flashCountIncreased = false;

    private GameObject aimParent;
    private float[] aimSphereDists;
    private bool isAiming = false;

    void Awake() {
        bodyMatReg = modelParent.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
        bodyMatStencil = modelParent.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;

        SpawnAimIcons();
    }

    // Use this for initialization
    void Start () {
        if (!facingRight)
            LookLeft();

        ResetColorPalette();
    }

    void Update() {
        if (wallSmoke != null)
            wallSmoke.transform.position = currentWallSmokeOrigin.transform.position;

        if (jumpSmokeTimer > 0)
            jumpSmokeTimer -= Time.deltaTime;

        if (jumpSmoke != null) {
            jumpSmoke.transform.position = jumpSmokeOrigin.transform.position;

            if (jumpSmokeTimer <= 0) {
                jumpSmoke.Stop();
                Destroy(jumpSmoke.gameObject, 1f);

                jumpSmoke = null;
            }
        }

        if (bodyFlashTimer > -1) {
            bodyFlashTimer += Time.deltaTime * bodyFlashSpeed;
            if (bodyFlashTimer > 2)
                bodyFlashTimer = 0;

            SetBodyFlash(bodyFlashTimer, bodyFlashMin, bodyFlashMax);

            if (!outOfDashes) {
                if (bodyFlashTimer >= 0.75f && !flashCountIncreased) {
                    landFlashCount += 1;
                    flashCountIncreased = true;
                }

                if (bodyFlashTimer < 0.75f && flashCountIncreased) {
                    flashCountIncreased = false;
                }

                if (landFlashCount >= maxLandBodyFlashes) {
                    bodyFlashTimer = -1;
                    SetBodyPaletteMix(0);
                }
            }
        }

        aimParent.SetActive(isAiming);
        isAiming = false;
    }

    public void LookLeft () {
        facingRight = false;
        Vector3 localModelScale = modelParent.localScale;
        localModelScale.x = -1;
        modelParent.localScale = localModelScale;
    }

    public void LookRight () {
        facingRight = true;
        Vector3 localModelScale = modelParent.localScale;
        localModelScale.x = 1;
        modelParent.localScale = localModelScale;
    }

    public void SpawnJumpSmoke () {
        if (jumpSmoke != null) {
            jumpSmoke.Stop();
            Destroy(jumpSmoke.gameObject, 1f);

            jumpSmoke = null;
        }

        jumpSmoke = Instantiate(jumpSmokePfb, jumpSmokeOrigin.position, Quaternion.identity).GetComponent<ParticleSystem>();
        jumpSmokeTimer = jumpSmokeLength;
    }

    #region Wall Slide
    public void StartWallSlide (bool wallOnRight, GameObject wall) {
        if (!ReferenceEquals(wall, lastWall)) {
            SpawnWallSmoke(wallOnRight);
            lastWall = wall;
        }
    }

    public void EndWallSlide () {
        wallSmoke.Stop();
        Destroy(wallSmoke.gameObject, 1f);

        wallSmoke = null;
        lastWall = null;
    }
    #endregion

    #region Dash
    public void StartDash() {
        //bodyFlashTimer = -1;
        //SetBodyPaletteMix(1);
        bodyFlashTimer = 0;
        landFlashCount = 0;
    }

    public void EndDash(bool dashesLeft) {
        if (dashesLeft)
            return;

        bodyFlashTimer = 0.5f;
        bodyFlashMin = bodyFlashMinMax.x;
        bodyFlashMax = bodyFlashMinMax.y;
        bodyFlashSpeed = bodyFlashSpeedAir;

        outOfDashes = true;
    }

    public void ResetDashes (bool playAnim) {
        if (!playAnim)
            return;

        bodyFlashTimer = 1.5f;
        bodyFlashMin = 0;
        bodyFlashMax = 1;
        bodyFlashSpeed = bodyFlashSpeedLand;

        landFlashCount = 0;

        outOfDashes = false;
    }
    #endregion

    public void Aim (float horiz, float vert) {
        aimParent.transform.rotation = Quaternion.Euler(0,0,Mathf.Atan2(-horiz, vert)*Mathf.Rad2Deg);
        isAiming = true;
    }

    public bool IsFacingRight () {
        return facingRight;
    }

    void SpawnWallSmoke(bool wallOnRight) {
        currentWallSmokeOrigin = wallOnRight ? rightWallSmokeOrigin : leftWallSmokeOrigin;
        wallSmoke = Instantiate(wallSmokePfb, currentWallSmokeOrigin.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
    }

    void SpawnAimIcons () {
        aimParent = new GameObject("AimParent");
        aimParent.transform.parent = this.transform;
        aimParent.transform.localScale = Vector3.one;
        aimParent.transform.localPosition = Vector3.zero;

        aimSphereDists = new float[numSpheres];

        GameObject newArrow = Instantiate(aimArrowPfb, Vector3.zero, Quaternion.identity, aimParent.transform);
        newArrow.transform.localPosition = (aimDistFromPlayer + aimLength) * Vector3.up;
        newArrow.transform.localScale = Vector3.one;

        float worldMinPos = ExtensionMethods.Map(aimSphereMinMax.x, 0, 1, aimDistFromPlayer, aimDistFromPlayer + aimLength);
        float worldMaxPos = ExtensionMethods.Map(aimSphereMinMax.y, 0, 1, aimDistFromPlayer, aimDistFromPlayer + aimLength);

        float lerpValue = 0;
        for (int i = 0; i < numSpheres; i++) {
            GameObject newSphere = Instantiate(aimSpherePfb, aimParent.transform);
            newSphere.transform.localScale = Vector3.one * aimSphereSize;

            aimSphereDists[i] = Mathf.Lerp(worldMinPos, worldMaxPos, lerpValue);
            newSphere.transform.localPosition = aimSphereDists[i] * Vector3.up;
                
            lerpValue += 1f / (numSpheres - 1);
        }

        aimParent.SetActive(false);
    }

    #region Colour palette
    void SetBodyFlash (float timer, float min, float max) {
        float flashAmount = ExtensionMethods.Map(Mathf.Sin(bodyFlashTimer * Mathf.PI), -1, 1, bodyFlashMin, bodyFlashMax);
        SetBodyPaletteMix(flashAmount);
    }

    void SetBodyPaletteMix(float mixAmount) {
        bodyMatReg.SetFloat("_PaletteMix", mixAmount);
        bodyMatStencil.SetFloat("_PaletteMix", mixAmount);
    }

    void ResetColorPalette() {
        SetBodyPaletteMix(0);
    }
    #endregion
}
