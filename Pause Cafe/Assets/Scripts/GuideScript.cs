using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// created by simon sepiol L3L1 2024
public class GuideScript : MonoBehaviour

{

    // scene de base
    private string previousScene;
    // added by Simon Sepiol l3l1 2024


    public void goToGuide(string actualScene){
        PlayerPrefs.SetString("PreviousScene",actualScene);
        SceneManager.LoadScene("Scenes/Guide");
    }

    

    public void Return(){

        SceneManager.LoadScene(previousScene);
    }
    // Start is called before the first frame update
    void Start()
    {
        previousScene=PlayerPrefs.GetString("PreviousScene", "MenuPrincipal");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
