using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using CMF;

namespace Player
{
    public class CharacterActionMapInput : CharacterInput
    {
        [Header("Locomotion Input")]
        [SerializeField] private Vector2 moveInput;
        [SerializeField] private bool isJumping;
        [SerializeField] private bool isSprinting;

        [Header("Character Actions Input")]
        [SerializeField] private bool isAiming;
        [SerializeField] private bool isFiring;
        [SerializeField] private bool isReloading;
        [SerializeField] private bool isSwapingWeapon;

        #region Input Callbacks
        
        public void OnMoveInput(InputAction.CallbackContext inputCallback)
        {
            moveInput = inputCallback.ReadValue<Vector2>();
        }

        public void OnJumpInput(InputAction.CallbackContext inputCallback)
        {
            isJumping = inputCallback.ReadValueAsButton();
        }

        public void OnSprintInput(InputAction.CallbackContext inputCallback)
        {
            isSprinting = inputCallback.ReadValueAsButton();
        }

        public void OnAimInput(InputAction.CallbackContext inputCallback)
        {
            isAiming = inputCallback.ReadValueAsButton();
        }

        public void OnFireInput(InputAction.CallbackContext inputCallback)
        {
            isFiring = inputCallback.ReadValueAsButton();
        }

        public void OnReloadInput(InputAction.CallbackContext inputCallback)
        {
            isReloading = inputCallback.ReadValueAsButton();
        }
        #endregion

        #region Main Value Getters
        public override float GetHorizontalMovementInput()
        {
            return moveInput.x;
        }

        public override float GetVerticalMovementInput()
        {
            return moveInput.y;
        }

        public override bool IsJumpKeyPressed()
        {
            return isJumping;
        }

        public override bool IsSprintKeyPressed()
        {
            return isSprinting;
        }
        #endregion
    }
}