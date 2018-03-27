using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carDoorBehaviour : MonoBehaviour {
    private float currAngle = 0;
    private float desireAngle = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        currAngle = Mathf.LerpAngle(currAngle, desireAngle,Time.deltaTime*3f);
        transform.localEulerAngles = new Vector3(0,0,currAngle);
	}
    void OpenDoors()
    {
        desireAngle = -50f;

    }
    void CloseDoors()
    {
        desireAngle = 0;
    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("MainCamera")) { OpenDoors(); }
        
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("MainCamera")) { CloseDoors(); }

    }
}
