using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneMenuPrincipal : MonoBehaviour
{

    // added by Simon Sepiol l3l1 2024
    public void changeScene(String scene){
        SceneManager.LoadScene(scene);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
