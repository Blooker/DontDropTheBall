using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnim))]
public class PlayerController : MonoBehaviour {

    //DEBUG
    public bool pauseOnJump = false;
    // DEBUG

    [SerializeField] private float speed, acceleration, jumpForce;
    [SerializeField] private float maxJumps = 2;

    [Header("Vert Checks (Ground/Ceiling)")]
    [SerializeField] private Vector2 vertCheckScale;
    [SerializeField] private float vertCheckMaxYScale, vertCheckMaxVel;

    [Header("Ground Check")]
    [SerializeField] private Transform grCheckStart;
    [SerializeField] private float grCheckMaxVel;
    [SerializeField] private float grCheckMoveOffsetX;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Ceiling Check")]
    [SerializeField] private Transform ceilCheckStart;
    [SerializeField] private float ceilCheckMaxVel;
    [SerializeField] private LayerMask whatIsCeiling;

    [Header("Wall Check")]
    [SerializeField] private float wallCheckDist;
    [SerializeField] private float wallCheckOffsetY;
    [SerializeField] private LayerMask whatIsWall;

    [Header("Wall Jump/Slide")]
    [SerializeField] private float wFallSpeedDecrease = 10f;
    [SerializeField] private float wJumpUpForce;
    [SerializeField] private float wJumpMoveSleepAmount = 0.2f;

    [Header("Dash")]
    [SerializeField] private float moveDashSpeed = 0.1f;
    [SerializeField] private float moveDashDist = 10f;
    [SerializeField] private int maxDashes = 1;
    [SerializeField] private LayerMask whatStopsDash;

    [Header("Attack")]
    [SerializeField] private float atkForce;
    [SerializeField] private float minAtkDashSpeed = 5;    
    [SerializeField] private float maxAtkDashDist = 5f;    

    private float moveInputX;

    private float numJumps;
    private ContactFilter2D groundFilter, ceilFilter;
    private Vector3 lastLandPos, lastCeilHitPos;

    private float regGravityScale;

    private float grCheckOffsetX = 0;

    private float wJumpMoveSleepTimer = 0;

    private enum DashType {Move, Attack};
    private DashType lastDashType;

    private int numMoveDashes = 0;
    private Vector3 dshStart, dshEnd, dshColNormal;
    private float dshLerpValue = 1, dshLerpLimit = 1f, dshCurrentSpeed;
    private ContactFilter2D dshFilter;

    private Vector2 lastAimDir;

    private Vector2 atkDir;
    private int numAirAttacks;
    private float atkDashDist, atkDashSpeed;
    private bool canAttack = true;

    private bool isGrounded = false, isLanded = false, isOnCeiling = false, isHitCeiling = false;
    private bool isJumping = false;
    private bool isWallSliding = false, wallOnRightSide = false;
    private bool isDashing = false;

    private BallController ball;

    private Collider2D playerColl, ballColl;
    private PlayerAnim playerAnim;
    private Rigidbody2D rb;

    // Use this for initialization
    void Awake() {
        playerColl = GetComponent<Collider2D>();

        playerAnim = GetComponent<PlayerAnim>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        ResetExtraJumps();
        ResetDashes(true);
        ResetAirAttacks();

        regGravityScale = rb.gravityScale;

        groundFilter = new ContactFilter2D();
        ceilFilter = new ContactFilter2D();
        dshFilter = new ContactFilter2D();

        groundFilter.SetLayerMask(whatIsGround);
        ceilFilter.SetLayerMask(whatIsCeiling);
        dshFilter.SetLayerMask(whatStopsDash);
    }

    void FixedUpdate() {
        if (!isDashing) {
            UpdateVertChecks();
        }
    }

    // Update is called once per frame
    void Update() {
        if (isLanded)
            Land();

        if (isJumping && rb.velocity.y < 0) {
            EndJump();
        }

        if (isHitCeiling)
            HitCeiling();

        if (isWallSliding && rb.velocity.y <= 0) {
            rb.AddForce(Vector2.up * wFallSpeedDecrease * Time.deltaTime);
        }

        if (wJumpMoveSleepTimer > 0) {
            wJumpMoveSleepTimer -= Time.deltaTime;
        }

        if (dshLerpValue < dshLerpLimit) {
            transform.position = Vector3.Lerp(dshStart, dshEnd, dshLerpValue);
            if (lastDashType == DashType.Attack && IsTouchingBall()) {
                Debug.Log("Ball touched");
                dshLerpLimit *= 0.9f;

                HitBall();

                canAttack = false;
            }

            dshLerpValue += dshCurrentSpeed * Time.deltaTime;
        } else if (isDashing) {
            EndDash(dshEnd);
        }
    }

    public void Move(float horiz) {
        if (isDashing)
            return;

        float moveDelta = 0;

        float _acceleration = acceleration * Time.deltaTime;

        if (wJumpMoveSleepTimer > 0) {
            horiz = 0;

            if (wallOnRightSide) {
                moveInputX = -1;
            } else {
                moveInputX = 1;
            }
        } else {
            if (!isGrounded) {
                moveDelta = _acceleration / 2;
            }
            else if (moveInputX > 0 && horiz <= 0 || moveInputX <= 0 && horiz > 0) {
                moveDelta = _acceleration * 2;
            }
            else {
                moveDelta = _acceleration;
            }

            GameObject wallTouchedRight = CheckWallRight();
            GameObject wallTouchedLeft = CheckWallLeft();

            if (wallTouchedRight != null && horiz > 0) {
                moveInputX = 0;
                if (!isGrounded) {
                    StartWallSlide(true, wallTouchedRight);
                }
                else if (isWallSliding) {
                    EndWallSlide();
                }
            }
            else if (wallTouchedLeft != null && horiz < 0) {
                moveInputX = 0;
                if (!isGrounded) {
                    StartWallSlide(false, wallTouchedLeft);
                }
                else if (isWallSliding) {
                    EndWallSlide();
                }
            }
            else {
                moveInputX = Mathf.MoveTowards(moveInputX, horiz, moveDelta);
                if (isWallSliding)
                    EndWallSlide();
            }
        }

        if (moveInputX != 0) {
            grCheckOffsetX = grCheckMoveOffsetX;
        } else {
            grCheckOffsetX = 0;
        }

        rb.velocity = new Vector2(moveInputX * speed, rb.velocity.y);

        if (!playerAnim.IsFacingRight() && moveInputX > 0) {
            playerAnim.LookRight();
        } else if (playerAnim.IsFacingRight() && moveInputX < 0) {
            playerAnim.LookLeft();
        }
    }

    public void StartJump() {
        if (pauseOnJump)
            Debug.Break();

        if (isWallSliding) {
            WallJump();
        } else {
            GroundJump();
        }        
    }

    public void EndJump () {
        if (isJumping && rb.velocity.y > 0) {
            Vector2 vel = rb.velocity;
            vel.y = rb.velocity.y / 4f;
            rb.velocity = vel;
        }

        isJumping = false;
    }

    public void Dash(float horiz, float vert) {
        if (isDashing || numMoveDashes <= 0)
            return;

        StartDash(horiz, vert, moveDashDist, moveDashSpeed);
        lastDashType = DashType.Move;

        playerAnim.StartMoveDash();
    }

    public void Aim (float horiz, float vert) {
        lastAimDir = new Vector2(horiz, vert).normalized;
        playerAnim.Aim(lastAimDir);
    }

    public void Attack () {
        atkDir = lastAimDir;

        if (numAirAttacks < 2) {
            atkDashDist = maxAtkDashDist / (numAirAttacks+1);
            atkDashSpeed = minAtkDashSpeed * (numAirAttacks+1);

            lastDashType = DashType.Attack;
            StartDash(lastAimDir.x, lastAimDir.y, atkDashDist, atkDashSpeed);
        } else if (IsTouchingBall()) {
            HitBall();
        }
    }

    public void SetBall (BallController _ball) {
        ball = _ball;
        ballColl = ball.GetComponent<Collider2D>();
    }

    private void GroundJump () {
        if (isGrounded)
            numJumps += 1;

        if (numJumps > 0) {
            rb.velocity = Vector2.up * jumpForce;
            playerAnim.SpawnJumpSmoke();
            numJumps -= 1;
        }

        isJumping = true;
    }

    private void WallJump () {
        rb.velocity = Vector2.up * wJumpUpForce;
        playerAnim.SpawnJumpSmoke();

        wJumpMoveSleepTimer = wJumpMoveSleepAmount;
        EndWallSlide();

        isJumping = true;
    }

    // Called when the player has just landed on the ground
    void Land () {
        ResetExtraJumps();
        ResetAirAttacks();

        if (numMoveDashes < maxDashes)
            ResetDashes(true);

        if (rb.velocity.y < 0) {
            Vector3 vel = rb.velocity;
            vel.y = 0;
            rb.velocity = vel;

            transform.position = new Vector3(transform.position.x, lastLandPos.y) + (Vector3.up * (transform.localScale.y / 2f));
        }

        isLanded = false;
    }

    // Called when the player has just touched the ceiling
    void HitCeiling () {
        if (rb.velocity.y > 0) {
            Vector3 vel = rb.velocity;
            vel.y = 0;
            rb.velocity = vel;

            transform.position = new Vector3(transform.position.x, lastCeilHitPos.y - 0.1f) + (Vector3.down * (transform.localScale.y / 2f));
        }

        isHitCeiling = false;
    }

    void StartWallSlide(bool wallOnRight, GameObject wall) {
        if(!isWallSliding)
            //Debug.Log("wall slide start on " + (wallOnRight ? "right" : "left"));

        isWallSliding = true;
        wallOnRightSide = wallOnRight;

        Vector2 currentPos = transform.position;
        if (wallOnRight) {
            currentPos.x = wall.transform.position.x - (transform.localScale.x / 2f) - (wall.GetComponent<BoxCollider2D>().size.x / 2f);
        } else {
            currentPos.x = wall.transform.position.x + (transform.localScale.x / 2f) + (wall.GetComponent<BoxCollider2D>().size.x / 2f);
        }
        transform.position = currentPos;

        playerAnim.StartWallSlide(wallOnRight, wall);
    }

    void EndWallSlide() {
        isWallSliding = false;
        playerAnim.EndWallSlide();
    }

    void StartDash(float horiz, float vert, float dashDistance, float dashSpeed) {
        if (isGrounded && vert < 0)
            vert = 0;

        Vector3 dashDir = new Vector2(horiz, vert).normalized;

        if (dashDir.magnitude == 0) {
            return;
        }

        RaycastHit2D[] hits = new RaycastHit2D[1];

        dshStart = transform.position;
        if (Physics2D.BoxCast(transform.position, transform.localScale * 0.95f, 0, dashDir, dshFilter, hits, dashDistance) > 0) {
            //Debug.Log("Obstacle found");
            dshEnd = hits[0].point;
            dshColNormal = hits[0].normal;

            //Debug.Log(dshColNormal);

            if (dashDir.y == 0) {
                dshEnd.y = transform.position.y;
            }

            if (dashDir.x == 0) {
                dshEnd.x = transform.position.x;
            }

            Vector3 startToEndDir = (dshEnd - dshStart).normalized;

            dshEnd += new Vector3(-startToEndDir.x * (transform.localScale.x / 1.5f), -startToEndDir.y * (transform.localScale.y / 1.5f));

            float startToEndDist = Vector3.Distance(dshStart, dshEnd);

            if (startToEndDist < 3.5f) {
                EndDash(dshEnd);
                return;
            }

            dshCurrentSpeed = dashSpeed / (startToEndDist / dashDistance);
        }
        else {
            dshEnd = transform.position + dashDir * dashDistance;
            dshCurrentSpeed = dashSpeed;
        }

        rb.simulated = false;
        dshLerpValue = 0;

        isDashing = true;
    }

    void EndDash (Vector3 endPos) {
        switch (lastDashType) {
            case DashType.Move:
                numMoveDashes -= 1;
                break;
            case DashType.Attack:
                numAirAttacks += 1;
                canAttack = true;
                break;
            default:
                break;
        }

        dshLerpValue = 1;
        dshLerpLimit = 1;

        rb.simulated = true;
        rb.velocity = Vector3.zero;

        UpdateVertChecks();

        if (isGrounded) {
            ResetDashes(false);
            ResetAirAttacks();
        } else {
            playerAnim.EndDash(numMoveDashes > 0);
        }

        isDashing = false;
    }

    void HitBall () {
        if (canAttack)
            ball.Hit(atkDir, atkForce);
    }

    bool IsTouchingBall () {
        return ballColl.bounds.Intersects(playerColl.bounds);
    }

    void UpdateVertChecks () {
        // Ground
        RaycastHit2D[] groundResult = CheckGround();
        bool touchedGround = groundResult != null;

        if (!isGrounded && touchedGround) {
            isLanded = true;
            lastLandPos = groundResult[0].point;
        }

        isGrounded = touchedGround;

        // Ceiling
        RaycastHit2D[] ceilResult = CheckCeiling();
        bool touchedCeiling = ceilResult != null;

        if (!isOnCeiling && touchedCeiling) {
            isHitCeiling = true;
            lastCeilHitPos = ceilResult[0].point;
        }

        isOnCeiling = touchedCeiling;
    }

    RaycastHit2D[] CheckGround () {
        RaycastHit2D[] hits = new RaycastHit2D[1];

        Vector3 _grCheckScale = vertCheckScale;
        if (rb.velocity.y <= 0) {
            float yScale = ExtensionMethods.Map(rb.velocity.y, 0, -grCheckMaxVel, vertCheckScale.y, vertCheckMaxYScale);
            _grCheckScale = new Vector3(vertCheckScale.x, yScale);
        }

        if (Physics2D.BoxCast(grCheckStart.position + Vector3.down*(_grCheckScale.y/2f), _grCheckScale, 0, Vector2.down, groundFilter, hits, _grCheckScale.y) > 0) {
            return hits;
        }

        return null;
    }

    RaycastHit2D[] CheckCeiling() {
        RaycastHit2D[] hits = new RaycastHit2D[1];

        Vector3 _ceilCheckScale = vertCheckScale;
        if (rb.velocity.y >= 0) {
            float yScale = ExtensionMethods.Map(rb.velocity.y, 0, ceilCheckMaxVel, vertCheckScale.y, vertCheckMaxYScale);
            _ceilCheckScale = new Vector3(vertCheckScale.x, yScale);
        }

        if (Physics2D.BoxCast(ceilCheckStart.position + Vector3.up * (_ceilCheckScale.y / 2f), _ceilCheckScale, 0, Vector2.up, ceilFilter, hits, _ceilCheckScale.y) > 0) {
            return hits;
        }

        return null;
    }

    // Checks for wall to left of player. Returns wall object that is in contact with the player.
    GameObject CheckWallLeft () {
        RaycastHit2D hitUp = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + wallCheckOffsetY), Vector2.left, wallCheckDist, whatIsWall);
        RaycastHit2D hitDown = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - wallCheckOffsetY), Vector2.left, wallCheckDist, whatIsWall);
        GameObject result;
        if (hitUp) {
            result = hitUp.collider.gameObject;
        } else if (hitDown) {
            result = hitDown.collider.gameObject;
        } else {
            result = null;
        }
        return result;
    }

    // Checks for wall to right of player. Returns wall object that is in contact with the player.
    GameObject CheckWallRight() {
        RaycastHit2D hitUp = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + wallCheckOffsetY), Vector2.right, wallCheckDist, whatIsWall);
        RaycastHit2D hitDown = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - wallCheckOffsetY), Vector2.right, wallCheckDist, whatIsWall);
        GameObject result;
        if (hitUp) {
            result = hitUp.collider.gameObject;
        } else if (hitDown) {
            result = hitDown.collider.gameObject;
        } else {
            result = null;
        }
        return result;
    }

    void ResetExtraJumps () {
        numJumps = maxJumps-1;
    }

    void ResetDashes (bool playAnimation) {
        numMoveDashes = maxDashes;
        playerAnim.ResetDashes(playAnimation);
    }

    void ResetAirAttacks () {
        numAirAttacks = 0;
    }

# if UNITY_EDITOR_WIN
    void OnDrawGizmosSelected() {
        // Ground check
        Gizmos.color = Color.yellow;
        Vector3 _grCheckScale = vertCheckScale;
        if (EditorApplication.isPlaying && rb != null) {
            if (rb.velocity.y <= 0) {
                float yScale = ExtensionMethods.Map(rb.velocity.y, 0, -grCheckMaxVel, vertCheckScale.y, vertCheckMaxYScale);
                _grCheckScale = new Vector3(vertCheckScale.x, yScale);
            }
        }

        Gizmos.DrawCube(grCheckStart.position + (Vector3.down * (_grCheckScale.y / 2f)), _grCheckScale);

        // Ceiling check
        Vector3 _ceilCheckScale = vertCheckScale;
        if (EditorApplication.isPlaying && rb != null) {
            if (rb.velocity.y >= 0) {
                float yScale = ExtensionMethods.Map(rb.velocity.y, 0, ceilCheckMaxVel, vertCheckScale.y, vertCheckMaxYScale);
                _ceilCheckScale = new Vector3(vertCheckScale.x, yScale);
            }
        }

        Gizmos.DrawCube(ceilCheckStart.position + (Vector3.up * (_ceilCheckScale.y / 2f)), _ceilCheckScale);

        // Wall check
        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y + wallCheckOffsetY), new Vector2(transform.position.x - wallCheckDist, transform.position.y + wallCheckOffsetY));
        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y - wallCheckOffsetY), new Vector2(transform.position.x - wallCheckDist, transform.position.y - wallCheckOffsetY));

        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y + wallCheckOffsetY), new Vector2(transform.position.x + wallCheckDist, transform.position.y + wallCheckOffsetY));
        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y - wallCheckOffsetY), new Vector2(transform.position.x + wallCheckDist, transform.position.y - wallCheckOffsetY));
    }
#endif

}

