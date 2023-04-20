using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

[RequireComponent(typeof(NetworkIdentity))]
public class PlayerCameraSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cameraPrefab;
    private GameObject playerCamera;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Instantiate(cameraPrefab);
            playerCamera.GetComponent<CameraControl>().TrySetTarget(gameObject);
        }
    }

    public bool TryGetCameraObj(out GameObject cameraObj)
    {
        cameraObj = null;
        if (playerCamera == null) return false;
        cameraObj = playerCamera;
        return true;
    }
}