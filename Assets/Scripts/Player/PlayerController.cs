using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnim))]
public class PlayerController : MonoBehaviour {

    [SerializeField] private float speed, acceleration, jumpForce;
    [SerializeField] private float maxJumps = 2;

    [Header("Ground Check")]
    [SerializeField] private float grCheckRadius;
    [SerializeField] private float grCheckDist, grCheckMoveOffsetX;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Wall Check")]
    [SerializeField] private float wallCheckRadius;
    [SerializeField] private float wallCheckDist;
    [SerializeField] private float wallCheckOffsetY;
    [SerializeField] private LayerMask whatIsWall;

    [Header("Wall Sliding")]
    [SerializeField] private float wallFallSpeedDecrease = 10f;

    private float moveInputX;
    private float numJumps;

    private float regGravityScale;

    private float grCheckOffsetX = 0;

    private bool isGrounded = false, isLanded = false, isWallSliding = false;

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
            rb.AddForce(Vector2.up * wallFallSpeedDecrease);
        }
    }

    public void Move(float horiz) {
        float moveDelta = 0;

        if (!isGrounded) {
            moveDelta = acceleration / 2;
        } else if (moveInputX > 0 && horiz <= 0 || moveInputX <= 0 && horiz > 0) {
            moveDelta = acceleration * 2;
        } else {
            moveDelta = acceleration;
        }

        GameObject wallTouchedRight = CheckWallRight();
        GameObject wallTouchedLeft = CheckWallLeft();

        if (wallTouchedRight != null && horiz > 0) {
            moveInputX = 0;
            if (!isGrounded)
                StartWallSlide(true, wallTouchedRight);
        } else if (wallTouchedLeft != null && horiz < 0) {
            moveInputX = 0;
            if (!isGrounded)
                StartWallSlide(false, wallTouchedLeft);
        } else {
            moveInputX = Mathf.MoveTowards(moveInputX, horiz, moveDelta);
            if (isWallSliding)
                EndWallSlide();
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
        if (isGrounded)
            numJumps += 1;

        if (numJumps > 0) {
            rb.velocity = Vector2.up * jumpForce;
            numJumps -= 1;
        }
    }

    // Call when the player has just landed on the ground
    void Land () {
        ResetExtraJumps();
        isLanded = false;

        //Debug.Log("Landed!");
    }

    void StartWallSlide(bool wallOnRight, GameObject wall) {
        isWallSliding = true;
        playerAnim.StartWallSlide(wallOnRight, wall);
    }

    void EndWallSlide() {
        isWallSliding = false;
        playerAnim.EndWallSlide();
    }

    bool CheckGround () {
        Vector2 circleCastOrigin;
        if (moveInputX > 0) {
            circleCastOrigin = new Vector2(transform.position.x - grCheckOffsetX, transform.position.y);
        } else {
            circleCastOrigin = new Vector2(transform.position.x + grCheckOffsetX, transform.position.y);
        }

        bool result = (bool)Physics2D.CircleCast(circleCastOrigin, grCheckRadius, Vector2.down, grCheckDist, whatIsGround);
        return result;
    }

    GameObject CheckWallLeft () {
        RaycastHit2D hit = Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y + wallCheckOffsetY), wallCheckRadius, Vector2.left, wallCheckDist, whatIsWall);
        GameObject result;
        if (hit) {
            result = hit.collider.gameObject;
            //Debug.Log("Touching left wall");
        } else {
            result = null;
        }
        return result;
    }

    GameObject CheckWallRight() {
        RaycastHit2D hit = Physics2D.CircleCast(new Vector2(transform.position.x, transform.position.y + wallCheckOffsetY), wallCheckRadius, Vector2.right, wallCheckDist, whatIsWall);
        GameObject result;
        if (hit) {
            result = hit.collider.gameObject;
            //Debug.Log("Touching right wall");
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
        Gizmos.DrawWireSphere(new Vector3(moveInputX > 0 ? transform.position.x - grCheckOffsetX : transform.position.x + grCheckOffsetX, transform.position.y - grCheckDist), grCheckRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - wallCheckDist, transform.position.y + wallCheckOffsetY), wallCheckRadius);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + wallCheckDist, transform.position.y + wallCheckOffsetY), wallCheckRadius);
    }
}
