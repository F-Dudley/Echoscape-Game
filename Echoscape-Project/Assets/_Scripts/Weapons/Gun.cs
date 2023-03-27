using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public enum GunType
    {
        Pistol = 0,
        Rifle = 1,
        Shotgun = 2,
        Sniper = 3,
    }

    [System.Serializable]
    public class Gun : MonoBehaviour
    {
        [Header("Stats")]
        public GunType gunType;
        [SerializeField] protected LayerMask fireCollisionMask;
        [SerializeField] protected int damage = 10;
        [SerializeField] protected float fireRate = 0.5f;
        [SerializeField] protected float range = 100f;
        [SerializeField] protected float impactForce = 30f;

        [Header("Ammo")]
        [SerializeField] protected int currentAmmo;
        [SerializeField] protected int magazineSize = 30;

        [Header("Reloading")]
        [SerializeField] protected float reloadTime = 1f;

        [SerializeField] protected Transform fireLocation;
        [SerializeField] protected ParticleSystem fireParticles;

        protected float fireDelayTime = 0f;
        protected WaitForSeconds reloadDelay;

        #region Base Gun Functions

        public bool CanShoot
        {
            get { return Time.time > fireDelayTime; }
        }

        public bool CanReload
        {
            get { return currentAmmo < magazineSize; }
        }

        public bool IsReloading
        {
            get { return false; }
        }

        protected virtual void Init()
        {
            reloadDelay = new WaitForSeconds(reloadTime);
            fireDelayTime = Time.time;

            currentAmmo = magazineSize;
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