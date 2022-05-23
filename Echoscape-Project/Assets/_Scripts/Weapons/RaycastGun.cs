using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    [System.Serializable]
    public class RaycastGun : Gun
    {
        #region Unity Functions
        private void Start()
        {

        }
        #endregion

        #region Gun Function Overrides
        public override void Shoot()
        {
            RaycastHit hit;
            Ray ray = new Ray(fireLocation.position, fireLocation.forward);

            if (Physics.Raycast(ray, out hit, range, fireCollisionMask))
            {
                Debug.Log("Hit: " + hit.transform.name);
            }
        }
        #endregion
    }    
}