using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

using Weapons;

namespace Player
{
    [System.Serializable]
    public struct WeaponSlot
    {
        public Gun weapon;
    }

    public class WeaponSystem : MonoBehaviour
    {
        [SerializeField] private int currentSlot;
        [SerializeField] private Gun equippedWeapon;
        [SerializeField] private WeaponSlot[] weaponSlots;

        [Header("References")]
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private Animator playerAnimator;
        private UserInput userInput;

        [Header("Animations")]
        [SerializeField] private bool isSwapAnimationPlaying;        
        [SerializeField] private Rig aimRig;
        private int _animSwapWeaponID;
        private int _animWeaponTypeID;

        #region Unity Functions
        private void Start()
        {
            playerAnimator = GetComponent<Animator>();
            userInput = GetComponent<UserInput>();

            foreach (Transform weapon in weaponHolder)
            {
                weapon.gameObject.SetActive(false);
            }

            isSwapAnimationPlaying = true;

            GetAnimationIDs();
        }

        private void Update()
        {
            if (aimRig.weight != 1 || aimRig.weight != 0)
            {
                aimRig.weight = Mathf.Lerp(aimRig.weight, isSwapAnimationPlaying ? 0 : 1, Time.deltaTime * 5);

                if (Mathf.Approximately(aimRig.weight, isSwapAnimationPlaying ? 0 : 1))
                {
                    aimRig.weight = isSwapAnimationPlaying ? 0 : 1;
                }
            }

            if (equippedWeapon == null || isSwapAnimationPlaying) return;

            if (userInput.shooting)
            {
                FireWeapon();
            }

            if (userInput.reloading)
            {
                ReloadWeapon();
            }

            if (userInput.swapWeapon)
            {
                SwapWeapon();
            }
        }
        #endregion
        
        #region Gun Functions
        private void FireWeapon()
        {
            if (equippedWeapon.CanShoot)
            {
                equippedWeapon.Shoot();
            }
        }

        private void ReloadWeapon()
        {
            if (equippedWeapon.CanReload)
            {
                equippedWeapon.Reload();
            }
        }

        private void SwapWeapon()
        {
            isSwapAnimationPlaying = true;
            equippedWeapon = null;

            ChangeWeaponVisability(currentSlot, false);            
            currentSlot = (currentSlot + 1) % weaponSlots.Length;

            playerAnimator.SetTrigger(_animSwapWeaponID);
            playerAnimator.SetInteger("WeaponType", (int) weaponSlots[currentSlot].weapon.gunType);
        }

        private void ChangeWeaponVisability(int slot, bool isVisible)
        {
            weaponSlots[slot].weapon.gameObject.SetActive(isVisible);
        }
        #endregion

        #region Animations
        private void GetAnimationIDs()
        {
            _animSwapWeaponID = Animator.StringToHash("ChangeWeapon");
            _animWeaponTypeID = Animator.StringToHash("WeaponType");
        }

        private void BackWeaponReached(AnimationEvent animationEvent)
        {
            ChangeWeaponVisability(currentSlot, true);
        }

        private void WeaponSwapFinished(AnimationEvent animationEvent)
        {
            equippedWeapon = weaponSlots[currentSlot].weapon;
            isSwapAnimationPlaying = false;
        }
        #endregion
    }    
}