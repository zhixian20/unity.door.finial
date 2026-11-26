using UnityEngine;

[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(CharacterController))]
public sealed class SimulatorBodyCollision : MonoBehaviour
{
    [SerializeField] private Transform trackedCamera;
    [SerializeField] private GameObject simulator;

    private CharacterController body;
    private Vector3 previousTrackingPosition;

    private void OnEnable()
    {
        body = GetComponent<CharacterController>();
        if (trackedCamera != null)
            previousTrackingPosition = trackedCamera.localPosition;
    }

    private void LateUpdate()
    {
        if (trackedCamera == null || simulator == null || !simulator.activeInHierarchy || !body.enabled)
            return;

        Vector3 trackingPosition = trackedCamera.localPosition;
        Vector3 delta = trackedCamera.parent.TransformVector(trackingPosition - previousTrackingPosition);
        delta = Vector3.ProjectOnPlane(delta, transform.up);

        // The simulator already moved the camera in tracking space. Sweep the
        // player capsule from its old position, then cancel only blocked motion.
        Vector3 oldHead = trackedCamera.parent.TransformPoint(previousTrackingPosition);
        Vector3 center = body.center;
        Vector3 localHead = transform.InverseTransformPoint(oldHead);
        center.x = localHead.x;
        center.z = localHead.z;
        body.center = center;
        previousTrackingPosition = trackingPosition;

        if (delta.sqrMagnitude > 0.00000001f)
        {
            Vector3 start = transform.position;
            body.Move(delta);
            Vector3 allowed = Vector3.ProjectOnPlane(transform.position - start, transform.up);
            transform.position = start + allowed - delta;
        }

        localHead = transform.InverseTransformPoint(trackedCamera.position);
        center.x = localHead.x;
        center.z = localHead.z;
        body.center = center;
    }
}
