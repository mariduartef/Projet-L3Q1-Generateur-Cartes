using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// created by simon sepiol l3l1 2024
//Edited by Mariana Duarte L3Q1 04/2025
public class SauvegardeGestionScript : MonoBehaviour
{
    public GameObject panelSavePrefab;
    public GameObject buttonParent;

    public void changeScene(String scene){
        SceneManager.LoadScene(scene);
    }

    
    public string[] findAllSaves(){

        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath+"/Save", "*db");
        for (int i = 0; i < saveFiles.Length; i++)
            {
                saveFiles[i] = Path.GetFileNameWithoutExtension(saveFiles[i]);
                print(Path.GetFileNameWithoutExtension(saveFiles[i]));
            }
        return saveFiles;
    }

    public void loadSave(string saveName){
        MainGame.startGameData = new StartGameData { loadSave = saveName };
        
        SceneManager.LoadScene(1);
    }

    public void deleteSave(string saveName, GameObject panelSave){
        print("essai de supprimer");
        print(Application.persistentDataPath + "/Save/"+ saveName+".db");
        print("file exists");
        print(File.Exists(Application.persistentDataPath + "/Save/"+ saveName+".db"));

        if(File.Exists(Application.persistentDataPath + "/Save/"+ saveName+".db")){
            Debug.Log("file exists");
            File.Delete(Application.persistentDataPath + "/Save/"+ saveName+".db");
            print(Application.persistentDataPath + "/Save/"+ saveName+" supprime");
            panelSave.SetActive(false);
        }

        if(File.Exists(Application.persistentDataPath + "/Save/"+ saveName+".db.meta")){
            File.Delete(Application.persistentDataPath + "/Save/"+ saveName+".db.meta");
        }

        //Added by Mariana Duarte L3Q1
        if(File.Exists(Application.persistentDataPath + "/Save/MapForGame/"+ saveName+"Map.db")){
            Debug.Log("Suppression carte");
            File.Delete(Application.persistentDataPath + "/Save/MapForGame/"+ saveName+".db.meta");
        }

        if(File.Exists(Application.persistentDataPath + "/Save/MapForGame/"+ saveName+"Map.db.meta")){
            File.Delete(Application.persistentDataPath + "/Save/MapForGame/"+ saveName+".db.meta");
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Save/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Save/");
        foreach ( string save in  findAllSaves()){
            print(save);
            print("une save");
            GameObject newPanelSave = Instantiate(panelSavePrefab,buttonParent.transform);
            Button newSaveButton = newPanelSave.transform.GetChild(0).GetComponent<Button>();
            Button newDeleteButton = newPanelSave.transform.GetChild(1).GetComponent<Button>();

            newSaveButton.GetComponent<Button>().onClick.AddListener(()=> loadSave(save));
            newSaveButton.GetComponentInChildren<Text>().text=save;
            
            newDeleteButton.GetComponent<Button>().onClick.AddListener(()=> deleteSave(save, newPanelSave));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
