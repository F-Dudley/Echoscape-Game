using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lift : MonoBehaviour
{
    [Header("Lift Settings")]
    [SerializeField] private bool lifting = false;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float delay = 5.0f;
    [SerializeField] private LayerMask liftMask;

    [Header("Lift Positions")]
    [SerializeField] private Vector3 stationaryPosition;
    [SerializeField] private Vector3 targetPosition;

    [Header("Lift Objects")]
    [SerializeField] private Transform liftPlatform;

    private void Start()
    {
        stationaryPosition = liftPlatform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object Entered Lift");
        Debug.Log($"Object: {other.name} - Layer: {other.gameObject.layer} - Tag: {other.gameObject.tag}");
        if ((other.gameObject.layer == liftMask || other.gameObject.CompareTag("Player")) && !lifting)
        {
            MovePlatform();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position + stationaryPosition, transform.position + targetPosition);
        Gizmos.DrawWireCube(transform.position + targetPosition, new Vector3(2f, 0.25f, 1f));
    }

    private void MovePlatform()
    {
        liftPlatform.DOLocalMove(targetPosition, speed)
            .SetEase(Ease.OutSine)
            .OnStart(() => lifting = true)
            .OnComplete(() =>
            {

                liftPlatform.DOLocalMove(stationaryPosition, speed)
                .SetDelay(delay)
                .SetEase(Ease.OutSine)
                .OnComplete(() => lifting = false);
            });
    }
}
