using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Teleporter : MonoBehaviour
{
    [Header("Teleporter Settings")]
    [SerializeField] private LayerMask teleportableLayers;

    [Header("Teleport State")]
    [SerializeField] private bool teleportationEnabled;
    [SerializeField] private int playersInTeleporterRange = 0;

    private Coroutine teleportingCoroutine;

    [Header("Teleporter Parts")]
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private ParticleSystem warpTunnel;
    [SerializeField] private ParticleSystem secondaryEffect;

    public bool TeleportationEnabled
    {
        get => teleportationEnabled;
        set
        {
            teleportationEnabled = value;
            triggerCollider.enabled = value;

            if (value)
            {
                warpTunnel.Play();
                secondaryEffect?.Play();
            }
            else
            {
                warpTunnel.Stop();
                secondaryEffect?.Stop();
            }
        }
    }

    private void Start()
    {
        TeleportationEnabled = SceneManager.GetSceneByName("GameHub").isLoaded;

        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.layer == teleportableLayers || other.gameObject.CompareTag("Player")))
        {
            playersInTeleporterRange++;

            if (teleportingCoroutine == null)
            {
                teleportingCoroutine = StartCoroutine(Teleporting());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.gameObject.layer == teleportableLayers || other.gameObject.CompareTag("Player")))
        {
            playersInTeleporterRange--;

            if (teleportingCoroutine != null)
            {
                StopCoroutine(teleportingCoroutine);
                Debug.Log("Cancelled Telport Coroutine");
            }
        }
    }

    private IEnumerator Teleporting()
    {
        yield return new WaitForSeconds(3f);

        SceneLoader.instance.LoadScene(SceneManager.GetActiveScene().name == "GameHub" ? "GameplayScene" : "GameHub");
    }
}
