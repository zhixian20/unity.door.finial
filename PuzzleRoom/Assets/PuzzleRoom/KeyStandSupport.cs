using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public sealed class KeyStandSupport : MonoBehaviour
{
    private Rigidbody body;
    private XRGrabInteractable grab;
    private RigidbodyConstraints releasedConstraints;
    private bool onStand;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        releasedConstraints = body.constraints;
        body.constraints = RigidbodyConstraints.FreezeAll;
        onStand = true;
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(ReleaseFromStand);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(ReleaseFromStand);
    }

    private void ReleaseFromStand(SelectEnterEventArgs args)
    {
        if (!onStand)
            return;

        onStand = false;
        body.constraints = releasedConstraints;
    }
}

