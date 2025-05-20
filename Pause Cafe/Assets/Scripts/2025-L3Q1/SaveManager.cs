using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Maps;
using Hexas;
using System.Reflection;

//Created by Mariana Duarte L3Q1 03/25
//This class manages the map saves
public static class SaveManager
{

    /// <summary>
    /// This method serializes and stores all of the objects from the provided map
    /// Author : Mariana Duarte L3Q1, 03/2025
    /// </summary>
    public static void saveMapObjects(Map map, string directoryName){

        if (!Directory.Exists(Application.persistentDataPath + directoryName))
            Directory.CreateDirectory(Application.persistentDataPath + directoryName);

        string path = Application.persistentDataPath + directoryName + "/" + map.name + ".db";
        BinaryWriter binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create));

        binaryWriter.Write(map.objects.Count);

        foreach (MapObject mapObject in map.objects){
            binaryWriter.Write(mapObject.gameObjectName);
            binaryWriter.Write(mapObject.x);
            binaryWriter.Write(mapObject.y);
            binaryWriter.Write(mapObject.z);
            binaryWriter.Write(mapObject.rotationX);
            binaryWriter.Write(mapObject.rotationY);
            binaryWriter.Write(mapObject.rotationZ);
            binaryWriter.Write(mapObject.rotationW);
            Debug.Log("saveMapObjets");
            Debug.Log($"objet {mapObject.gameObjectName}, x {mapObject.x},y {mapObject.y},z {mapObject.z}");
        }

        Debug.Log("objets sauvegardee");
    }

    /// <summary>
    /// This method serializes and stores all of the information from the provided map's grid
    /// Author : Mariana Duarte L3Q1, 03/2025
    /// </summary>
    public static void saveMapGrid(Map map, string directoryName){
        if (!Directory.Exists(Application.persistentDataPath + directoryName))
            Directory.CreateDirectory(Application.persistentDataPath + directoryName);

        string path = Application.persistentDataPath + directoryName + "/" + map.name + "Grid.db";
        BinaryWriter binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create));

        binaryWriter.Write(map.width);
        binaryWriter.Write(map.height);

        foreach(Hexa hexa in map.grid.hexaList){
            binaryWriter.Write((byte)hexa.type);
        }

        Debug.Log("grille sauvegardee");
    }

    /// <summary>
    /// This method creates a map from a file (path provided as parameter), 
    /// deserealizing all of the stored information
    /// Author : Mariana Duarte L3Q1, 03/2025
    /// </summary>
    public static Map createMapFromFile(string savePathObjects, string savePathGrid){
        Debug.Log($"path objects {savePathObjects}");
        Debug.Log($"path grid {savePathGrid}");

        if(File.Exists(savePathObjects) && File.Exists(savePathGrid)){
            try{   
                
                HexaGrid grid = new HexaGrid();
                grid.createGridFromFile(savePathGrid);
                Debug.Log("hexa grid crée");

                Map map = new Map();
                map.grid = grid;

                BinaryReader reader = new BinaryReader(File.Open(savePathObjects, FileMode.Open));
                int lenghtObjectList = reader.ReadInt32();

                map.grid = grid;
                map.width = grid.w;
                map.height = grid.h;
                map.objects = new List<MapObject>();

                for(int i=0; i<lenghtObjectList; i++){
                    MapObject mapObject = new MapObject();
                    mapObject.gameObjectName = reader.ReadString();

                    GameObject gameObject = GameObject.Find(mapObject.gameObjectName);
                        if(gameObject != null)
                            mapObject.gameObject = gameObject;
                        else
                            Debug.Log("Object "+mapObject.gameObjectName+" not found");

                    mapObject.x = reader.ReadSingle();
                    mapObject.y = reader.ReadSingle();
                    mapObject.z = reader.ReadSingle();
                    mapObject.rotationX = reader.ReadSingle();
                    mapObject.rotationY = reader.ReadSingle();
                    mapObject.rotationZ = reader.ReadSingle();
                    mapObject.rotationW = reader.ReadSingle();
                    Debug.Log("create map from file");

                    Debug.Log($"objet {mapObject.gameObjectName}, x {mapObject.x},y {mapObject.y},z {mapObject.z}");

                    map.objects.Add(mapObject);
                    
                }

                Debug.Log("fin creation carte");
                return map;

            }catch(SerializationException){
                Debug.Log("Failed to load file");
            }
        }

        return null;
    }
}