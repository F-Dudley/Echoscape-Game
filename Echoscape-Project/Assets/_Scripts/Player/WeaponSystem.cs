using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private WeaponSlot[] weaponSlots;

        [Header("References")]
        [SerializeField] private Transform weaponHolder;

        #region Unity Functions
        private void Start()
        {
            Transform child;
            for (int i = 0; i < weaponHolder.childCount; i++)
            {
                child = weaponHolder.GetChild(i);
                child.gameObject.SetActive(i == currentSlot ? true : false);
            }
        }

        private void Update()
        {
            
        }
        #endregion
        
        #region Gun Functions
        public void SwapWeapon(int newWeaponSlot)
        {
            ChangeWeaponVisability(currentSlot, false);          
            ChangeWeaponVisability(newWeaponSlot, true);
            
            currentSlot = newWeaponSlot;
        }

        private void ChangeWeaponVisability(int slot, bool isVisible)
        {
            weaponSlots[slot].weapon.gameObject.SetActive(isVisible);
        }
        #endregion
    }    
}