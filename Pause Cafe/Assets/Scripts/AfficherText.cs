using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AfficherText : MonoBehaviour
{

    public AfficheurTextCharacter afficheurText;

    TextMeshProUGUI tmPro;

    public int noPerso;

    public GameObject carte;

    // Start is called before the first frame update
    void Start()
    {
        tmPro = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (IsPointerOver(transform.GetComponent<RectTransform>()))
        {
            afficheurText.afficheText(tmPro.text,noPerso);
        }
        else
        {
            afficheurText.enleverText(noPerso);
        }
    }

    bool IsPointerOver(RectTransform rectangle)
    {
        Vector2 positonSouris;
        if (
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectangle,
                Input.mousePosition,
                null,
                out positonSouris
            )
        )
        {
            if (rectangle.rect.Contains(positonSouris))
            {
                return true;
            }
        }
        return false;
    }
}
