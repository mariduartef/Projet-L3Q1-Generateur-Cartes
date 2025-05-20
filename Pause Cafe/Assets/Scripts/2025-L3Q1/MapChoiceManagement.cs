using System;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using System.IO;

// Created by Mariana Duarte L3Q1 03/2025
public class MapChoiceManagement : MonoBehaviour
{
    public Button playButton;
    public Texture[] mapTextures;
    public RawImage terrainImage;
    public GameObject mapChoice;
    public GameObject panelSavePrefab;
    public GameObject buttonParent;
    public Toggle screenshotToggle;
    int lastMapButtonPressed = -1;
    string nameLastMapPressed = "";
    public static StartGameData startGameData;

    /// <summary>
    /// Changes to the scene passed as a parameter
    /// Author : Mariana Duarte L3Q1, 03/2025
    /// </summary>
    public void changeScene(String scene){
        SceneManager.LoadScene(scene);
    }

    // Start is called before the first frame update
    void Start()
    {
        mapChoice.SetActive(true);
        instantiateAllMapButtons();
        createListeners();
        screenshotToggle.isOn = false;
        screenshotToggle.onValueChanged.AddListener(onToggleAction);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Instantiates all of the map buttons
    /// Author : Mariana Duarte L3Q1, 04/2025
    /// </summary>
    public void instantiateAllMapButtons(){
        //Instantiation of the buttons for the predefined maps
        instantiateMapButton("Ruines", false, 0);
        instantiateMapButton("Arène", false, 1);
        instantiateMapButton("Forêt", false, 2);

        print("start instantiation buttons");
        if (!Directory.Exists(Application.persistentDataPath + "/Save/Maps"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Save/Maps");
        foreach (string save in SaveMapManagement.findAllMap()){
            print(save);
            print("une carte");
            instantiateMapButton(save, true, 4);            
        }
    }

    /// <summary>
    /// Instantiates a map button
    /// Parameters : text is the text of the button 
    /// hasDeleteButton is a boolean that is true for the personalized maps 
    /// Author : Mariana Duarte L3Q1, 04/2025
    /// </summary>
    public void instantiateMapButton(string text, bool hasDeleteButton, int number){
        GameObject newPanelSave = Instantiate(panelSavePrefab,buttonParent.transform);
        Button newSaveButton = newPanelSave.transform.GetChild(0).GetComponent<Button>();
        Button newDeleteButton = newPanelSave.transform.GetChild(1).GetComponent<Button>();

        newSaveButton.GetComponent<Button>().onClick.AddListener(()=> changeImage(number, text)); //De 0 à 3, ce sont les cartes prédéfinies
        newSaveButton.GetComponentInChildren<Text>().text = text;

        if(hasDeleteButton){
            newDeleteButton.GetComponent<Button>().onClick.AddListener(()=> SaveMapManagement.deleteSave(text, newPanelSave));
        }
        else{
            newDeleteButton.gameObject.SetActive(false);
        }
    }

    // <summary>
    /// Creates listeners for all of the buttons present in the scene
    /// Author : Mariana Duarte L3Q1, 03/2025
    /// </summary>
    public void createListeners(){
        playButton.onClick.AddListener(playButtonPressed);
    }   

    // <summary>
    /// Makes the image visible or invisible according to the toggle's value
    /// Author : Mariana Duarte L3Q1, 03/2025
    /// </summary>
    public void onToggleAction(bool isOn){
        if(isOn && lastMapButtonPressed!=1){
            terrainImage.color = new Color(1f,1f,1f,1f);
        }
        else{
            terrainImage.color = new Color(0f,0f,0f,0f);
        }
    }

    // <summary>
    /// Changes the image in the center of the scene to show the user a preview of the map
    /// Author : Mariana Duarte L3Q1, 03/2025
    /// </summary>
    void changeImage(int index, string name){
        if(index<4){
            terrainImage.texture = mapTextures[index];
        }
        else{
            if(File.Exists(Application.persistentDataPath + "/Save/MapScreenshots/" + name + ".png")){
                byte[] fileData = File.ReadAllBytes(Application.persistentDataPath + "/Save/MapScreenshots/" + name + ".png");

                Texture2D originalTexture = new Texture2D(1, 1);
                originalTexture.LoadImage(fileData);

                terrainImage.texture = originalTexture;
            }
        }
        lastMapButtonPressed = index;
        nameLastMapPressed = name;
    }

    // <summary>
    /// Loads the game when the play button is pressed
    /// Author : Mariana Duarte L3Q1, 03/2025
    /// </summary>
    public void playButtonPressed(){
        if(lastMapButtonPressed != -1){
            MainGame.startGameData = startGameData;
            MainGame.startGameData.mapChosen = lastMapButtonPressed;
            MainGame.startGameData.mapName = nameLastMapPressed;
            SceneManager.LoadScene("Assets/Scenes/Map.unity");
        }
    }
}