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
        GROUND//,
      //  MIDAIR
    }

    #region PUBLIC_MEMBERS
    public PlaneFinderBehaviour m_PlaneFinder;
    // public MidAirPositionerBehaviour m_MidAirPositioner;
    public TrackableBehaviour imageTrackable;
    public GameObject m_PlaneAugmentation;// m_MidAirAugmentation;
   // public UnityEngine.UI.Text m_TitleMode;
    public UnityEngine.UI.Text m_OnScreenMessage;
    public UnityEngine.UI.Image m_PlaneModeIcon;
   // public UnityEngine.UI.Toggle m_GroundToggle;// m_MidAirToggle;
    public UnityEngine.UI.Button m_ResetButton;
    public CanvasGroup m_GroundReticle;
    public GameObject cube;
    //public GameObject Buttonreset;
    #endregion // PUBLIC_MEMBERS


    #region PRIVATE_MEMBERS
    PositionalDeviceTracker positionalDeviceTracker;
    ContentPositioningBehaviour contentPositioningBehaviour;
    GameObject m_PlaneAnchor;// m_MidAirAnchor;
    AnchorBehaviour[] anchorBehaviours;
    StateManager stateManager;
    Camera mainCamera;
   // int AutomaticHitTestFrameCount;
    PlaneMode planeMode = PlaneMode.GROUND;
    Sprite m_IconGroundMode;
    //Sprite m_IconMidAirMode;
    const string TITLE_GROUNDPLANE = "Ground Plane";
    //const string TITLE_MIDAIR = "Mid-Air";
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
        m_PlaneFinder.enabled = false;

       // m_TitleMode.text = TITLE_GROUNDPLANE;
        m_PlaneFinder.HitTestMode = HitTestMode.INTERACTIVE;

      //  m_GroundToggle.interactable = false;
       // m_MidAirToggle.interactable = false;
       m_ResetButton.interactable = false;

        m_IconGroundMode = Resources.Load<Sprite>("icon_ground_mode");
        // m_IconMidAirMode = Resources.Load<Sprite>("icon_midair_mode");

        

        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        /*if (0 == Time.frameCount)
        {
            // We got an automatic hit test this frame
            m_GroundReticle.alpha = 0; // hide the onscreen reticle
            m_OnScreenMessage.enabled = false; // hide the onscreen message
            SetSurfaceIndicatorVisible(true); // display the surface indicator
        }
        else*/
        //   
        if (0 == Time.frameCount) { 
            m_GroundReticle.alpha = 0; // hide the onscreen reticle
        SetSurfaceIndicatorVisible(true);}// display the surface indicator
        else{
                                          // No automatic hit test, so set alpha based on which plane mode is active
        m_GroundReticle.alpha = (planeMode == PlaneMode.GROUND) ? 1 : 0;

            if (planeMode == PlaneMode.GROUND)
            {
                m_OnScreenMessage.enabled = true;
                m_OnScreenMessage.text = "Punta il dispositivo verso il piano";
            }
            /*if (planeMode == PlaneMode.MIDAIR)
            {
                m_OnScreenMessage.enabled = true;
                m_OnScreenMessage.text = "Touch anywhere on screen";
            }*/
           // SetSurfaceIndicatorVisible(false);
       }
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

        stateManager = TrackerManager.Instance.GetStateManager();
      imageTrackable.RegisterTrackableEventHandler(this);
    }

    void OnVuforiaPaused(bool paused)
    {
        Debug.Log("OnVuforiaPaused(" + paused.ToString() + ") called.");

        //if (paused)
         //   Reset();
    }

    #endregion // VUFORIA_CALLBACKS


    #region PRIVATE_METHODS

    void SetSurfaceIndicatorVisible(bool isVisible)
    {
        Renderer[] renderers = m_PlaneFinder.PlaneIndicator.GetComponentsInChildren<Renderer>(true);
        Canvas[] canvas = m_PlaneFinder.PlaneIndicator.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas c in canvas)
            c.enabled = isVisible;

        foreach (Renderer r in renderers)
            r.enabled = isVisible;
    }

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

                if ((behaviour.Trackable.Name.Contains("PlaneAnchor") && planeMode == PlaneMode.GROUND) //||
                   // (behaviour.Trackable.Name.Contains("Mid AirAnchor") && planeMode == PlaneMode.MIDAIR)
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

    private void RotateTowardCamera(GameObject augmentation)
    {
        var lookAtPosition = mainCamera.transform.position - augmentation.transform.position;
        lookAtPosition.y = 0;
        var rotation = Quaternion.LookRotation(lookAtPosition);
        augmentation.transform.rotation = rotation;
    }

    private void RotateTowardImageTarget(GameObject augmentation)
    {
        var lookAtPosition = cube.transform.position;// - augmentation.transform.position;
        //lookAtPosition.y = 0;

        //var lookAtPositionTarget = imageTarget.transform.position;
        //var rotation = Quaternion.LookRotation(lookAtPosition);
        Debug.Log("rotateToward");
        var rotation = cube.transform.rotation;
        rotation.x = 0;
        rotation.z = 0;
        //rotation.y =180;

        augmentation.transform.rotation = rotation;
    }

    #endregion // PRIVATE_METHODS

    #region PUBLIC_METHODS

    /*public void HandleAutomaticHitTest(HitTestResult result)
    {
        Debug.Log("Result: " + result.Position);

        AutomaticHitTestFrameCount = Time.frameCount;

        if (!m_GroundToggle.interactable)
        {
            // Runs only once after first successful Automatic hit test
            m_GroundToggle.interactable = true;
            m_GroundToggle.isOn = true;
        }
    }
    */
    public void HandleInteractiveHitTest(HitTestResult result)
    {
        Debug.Log("HandleInteractiveHitTest() called.");
        

        if (result == null)
        {
            Debug.LogError("Invalid hit test result!");
            return;
        }

        // Place object based on Ground Plane mode
        switch (planeMode)
        {  
            case PlaneMode.GROUND:
                Debug.Log("case PlaneGround");
                if (positionalDeviceTracker != null && positionalDeviceTracker.IsActive)
                {
                    
                    DestroyAnchors();

                    contentPositioningBehaviour = m_PlaneFinder.GetComponent<ContentPositioningBehaviour>();
                    contentPositioningBehaviour.PositionContentAtPlaneAnchor(result);
                    // Debug.Log("mannaggiasanda");

                    /*if (!m_MidAirToggle.interactable)
                    {
                        // Runs only once after first successful Ground Anchor is created
                        m_MidAirToggle.interactable = true;
                        m_ResetButton.interactable = true;
                    }*/
                    m_ResetButton.interactable = true;
                    //Buttonreset.SetActive(true);

                }

                if (!m_PlaneAugmentation.activeInHierarchy)
                {
                    //Debug.Log("Setting Plane Augmentation to Active");
                    // On initial run, unhide the augmentation
                    //m_PlaneAugmentation.transform.localPosition = Vector3.zero;
                    //m_PlaneAugmentation.transform.rotation = Quaternion.identity;
                    m_PlaneAugmentation.SetActive(true);
                }
                

                Debug.Log("Positioning Plane Augmentation at: " + cube.transform.position);
                m_PlaneAugmentation.PositionAt(cube.transform.position);
                RotateTowardImageTarget(m_PlaneAugmentation);

                break;

            default:
               // Debug.LogError("Invalid Ground Plane state: " + planeMode);
                break;
        }
    }



    /*public void PlaceObjectInMidAir(Transform midAirTransform)
    {
        if (planeMode != PlaneMode.MIDAIR)
        {
            Debug.Log("Invalid Ground Plane Mode:" + planeMode);
            return;
        }

        Debug.Log("PlaceObjectInMidAir() called.");

        if (positionalDeviceTracker != null && positionalDeviceTracker.IsActive)
        {
            DestroyAnchors();

            //contentPositioningBehaviour = m_MidAirPositioner.GetComponent<ContentPositioningBehaviour>();
            contentPositioningBehaviour.PositionContentAtMidAirAnchor(midAirTransform);

           if (!m_MidAirAugmentation.activeInHierarchy)
            {
                Debug.Log("Setting Mid-Air Augmentation to Active");
                // On initial run, unhide the augmentation
                m_MidAirAugmentation.SetActive(true);
            }

            Debug.Log("Positioning Mid-Air Augmentation at: " + midAirTransform.position.ToString());
            m_MidAirAugmentation.PositionAt(midAirTransform.position);
            RotateTowardCamera(m_MidAirAugmentation);
        }
    }*/

    public void SetGroundMode(bool active)
    {
        if (active)
        {
            planeMode = PlaneMode.GROUND;
           // m_TitleMode.text = TITLE_GROUNDPLANE;
            m_PlaneModeIcon.sprite = m_IconGroundMode;
            m_PlaneFinder.gameObject.SetActive(true);
          //  m_MidAirPositioner.gameObject.SetActive(false);
        }
    }

    /*public void SetMidAirMode(bool active)
    {
        if (active)
        {
            planeMode = PlaneMode.MIDAIR;
            m_TitleMode.text = TITLE_MIDAIR;
           // m_PlaneModeIcon.sprite = m_IconMidAirMode;
            m_PlaneFinder.gameObject.SetActive(false);
           // m_MidAirPositioner.gameObject.SetActive(true);
        }
    }*/

    public void Reset()
    {
        //Debug.Log("Reset() called.");

        // reset augmentations
        m_PlaneAugmentation.transform.position = Vector3.zero;
        m_PlaneAugmentation.transform.localEulerAngles = Vector3.zero;
        m_PlaneAugmentation.SetActive(false);
        //Buttonreset.SetActive(false);
        // m_MidAirAugmentation.transform.position = Vector3.zero;
        // m_MidAirAugmentation.transform.localEulerAngles = Vector3.zero;
        //  m_MidAirAugmentation.SetActive(false);

        // reset buttons
        // m_GroundToggle.isOn = true;
        //m_MidAirToggle.interactable = false;
        m_ResetButton.interactable = false;
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
           // Debug.Log("Trackable " + imageTrackable.TrackableName + " found");
           

            if (!primo)
            {
                if (!m_ResetButton) m_ResetButton.interactable = true;
                m_PlaneFinder.enabled = true;
               // Vector2 screenPoint = mainCamera.WorldToScreenPoint(imageTrackable.transform.position);
                Vector2 screenPoint;
                 screenPoint=mainCamera.WorldToScreenPoint(cube.transform.position);
                Debug.Log(screenPoint);

                //screenPoint.x = screenPoint.x + 500;
                //screenPoint.y = screenPoint.y + 500;
                  m_PlaneFinder.PerformHitTest(screenPoint);
                m_PlaneFinder.enabled = false;

               m_PlaneAugmentation.transform.position = cube.transform.position;
              // m_PlaneAugmentation.transform.localPosition = Vector3.zero;
              // m_PlaneAugmentation.transform.rotation = Quaternion.identity;
                m_PlaneAugmentation.SetActive(true);
                cube.SetActive(false);
               Debug.Log("Trackable found first time");
                // m_GroundToggle.isOn = true;
                primo = true;
            }
            else
            {
                primo = false;
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
