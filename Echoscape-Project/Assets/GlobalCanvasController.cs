using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalCanvasController : MonoBehaviour
{
    public static GlobalCanvasController instance;

    [Header("UI Components")]
    [SerializeField] private Transform menusHolder;
    [SerializeField] private GameObject menuBackground;

    [SerializeField] private Stack<GameObject> canvasObjects = new Stack<GameObject>();

    #region Unity Functions
    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Stack Behaviour
    private void AddToStack(ref GameObject menuItem)
    {

        if (canvasObjects.Count > 0)
        {
            canvasObjects.Peek().SetActive(false);
        }

        canvasObjects.Push(menuItem);
        menuBackground.SetActive(true);
    }

    private void RemoveFromStack(ref GameObject menuItem)
    {
        if (canvasObjects.Peek() != menuItem) return;

        canvasObjects.Pop();

        bool isEmpty = canvasObjects.Count == 0;
        menuBackground.SetActive(!isEmpty);

        if (!isEmpty)
        {
            GameObject newStackTop = canvasObjects.Peek();

            newStackTop.SetActive(true);
            if (newStackTop.TryGetComponent<GameMenu>(out GameMenu gameMenu))
            {
                gameMenu.ActivateMenu();
            }
        }
    }
    #endregion

    #region Canvas Behaviour
    public void AddMenu(ref GameObject menuPrefab)
    {
        menuPrefab = Instantiate(menuPrefab, menusHolder);

        AddToStack(ref menuPrefab);
        menuPrefab.GetComponent<GameMenu>().ActivateMenu();
    }

    public void AddInstantiatedMenu(ref GameObject menuObject)
    {
        AddToStack(ref menuObject);
        menuObject.GetComponent<GameMenu>().ActivateMenu();
    }

    public void RemoveMenu(ref GameObject menuObject)
    {
        RemoveFromStack(ref menuObject);
    }
    #endregion
}
