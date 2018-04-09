using System;
using UnityEngine;
using Vuforia;

public class PlaceContentFromImageTarget : MonoBehaviour 
{
    //This class should replace the default ContentPositioningBehaviour in the PlaneFinder GameObject
    //Don't forget to set the OnInteractiveHitTest of the PlaneFinderBehaviour to call this class

    //IMPORTANT:
    // Be sure to enable Device Tracker and set mode to POSITIONAL in the Vuforia Configuration

    public AnchorStageBehaviour AnchorStage;
    public ImageTargetBehaviour ImageTarget;
    private PositionalDeviceTracker _deviceTracker;
    
    

    private bool _anchorSet;

    public void Start ()
    {
        if (AnchorStage == null)
        {
            Debug.LogWarning("AnchorStage must be specified");
            return;
        }

        if (ImageTarget == null)
        {
            Debug.LogWarning("Image Target must be specified");
            return;
        }

        AnchorStage.gameObject.SetActive(false);
       
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
 
    public void OnInteractiveHitTest(HitTestResult result)
    {
        if (_anchorSet)
        {
            //Leave if the anchor has already been set
            return;
        }

        if (result == null || AnchorStage == null)
        {
            Debug.LogWarning("Hit test is invalid or AnchorStage not set");
            return;
        }
        
        //Let's go ahead and create the anchor at the position of the hit test
        var anchor = _deviceTracker.CreatePlaneAnchor(Guid.NewGuid().ToString(), result);
        
        if (anchor != null)
        {
            AnchorStage.transform.parent = anchor.transform;
            AnchorStage.transform.localPosition = Vector3.zero;
            AnchorStage.transform.localRotation = Quaternion.identity;
            AnchorStage.gameObject.SetActive(true);

            //Make sure that the Image Target is currently tracking
            if (ImageTarget.CurrentStatus == TrackableBehaviour.Status.DETECTED ||
                ImageTarget.CurrentStatus == TrackableBehaviour.Status.TRACKED ||
                ImageTarget.CurrentStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
                

              
                //Let's move the contents of the image target onto the ground plane
                for (var i = 0; i < ImageTarget.transform.childCount; i++)
                {
                    var content = ImageTarget.transform.GetChild(i);
                    content.parent = AnchorStage.transform;
                }
            }

            _anchorSet = true;

        }
    }
}
