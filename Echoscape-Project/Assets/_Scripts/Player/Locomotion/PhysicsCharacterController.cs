using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCharacterController : MonoBehaviour
{
    [Header("Locomotion")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 150f;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        
    }
}
