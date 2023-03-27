using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOpener : MonoBehaviour
{
    [SerializeField] private GameObject menuObject;

    private void Start()
    {
        GlobalCanvasController.instance.AddMenu(ref menuObject);
    }

    private void OnDestroy()
    {
        GlobalCanvasController.instance.RemoveMenu(ref menuObject);
        Destroy(menuObject);
    }
}
