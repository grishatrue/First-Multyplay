using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private float sensitivity = 2;
    [SerializeField] private float topClamp = -80f;
    [SerializeField] private float bottomClamp = 80f;
    [SerializeField] private float distance = 15f;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private GameObject target;

    [SerializeField] private float zoomStep = 0.25f;
    [SerializeField] private float zoomMax = 10;
    [SerializeField] private float zoomMin = 5;

    [Tooltip("After this time, the object will be destroyed.")]
    [SerializeField] private float waitMissingTagetTime = 1f;
    private float rotX, rotY;
    private bool isPlayerMissing = false;

    public GameObject Target => target;

    private void Start()
    {
        CursorManager.SwitchCursorState(false);
    }

    private void LateUpdate()
    {
        Zoom();
        Move();

        if (!isPlayerMissing) StartCoroutine(OnPlayerMissing());
    }

    private void Zoom()
    {
        if (!CursorManager.isCursorVisibleStatic)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0) offset.z += zoomStep;
            else if (Input.GetAxis("Mouse ScrollWheel") < 0) offset.z -= zoomStep;

            offset.z = Mathf.Clamp(offset.z, -Mathf.Abs(zoomMax), -Mathf.Abs(zoomMin));
        }
    }

    private void Move()
    {
        if (target == null) return;

        if (!CursorManager.isCursorVisibleStatic)
        {
            rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
            rotY += Input.GetAxis("Mouse Y") * sensitivity;
        }

        rotY = Mathf.Clamp(rotY, topClamp, bottomClamp);

        transform.localEulerAngles = new Vector3(-rotY, rotX, 0);
        transform.position = transform.localRotation * offset + target.transform.position;
    }

    public bool TrySetTarget(GameObject newObj)
    {
        if (target != null) return false;

        target = newObj;
        return true;
    }

    private IEnumerator OnPlayerMissing()
    {
        if (target == null)
        {
            isPlayerMissing = true;
            float timer = waitMissingTagetTime;

            while (timer > 0)
            {
                if (target != null)
                {
                    isPlayerMissing = false;
                    yield break;
                }

                var tDelta = Time.deltaTime;
                timer -= tDelta;
                yield return new WaitForSeconds(tDelta);
            }

            Destroy(gameObject);
        }
    }
}
