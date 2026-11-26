using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
#endif

[DefaultExecutionOrder(-125)]
public sealed class SimulatorPointerAim : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private XRInteractionSimulator simulator;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TrackedPoseDriver leftController;
    [SerializeField] private TrackedPoseDriver rightController;

    private Controller left;
    private Controller right;
    private readonly RaycastHit[] hits = new RaycastHit[16];

    private void OnEnable()
    {
        left = new Controller(leftController);
        right = new Controller(rightController);
    }

    private void OnDisable()
    {
        left?.Restore();
        right?.Restore();
    }

    private void Update()
    {
        var mouse = Mouse.current;
        bool aimLeft = simulator.manipulatingLeftController;
        bool aimRight = simulator.manipulatingRightController || simulator.manipulatingFPS;
        left.SetPointing(aimLeft);
        right.SetPointing(aimRight);

        if (mouse == null || (!aimLeft && !aimRight) || mouse.rightButton.isPressed)
            return;
        Vector2 cursor = mouse.position.ReadValue();
        if (!playerCamera.pixelRect.Contains(cursor))
            return;

        Ray ray = playerCamera.ScreenPointToRay(cursor);
        float distance = 4f;
        int count = Physics.RaycastNonAlloc(ray, hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            if (!(hits[i].collider is CharacterController))
                distance = Mathf.Min(distance, hits[i].distance);
        }

        Vector3 point = ray.GetPoint(distance);
        if (aimLeft) left.Aim(point);
        if (aimRight) right.Aim(point);
    }

    private sealed class Controller : IXRInputButtonReader
    {
        private readonly TrackedPoseDriver pose;
        private readonly TrackedPoseDriver.TrackingType tracking;
        private readonly XRInputButtonReader select;
        private readonly IXRInputButtonReader formerBypass;
        private readonly CurveInteractionCaster caster;
        private readonly bool stabilization;
        private bool pointing;

        public Controller(TrackedPoseDriver pose)
        {
            this.pose = pose;
            tracking = pose.trackingType;
            var interactor = pose.GetComponentInChildren<NearFarInteractor>(true);
            select = interactor.selectInput;
            formerBypass = select.bypass;
            select.bypass = this;
            caster = interactor.farInteractionCaster as CurveInteractionCaster;
            stabilization = caster != null && caster.enableStabilization;
        }

        public void SetPointing(bool active)
        {
            if (pointing == active)
                return;
            pointing = active;
            pose.trackingType = active ? TrackedPoseDriver.TrackingType.PositionOnly : tracking;
            if (caster != null)
                caster.enableStabilization = !active && stabilization;
        }

        public void Aim(Vector3 target)
        {
            Transform hand = pose.transform;
            Vector3 direction = target - hand.position;
            if (direction.sqrMagnitude > .0001f)
                hand.rotation = Quaternion.LookRotation(direction);
        }

        public void Restore()
        {
            SetPointing(false);
            select.bypass = formerBypass;
        }

        private IXRInputButtonReader Original => formerBypass ?? select;
        private bool MouseHeld => pointing && Mouse.current != null && Mouse.current.leftButton.isPressed;

        public bool ReadIsPerformed() => Original.ReadIsPerformed() || MouseHeld;

        public bool ReadWasPerformedThisFrame() => Original.ReadWasPerformedThisFrame() ||
            (pointing && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        public bool ReadWasCompletedThisFrame() => !ReadIsPerformed() &&
            (Original.ReadWasCompletedThisFrame() ||
                (pointing && Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame));

        public float ReadValue() => MouseHeld ? 1f : Original.ReadValue();

        public bool TryReadValue(out float value)
        {
            if (MouseHeld)
            {
                value = 1f;
                return true;
            }
            return Original.TryReadValue(out value);
        }
    }
#endif
}

