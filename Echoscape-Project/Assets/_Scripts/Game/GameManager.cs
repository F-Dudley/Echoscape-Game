using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject.transform.root.gameObject);

        GameObject.DontDestroyOnLoad(this.transform.root.gameObject);
    }

    private void Update()
    {
        
    }
}
