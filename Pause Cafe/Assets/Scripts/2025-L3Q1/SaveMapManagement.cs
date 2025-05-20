using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Maps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// created by Doralie Dorcal L3Q1 2025
//This class manages the personalized map saves
public class SaveMapManagement : MonoBehaviour
{
    
    public static string[] findAllMap(){

        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath+"/Save/Maps", "*.db");
        
        for (int i = 0; i < saveFiles.Length; i++)
            {
                saveFiles[i] = Path.GetFileNameWithoutExtension(saveFiles[i]);
                print(Path.GetFileNameWithoutExtension(saveFiles[i]));
            }
        return saveFiles;
    }

    public static void deleteSave(string saveName, GameObject panelSave){
        print("essai de supprimer");
        print(Application.persistentDataPath + "/Save/Maps"+ saveName+".db");

        if(File.Exists(Application.persistentDataPath + "/Save/Maps/"+ saveName+".db")){
            File.Delete(Application.persistentDataPath + "/Save/Maps/"+ saveName+".db");
            print(Application.persistentDataPath + "/Save/"+ saveName+" supprime");
            panelSave.SetActive(false);
        }

        if(File.Exists(Application.persistentDataPath + "/Save/Maps/"+ saveName+".db.meta")){
            File.Delete(Application.persistentDataPath + "/Save/Maps/"+ saveName+".db.meta");
            
        }

        //Added by Mariana Duarte 04/2025
        if(File.Exists(Application.persistentDataPath + "/Save/MapScreenshots/" + saveName + ".png")){
            File.Delete(Application.persistentDataPath + "/Save/MapScreenshots/" + saveName + ".png");
        }

        if(File.Exists(Application.persistentDataPath + "/Save/MapScreenshots/" + saveName + ".png.meta")){
            File.Delete(Application.persistentDataPath + "/Save/MapScreenshots/" + saveName + ".png.meta");
        }
        
    }

}
