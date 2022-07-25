using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;

    #region Menu Functions
    public void ActivateMenu()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }
    #endregion
}
