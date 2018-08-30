using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BallController : MonoBehaviour {


    [Header("Bounce")]
    [SerializeField] private float bounceAmount = 0.8f;
    [SerializeField] private LayerMask canBounceOff;
    [SerializeField] private float bCheckMaxDist = 5;
    [SerializeField] private float bCheckMinMagnitude = 5, bCheckMaxMagnitude = 150;

    private Vector2 lastVel;

    private ContactFilter2D bounceFilter;

    private Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();

        bounceFilter = new ContactFilter2D();
        bounceFilter.SetLayerMask(canBounceOff);
    }

    // Use this for initialization
    void Start () {
		
	}

    void FixedUpdate() {

    }

    // Update is called once per frame
    void LateUpdate () {
        if (rb != null) {
            lastVel = rb.velocity;
        }
    }

    void Bounce (Vector2 hitNormal, Vector2 hitPoint) {
        Vector2 dir = lastVel.normalized;

        Vector2 reflection = Vector2.Reflect(dir, hitNormal);

        Debug.DrawLine(hitPoint, hitPoint - dir * 10f, Color.blue, 10f);
        Debug.DrawLine(transform.position, (Vector2)transform.position + reflection * 10f, Color.cyan, 10f);
        Debug.DrawLine(hitPoint, hitPoint + hitNormal * 10f, Color.green, 10f);

        Vector2 newVel = reflection * lastVel.magnitude * bounceAmount;
        rb.velocity = newVel;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (canBounceOff == (canBounceOff | (1 << collision.gameObject.layer))) {
            ContactPoint2D[] contacts = new ContactPoint2D[1];
            collision.GetContacts(contacts);
            
            Bounce(contacts[0].normal, contacts[0].point);
        }
    }
}
