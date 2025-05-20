using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hexas;
using Maps;
using UnityEditor.SceneManagement;

/// <summary>
/// RandomMapGenerator
/// Created by Doralie, 04/2025
/// </summary>
public class RandomMapGenerator : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject parameters;
    public GameObject mapGeneratorObj;
    public TMP_InputField quantityInput; 
    public List<Toggle> objectTypeToggles; 
    public TMP_Text errorText;
    public Button okButton; 

    [Header("Placement Settings")]
    private MapGenerator mapGenerator;

    // Probabilités de placement
    public float baseProbability = 0.3f;
    public float probabilityReductionFactor = 0.5f;
    public int maxRadiusEffect = 10;
    public float minProbabilityThreshold = 0.01f;

    [Header("Object Management")]
    private List<GameObject> prefabs;          

    // Dictionnaire pour stocker la probabilité d'apparition sur chaque case
    private Dictionary<Hexa, float> probabilityMap;

    private int quantityToPlace;



    void Start()
    {
        mapGenerator = MapGenerator.instance;

        if (mapGenerator != null && (prefabs == null || prefabs.Count == 0))
            prefabs = mapGenerator.prefabs;

        createListeners();       
    }


    /// <summary>
    /// Affiche la pop-up de paramètres de génération 
    /// </summary>
    public void OnCarteAleatoireButtonPressed()
    {
        parameters.SetActive(true);
        mapGeneratorObj.SetActive(false);

        if (mapGenerator == null)
        {
            mapGenerator = MapGenerator.instance;
        }

        if (prefabs == null || prefabs.Count == 0)
        {
            if (mapGenerator != null && mapGenerator.prefabs.Count > 0)
                prefabs = mapGenerator.prefabs;
        }

        createListeners();

        /*if (toggleYes != null && toggleNo != null)
        {
            
            toggleYes.onValueChanged.RemoveAllListeners();
            toggleNo.onValueChanged.RemoveAllListeners();

            toggleYes.onValueChanged.AddListener(OnToggleYesChanged);
            toggleNo.onValueChanged.AddListener(OnToggleNoChanged);
        }*/
    }

    /// <summary>
    /// Crée les écouteurs pour les boutons
    /// </summary>
    private void createListeners()
    {
        if (okButton != null)
        {
            okButton.onClick.AddListener(OnOkButtonPressed);
        }
    }

        /// <summary>
    /// Lance la génération automatique de la carte.
    /// </summary>
    public void Generate()
    {
        // Sécurités
        if (mapGenerator == null || mapGenerator.hexaGrid == null || mapGenerator.map == null)
        {
            Debug.LogError("Il manque MapGenerator, hexaGrid ou la carte !");
            return;
        }

        mapGenerator.disableCharacterSpawnPoints();          // bloque les cases de spawn

        List<GameObject> filteredPrefabs = GetFilteredPrefabs();
        if (filteredPrefabs == null || filteredPrefabs.Count == 0)
        {
            Debug.LogWarning("Aucun prefab ne correspond à la sélection actuelle.");
            return;
        }

        InitializeProbabilityMap(mapGenerator.hexaGrid);      // proba de base

        List<Hexa> hexasByDistance = SortHexasByDistanceFromCenter(mapGenerator.hexaGrid);

        int placedCount = 0;

        // 1ᵉʳ PASSAGE : on respecte les probabilités
        foreach (Hexa h in hexasByDistance)
        {
            if (placedCount >= quantityToPlace) break;
            if (!h.IsAvailable)                      continue;
            if (probabilityMap[h] < minProbabilityThreshold) continue;

            if (TryPlaceRandomObject(h, mapGenerator.hexaGrid, mapGenerator.map,
                                    filteredPrefabs, ignoreProbability: false))
            {
                placedCount++;
            }
        }

        // 2ᵉ PASSAGE : on force le remplissage des cases encore libres
        if (placedCount < quantityToPlace)
        {
            
            foreach (Hexa h in mapGenerator.hexaGrid.hexaList.OrderBy(_ => Random.value))
            {
                if (placedCount >= quantityToPlace) break;
                if (!h.IsAvailable) continue;

                if (TryPlaceRandomObject(h, mapGenerator.hexaGrid, mapGenerator.map,
                                        filteredPrefabs, ignoreProbability: true))
                {
                    placedCount++;
                }
            }
        }

        Debug.Log($"Génération terminée : {placedCount}/{quantityToPlace} objet(s) placé(s).");

        mapGenerator.ResetAllHexaColors();           
    }


    /// <summary>
    /// Filtre <see cref="prefabs"/> selon les types sélectionnés (Mur, Arbre, Box).
    /// </summary>
    /// <returns>Nouvelle <see cref="List{T}"/> ne contenant que les prefabs autorisés.</returns>
    private List<GameObject> GetFilteredPrefabs()
    {
        List<GameObject> result = new List<GameObject>();
      
        bool murSelected = objectTypeToggles[0].isOn;
        bool arbreSelected = objectTypeToggles[1].isOn;
        bool boxSelected = objectTypeToggles[2].isOn;
        bool rocheSelected = objectTypeToggles[3].isOn;
        bool vegetationSelected = objectTypeToggles[4].isOn;

        foreach (var p in prefabs)
        {
            string lowerName = p.name.ToLower();

            if (murSelected && lowerName.Contains("mur"))
            {
                result.Add(p);
                continue;
            }

            if (arbreSelected && (lowerName.Contains("arbre") || lowerName.Contains("tree")))
            {
                result.Add(p);
                continue;
            }

            if (boxSelected && (lowerName.Contains("box") || lowerName.Contains("barrel") || lowerName.Contains("chest")))
            {
                result.Add(p);
                continue;
            }

            if (rocheSelected && lowerName.Contains("rock"))
            {
                result.Add(p);
                continue;
            }

            if (vegetationSelected && (lowerName.Contains("buisson") || lowerName.Contains("mushroom") || lowerName.Contains("bush")))
            {
                result.Add(p);
                continue;
            }
        }

        return result;
    }

    /// <summary>
    /// Remplit <see cref="probabilityMap"/> pour chaque hexagone de la grille.
    /// </summary>
    /// <param name="grid">Grille hexagonale complète.</param>
    private void InitializeProbabilityMap(HexaGrid grid)
    {
        probabilityMap = new Dictionary<Hexa, float>();
        foreach (Hexa h in grid.hexaList)
        {
            probabilityMap[h] = (h.IsAvailable) ? baseProbability : 0f;
        }
    }

    /// <summary>
    /// Trie les cases par distance croissante depuis le centre de la grille.
    /// </summary>
    private List<Hexa> SortHexasByDistanceFromCenter(HexaGrid grid)
    {
        int centerX = grid.w / 2;
        int centerY = grid.h / 2;
        return grid.hexaList
                   .OrderBy(h => grid.getDistance(centerX, centerY, h.getX(), h.getY()))
                   .ToList();
    }

    /// <summary>
    /// Essaie de placer au hasard un prefab issu de la liste filtrée sur la case centerHexa.
    /// Retourne true si un objet est placé, false sinon.
    /// </summary>
    private bool TryPlaceRandomObject(Hexa          anchorHexa,
                                  HexaGrid      grid,
                                  Map           map,
                                  List<GameObject> allowedPrefabs,
                                  bool          ignoreProbability = false)
    {
        foreach (GameObject prefab in ShuffleList(allowedPrefabs))
        {

                if (!ignoreProbability)
            {
                float p = probabilityMap[anchorHexa];
                if (Random.value >= p) break;        
            }
        
            GameObject ghost = Instantiate(prefab,
                                        anchorHexa.go.transform.position,
                                        prefab.transform.rotation);
            
            //Réglage spécial pour mur6 en attendant                            
            if (prefab.name.Equals("mur6", System.StringComparison.OrdinalIgnoreCase))
                {
                    Vector3 p = ghost.transform.position;
                    ghost.transform.position = new Vector3(p.x, 2f, p.z);
                }
            if (!mapGenerator.isObjectOnMap(prefab, ghost.transform.position))
                {
                    Destroy(ghost);
                    continue;        
                }

            foreach (var col in ghost.GetComponentsInChildren<Collider>())
                col.isTrigger = true;

            if (!ghost.TryGetComponent(out Rigidbody rb))
            {
                rb = ghost.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }

            var handler   = ghost.AddComponent<DragCollisionHandler>();
            var prevMode  = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.simulationMode = prevMode;

        
            if (!handler.IsValidPlacement())
            {
                Destroy(ghost);
                continue;                
            }

            List<Hexa> footprint = new List<Hexa>(handler.collidingHexas);
            Destroy(ghost);

            
            PlaceObject(anchorHexa, prefab, footprint, map);

            ReduceProbabilityAround(anchorHexa, grid, maxRadiusEffect);
            return true;  
        }
        return false;
    }


    

/***    /// <summary>
    /// Calcule l’empreinte hexagonale qu’occuperait un <paramref name="prefab"/> s’il était
    /// placé sur l’hexagone <paramref name="center"/>.
    /// </summary>
    /// <param name="center">Hexagone de référence où l’on teste le prefab.</param>
    /// <param name="prefab">Prefab à évaluer.</param>
    /// <returns>Liste d’hexagones représentant l’empreinte de l’objet.</returns>
    /// Added by chen christophe 2025 
    private List<Hexa> GetFootprintWithDragHandler(Hexa center, GameObject prefab)
{

    GameObject ghost = Instantiate(prefab, center.go.transform.position, prefab.transform.rotation);


    foreach (var col in ghost.GetComponentsInChildren<Collider>())
        col.isTrigger = true;

 
    if (!ghost.TryGetComponent(out Rigidbody rb))
    {
        rb = ghost.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

   
    var handler = ghost.AddComponent<DragCollisionHandler>();
    var prevMode = Physics.simulationMode;
    Physics.simulationMode = SimulationMode.Script;
    Physics.Simulate(Time.fixedDeltaTime);
    Physics.simulationMode = prevMode;
    List<Hexa> footprint = handler.collidingHexas.Count > 0
        ? new List<Hexa>(handler.collidingHexas)
        : new List<Hexa> { center };

    Destroy(ghost);
    return footprint;
}
  

    /// <summary>
    /// Teste si l’empreinte est exploitable 
    /// </summary>
    /// <param name="footprint">
    /// Liste des <see cref="Hexa"/> constituant l’empreinte potentielle
    /// </param>
    /// <returns>
    /// <c>true</c> si l’empreinte est utilisable, sinon <c>false</c>.
    /// </returns>
    private bool IsFootprintValid(List<Hexa> footprint)
    {
        if (footprint == null || footprint.Count == 0)
            return false;
        return footprint.All(h => h != null && h.IsAvailable && h.type == HexaType.GROUND);
    } ***/ 




    /// <summary>
    /// Instancie un prefab sur l’empreinte fournie, met à jour la grille et la structure Map.
    /// </summary>
    /// <param name="anchorHexa">Hexagone de référence fourni par l’appelant.</param>
    /// <param name="prefab">Prefab à instancier.</param>
    /// <param name="footprint">Liste des hexagones que l’objet occupe réellement.</param>
    /// <param name="map">Objet logique contenant la carte et ses éléments (sauvegarde).</param>                   
   private void PlaceObject(Hexa anchorHexa,GameObject prefab, List<Hexa> footprint,Map map)
{
    
    Vector3 spawnPos = anchorHexa.go.transform.position;
    GameObject placed = Instantiate(prefab, spawnPos, prefab.transform.rotation);
    //Réglage spécial pour mur6 en attendant
   if (placed.name.Equals("mur6(Clone)", System.StringComparison.OrdinalIgnoreCase))
{
    Vector3 pos = placed.transform.position;
    pos.y = 2f;                 // force uniquement la hauteur
    placed.transform.position = pos;
}
 
    var occ = footprint.Select(h => new Vector2Int(h.getX(), h.getY()))
                       .ToList();
    MapGenerator.instance.placedObjectsOccupiedHex[placed] = occ;

    foreach (Hexa h in footprint)
    {
        switch (placed.tag)
        {
            case "Obstacle": h.changeType(HexaType.WALL);   break;
            case "buisson" : h.changeType(HexaType.BUSH);   break;
            case "bonus"   : h.changeType(HexaType.BONUS);  break;
            case "PORTAL"  : h.changeType(HexaType.PORTAL); break;
        }
        h.IsAvailable = false;
    }


    map.objects.Add(new MapObject(
        placed.transform.position.x,
        placed.transform.position.y,
        placed.transform.position.z,
        placed.transform.rotation,
        placed));
}



    /// <summary>
    /// Diminue la probabilité de placement autour d’un centre sur un rayon donné.
    /// </summary>
    /// <param name="center">Hexagone placé au centre de l’effet.</param>
    /// <param name="grid">Grille complète pour calcul de distance.</param>
    /// <param name="radius">Rayon (en hexas) sur lequel appliquer le facteur.</param>
    private void ReduceProbabilityAround(Hexa center, HexaGrid grid, int radius)
    {
        List<Hexa> ring = GetHexasWithinRadius(grid, center, radius);
        foreach (Hexa h in ring)
        {
            probabilityMap[h] *= probabilityReductionFactor;
        }
    }


    /// <summary>
    /// Retourne une collection d’hexas dont la distance au « center » est ≤ radius.
    /// </summary>
    private List<Hexa> GetHexasWithinRadius(HexaGrid grid, Hexa center, int radius)
    {
        return grid.hexaList
                   .Where(h => grid.getDistance(center.getX(), center.getY(), h.getX(), h.getY()) <= radius)
                   .ToList();
    }

    /// <summary>
    /// Mélange en place une liste (Fisher–Yates) afin d’éviter des placements répétitifs.
    /// </summary>
    private List<GameObject> ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            GameObject temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
        return list;
    }



    //  Gestion des toggles Oui/Non
    /*private void OnToggleYesChanged(bool value)
    {
        if (value)
            toggleNo.isOn = false;
    }

    private void OnToggleNoChanged(bool value)
    {
        if (value)
            toggleYes.isOn = false;
    }*/


    /// <summary>
    /// Vérifie toutes les entrées UI avant de lancer la génération.
    /// </summary>
    /// <returns>true si tout est OK, sinon false + message d’erreur.</returns>
    public bool ValidateInputs()
    {
        errorText.text = "";

        // Récupération du nombre total d'hexas dans la grille
        int totalHexCount = 0;
        if (mapGenerator != null && mapGenerator.hexaGrid != null)
        {
            totalHexCount = mapGenerator.hexaGrid.hexaList.Count;
        }

        // Vérifie la quantité fournie
        if (!int.TryParse(quantityInput.text, out int quantity))
        {
            errorText.text = "Veuillez entrer un nombre entier pour la quantité.";
            return false;
        }

        int maxAllowed = (int)(totalHexCount * 0.05);

        if (quantity < 1 || quantity > maxAllowed)
        {
            errorText.text = $"La quantité doit être comprise entre 1 et {maxAllowed}.";
            return false;
        }
        quantityToPlace = quantity;

    
        /*bool isYesSelected = toggleYes.isOn;
        bool isNoSelected = toggleNo.isOn;
        if (isYesSelected == isNoSelected)
        {
            errorText.text = "Veuillez sélectionner soit 'Oui', soit 'Non'.";
            return false;
        }*/

        // Vérifie qu'au moins un type d'objet (Mur, Arbre, Box) est sélectionné
        bool typeSelected = objectTypeToggles.Any(t => t.isOn);
        if (!typeSelected)
        {
            errorText.text = "Veuillez sélectionner au moins un type d'objet.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Callback du bouton OK
    /// </summary>
    public void OnOkButtonPressed()
    {
        if (!ValidateInputs())
        {
            Debug.LogWarning("Erreur de validation des paramètres.");
            return;
        }

        mapGenerator.DeleteAllPlacedObjects();

        Debug.Log("Validation réussie, ancienne carte nettoyée !");
        parameters.SetActive(false);
        mapGeneratorObj.SetActive(true);
        Generate();
    }
}