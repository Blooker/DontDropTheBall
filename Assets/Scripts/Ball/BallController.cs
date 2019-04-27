using UnityEngine;

public class BallController : MonoBehaviour {

    [Header("Bounce")]
    [SerializeField] private float bounceAmount = 0.8f;
    [SerializeField] private LayerMask canBounceOff;
    [SerializeField] private float restHitCheckDist = 0.2f;

    private Vector2 lastVel;
    private bool isBouncing = true;

    private ContactFilter2D bounceFilter;

    private Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();

        bounceFilter = new ContactFilter2D();
        bounceFilter.SetLayerMask(canBounceOff);
    }

    // Update is called once per frame
    void LateUpdate() {
        if (rb != null)
        {
            lastVel = rb.velocity;
        }
    }
    
    public void Hit (Vector2 dir, float force) {
        RaycastHit2D[] hits = new RaycastHit2D[1];
        if (Physics2D.CircleCast(transform.position, transform.localScale.x / 2f, dir, bounceFilter, hits, restHitCheckDist) > 0)
        {
            Bounce(hits[0].normal, hits[0].point, dir * force, 1);
        }
        else
        {
            rb.velocity = dir * force;
        }
    }

    void Bounce(Vector2 hitNormal, Vector2 hitPoint) {
        Bounce(hitNormal, hitPoint, lastVel, bounceAmount);
    }

    void Bounce(Vector2 hitNormal, Vector2 hitPoint, Vector2 vel, float amount) {
        Vector2 dir = vel.normalized;

        Vector2 reflection = Vector2.Reflect(dir, hitNormal);

        Debug.DrawLine(hitPoint, hitPoint - dir * 10f, Color.blue, 10f);
        Debug.DrawLine(transform.position, (Vector2)transform.position + reflection * 10f, Color.cyan, 10f);
        Debug.DrawLine(hitPoint, hitPoint + hitNormal * 10f, Color.green, 10f);

        Vector2 newVel = reflection * vel.magnitude * amount;
        rb.velocity = newVel;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (canBounceOff == (canBounceOff | (1 << collision.gameObject.layer)))
        {
            ContactPoint2D[] contacts = new ContactPoint2D[1];
            collision.GetContacts(contacts);

            Bounce(contacts[0].normal, contacts[0].point);
        }
    }
}
