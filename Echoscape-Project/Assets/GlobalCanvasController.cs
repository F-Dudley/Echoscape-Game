using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalCanvasController : MonoBehaviour
{
    public static GlobalCanvasController instance;

    [Header("UI Components")]
    [SerializeField] private Transform menusHolder;
    [SerializeField] private GameObject menuBackground;

    [SerializeField] private List<GameObject> canvasObjects;

    #region Unity Functions
    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Canvas Behaviour
    public void AddMenu(ref GameObject menuPrefab)
    {
        menuPrefab = Instantiate(menuPrefab, menusHolder);

        canvasObjects.Add(menuPrefab);
        menuBackground.SetActive(true);
    }

    public void RemoveMenu(ref GameObject menuObject)
    {
        if (canvasObjects.Contains(menuObject))
        {
            canvasObjects.Remove(menuObject);
            menuBackground.SetActive(canvasObjects.Count > 0);
        }
    }
    #endregion
}
