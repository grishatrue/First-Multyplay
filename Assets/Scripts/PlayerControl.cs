using Mirror;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(PlayerCameraSpawner))]
public class PlayerControl : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [Tooltip("100f ~ 1 sec if there is no friction force.")]
    [SerializeField] private float acceleration = 25f;
    [Tooltip("100f ~ 1 sec if there is no friction force.")]
    [SerializeField] private float accelerationInFly = 50f;
    [Tooltip("How fast the player will turn in the movement direction.")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private SphereCollider groundTrigger;

    [Header("Dash")]
    [Tooltip("Set it to false so that the character moves in the direction of HIS gaze.")]
    [SerializeField] private bool isDashToDesireDirection = true;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashTime = 0.5f;
    [SerializeField] private float stayTimeAfterDash = 0f;
    [SerializeField] private AnimationCurve dashVelocityCurve;

    private Vector3 desiredDirection = Vector3.zero;
    private Vector3 dashNotDesireDirection = Vector3.zero;
    private Rigidbody rb;
    private GameObject playerCamera;
    private PlayerCameraSpawner plCamManager;
    private bool isDashing = false;

    public bool IsDashing { get => isDashing; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        plCamManager = GetComponent<PlayerCameraSpawner>();
    }

    private void Update()
    {
        if (!isOwned) return;

        if (!CursorManager.isCursorVisibleStatic)
        {
            if (!isDashing)
            {
                RotateToDesireDirection();
            }
            else
            {
                StartCoroutine(RotateToMoveDirection(!isDashing));
            }

            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(Dash());
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isOwned) return;

        if (!isDashing)
        {
            Move();
        }
    }

    private void Move()
    {
        Vector3 direction = Vector3.zero;

        if (!CursorManager.isCursorVisibleStatic)
        {
            direction = GetInpDirectionNormalized();
        }

        Vector3 directedSpeed = direction * speed;
        //rb.velocity = new Vector3(directedSpeed.x, rb.velocity.y, directedSpeed.z); // Use to no acceleration movement
        
        Vector3 desiredSpeed = new Vector3(directedSpeed.x, rb.velocity.y, directedSpeed.z);
        float currAcc = OnGround() ? acceleration : accelerationInFly;
        float t = currAcc * Time.deltaTime / (desiredSpeed - rb.velocity).magnitude;
        var currentSpeed = Vector3.Lerp(rb.velocity, desiredSpeed, t);

        rb.velocity = currentSpeed;
    }

    private Vector3 GetInpDirectionNormalized()
    {
        if (!plCamManager.TryGetCameraObj(out playerCamera)) return Vector3.zero;

        float hInp = Input.GetAxisRaw("Horizontal");
        float vInp = Input.GetAxisRaw("Vertical");
        //Vector3 inpDirection = new Vector3(hInp, 0f, vInp).normalized; // Use to global coord system control

        Vector3 hVec = playerCamera.transform.right * hInp;
        Vector3 vVec = playerCamera.transform.forward * vInp;
        vVec.y = 0f;
        vVec.Normalize();
        Vector3 inpDirection = (hVec + vVec).normalized;

        return inpDirection;
    }

    private bool OnGround()
    {
        var collisions = Physics.OverlapSphere(groundTrigger.transform.position, groundTrigger.radius);

        for (int i = 0; i < collisions.Length; i++)
        {
            if (!collisions[i].CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }

    private void RotateToDesireDirection()
    {
        desiredDirection = GetInpDirectionNormalized();

        if (desiredDirection == Vector3.zero) return;

        Quaternion rotation = Quaternion.LookRotation(desiredDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator RotateToMoveDirection(bool doBreak)
    {
        Vector3 pos1 = Vector3.zero;
        Vector3 pos2 = Vector3.zero;

        pos1 = transform.position;

        yield return new WaitForFixedUpdate();

        if (doBreak) yield break;
        pos2 = transform.position;

        dashNotDesireDirection = (pos2 - pos1).normalized;
        if (dashNotDesireDirection == Vector3.zero) yield break;

        Quaternion rotation = Quaternion.LookRotation(dashNotDesireDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator Dash()
    {
        if (isDashing) yield break;

        var currSpeed = rb.velocity;
        Vector3 dashDir = transform.forward;

        if (isDashToDesireDirection) dashDir = GetInpDirectionNormalized();

        if (dashDir == Vector3.zero)
        {
            if (!plCamManager.TryGetCameraObj(out playerCamera)) yield break;
            var _ = playerCamera.transform.forward;
            dashDir = new Vector3(_.x, 0f, _.z).normalized;
        }

        isDashing = true;
        float timer = dashTime;

        while (true)
        {
            if (timer <= 0) break;

            var newSpd = dashDir * dashSpeed * dashVelocityCurve.Evaluate(dashTime - timer);
            rb.velocity = new Vector3(newSpd.x, currSpeed.y, newSpd.z);
            var tDelta = Time.deltaTime;
            timer -= tDelta;
            yield return new WaitForSeconds(tDelta);
        }

        if (stayTimeAfterDash != 0) yield return new WaitForSeconds(stayTimeAfterDash);
        isDashing = false;
    }
}
