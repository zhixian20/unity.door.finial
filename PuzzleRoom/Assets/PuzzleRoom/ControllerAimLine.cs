using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[RequireComponent(typeof(LineRenderer))]
public sealed class ControllerAimLine : MonoBehaviour
{
    [SerializeField] private NearFarInteractor interactor;
    [SerializeField, Min(0.1f)] private float idleDistance = 4f;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    private void OnDisable()
    {
        if (line != null)
            line.enabled = false;
    }

    private void LateUpdate()
    {
        if (interactor == null || !interactor.isActiveAndEnabled || interactor.farInteractionCaster == null)
        {
            line.enabled = false;
            return;
        }

        Transform origin = interactor.curveOrigin;
        if (origin == null || !IsFinite(origin.position) || !IsFinite(origin.forward))
        {
            line.enabled = false;
            return;
        }

        Vector3 start = origin.position;
        var hit = interactor.TryGetCurveEndPoint(out Vector3 end);
        if (hit == EndPointType.None || !IsFinite(end) || (end - start).sqrMagnitude < 0.000025f)
            end = start + origin.forward * idleDistance;

        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.enabled = true;
    }

    private static bool IsFinite(Vector3 point) =>
        float.IsFinite(point.x) && float.IsFinite(point.y) && float.IsFinite(point.z);
}
