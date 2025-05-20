using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class AfficheurTextCharacter : MonoBehaviour
{
    public RectTransform rectangleMainMenu;

    public TextMeshProUGUI text;

    public GameObject carte; // pour savoir si elles ont ete desactivées


    //public String[] textAAfficher = new string[11];
    public List<string> textAAfficher = new List<string>();

    public Vector2 decalage ;
    // Start is called before the first frame update
    void Start()
    {
        text=GetComponent<TextMeshProUGUI>();
        text.text="";
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 positionSouris = Input.mousePosition;

        // Convertir les coordonnées de la souris en coordonnées locales par rapport au rectangle
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectangleMainMenu, positionSouris, null, out Vector2 localPoint);

        // ajoute un decalage pour que le texte soit en dessous de la souris et eviter qu'il sorte de l'ecran
        transform.localPosition = localPoint + decalage;

        if(!carte.activeSelf){
            text.text="";
        }
        //decalage.y = -transform.GetComponent<RectTransform>().rect.height / 2; Tentative de décalage dynamique
    }
    /*
    public void afficheText(String nouveauText, int noPerso) {
        textAAfficher[noPerso] = nouveauText;
        text.text=String.Join("",textAAfficher);
    }

    public void enleverText(int indice){
        textAAfficher[indice]="";
        text.text=String.Join("",textAAfficher);
    }

    */
    public void afficheText(String nouveauText, int noPerso)
    {
        // Si l'indice est trop grand, agrandir la liste dynamiquement
        while (noPerso >= textAAfficher.Count)
        {
            textAAfficher.Add("");
        }

        textAAfficher[noPerso] = nouveauText;
        text.text = String.Join("", textAAfficher);
    }

    public void enleverText(int indice)
    {
        if (indice >= 0 && indice < textAAfficher.Count)
        {
            textAAfficher[indice] = "";
            text.text = String.Join("", textAAfficher);
        }
    }
}
