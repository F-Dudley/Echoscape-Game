using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestAI : MonoBehaviour
{
    [SerializeField] private Transform planetCentre;
    [SerializeField] private float planetRadius;

    [SerializeField] private Transform target;

    private Vector3 test;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        test = planetCentre.position + ((target.position - planetCentre.position).normalized * planetRadius);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, test);
    }
}
