using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Color colorGizmo = new Color(0.5f, 1f, 0f, 0.5f);

    private void OnDrawGizmos()
    {
        BoxCollider _bc = GetComponent<BoxCollider>();
        Gizmos.color = colorGizmo;
        Gizmos.DrawCube(transform.position, _bc.size);
    }
}
