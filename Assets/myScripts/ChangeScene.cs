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
      
        SceneManager.LoadSceneAsync("BOTTEGHE_scene",LoadSceneMode.Single);



    }
    public void ScenaMosaico()
    {

        SceneManager.LoadSceneAsync("MOSAICO_scene", LoadSceneMode.Single);
    }

    public void ScenaMenu() {
        
        SceneManager.LoadSceneAsync("menu", LoadSceneMode.Single);
       


    }
    public void ScenaAnfiteatro()
    {

        SceneManager.LoadSceneAsync("ANFITEATRO_scene", LoadSceneMode.Single);



    }




    public void Esci()
    {
        Application.Quit();
    }





}

