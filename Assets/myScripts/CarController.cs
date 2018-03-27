using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private bool sound = false;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!sound && transform.localPosition.y < .05f)
        {
            sound = true;
            StartCoroutine(DelayPlaySound());
        }
    }
    public void MoveCar()
    {
        transform.localPosition += new Vector3(0,20, 0);
        transform.eulerAngles += new Vector3(5, 20, 5);
        sound = false;
    }
    IEnumerator DelayPlaySound() {
        yield return new WaitForSeconds(.1f);
        GetComponent<AudioSource>().Play();
    }
}
