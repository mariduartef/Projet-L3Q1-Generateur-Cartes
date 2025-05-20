using UnityEngine;
using UnityEngine.UI; 


//Created by Chen Chrisotphe L3Q1
//Edited by Mariana Duarte L3Q1 04/2025
public class CategorySwitcher : MonoBehaviour
{
    public ScrollRect scrollRect;
    public GameObject contentArbres;
    public GameObject contentMurs;
    public GameObject contentAutres;

    //Added by Mariana Duarte L3Q1 04/2025
    public GameObject contentRoches;
    public GameObject contentVegetation;

    void Start()
    {
       
        contentArbres.SetActive(true);
        contentMurs.SetActive(false);
        contentAutres.SetActive(false);
        contentRoches.SetActive(false);
        contentVegetation.SetActive(false);

        // S'assurer que le ScrollRect pointe sur le bon content
        scrollRect.content = contentArbres.GetComponent<RectTransform>();
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Active le content des arbres
    public void ShowArbres()
    {
        contentArbres.SetActive(true);
        contentMurs.SetActive(false);
        contentAutres.SetActive(false);
        contentRoches.SetActive(false);
        contentVegetation.SetActive(false);
        scrollRect.content = contentArbres.GetComponent<RectTransform>();
        
    }
    //Autor :Chen Chrisophe L3Q1 2025
    //Active le content des murs
    public void ShowMurs()
    {
        contentArbres.SetActive(false);
        contentAutres.SetActive(false);
        contentMurs.SetActive(true);
        contentRoches.SetActive(false);
        contentVegetation.SetActive(false);
        scrollRect.content = contentMurs.GetComponent<RectTransform>();
        
    }
    //Author :Chen Chrisophe L3Q1 2025
    //Active le content des autres objets
    public void ShowAutres()
    {
        contentAutres.SetActive(true);
        contentArbres.SetActive(false);
        contentMurs.SetActive(false);
        contentRoches.SetActive(false);
        contentVegetation.SetActive(false);
        scrollRect.content = contentAutres.GetComponent<RectTransform>();
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Active le content des roches
    public void ShowRoches()
    {
        contentArbres.SetActive(false);
        contentMurs.SetActive(false);
        contentAutres.SetActive(false);
        contentRoches.SetActive(true);
        contentVegetation.SetActive(false);
        scrollRect.content = contentRoches.GetComponent<RectTransform>();
        
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Active le content des végétations
    public void ShowVegetation()
    {
        contentArbres.SetActive(false);
        contentMurs.SetActive(false);
        contentAutres.SetActive(false);
        contentRoches.SetActive(false);
        contentVegetation.SetActive(true);
        scrollRect.content = contentVegetation.GetComponent<RectTransform>();
        
    }
}
