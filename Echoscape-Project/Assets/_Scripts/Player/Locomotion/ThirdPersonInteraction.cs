using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Player
{
    public class ThirdPersonInteraction : MonoBehaviour
    {

        [Header("Aim Settings")]
        [SerializeField] private float aimDistance = 50f;
        [SerializeField] private Vector3 aimPosition;
        [SerializeField] private Transform aimTransform;

        [SerializeField] private LayerMask aimCollisionMask;

        private Vector2 crosshairLocation = new Vector2(0.5f, 0.5f);

        [Header("Cinemachine Cameras")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CinemachineVirtualCamera aimCamera;

        #region Unity Functions
        private void Start()
        {
            
        }

        private void Update()
        {
            GetAimLocation();
            TranslateAimPosition();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aimPosition, 0.25f);
        }
        #endregion

        #region Aiming Functions
        private void GetAimLocation()
        {
            RaycastHit hit;
            Ray aimRay = playerCamera.ViewportPointToRay(crosshairLocation);

            if (Physics.Raycast(aimRay, out hit, aimCollisionMask))
            {
                aimPosition = hit.point;
            }
        }

        private void TranslateAimPosition()
        {
            aimTransform.position = Vector3.Lerp(aimTransform.position, aimPosition, Time.deltaTime * 10f);
        }
        #endregion
    }    
}