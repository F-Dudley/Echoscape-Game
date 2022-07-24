using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class UserInput : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool aiming;
		public bool shooting;
		public bool reloading;

		public bool swapWeapon;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		#region Action Map Inputs
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnAim(InputValue value)
		{
			AimInput(value.isPressed);
		}

		public void OnFire(InputValue value)
		{
			FireInput(value.isPressed);
		}

		public void OnReload(InputValue value)
		{
			ReloadInput(value.isPressed);
		}

		public void OnSwapWeapon(InputValue value)
		{
			SwapWeaponInput(value.isPressed);
		}
		#endregion

		#region Input Functionality
		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void FireInput(bool newShootState)
		{
			shooting = newShootState;
		}

		public void ReloadInput(bool newReloadState)
		{
			reloading = newReloadState;
		}

		public void AimInput(bool newAimState)
		{
			aiming = newAimState;
		}

		public void SwapWeaponInput(bool newSwapState)
		{
			swapWeapon = newSwapState;
		}
		#endregion

		#region Cursor Functionality
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
		#endregion
	}
	
}