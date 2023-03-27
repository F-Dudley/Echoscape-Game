using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RotateSource : MonoBehaviour
{
    [SerializeField] private Vector2 rotateSpeed = new Vector2(0.5f, 0.5f);

    private void Start()
    {

    }

    private void Update()
    {
        float sin = Mathf.Sin(Time.time * Mathf.Deg2Rad);

        transform.RotateAround(transform.position, Vector3.up, rotateSpeed.x * Time.deltaTime);
        transform.RotateAround(transform.position, Vector3.right, sin * rotateSpeed.y * Time.deltaTime);
    }
}
