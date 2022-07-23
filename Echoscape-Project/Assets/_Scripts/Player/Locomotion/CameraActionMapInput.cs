using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using CMF;

namespace Player
{
    public class CameraActionMapInput : CameraInput
    {
        [SerializeField] private Vector2 lookInput;
        
        
        #region Input Callbacks
        public void OnLookInput(InputAction.CallbackContext inputCallback)
        {
            lookInput = inputCallback.ReadValue<Vector2>();
        }
        #endregion

        public override float GetHorizontalCameraInput()
        {
            return lookInput.x;
        }

        public override float GetVerticalCameraInput()
        {
            return lookInput.y;
        }
    }    
}