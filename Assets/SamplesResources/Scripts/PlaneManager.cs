/*===============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class PlaneManager : MonoBehaviour, ITrackableEventHandler
{
    private enum PlaneMode
    {
        GROUND
    }

    #region PUBLIC_MEMBERS
    public PlaneFinderBehaviour m_PlaneFinder;
    public TrackableBehaviour imageTrackable;
    public GameObject m_PlaneAugmentation;
    //public GameObject m_GroundPlane;
 //   public UnityEngine.UI.Button m_ResetButton;
   // public CanvasGroup m_GroundReticle;
    public GameObject cube;
    public float angoloX;
    public float angoloZ;
    public float angoloY;
  
    #endregion // PUBLIC_MEMBERS


    #region PRIVATE_MEMBERS
    PositionalDeviceTracker positionalDeviceTracker;
    ContentPositioningBehaviour contentPositioningBehaviour;
    GameObject m_PlaneAnchor;
    AnchorBehaviour[] anchorBehaviours;
    StateManager stateManager;
    Camera mainCamera;
   // int AutomaticHitTestFrameCount;
    PlaneMode planeMode = PlaneMode.GROUND;
    const string TITLE_GROUNDPLANE = "Ground Plane";
    bool primo=false;
    
    #endregion // PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    void Start()
    {
        Debug.Log("Start() called.");
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnVuforiaPaused);
        DeviceTrackerARController.Instance.RegisterTrackerStartedCallback(OnTrackerStarted);
       DeviceTrackerARController.Instance.RegisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
        m_PlaneFinder.HitTestMode = HitTestMode.AUTOMATIC;
      // m_ResetButton.interactable = false;
        mainCamera = Camera.main;
    }

   
    void OnDestroy()
    {
        Debug.Log("OnDestroy() called.");

        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.UnregisterOnPauseCallback(OnVuforiaPaused);
        DeviceTrackerARController.Instance.UnregisterTrackerStartedCallback(OnTrackerStarted);
        DeviceTrackerARController.Instance.UnregisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
    }

    #endregion // MONOBEHAVIOUR_METHODS

    #region VUFORIA_CALLBACKS

    void OnVuforiaStarted()
    {
        Debug.Log("OnVuforiaStarted() called.");

      imageTrackable.RegisterTrackableEventHandler(this);
      stateManager = TrackerManager.Instance.GetStateManager();
      
       
    }

    void OnVuforiaPaused(bool paused)
    {
        Debug.Log("OnVuforiaPaused(" + paused.ToString() + ") called.");
    }

    #endregion // VUFORIA_CALLBACKS


    #region PRIVATE_METHODS


    private void DestroyAnchors()
    {
        IEnumerable<TrackableBehaviour> trackableBehaviours = stateManager.GetActiveTrackableBehaviours();

        string destroyed = "Destroying: ";

        foreach (TrackableBehaviour behaviour in trackableBehaviours)
        {
            if (behaviour is AnchorBehaviour)
            {
                // First determine which mode (Plane or MidAir) and then delete only the anchors for that mode
                // Leave the other mode's anchors intact
                // PlaneAnchor_<GUID>
                // Mid AirAnchor_<GUID>

                if ((behaviour.Trackable.Name.Contains("PlaneAnchor") && planeMode == PlaneMode.GROUND)
                   )
                {
                    destroyed +=
                        "\nGObj Name: " + behaviour.name +
                       "\nTrackable Name: " + behaviour.Trackable.Name +
                       "\nTrackable ID: " + behaviour.Trackable.ID +
                       "\nPosition: " + behaviour.transform.position.ToString();

                    stateManager.DestroyTrackableBehavioursForTrackable(behaviour.Trackable);
                    stateManager.ReassociateTrackables();
                }
            }
        }

        Debug.Log(destroyed);
    }

   
    private void RotateTowardImageTarget(GameObject augmentation)
    {
        //var lookAtPosition = cube.transform.position;
        //lookAtPosition.y = 0;

        var rotation = cube.transform.rotation;
   
        Debug.Log("rotateToward");
        rotation.y +=angoloY;
        rotation.x += angoloX;
        rotation.z+=angoloZ;
        rotation.x = 0;
        rotation.z = 0;

        augmentation.transform.rotation = rotation;

    }

    #endregion // PRIVATE_METHODS

    #region PUBLIC_METHODS
    public void HandleAutomaticHitTest(HitTestResult result)
    {
        if (primo) return;
        Debug.Log("Result: " + result.Position);

       // AutomaticHitTestFrameCount = Time.frameCount;

       
    }

    public void HandleInteractiveHitTest(HitTestResult result)
    {
        Debug.Log("HandleInteractiveHitTest() called.");
        

        if (result == null && primo)
        {
            Debug.LogError("Invalid hit test result!");
            return;
        }

        // Place object based on Ground Plane mode
        switch (planeMode)
        {  
           case PlaneMode.GROUND:
              //  Debug.Log("case PlaneGround");
                if (positionalDeviceTracker != null && positionalDeviceTracker.IsActive)
                {
                    
                    DestroyAnchors();

                    contentPositioningBehaviour = m_PlaneFinder.GetComponent<ContentPositioningBehaviour>();
                    contentPositioningBehaviour.PositionContentAtPlaneAnchor(result);

                   

                   
                   // m_ResetButton.interactable = true;
                    

                }

                if (!m_PlaneAugmentation.activeInHierarchy && !primo)
                {
                    Debug.Log("Setting Plane Augmentation to Active");
                    // On initial run, unhide the augmentation
                    
                    m_PlaneAugmentation.SetActive(true);
            primo = true;
                }
                

                Debug.Log("Positioning Plane Augmentation at: " + cube.transform.position);
                
                    m_PlaneAugmentation.PositionAt(cube.transform.position);
                    RotateTowardImageTarget(m_PlaneAugmentation);
                

               break;

           default:
                Debug.LogError("Invalid Ground Plane state: " + planeMode);
               break;
        }
    }



   

    public void SetGroundMode(bool active)
    {
        if (active)
        {
            planeMode = PlaneMode.GROUND;
            m_PlaneFinder.gameObject.SetActive(true);
            //m_GroundPlane.gameObject.SetActive(true);
            
          
        }
    }

  

    public void Reset()
    {
       

        // reset augmentations
        m_PlaneAugmentation.transform.position = Vector3.zero;
        m_PlaneAugmentation.transform.localEulerAngles = Vector3.zero;
        m_PlaneAugmentation.SetActive(false);
       
       // m_ResetButton.interactable = false;
        primo = false;
     
    }

    #endregion // PUBLIC_METHODS

    #region IMAGE_TRACKER_CALLBACKS
    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {

        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {

            


            if (!primo)
            {
                
                //if (!m_ResetButton) m_ResetButton.interactable = true;
                Vector2 screenPoint;
              
                screenPoint = mainCamera.WorldToScreenPoint(cube.transform.position);
                Debug.Log(screenPoint);

                m_PlaneFinder.PerformHitTest(screenPoint);
                
                cube.SetActive(false);
                Debug.Log("Trackable found first time");
                primo = true;

            }
        }
    }
    #endregion // IMAGE_TRACKER_CALLBACKS

    #region DEVICE_TRACKER_CALLBACKS

    void OnTrackerStarted()
    {
        Debug.Log("OnTrackerStarted() called.");

        positionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

        if (positionalDeviceTracker != null)
        { 
            if (!positionalDeviceTracker.IsActive)
                positionalDeviceTracker.Start();

            Debug.Log("PositionalDeviceTracker is Active?: " + positionalDeviceTracker.IsActive);
        }
    }

    void OnDevicePoseStatusChanged(TrackableBehaviour.Status status)
    {
        Debug.Log("OnDevicePoseStatusChanged(" + status.ToString() + ")");
    }

    #endregion // DEVICE_TRACKER_CALLBACK_METHODS
}
