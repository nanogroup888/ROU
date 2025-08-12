using UnityEngine;
using Mirror;

public class CameraOrbitRMB : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public bool autoFindLocalPlayer = true;

    [Header("Orbit")]
    public float distance = 6f;
    public float minDistance = 2f;
    public float maxDistance = 50f;
    public float yaw = 0f;
    public float pitch = 20f;
    public float minPitch = -30f;
    public float maxPitch = 75f;

    [Header("Tuning")]
    public float rotateSensitivity = 2f;
    public float zoomSensitivity = 2f;
    public float followSmoothing = 0.15f;
    public LayerMask clipMask = ~0;

    Vector3 currentPosVel;
    bool rotating;

    void LateUpdate()
    {
        if (autoFindLocalPlayer && target == null)
            TryAutoBindLocalPlayer();

        if (!target) return;

        if (Input.GetMouseButtonDown(1)) { rotating = true; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        if (Input.GetMouseButtonUp(1))   { rotating = false; Cursor.lockState = CursorLockMode.None;  Cursor.visible = true;  }

        if (rotating)
        {
            yaw   += Input.GetAxis("Mouse X") * rotateSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * rotateSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
            distance = Mathf.Clamp(distance - scroll * zoomSensitivity * 5f, minDistance, maxDistance);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPos = target.position + rot * (Vector3.back * distance);

        if (Physics.Linecast(target.position, desiredPos, out RaycastHit hit, clipMask, QueryTriggerInteraction.Ignore))
            desiredPos = hit.point + hit.normal * 0.15f;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentPosVel, followSmoothing);
        transform.rotation = rot;
    }

    void TryAutoBindLocalPlayer()
    {
        if (NetworkClient.active && NetworkClient.localPlayer != null)
        {
            target = NetworkClient.localPlayer.transform;
            return;
        }

        // fallback: by tag
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var go in players)
        {
            var ni = go.GetComponent<NetworkIdentity>();
            if (ni != null && ni.isLocalPlayer)
            {
                target = ni.transform;
                return;
            }
        }
    }
}
