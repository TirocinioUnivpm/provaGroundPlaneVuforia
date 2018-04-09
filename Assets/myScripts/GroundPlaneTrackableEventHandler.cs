using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using System;

public class GroundPlaneTrackableEventHandler : DefaultTrackableEventHandler
{
    public GameObject AnchorStage;
    private PositionalDeviceTracker _deviceTracker;
    private GameObject _previousAnchor;

    private List<GameObject> trackedObjects = new List<GameObject>();
    private GameObject trackingParent;

    AnchorInputListenerBehaviour anchorInput;

    protected override void Start()
    {
        base.Start();

        if (AnchorStage == null)
        {
            Debug.Log("AnchorStage must be specified");
            return;
        }
        OnTrackingLost();
    }

    public void Awake()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
    }

    public void OnDestroy()
    {
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);
    }

    private void OnVuforiaStarted()
    {
        _deviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
    }
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();

        if (trackingParent == null)
        {
            trackingParent = new GameObject("trackingParent");
            trackingParent.transform.SetParent(transform, false);
        }

        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                trackedObjects.Add(transform.GetChild(i).gameObject);
            }

            foreach (GameObject trackedObj in trackedObjects)
            {
                trackedObj.transform.SetParent(trackingParent.transform, true);
            }

            trackingParent.transform.SetParent(transform.parent, true);
        }

        trackingParent.transform.position = transform.position;
        trackingParent.transform.rotation = transform.rotation;



    }

    protected override void OnTrackingLost()
    {
        if (_previousAnchor != null)
        {
            return;
        }
        base.OnTrackingLost();
    }

    public void OnInteractiveHitTest(HitTestResult result)
    {
        if (_previousAnchor != null)
        {
            return;
        }
        if (result == null || AnchorStage == null)
        {
            Debug.LogWarning("Hit test is invalid or AnchorStage not set");
            return;
        }

        var anchor = _deviceTracker.CreatePlaneAnchor(Guid.NewGuid().ToString(), result);

        if (anchor != null)
        {
            AnchorStage.transform.parent = anchor.transform;
            AnchorStage.transform.localPosition = Vector3.zero;
            AnchorStage.transform.localRotation = Quaternion.identity;
            AnchorStage.SetActive(true);
        }

        if (_previousAnchor != null)
        {
            Destroy(_previousAnchor);
        }

        _previousAnchor = anchor;
    }
}