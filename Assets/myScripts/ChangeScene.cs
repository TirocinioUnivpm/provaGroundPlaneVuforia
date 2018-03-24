using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
public void  ScenaBotteghe() {
      
        SceneManager.LoadScene("scenaBotteghe",LoadSceneMode.Single);
}
    public void ScenaMosaico()
    {

        SceneManager.LoadScene("scenaMosaico", LoadSceneMode.Single);
    }

    public void ScenaMenu() {
        SceneManager.LoadScene("menu", LoadSceneMode.Single);


    }

    public void Esci()
    {
        Application.Quit();
    }





}

