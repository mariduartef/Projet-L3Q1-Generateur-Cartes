using System;
using System.Collections;
using System.Collections.Generic;
using Hexas;
using UnityEngine;

//Created by Mariana Duarte L3Q1 03/25
namespace Maps{

    //Represents the objects placed on the map
    [Serializable]
    public class MapObject{
        public float x;
        public float y;
        public float z;
        public float rotationX; 
        public float rotationY; 
        public float rotationZ; 
        public float rotationW; 
        public string gameObjectName;
        [NonSerialized] public GameObject gameObject;

        public MapObject(float x, float y, float z, Quaternion rot, GameObject obj){
            this.x = x;
            this.y = y;
            this.z = z;
            this.rotationX = rot.x;
            this.rotationY = rot.y;
            this.rotationZ = rot.z;
            this.rotationW = rot.w;
            this.gameObjectName = obj.name.Replace("(Clone)", "").Trim();
            gameObject = obj; 
        }

        public MapObject(){ }

        public Quaternion getRotation() {
            return new Quaternion(rotationX, rotationY, rotationZ, rotationW);
        }
    }


    //Represents a game map
    [Serializable]
    public class Map
    {
        public string name;
        public int width;
        public int height;
        public List<MapObject> objects;
        [NonSerialized] public HexaGrid grid;
    }

}