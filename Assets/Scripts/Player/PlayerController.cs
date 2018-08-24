﻿using System.Collections;
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
    [SerializeField] private LayerMask whatStopsDash;

    private float moveInputX;
    private float numJumps;

    private float regGravityScale;

    private float grCheckOffsetX = 0;

    private float wJumpMoveSleepTimer = 0;

    private Vector3 dshStart, dshEnd;
    private float dshLerpValue = 1, dshCurrentSpeed;

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

        if (dshLerpValue < 0.95f) {
            transform.position = Vector3.Lerp(dshStart, dshEnd, dshLerpValue);
            dshLerpValue += dshCurrentSpeed * Time.deltaTime;
        } else if (isDashing) {
            EndDash(dshEnd);
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

        rb.velocity = new Vector2(moveInputX * speed * Time.deltaTime, rb.velocity.y);

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

    public void StartDash(float horiz, float vert) {
        if (isGrounded && vert < 0)
            vert = 0;

        Vector3 dashDir = new Vector2(horiz, vert).normalized;

        if (dashDir.magnitude == 0)
            return;

        RaycastHit2D[] hits = new RaycastHit2D[1];

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(whatStopsDash);

        dshStart = transform.position;

        if (Physics2D.BoxCast(transform.position, transform.localScale, 0, dashDir, filter, hits, dshDistance) > 0) {
            //Debug.Log("Obstacle found");
            dshEnd = hits[0].point;

            if (dashDir.y == 0) {
                dshEnd.y = transform.position.y;
            }

            if (dashDir.x == 0) {
                dshEnd.x = transform.position.x;
            }

            float startToEndDist = Vector3.Distance(dshStart, dshEnd);
            if (startToEndDist < 3.5f) {
                EndDash(dshEnd);
                return;
            }

            dshCurrentSpeed = dshSpeed / (startToEndDist / dshDistance);
        } else {
            dshEnd = transform.position + dashDir * dshDistance;
            dshCurrentSpeed = dshSpeed;
        }

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

    void EndDash (Vector3 endPos) {

        transform.position = GetDashEndPlayerPos(endPos);

        dshLerpValue = 1;

        rb.simulated = true;

        rb.velocity = Vector3.zero;
        isDashing = false;
    }

    // Gets the correct dash end position so that the player doesn't clip into terrain
    Vector3 GetDashEndPlayerPos (Vector3 endPos) {
        Vector3 result;

        if (dshCurrentSpeed != dshSpeed) {
            Vector3 startEndDir = (dshEnd - dshStart).normalized;
            Debug.Log(startEndDir);

            Vector3 endOffset = Vector3.zero;
            if (startEndDir.y > 0.75f && startEndDir.x > -0.75f && startEndDir.x < 0.75f) {
                // up code
                endOffset = Vector3.down * (transform.localScale.y / 2f);
            }
            else if (startEndDir.x < -0.75f && startEndDir.y > -0.75f && startEndDir.y < 0.75f) {
                // left code
                endOffset = Vector3.right * (transform.localScale.x / 2f);
            }
            else if (startEndDir.y < -0.75f && startEndDir.x > -0.75f && startEndDir.x < 0.75f) {
                // down code
                endOffset = Vector3.up * (transform.localScale.y / 2f);
            }
            else if (startEndDir.x > 0.75f && startEndDir.y > -0.75f && startEndDir.y < 0.75f) {
                // right code
                endOffset = Vector3.left * (transform.localScale.x / 2f);
            }

            result = endPos + endOffset;
        }
        else {
            result = endPos;
        }

        return result;
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
        // Ground check
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(new Vector3(moveInputX > 0 ? transform.position.x - grCheckOffsetX : transform.position.x + grCheckOffsetX, transform.position.y - grCheckDist), grCheckRadius);
        Gizmos.DrawCube(transform.position + (Vector3.down * grCheckDist), grCheckScale);

        // Wall check
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - wallCheckDist, transform.position.y + wallCheckOffsetY), wallCheckRadius);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + wallCheckDist, transform.position.y + wallCheckOffsetY), wallCheckRadius);

        // Dash stop check
        Gizmos.color = Color.white;
        //Gizmos.DrawCube(transform.position - Vector3.right * (transform.localScale.x / 2f), new Vector3(dshStopCheckScale.x, transform.localScale.y));
        //Gizmos.DrawCube(transform.position + Vector3.right * (transform.localScale.x / 2f), new Vector3(dshStopCheckScale.x, transform.localScale.y));

        //Gizmos.DrawCube(transform.position - Vector3.up * (transform.localScale.y / 2f), new Vector3(transform.localScale.x, dshStopCheckScale.y));
        //Gizmos.DrawCube(transform.position + Vector3.up * (transform.localScale.y / 2f), new Vector3(transform.localScale.x, dshStopCheckScale.y));
    }
}
