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

        [SerializeField] private PlayerInput pInput;

        private void Awake()
        {
            pInput = GetComponent<PlayerInput>();
        }

        #region Input Callbacks

        public void OnMoveInput(InputAction.CallbackContext inputCallback)
        {
            if (!InputIsActive()) return;

            moveInput = inputCallback.ReadValue<Vector2>();
        }

        public void OnJumpInput(InputAction.CallbackContext inputCallback)
        {
            if (!InputIsActive()) return;
            
            isJumping = inputCallback.ReadValueAsButton();
        }

        public void OnSprintInput(InputAction.CallbackContext inputCallback)
        {
            if (!InputIsActive()) return;

            isSprinting = inputCallback.ReadValueAsButton();
        }

        public void OnAimInput(InputAction.CallbackContext inputCallback)
        {
            if (!InputIsActive()) return;

            isAiming = inputCallback.ReadValueAsButton();
        }

        public void OnFireInput(InputAction.CallbackContext inputCallback)
        {
            if (!InputIsActive()) return;

            isFiring = inputCallback.ReadValueAsButton();
        }

        public void OnReloadInput(InputAction.CallbackContext inputCallback)
        {
            if (!InputIsActive()) return;

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

        private bool InputIsActive() => pInput.inputIsActive;
        #endregion
    }
}