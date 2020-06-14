﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public sealed class Interactable : MonoBehaviour
{
    public InteractedEvent Interacted;

#if UNITY_EDITOR
    private new CapsuleCollider collider;
#endif

    [SerializeField, Tooltip("Text displayed on InteractionWheel.")]
    private string context = string.Empty;

    [SerializeField, Tooltip("Icon displayed on InteractionWheel button.")]
    private UnityEngine.Sprite icon;

    public string Context => context;

    public UnityEngine.Sprite Icon
    {
        get { return icon; }
        set { icon = value; }
    }

    public void Interact(GameObject player)
    {
        Interacted.Invoke(player);
    }

#if UNITY_EDITOR

    // TODO: Find a better way to add this listener. It currently has to wait for Interacted to initialize.
    private IEnumerator AddListener(IInteractable interactable)
    {
        while (Interacted == null)
            yield return null;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(Interacted, interactable.OnInteracted);
    }

    private CapsuleCollider FindOrCreateCollider()
    {
        var found = transform.Find("Interactable");
        if (found)
            return found.GetComponent<CapsuleCollider>();

        var interactable = new GameObject("Interactable")
        {
            layer = LayerMask.NameToLayer(Layer.Interactable)
        };

        interactable.transform.parent = transform;
        interactable.transform.localPosition = new Vector3();

        var collider = interactable.AddComponent<CapsuleCollider>();
        collider.radius = 7;
        collider.isTrigger = true;

        return collider;
    }

    private void OnDestroy()
    {
        if (collider)
            DestroyImmediate(collider.gameObject);
    }

    private void Reset()
    {
        collider = FindOrCreateCollider();

        if (string.IsNullOrEmpty(Context))
            context = name;

        var interactable = GetComponent<IInteractable>();
        Icon = Resources.Load<UnityEngine.Sprite>(interactable.DefaultIconPath);
        StartCoroutine(AddListener(interactable));
    }

#endif

    [Serializable]
    public class InteractedEvent : UnityEvent<GameObject> { }
}