using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityCentre : MonoBehaviour
{
    private void Awake()
    {
        GameManager.instance.SetGravityCentre(this.transform);
    }

    private void OnDestroy()
    {
        GameManager.instance.SetGravityCentre(null);
    }
}
