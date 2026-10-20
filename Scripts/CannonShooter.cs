using UnityEngine;
using UnityEngine.InputSystem;

public class CannonShooter : MonoBehaviour
{
    [Header("References")]
    public Rigidbody projectilePrefab;
    public Transform muzzle;
    public Transform head;

    [Header("Launch")]
    public float launchSpeed = 15f;
    public float aimSpeed = 40f;
    public float fireCooldown = 0.3f;
    public float projectileLifetime = 6f;

    [Header("Trajectory Preview")]
    public LineRenderer line;
    public int previewPoints = 40;
    public float timeStep = 0.1f;

    float nextFireTime;
    float pitch;
    float yaw;

    void Start()
    {
        Vector3 e = head.localEulerAngles;
        pitch = e.x; yaw = e.y;
    }

    void Update()
    {
        HandleAiming();
        DrawTrajectory();

        bool fire = Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Mouse.current.leftButton.wasPressedThisFrame;

        if (fire && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void HandleAiming()
    {
        var kb = Keyboard.current;
        float dt = Time.deltaTime * aimSpeed;

        if (kb.upArrowKey.isPressed || kb.wKey.isPressed)    pitch -= dt;
        if (kb.downArrowKey.isPressed || kb.sKey.isPressed)  pitch += dt;
        if (kb.leftArrowKey.isPressed || kb.aKey.isPressed)  yaw -= dt;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) yaw += dt;

        if (kb.qKey.isPressed) launchSpeed = Mathf.Max(1f, launchSpeed - 10f * Time.deltaTime);
        if (kb.eKey.isPressed) launchSpeed += 10f * Time.deltaTime;

        head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void Shoot()
    {
        Rigidbody rb = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
        rb.linearVelocity = muzzle.forward * launchSpeed;
        Destroy(rb.gameObject, projectileLifetime);
    }

    void DrawTrajectory()
    {
        if (line == null) return;

        Vector3 p0 = muzzle.position;
        Vector3 v0 = muzzle.forward * launchSpeed;
        Vector3 g = Physics.gravity;

        line.positionCount = previewPoints;
        for (int i = 0; i < previewPoints; i++)
        {
            float t = i * timeStep;
            line.SetPosition(i, p0 + v0 * t + 0.5f * g * t * t);
        }
    }

    void OnGUI()
    {
        Vector3 v0 = muzzle.forward * launchSpeed;
        float g = -Physics.gravity.y;
        float vy = v0.y;
        float vh = new Vector2(v0.x, v0.z).magnitude;

        float flightTime = (vy + Mathf.Sqrt(vy * vy + 2f * g * muzzle.position.y)) / g;
        float range = vh * flightTime;
        float maxHeight = muzzle.position.y + (vy * vy) / (2f * g);
        float angle = Mathf.Atan2(vy, vh) * Mathf.Rad2Deg;

        GUI.Label(new Rect(10, 10, 400, 100),
            $"Speed: {launchSpeed:F1} m/s\nAngle: {angle:F1}°\n" +
            $"Time of flight: {flightTime:F2} s\nRange: {range:F1} m\nMax height: {maxHeight:F1} m");
    }
}