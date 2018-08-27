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
    [SerializeField] private float aimLenNormalHit, aimLenStrongHit;
    [SerializeField] private GameObject aimArrowPfb, aimSpherePfb;
    [SerializeField] private float aimSphereSize, aimSphereGap;
    [SerializeField] private Vector2 aimSphereMinMax;

    private Material bodyMat;

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
    private float[] aimSphereNormDists, aimSphereStrongDists;
    private bool isAiming = false;

    void Awake() {
        bodyMat = modelParent.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
    }

    // Use this for initialization
    void Start () {
        if (!facingRight)
            LookLeft();

        ResetColorPalette();
        SpawnAimIcons();
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

        //aimParent.SetActive(isAiming);
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

    public void EndDash() {
        bodyFlashTimer = 0.5f;
        bodyFlashMin = bodyFlashMinMax.x;
        bodyFlashMax = bodyFlashMinMax.y;
        bodyFlashSpeed = bodyFlashSpeedAir;

        outOfDashes = true;
    }

    public void ResetDashes (bool groundDash) {
        if (groundDash)
            return;

        bodyFlashTimer = 1.5f;
        bodyFlashMin = 0;
        bodyFlashMax = 1;
        bodyFlashSpeed = bodyFlashSpeedLand;

        landFlashCount = 0;

        outOfDashes = false;
    }
    #endregion

    public void ShowAim (Vector2 aimDir, bool isStrongHit) {
        float length;
        if (isStrongHit) {
            length = aimLenStrongHit;
        } else {
            length = aimLenNormalHit;
        }

        SetAimSpherePos(length, aimSphereSize, aimSphereGap, aimSphereMinMax.x, aimSphereMinMax.y);

        isAiming = true;
    }

    public void HideAim() {
        isAiming = false;
    }

    public bool IsFacingRight () {
        return facingRight;
    }

    void SpawnWallSmoke(bool wallOnRight) {
        currentWallSmokeOrigin = wallOnRight ? rightWallSmokeOrigin : leftWallSmokeOrigin;
        wallSmoke = Instantiate(wallSmokePfb, currentWallSmokeOrigin.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
    }

    void SetAimSpherePos (float length, float sphereSize, float gapSize, float minPos, float maxPos) {

    }

    void SpawnAimIcons () {
        aimParent = new GameObject("AimParent");
        aimParent.transform.parent = this.transform;
        aimParent.transform.localScale = Vector3.one;
        aimParent.transform.localPosition = Vector3.zero;

        // Normal hit
        float normSphereSize = aimSphereSize /** aimLenNormalHit*/, normSphereGap = aimSphereGap * aimLenNormalHit;
        Vector2 normSphereMinMax = aimSphereMinMax * aimLenNormalHit /*+ (aimDistFromPlayer * Vector2.up)*/;

        float normSphereUnit = normSphereSize + normSphereGap;
        float normSphereLen = normSphereMinMax.y - normSphereMinMax.x;
        Debug.Log(normSphereUnit);
        float numNormUnits = normSphereLen / normSphereUnit;
        

        int numNormSpheres = Mathf.FloorToInt(numNormUnits);
        float normGapLen = normSphereGap + ((normSphereLen - (numNormSpheres - 1) * normSphereUnit + normSphereSize) / (numNormSpheres - 1f));


        aimSphereNormDists = new float[numNormSpheres];
        if (numNormSpheres > 0) {
            aimSphereNormDists[0] = normSphereMinMax.x + aimDistFromPlayer;
            for (int i = 1; i < aimSphereNormDists.Length; i++) {
                aimSphereNormDists[i] = aimSphereNormDists[i - 1] + normGapLen + (normSphereSize / 2f);
            }
        }


        // Strong hit
        float strongSphereSize = aimSphereSize /** aimLenStrongHit*/, strongSphereGap = aimSphereGap * aimLenStrongHit;
        Vector2 strongSphereMinMax = aimSphereMinMax * aimLenStrongHit /*+ (aimDistFromPlayer * Vector2.up)*/;

        float strongSphereUnit = strongSphereSize + strongSphereGap;
        float strongSphereLen = strongSphereMinMax.y - strongSphereMinMax.x;
        float numStrongUnits = strongSphereLen / strongSphereUnit;

        int numStrongSpheres = Mathf.FloorToInt(numStrongUnits);
        float strongGapLen = strongSphereGap + ((strongSphereLen - (numStrongSpheres - 1) * strongSphereUnit + strongSphereSize) / (numStrongSpheres - 1f));

        aimSphereStrongDists = new float[numStrongSpheres];
        if (numStrongSpheres > 0) {
            aimSphereStrongDists[0] = strongSphereMinMax.x + aimDistFromPlayer;
            for (int i = 1; i < aimSphereStrongDists.Length; i++) {
                aimSphereStrongDists[i] = aimSphereStrongDists[i - 1] + strongGapLen + (strongSphereSize / 2f);
            }
        }

        GameObject newArrow = Instantiate(aimArrowPfb, Vector3.zero, Quaternion.identity, aimParent.transform);
        newArrow.transform.localPosition = (normSphereMinMax.y + (aimLenNormalHit - normSphereMinMax.y)) * aimDistFromPlayer * Vector3.up;
        newArrow.transform.localScale = new Vector3(aimArrowPfb.transform.localScale.x/transform.localScale.x, aimArrowPfb.transform.localScale.y/transform.localScale.y, 1f);
        for (int i = 0; i < aimSphereStrongDists.Length; i++) {
            GameObject newSphere = Instantiate(aimSpherePfb, Vector3.zero, Quaternion.identity, aimParent.transform);
            if (i > aimSphereNormDists.Length - 1) {
                newSphere.SetActive(false);
                continue;
            }

            newSphere.transform.localPosition = aimSphereNormDists[i] * Vector3.up;

            newSphere.transform.localScale = Vector3.one * aimSphereSize;
        }


        aimParent.SetActive(true);
    }

    #region Colour palette
    void SetBodyFlash (float timer, float min, float max) {
        float flashAmount = ExtensionMethods.Map(Mathf.Sin(bodyFlashTimer * Mathf.PI), -1, 1, bodyFlashMin, bodyFlashMax);
        SetBodyPaletteMix(flashAmount);
    }

    void SetBodyPaletteMix(float mixAmount) {
        bodyMat.SetFloat("_PaletteMix", mixAmount);
    }

    void ResetColorPalette() {
        SetBodyPaletteMix(0);
    }
    #endregion
}
