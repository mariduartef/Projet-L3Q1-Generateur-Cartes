using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeBackground : MonoBehaviour
{

    RawImage background;
    public Texture image;
    public Button newGame;

    // Start is called before the first frame update
    void Start()
    {
        background = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
