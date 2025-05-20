using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using Maps;
using UnityEngine.SceneManagement;
using Hexas;
using System.IO;

//Created by Christophe Chen L3Q1 03/2025
//Edited by Mariana Duarte L3Q1 03/2025
public class MapGenerator : MonoBehaviour
{
    [Header("UI Settings")]
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public TMP_InputField nameInput;
    public Transform plane1;
    public Transform plane2;
    public RectTransform rawImageRectTransform;

    [Header("Hexa Settings")]
    public GameObject hexaTemplate;
    public Mesh hexaFilledMesh;
    public Mesh hexaHollowMesh;
    public Mesh hexaWallMesh;
    public LayerMask hexaLayerMask;
    public GameObject hexasFolder;
    public GameObject mapHexaFolder;

    [Header("Camera Settings")]
    public Camera mainCamera;

    public Hexas.HexaGrid hexaGrid;
    private Hexas.Hexa hoveredHexa;

    [Header("UI Prefab Assignment")]
    //public List<Button> uiButtons;
    public List<GameObject> prefabs;

    private GameObject currentObject;
    private int selectedPrefabIndex = -1;
    private bool isDragging = false;


    public static MapGenerator instance;

    private List<Hexas.Hexa> targetHexas = new List<Hexas.Hexa>();

    public Dictionary<GameObject, List<Vector2Int>> placedObjectsOccupiedHex = new Dictionary<GameObject, List<Vector2Int>>();
  
    //Author : Mariana Duarte L3Q1 03/2025
    public GameObject mapGenerator;
    public GameObject parameters;
    public GameObject confirmationMessage;
    public GameObject mapMessage;
    public GameObject image;
    public TMP_Text errorText;
    public Button okButton;
    string mapName = "";
    int width = -1;
    int height = -1;
    bool allHexasReachable = true;
    public Map map = new Map();
    public Button returnButton;
    public Button returnButton2;
    public Button saveButton;

    [Header("Selection PlacedObject")]
    public LayerMask placedObjectsLayerMask;
    public GameObject buttonDelete;
    
    private GameObject selectedObject;
    private Renderer selectedRenderer;
    private Color originalColor;

    void Awake()
    {
        InitializeHexaSystem();
        instance = this;
        buttonDelete.SetActive(false);
    }

    void Start()
    {

        //Author : Mariana Duarte L3Q1 03/2025
        mapGenerator.SetActive(false);
        parameters.SetActive(true);
        confirmationMessage.SetActive(false);
        mapMessage.SetActive(false);
        createListeners();

        errorText.text = "";
        mainCamera.orthographicSize = 15f;
    }

    void Update()
    {
        HandleMouseHover();
        HandleDragAndDrop();

        if (Input.GetMouseButtonDown(0))
        {
            HandleSelectionPlacedObject();
        }

        if(!allHexasReachable)
            mapMessage.SetActive(true);
        else
            mapMessage.SetActive(false);

    }


    //Author :Chen Chrisophe L3Q1 2025
    //Initialise les attributs pour la construction de la grille hexagonale 
    private void InitializeHexaSystem()
    {
        Hexas.Hexa.hexaTemplate = hexaTemplate;
        Hexas.Hexa.hexaFilledMesh = hexaFilledMesh;
        Hexas.Hexa.hexaHollowMesh = hexaHollowMesh;
        Hexas.Hexa.hexaWallMesh = hexaWallMesh;
        Hexas.Hexa.hexasFolder = hexasFolder;
        Hexas.Hexa.mapHexaFolder = mapHexaFolder;
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Gestion de la grille hexagonale
    public void OnGridSizeChanged()
    {
        if (int.TryParse(widthInput.text, out int width) && int.TryParse(heightInput.text, out int height))
        {
            CreateHexGrid(width, height);
            UpdatePlaneScales(width, height);
        }
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Creates the listeners for all the buttons present in the scene
    public void createListeners(){
        okButton.onClick.AddListener(okButtonPressed);
        returnButton.onClick.AddListener(returnButtonPressed);
        returnButton2.onClick.AddListener(returnButtonPressed);
        saveButton.onClick.AddListener(saveButtonPressed);
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Goes back to the main menu scene when return button is pressed
    public void returnButtonPressed(){
        SceneManager.LoadScene("Assets/Scenes/Menu Principal.unity");
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Saves the map and goes back to the main menu scene
    public void saveButtonPressed(){
        map.height = height;
        map.width = width;
        map.name = mapName;
        map.grid = hexaGrid;

        Debug.Log($"Map: name {map.name}, h {map.height}, w {map.width}");
        
        SaveManager.saveMapObjects(map, "/Save/Maps");
        SaveManager.saveMapGrid(map, "/Save/Grids");

        mainCamera.orthographicSize = 19f;
        StartCoroutine(CameraController2.captureScreenshot(map.name));
        StartCoroutine(showMessageAndLoadScene());
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Displays the confirmation message for 0.8 seconds, then loads the main menu scene.
    public IEnumerator showMessageAndLoadScene()
    {
        confirmationMessage.SetActive(true);
        image.SetActive(false);

        yield return new WaitForSeconds(0.8f);

        SceneManager.LoadScene("Assets/Scenes/Menu Principal.unity");
    }


    //Author : Mariana Duarte L3Q1 03/2025
    //Sets the value of mapName to the value from the input field
    public void getInputName(){
        mapName = nameInput.text;
        if(!isMapNameValid())
            errorText.text = "Une carte portant ce nom existe déjà.";
        else
            errorText.text = "";
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Returns a boolean indicating whether the map name is valid (if there is not another map with the same name)
    public bool isMapNameValid(){
        return !File.Exists(Application.persistentDataPath+"/Save/Maps/"+mapName+".db");
    }

    //Author : Mariana Duarte L3Q1 03/2025
    //Sets the value of width to the value from the input field
    public void getInputWidth(){
        if(int.TryParse(widthInput.text, out int parsedWidth) && parsedWidth>=5 && parsedWidth<=34 ){
            width = parsedWidth;
            errorText.text = "";
        }
        else{
            errorText.text = "Veuillez entrer des valeurs entières pour la longueur et la largeur, comprises dans l'intervalle indiqué.";
            width = -2;
        }
    }
    //Author : Mariana Duarte L3Q1 03/2025
    //Sets the value of height to the value from the input field
    public void getInputHeight(){
        if(int.TryParse(heightInput.text, out int parsedHeight) && parsedHeight>=5 && parsedHeight<=30){
            height = parsedHeight;
            errorText.text = "";
        }
        else{
            errorText.text = "Veuillez entrer des valeurs entières pour la longueur et la largeur, comprises dans l'intervalle indiqué.";
            height = -2;
        }
    }
    //Author : Mariana Duarte L3Q1 03/2025
    //If all of the values entered in the input fields are valid, it creates the hexa grid
    public void okButtonPressed(){
        getInputName();
        getInputWidth();
        getInputHeight();
        if(mapName.Equals(""))
            errorText.text = "Veuillez saisir les valeurs de toutes les cases";
        else if(!isMapNameValid())
            errorText.text = "Une carte portant ce nom existe déjà.";
        else if(width == -1 || height == -1)
            errorText.text = "Veuillez saisir les valeurs de toutes les cases";
        else if(width == -2 || height == -2)
            errorText.text = "Veuillez entrer des valeurs entières pour la longueur et la largeur, comprises dans l'intervalle indiqué.";
        else{
            OnGridSizeChanged();
            parameters.SetActive(false);
            mapGenerator.SetActive(true);
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Crée la grille hexagonale
    private void CreateHexGrid(int width, int height)
    {
        // Détruit l'ancienne grille
        if (hexaGrid != null)
        {
            foreach (var hexa in hexaGrid.hexaList)
            {
                Destroy(hexa.go);
            }
        }
        // Crée la nouvelle grille
        hexaGrid = new Hexas.HexaGrid();
        hexaGrid.createRectGrid(width, height);
        disableCharacterSpawnPoints();
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Adapte la taille du terrain en fonction de la grille hexagonale
    private void UpdatePlaneScales(int width, int height)
    {
        // Calcul des nouvelles échelles pour plane1
        float scaleX1 = (5f / 34f) * width;
        float scaleZ1 = (5f / 30f) * height;
        plane1.localScale = new Vector3(scaleX1, 1, scaleZ1);
        Debug.Log($"Plane1 Scale: {scaleX1}, {scaleZ1}");

        // Calcul des nouvelles échelles pour plane2
        float scaleX2 = (2.7f / 34f) * width;
        float scaleZ2 = (2.7f / 30f) * height;
        plane2.localScale = new Vector3(scaleX2, 1, scaleZ2);
        Debug.Log($"Plane2 Scale: {scaleX2}, {scaleZ2}");
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Gestion du survol des hexagones
    private void HandleMouseHover()
    {
        
        //Vector3 mouseWorldPosition = GetMouseWorldPosition();
        //Debug.Log("Position de la souris sur le terrain : " + mouseWorldPosition);

        Hexas.Hexa hexa = GetHexUnderMouse();
        UpdateHoveredHexa(hexa);
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Convertir la position du curseur sur l'interface graphique en positon monde   
    public Vector3 GetMouseWorldPosition()
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImageRectTransform,
            Input.mousePosition,
            null, 
            out localPoint))
        {

            Rect rect = rawImageRectTransform.rect;
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            Vector2 viewportPoint = new Vector2(normalizedX, normalizedY);


            Ray ray = mainCamera.ViewportPointToRay(viewportPoint);

            Plane terrainPlane = new Plane(Vector3.up, Vector3.zero);
            if (terrainPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(rawImageRectTransform, Input.mousePosition, null))
            {
                return ray.GetPoint(distance);
            }

        }

        return Vector3.zero;
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Retourne l'hexagone sous le curseur 
    public Hexas.Hexa GetHexUnderMouse()
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImageRectTransform,
            Input.mousePosition,
            null,
            out localPoint))
        {
            Rect rect = rawImageRectTransform.rect;
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            Vector2 viewportPoint = new Vector2(normalizedX, normalizedY);

            Ray ray = mainCamera.ViewportPointToRay(viewportPoint);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hexaLayerMask))
            {
                return GetHexFromObject(hit.collider.gameObject);
            }
        }
        return null;
    }
    
    //Author :Chen Chrisophe L3Q1 2025
    //Retourne le gameobject hexagone en verifiant s'il appartient à la grille 
    public Hexas.Hexa GetHexFromObject(GameObject hexObject)
    {
        foreach (var hexa in hexaGrid.hexaList)
        {
            if (hexa.go == hexObject)
                return hexa;
        }
        return null;
    }
    
    //Author :Chen Chrisophe L3Q1 2025
    //Change le couleur de l'hexagone lorsqu'il est survolé
    private void UpdateHoveredHexa(Hexas.Hexa newHexa)
    {
        if (hoveredHexa == newHexa)
            return;

        // Réinitialiser l'ancien hexagone
        if (hoveredHexa != null)
            hoveredHexa.defaultColor();

        hoveredHexa = newHexa;

        // Applique la nouvelle couleur à l'hexagone survolé
        if (hoveredHexa != null)
            hoveredHexa.changeColor(Color.green);
    }
    
    //Author :Chen Chrisophe L3Q1 2025
    //Prépare l'objet à poser sur la carte
    private void StartDragging()
    {
        Vector3 mousePosition = GetMouseWorldPosition(); 
        Vector3 prefabDefaultPosition = prefabs[selectedPrefabIndex].transform.position;
        Vector3 spawnPosition = new Vector3(mousePosition.x, prefabDefaultPosition.y, mousePosition.z);
        currentObject = Instantiate(prefabs[selectedPrefabIndex], spawnPosition, prefabs[selectedPrefabIndex].transform.rotation);

        // Configurer tous les colliders comme triggers
        foreach (var collider in currentObject.GetComponentsInChildren<Collider>())
        {
            collider.isTrigger = true;
        }

        // Ajouter le composant de détection
        currentObject.AddComponent<DragCollisionHandler>();

        isDragging = true;
    }
    
    //Author :Chen Chrisophe L3Q1 2025
    //Suit le curseur lorsque l'objet est selectionné
    private void UpdateInstantiateObjectPosition()
    {

        Vector3 mousePosition = GetMouseWorldPosition();
        Vector3 prefabDefaultPosition = prefabs[selectedPrefabIndex].transform.position;
        Vector3 spawnPosition = new Vector3(mousePosition.x, prefabDefaultPosition.y, mousePosition.z);

        currentObject.transform.position = spawnPosition;

        DragCollisionHandler handler = currentObject.GetComponent<DragCollisionHandler>();
        targetHexas = handler.collidingHexas;

        
        foreach (var hexa in handler.collidingHexas)
        {
            //hexa.changeColor(handler.IsValidPlacement() ? Color.green : Color.red);
            Color targetColor = handler.IsValidPlacement() ? Color.green : Color.red;
            StartCoroutine(ResetColorAfterDelay(hexa, 0.5f,targetColor));
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Edited by Mariana Duarte L3Q1 04/2025
    //instancie definitivement l'objet selectione sur la map
    private void StopDragging()
    {
        bool isValid = targetHexas.Count > 0 && targetHexas.All(h => h.IsAvailable)
                            && isObjectOnMap(prefabs[selectedPrefabIndex],currentObject.transform.position);

        Hexas.Hexa Hex = GetHexUnderMouse();//nouveau
                                            //hexagone sous le curseur

        if (isValid && Hex!=null)
        {
            List<Vector2Int> occupiedHexas = new List<Vector2Int>(); //liste pour les positions

            // Instancier l'objet 
            GameObject placedObject = Instantiate(
                prefabs[selectedPrefabIndex],
                currentObject.transform.position, 
                currentObject.transform.rotation
            );

            //Ajouter dans le dictionnaire 
            if (Hex != null)
            {
                MapObject mapObject = new MapObject( //Added by Mariana Duarte L3Q1 04/2025
                    currentObject.transform.position.x,
                    currentObject.transform.position.y,
                    currentObject.transform.position.z, 
                    currentObject.transform.rotation,
                    prefabs[selectedPrefabIndex]
                );     

                map.objects.Add(mapObject); 
                /*placedObjectPositions.Add(placedObject, currentObject.transform.position);
                Debug.Log($"object position {currentObject.transform.position}");*/
                
            }

            // Marquer les hexagones sous l'objet
            foreach (Hexas.Hexa hexa in targetHexas)
            {
                hexa.IsAvailable = false;
                //hexa.changeColor(Color.red);

                StartCoroutine(ResetColorAfterDelay(hexa, 0.5f,Color.red));


                //changer le HexaType GROUND en WALL
                //hexa.changeType(Hexas.HexaType.BUSH);
                if (placedObject.tag == "Obstacle")
                {
                    hexa.changeType(Hexas.HexaType.WALL);

                }
                else if (placedObject.tag == "Buisson")
                {
                    hexa.changeType(Hexas.HexaType.BUSH);

                }
                else if (placedObject.tag == "Bonus")
                {
                    hexa.changeType(Hexas.HexaType.BONUS);

                }
                else if (placedObject.tag == "Portal")
                {
                    hexa.changeType(Hexas.HexaType.PORTAL);

                }
                occupiedHexas.Add(new Vector2Int(hexa.getX(), hexa.getY()));
            }

            placedObjectsOccupiedHex.Add(placedObject, occupiedHexas); 
            print("placed object "+placedObject);
            print("dans dictionnaire "+placedObjectsOccupiedHex.TryGetValue(placedObject, out List<Vector2Int> o));

        }
        else
        {
            // Réinitialiser les couleurs
            foreach (Hexas.Hexa hexa in targetHexas)
            {
                hexa.defaultColor();
            }
        }

        Destroy(currentObject);
        isDragging = false;
        selectedPrefabIndex = -1;
        targetHexas.Clear();
        allHexasReachable = AllGroundHexasReachable();
    }
    
    //Author :Chen Chrisophe L3Q1 2025
    //Sélectionne l'objet à déposer depuis la liste des objets disponibles
    public void SelectPrefab(int index)
    {
        
        if (isDragging)
        {
            Destroy(currentObject);
            isDragging = false;
        }

        selectedPrefabIndex = index;
        Debug.Log($"Prefab sélectionné : {prefabs[index].name}");

        
        StartDragging();
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Gestion du système de dépôt d'objet sur la carte
    private void HandleDragAndDrop()
    {
        if (Input.GetMouseButtonDown(0))
            if (isDragging)
                StopDragging();

        if (isDragging)
        {
            UpdateInstantiateObjectPosition();

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                RotateObject(-10f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {

                RotateObject(10f);
            }
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Change la rotation de l'objet à placer
    private void RotateObject(float angle)
    {
      currentObject.transform.Rotate(0, angle, 0, Space.World);
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Gestion du système de sélection des objets déjà placés sur la carte
    public void HandleSelectionPlacedObject()
    {
        GameObject clickedObject = GetObjectPlacedOnMouseClicked();

        if (clickedObject != null)
        {
            if (clickedObject == selectedObject)
            {
                DeselectObject(); 
            }
            else
            {
                if (selectedObject != null)
                {
                    DeselectObject();
                }

                SelectObject(clickedObject);
            }
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Retourne le gameobject selectionné
    public GameObject GetObjectPlacedOnMouseClicked()
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImageRectTransform,
            Input.mousePosition,
            null,
            out localPoint))
        {
            Rect rect = rawImageRectTransform.rect;
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            Vector2 viewportPoint = new Vector2(normalizedX, normalizedY);

            Ray ray = mainCamera.ViewportPointToRay(viewportPoint);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placedObjectsLayerMask))
            {
                GameObject ObjectClicked = hit.collider.gameObject;
                return ObjectClicked;
                
            }
        }
        return null;
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Edited by Mariana Duarte L3Q1 04/2025
    //Met en état selectionné l'objet en paramètre
    private void SelectObject(GameObject obj)
    {
        buttonDelete.SetActive(true);

        selectedObject = obj;
        selectedRenderer = obj.GetComponent<Renderer>();

        if(selectedRenderer!=null && selectedRenderer.material.HasProperty("_Color")){
            originalColor = selectedRenderer.material.color;
            selectedRenderer.material.color = Color.red;
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Edited by Mariana Duarte L3Q1 04/2025
    //Remet en état normal l'objet selectionné
    private void DeselectObject()
    {
        if (selectedRenderer != null && selectedRenderer.material.HasProperty("_Color"))
        {
            selectedRenderer.material.color = originalColor; 
        }
        selectedObject = null;
        selectedRenderer = null;

        buttonDelete.SetActive(false);
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Supprime l'objet selectionné de la carte
    public void DeletePlacedObject()
    {
        if (selectedObject == null) return;

        if (placedObjectsOccupiedHex.TryGetValue(selectedObject, out List<Vector2Int> occupiedHexas))
        {
            foreach (Vector2Int hexPos in occupiedHexas)
            {
                Hexas.Hexa hexa = hexaGrid.hexaList.Find(h => h.getX() == hexPos.x && h.getY() == hexPos.y);
                if (hexa != null)
                {
                    hexa.IsAvailable = true;
                    hexa.defaultColor();
                    hexa.changeType(Hexas.HexaType.GROUND); 
                }
            }
            placedObjectsOccupiedHex.Remove(selectedObject);
        }

        Vector3 objPosition = selectedObject.transform.position;
        MapObject mapObjectToRemove = map.objects.Find(mo =>
            mo.x == objPosition.x &&
            mo.y == objPosition.y &&
            mo.z == objPosition.z);
        if (mapObjectToRemove != null)
        {
            map.objects.Remove(mapObjectToRemove);
        }

        Destroy(selectedObject);
        DeselectObject();
        buttonDelete.SetActive(false);
        
        allHexasReachable = AllGroundHexasReachable();
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Retourne vrai ou faux s'il exite des hexagones inaccessibles et les mets en évident s'il y en a 
    public bool AllGroundHexasReachable()
    {
        var hexaGrid = this.hexaGrid;
        var coordToHexa = hexaGrid.hexaList.ToDictionary(h => new Vector2Int(h.getX(), h.getY()));
        var groundHexas = hexaGrid.hexaList.Where(h => h.type == Hexas.HexaType.GROUND || h.type == Hexas.HexaType.BONUS || h.type == Hexas.HexaType.BUSH || h.type == Hexas.HexaType.PORTAL).ToList();

        if (groundHexas.Count == 0) return true;

        var visited = new HashSet<Hexas.Hexa>();
        var queue = new Queue<Hexas.Hexa>();
        var startHex = groundHexas.First();
        queue.Enqueue(startHex);
        visited.Add(startHex);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int x = current.getX(), y = current.getY();


            Vector2Int[] neighbors = x % 2 == 0
            ? new Vector2Int[6] { // Ligne paire 
             new(x , y-1),    // Nord
             new(x , y+1),    // Sud
             new(x +1, y ),    // Nord-Est 
             new(x - 1, y ),// Nord-Ouest 
             new(x+1, y+1 ),    // Sud-Est 
             new(x - 1, y + 1 ) // Sud-Ouest 
                }
             : new Vector2Int[6] { // Ligne impaire 
             new(x , y-1),    // Nord
             new(x , y+1),    // Sud
             new(x + 1, y - 1),// Nord-Est 
             new(x - 1 , y - 1),    // Nord-Ouest
             new(x + 1 , y ),// Sud-Est
             new(x - 1, y )     // Sud-Ouest
                };

            foreach (var coord in neighbors)
            {
                if (coordToHexa.TryGetValue(coord, out var neighbor)
                    && (neighbor.type == Hexas.HexaType.GROUND|| neighbor.type == Hexas.HexaType.BONUS || neighbor.type == Hexas.HexaType.PORTAL || neighbor.type == Hexas.HexaType.BUSH)
                    && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Debug des hexagones isolés
        if (visited.Count != groundHexas.Count)
        {
            var unconnected = groundHexas.Where(h => !visited.Contains(h)).ToList();
            Debug.Log($"Hexagones isolés ({unconnected.Count}) :");
            foreach (var hex in unconnected)
            {

                Debug.Log($"[{hex.getX()}, {hex.getY()}]");
                //StartCoroutine(ResetColorAfterDelay(hex, 5f));// Marqueur visuel

            }
            return false;
        }

        return true;
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Change la couleur d'un hexagone pour un delay donné
    private IEnumerator ResetColorAfterDelay(Hexas.Hexa hex, float delay, Color color)
    {
        
        hex.changeColor(color);

        yield return new WaitForSeconds(delay);

        hex.defaultColor();
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //Returns a boolean indicating whether the object is entirely inside the map 
    public bool isObjectOnMap(GameObject obj, Vector3 position){
        Renderer renderer = obj.GetComponent<MeshRenderer>();

        if (renderer==null)
            return false;

        print("on est sortis du renderer ");
        Vector3 objSize = renderer.bounds.size;

        Vector3 halfSize = objSize * 0.5f;
        Vector3 objMin = position - halfSize;
        Vector3 objMax = position + halfSize;

        Vector3 leftAndTopBorder = Hexa.hexaPosToReal(0,0,0f);
        Vector3 rightAndBottomBorder = Hexa.hexaPosToReal(width,height,0f);

        if(objMin.x < leftAndTopBorder.x//The object is more to the left than the left border
            || objMin.z > leftAndTopBorder.z //The object is more to the top than the top border
            || objMax.x > rightAndBottomBorder.x //The object is more to the right than the right border
            || objMax.z < rightAndBottomBorder.z) //The object is more to the bottom than the bottom border
            
            return false;

        return true;
    }

    //Author : Mariana Duarte L3Q1 04/2025
    //For every hexa where the characters can be instantiated, its attribute isAvailable becomes false
    public void disableCharacterSpawnPoints(){
        for(int i=0; i<5; i++){
            hexaGrid.getHexa(width/2 - 2 + i, height - 2).IsAvailable = false;
            hexaGrid.getHexa(width/2 - 2 + i, 2).IsAvailable = false;
        }
    }

    //added by Doralie Dorcal 04/25 L3Q1
    public void DeleteAllPlacedObjects()
    {
        var objects = placedObjectsOccupiedHex.Keys.ToList();

        foreach (GameObject obj in objects)
        {
            if (placedObjectsOccupiedHex.TryGetValue(obj, out var occ))
            {
                foreach (Vector2Int c in occ)
                {
                    Hexas.Hexa h = hexaGrid.hexaList
                                    .Find(x => x.getX() == c.x && x.getY() == c.y);
                    if (h != null)
                    {
                        h.IsAvailable = true;
                        h.changeType(Hexas.HexaType.GROUND);
                        h.defaultColor();
                    }
                }
            }
            Destroy(obj);
        }

        placedObjectsOccupiedHex.Clear();
        map.objects.Clear();
    }


    //added by Doralie Dorcal 04/25 L3Q1
    public void ResetAllHexaColors()
    {
        if (hexaGrid == null) return;

        foreach (Hexas.Hexa h in hexaGrid.hexaList)
            h.defaultColor();   
    }


   
}


//Created by Christophe Chen L3Q1 2025
public class DragCollisionHandler : MonoBehaviour
{
    public List<Hexas.Hexa> collidingHexas = new List<Hexas.Hexa>();

    //Author :Chen Chrisophe L3Q1 2025
    //Détecte les collisions entre les hexagones et les objets placés
    void OnTriggerEnter(Collider other)
    {
        Hexas.Hexa hexa = MapGenerator.instance.GetHexFromObject(other.gameObject);
        if (hexa != null && !collidingHexas.Contains(hexa))
        {
            collidingHexas.Add(hexa);
            
            hexa.changeColor(IsValidPlacement() ? Color.green : Color.red);
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Détecte les hexagones qui ne sont plus en collision avec les objets placés
    void OnTriggerExit(Collider other)
    {
        Hexas.Hexa hexa = MapGenerator.instance.GetHexFromObject(other.gameObject);
        if (hexa != null && collidingHexas.Contains(hexa))
        {
            collidingHexas.Remove(hexa);
            
            if (!hexa.IsAvailable) return; 
            hexa.defaultColor();
        }
    }

    //Author :Chen Chrisophe L3Q1 2025
    //Vérifie si les hexagones sous l'objete est libre
    public bool IsValidPlacement()
    {
        return collidingHexas.All(h => h.IsAvailable);
    }
}