using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsAgent : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed;

    [Header("Detection")]
    [SerializeField] private float detectionRange;
    [SerializeField] private LayerMask detectionMask;

    [Header("Physics")]
    [SerializeField] private float rideHeight;
    [SerializeField] private float springStrength;
    [SerializeField] private float springDamper;

    [SerializeField] private bool isGrounded;
    [SerializeField] private LayerMask groundMask;

    private RaycastHit downRaycastHit;

    [Header("Components")]
    [SerializeField] private Transform agentBody;

    [Space]

    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private SmallDroneSystem parentSystem;

    #region Unity Functions
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnDisable()
    {
        parentSystem.ReturnToPool(this);
    }

    private void Update()
    {
        if (Random.Range(0, 100) < 10 && Vector3.Distance(target.position, rb.position) < 12)
        {
            Debug.Log("Shooting");
        }
    }

    private void FixedUpdate()
    {
        CalculateRideHeight();

        TravelTowardsTarget();

        agentBody.LookAt(target.position);

        if (!isGrounded) rb.AddRelativeForce(-transform.up * 2f, ForceMode.Force);
    }
    #endregion

    #region Get Sets
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void ResetVelocity()
    {
        rb.velocity = Vector3.zero;
    }

    public void SetParentSystem(SmallDroneSystem system)
    {
        parentSystem = system;
    }
    #endregion

    #region Physics Calculation
    private void CalculateRideHeight()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, out downRaycastHit, rideHeight * 1.25f, groundMask, QueryTriggerInteraction.Ignore);

        if (isGrounded && rb.velocity.magnitude > maxSpeed)
        {
            Vector3 rayDir = transform.TransformDirection(-transform.up);

            Vector3 otherVel = Vector3.zero;

            float rayDirVel = Vector3.Dot(rayDir, rb.velocity);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            float distanceToRideHeight = downRaycastHit.distance - rideHeight;

            float springForce = (distanceToRideHeight * springStrength) - (relVel * springDamper);

            rb.AddForce(rayDir * springForce);
        }
    }

    private void TravelTowardsTarget()
    {
        if (!Physics.CheckSphere(rb.position, detectionRange, detectionMask))
        {
            rb.AddRelativeForce((target.position - rb.position).normalized);
        }
        else rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.5f * Time.deltaTime);
    }
    #endregion
}
