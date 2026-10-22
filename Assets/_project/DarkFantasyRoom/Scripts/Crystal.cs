using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Crystal : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool collected = false;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (collected) return;

        collected = true;

        GameManager.instance.CollectCrystal();

        Destroy(gameObject);
    }
}