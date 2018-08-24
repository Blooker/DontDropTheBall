using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnim))]
public class PlayerController : MonoBehaviour {

    [SerializeField] private float speed, acceleration, jumpForce;
    [SerializeField] private float maxJumps = 2;

    [Header("Ground Check")]
    [SerializeField] private /*float grCheckRadius*/ Vector2 grCheckScale;
    [SerializeField] private float grCheckDist, grCheckMoveOffsetX;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Wall Check")]
    [SerializeField] private float wallCheckRadius;
    [SerializeField] private float wallCheckDist;
    [SerializeField] private float wallCheckOffsetY;
    [SerializeField] private LayerMask whatIsWall;

    [Header("Wall Jump/Slide")]
    [SerializeField] private float wFallSpeedDecrease = 10f;
    [SerializeField] private float wJumpUpForce;
    [SerializeField] private float wJumpMoveSleepAmount = 0.2f;

    [Header("Dash")]
    [SerializeField] private float dshSpeed = 0.1f;
    [SerializeField] private float dshDistance = 10f;

    private float moveInputX;
    private float numJumps;

    private float regGravityScale;

    private float grCheckOffsetX = 0;

    private float wJumpMoveSleepTimer = 0;

    private Vector3 dshStart, dshEnd;
    private float dshLerpValue = 1;

    private bool isGrounded = false, isLanded = false;
    private bool isWallSliding = false, wallOnRightSide = false;
    private bool isDashing = false;

    private PlayerAnim playerAnim;
    private Rigidbody2D rb;

    // Use this for initialization
    void Awake() {
        playerAnim = GetComponent<PlayerAnim>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        ResetExtraJumps();
        regGravityScale = rb.gravityScale;
    }

    void FixedUpdate() {
        bool circleCast = CheckGround();

        if (!isGrounded && circleCast) {
            isLanded = true;
        }

        isGrounded = circleCast;
    }

    // Update is called once per frame
    void Update() {
        if (isLanded)
            Land();

        if (isWallSliding && rb.velocity.y <= 0) {
            rb.AddForce(Vector2.up * wFallSpeedDecrease);
        }

        if (wJumpMoveSleepTimer > 0) {
            wJumpMoveSleepTimer -= Time.deltaTime;
        }

        if (dshLerpValue < 1) {
            transform.position = Vector3.Lerp(dshStart, dshEnd, dshLerpValue);

            dshLerpValue += dshSpeed;
        } else if (isDashing) {
            EndDash();
        }
    }

    public void Move(float horiz) {
        float moveDelta = 0;

        if (wJumpMoveSleepTimer > 0) {
            horiz = 0;

            if (wallOnRightSide) {
                moveInputX = -1;
            } else {
                moveInputX = 1;
            }
        } else {
            if (!isGrounded) {
                moveDelta = acceleration / 2;
            }
            else if (moveInputX > 0 && horiz <= 0 || moveInputX <= 0 && horiz > 0) {
                moveDelta = acceleration * 2;
            }
            else {
                moveDelta = acceleration;
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

    public void Jump() {
        if (isWallSliding) {
            WallJump();
        } else {
            GroundJump();
        }        
    }

    public void StartDash (float horiz, float vert) {
        Vector3 dashDir = new Vector2(horiz, vert).normalized;

        dshStart = transform.position;
        dshEnd = transform.position + dashDir * dshDistance;

        rb.simulated = false;
        dshLerpValue = 0;

        isDashing = true;
    }

    private void GroundJump () {
        if (isGrounded)
            numJumps += 1;

        if (numJumps > 0) {
            rb.velocity = Vector2.up * jumpForce;
            playerAnim.SpawnJumpSmoke();
            numJumps -= 1;
        }
    }

    private void WallJump () {
        rb.velocity = Vector2.up * wJumpUpForce;
        playerAnim.SpawnJumpSmoke();

        wJumpMoveSleepTimer = wJumpMoveSleepAmount;
        EndWallSlide();
    }

    // Call when the player has just landed on the ground
    void Land () {
        ResetExtraJumps();
        isLanded = false;
    }

    void StartWallSlide(bool wallOnRight, GameObject wall) {
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

    void EndDash () {
        transform.position = dshEnd;
        rb.simulated = true;

        rb.velocity = Vector3.zero;
        isDashing = false;
    }

    bool CheckGround () {
        //Vector2 circleCastOrigin;
        //if (moveInputX > 0) {
        //    circleCastOrigin = new Vector2(transform.position.x - grCheckOffsetX, transform.position.y);
        //} else {
        //    circleCastOrigin = new Vector2(transform.position.x + grCheckOffsetX, transform.position.y);
        //}

        //bool result = (bool)Physics2D.CircleCast(circleCastOrigin, grCheckRadius, Vector2.down, grCheckDist, whatIsGround);
        bool result = (bool)Physics2D.BoxCast(transform.position, grCheckScale, 0, Vector2.down, grCheckDist, whatIsGround);
        return result;
    }

    // Checks for wall to left of player. Returns wall object that is in contact with the player.
    GameObject CheckWallLeft () {
        RaycastHit2D hit = Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y + wallCheckOffsetY), wallCheckRadius, Vector2.left, wallCheckDist, whatIsWall);
        GameObject result;
        if (hit) {
            result = hit.collider.gameObject;
        } else {
            result = null;
        }
        return result;
    }

    // Checks for wall to right of player. Returns wall object that is in contact with the player.
    GameObject CheckWallRight() {
        RaycastHit2D hit = Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y + wallCheckOffsetY), wallCheckRadius, Vector2.right, wallCheckDist, whatIsWall);
        GameObject result;
        if (hit) {
            result = hit.collider.gameObject;
        } else {
            result = null;
        }
        return result;
    }

    void ResetExtraJumps () {
        numJumps = maxJumps-1;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(new Vector3(moveInputX > 0 ? transform.position.x - grCheckOffsetX : transform.position.x + grCheckOffsetX, transform.position.y - grCheckDist), grCheckRadius);
        Gizmos.DrawCube(transform.position + (Vector3.down * grCheckDist), grCheckScale);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - wallCheckDist, transform.position.y + wallCheckOffsetY), wallCheckRadius);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + wallCheckDist, transform.position.y + wallCheckOffsetY), wallCheckRadius);
    }
}
