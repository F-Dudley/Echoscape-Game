using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public enum GunType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper
    }

    [System.Serializable]
    public class Gun : MonoBehaviour
    {
        [Header("Base Gun Stats")]
        public GunType gunType;
        [SerializeField] protected LayerMask fireCollisionMask;
        [SerializeField] protected int damage = 10;
        [SerializeField] protected float fireRate = 0.5f;
        [SerializeField] protected float range = 100f;
        [SerializeField] protected float impactForce = 30f;
        [SerializeField] protected float reloadTime = 1f;

        [SerializeField] protected Transform fireLocation;

        protected float fireDelayTime = 0f;
        protected WaitForSeconds reloadDelay;

        #region Base Gun Functions

        public bool CanShoot
        {
            get { return Time.time > fireDelayTime; }
        }

        public bool IsReloading
        {
            get { return false; }
        }

        protected virtual void Init()
        {
            reloadDelay = new WaitForSeconds(reloadTime);
            fireDelayTime = Time.time;
        }

        public virtual void Shoot()
        {

        }

        public virtual void Reload()
        {

        }

        protected virtual void ChangeWeaponAnimation()
        {

        }
        #endregion
    }    
}