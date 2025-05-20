using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using AI_Class;
using AI_Util;
using Characters;
using Hexas;
using Misc;
using Mono.Data.Sqlite;
using Stats;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UtilCPU.UtilCPU;
using static CameraController;
using System.Text.RegularExpressions;
using Maps;

// ##################################################################################################################################################
// MAIN
// Author : ?
// Edited by L3Q1, VALAT Thibault and GOUVEIA Klaus
// Edited by Simon Sepiol L3L1
// Edited by Julien D'aboville L3L1 2024
// Commented by L3Q1, VALAT Thibault
// Edited by Mariana Duarte L3Q1 2025
// ##################################################################################################################################################

public class MainGame : MonoBehaviour
{

    // Edited by L3L2, Daniel DE-BERTHIN, 2023-2024
    //0- ForceField / 1- Fury / 2- Sorcery / 3- ExplosiveArrow / 4- POSITION_EXCHANGE / 5- Metamorphosis / 6- Dominate / 7- Lightning 
    public static List<GameObject> EFFETS;

    // Edited by L3L2, Daniel DE-BERTHIN, 2023-2024
    public List<GameObject> effects;




    // Init in character selection menu
    public static StartGameData startGameData;

    // Add Socrate Louis Deriza, L3C1
    public static EndGameDataCharacter[] endGameDataCharacter;

    // Declaraion of variables
    public Mesh hexaFilledMesh;
    public Mesh hexaHollowMesh;
    public Mesh hexaWallMesh;
    public GameObject ruinsMap;
    public GameObject libreMap; //L3C1, Y.H
    public GameObject areneMap; // Added by Timothé MIEL - L3L1
    public GameObject foretMap; // Added by Timothé MIEL - L3L1
    public GameObject mapObjects; //Added by Mariana Duarte L3Q1 04/2025
    public GameObject mapHexaFolder;
    public GameObject arrow;
    public GameObject hexaHighlight;
    public GameObject hexasFolder;
    public GameObject hexaTemplate;
    public GameObject charactersFolder;
    public GameObject characterTemplate;
    public List<GameObject> characterTemplateModels;
    public GameObject particleExplosion;
    public GameObject particleHeal;
    public GameObject damageValueDisplay;




    //Added by Julien D'aboville L3L1 2024
    public GameObject particleRagePrefab;
    public AudioSource shieldBreaksource;
    public AudioSource howlsource;
    public GameObject particleSmoke;
    public AudioSource hammerExplosionSource;
    public HexaGrid hexas;

    public GameObject particleStormPrefab;
    public GameObject particleSoulAbsorptionPrefab;
    public GameObject particlePassiveForgeron;
    public GameObject particlePassiveDruide;
    public GameObject particlePassiveNetherfang;
    public GameObject particleHammerLaunch;
    public GameObject particleFreeze;
    public GameObject particleHowl;
    public GameObject particleDead;



    public CameraController cameraController;
    public Transform cameraPos;
    public GameObject UICurrentChar;
    public GameObject UIEnemyChar;
    public GameObject Initiative;
    public GameObject BonusCard;
    public List<Texture> charSquares;
    public GameObject UICharTurns;
    public GameObject UICharTurnTemplate;
    public GameObject UIAction;
    public GameObject UIPauseMenu;


    //Author : L3C1 CROUZET Oriane, 08/04/2023
    public GameObject UIHelpMenu;
    public Text currentCharacterText;
    public GameObject UITooltip;
    public Text tooltipText;
    public int doomDamage;

    //public Character.CharClass classCharacter;
    public GameObject UIVictoryScreen;
    public List<Texture> charCards;
    public int tileW;
    public int tileH;
    public bool lockedCamera;
        public bool debugMode;
    public bool pauseMenu;
    public bool saveMenu;
    //public bool helpMenu;

    public int frame;
    public bool updateUI;
    public bool updateMouseHover;
    public HexaGrid hexaGrid;
    public Vector3 mousePosOld;
    public Vector3 mousePos;
    public Hexa hexaHoveredOld;
    public Hexa hexaHovered;
    public static Character currentCharControlled;
    public int currentCharControlledID;

    public List<GameObject> UICharTurnsList;
    public List<GameObject> pathFinderDisplay;
    public List<GameObject> lineOfSightDisplay;
    public List<GameObject> dangerDisplay;
    public List<Point> pathWalk;
    public Character charHovered;
    public int attackUsed;
    public Point attackUsedTargetHexa;
    public CharsDB.Attack attackUsedAttack;
    public int pathWalkpos;
    public int newTurn;
    public int AIwaitingTime;
    public int winner;
    public static int bonusTeam;
    public StatsGame statsGame;
    public Color pathDisplayColor;
    public Color allPathsDisplayColor;
    public Color lineOfSightColor;
    public Color blockedSightColor;
    public Color AoEColor;
    public Color neutralBonusDisplayColor;
    public Color redBonusDisplayColor;
    public Color blueBonusDisplayColor;
    public LayerMask mapObjectsLayer; //Added by Mariana Duarte L3Q1 04/2025

    // turn counter for the damage over time
    // Added by Youcef MEDILEH, L3C1
   
    public int turnCounterDamageOverTimeTEAM1;

    public int turnCounterDamageOverTimeTEAM2;
    //Edited by Julien D'aboville L3L1 2024
    public enum ActionType
    {
        MOVE,
        ATK1,
        ATK2,
        ATK3,
        ATK4,
        SKIP,
        WAIT
    };

    
    public ActionType actionType;
    public List<ActionAIPos> decisionSequence; // AI decisions
    public List<(Character chr, ActionAIPos act)> decisionSequenceCPU; // CPU decisions
    public Slider slider;
    public int countTurn;
    public bool charge = false;
    public Material pathDisplayMat;
    public Material bonusDisplayMat;

    public Material allPathsDisplayMat;
    public Material lineOfSightMat;
    public Material blockedSightMat;
    public Material aoeMat;
    List<Hexa> bonusPoints;
    List<Hexa> poisonnedHexas;
    Hexa caseBonus;
    // Added by Timothé MIEL - L3L1
    public List<Hexa> portals;
    public bool portalOpened;
    public Hexa portal1;
    public Hexa portal2;
    public float tourActivationPortail;
    public Color openedPortal = new Color(0.98f, 0.06f, 0.06f);
    public Color closedPortal = new Color(0.35f, 0.2f, 0.06f);

    public bool bonusFixe;

    public string actualLoad = "0";

  
    public float tour = 1;


    public GameObject panelSave;
    public InputField saveNameInput;

    public GameObject badSaveNameText;

    //Awake is called before Start
    void Awake()
    {
        Application.targetFrameRate = 75;
        QualitySettings.vSyncCount = 0;
    }

    // Start is called before the first frame update
    // Initialisation of variables and game settings
    //Author : ??
    //Edited by L3Q1 VALAT Thibault
    void Start()
    {
        EFFETS = effects;
        cameraController = Camera.main.GetComponent<CameraController>();

        //Initialisation of textures and colors
        pathDisplayMat = new Material(Shader.Find("Standard"));
        bonusDisplayMat = new Material(Shader.Find("Standard"));
        allPathsDisplayMat = new Material(Shader.Find("Standard"));
        lineOfSightMat = new Material(Shader.Find("Standard"));
        blockedSightMat = new Material(Shader.Find("Standard"));
        aoeMat = new Material(Shader.Find("Standard"));

        pathDisplayMat.color = pathDisplayColor;
        allPathsDisplayMat.color = allPathsDisplayColor;
        lineOfSightMat.color = lineOfSightColor;
        blockedSightMat.color = blockedSightColor;
        aoeMat.color = AoEColor;

        // Initialisation of Hexas and Characters global variables
        Hexa.hexasFolder = hexasFolder;
        Hexa.mapHexaFolder = mapHexaFolder;
        Hexa.hexaFilledMesh = hexaFilledMesh;
        Hexa.hexaHollowMesh = hexaHollowMesh;
        Hexa.hexaWallMesh = hexaWallMesh;
        Hexa.hexaTemplate = hexaTemplate;
        Character.characterTemplate = characterTemplate;
        Character.characterTemplateModels = characterTemplateModels;
        Character.charactersFolder = charactersFolder;
        CharsDB.initCharsDB();
        doomDamage = CharsDB.list[(int)CharClass.ENVOUTEUR].skill_2.effectValue;
        bonusPoints = new List<Hexa>();
        poisonnedHexas = new List<Hexa>();
        Hexa.offsetX = -((tileW - 1) * 0.75f) / 2;
        Hexa.offsetY = -((tileH - 1) * -0.86f + ((tileW - 1) % 2) * 0.43f) / 2;
        List<Point> casesBonus = new List<Point>();
        hexaHighlight.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.25f);
        frame = 0;
        updateUI = true;
        updateMouseHover = true;
        bonusTeam = -1;

        //Added by L3C1 CROUZET Oriane, 08/04/2023
        pauseMenu = false;
        //helpMenu = false;
        

        // Added by Youcef MEDILEH, L3C1
        turnCounterDamageOverTimeTEAM1 = 0;
        turnCounterDamageOverTimeTEAM2 = 0;

        // modified simon Sepiol l3l1
        // Initialisation of game data if it's not (it should be in the main menu)
        if (startGameData == null)
        {
            Debug.Log("startGameData == null");
            startGameData = new StartGameData();
            startGameData.loadSave = null;
            startGameData.charsTeam1 = new List<CharClass>();
            startGameData.charsTeam1.Add(CharClass.MAGE);
            startGameData.charsTeam1.Add(CharClass.FORGERON);
            startGameData.charsTeam2 = new List<CharClass>();
            startGameData.charsTeam2.Add(CharClass.MAGE);
            startGameData.charsTeam2.Add(CharClass.FORGERON);
            startGameData.player1Type = PlayerType.AI_EASY;
            startGameData.player2Type = PlayerType.AI_HARD;
            startGameData.mapChosen = 3;
            startGameData.mapName = "";
            startGameData.map = null;
            //cameraController.EditorMode();
        }

        // added simon sepiol l3l1 2024
        if (!(startGameData.loadSave == null))
        {
            //print("on pense etre dans save au debut de la partie");
            actualLoad = startGameData.loadSave;
            //Debug.Log("startGameData.loadSave"+actualLoad);
            loadGame("/Save/" + actualLoad + ".db");
        }
        
        else
        {
            //print("on est pas dans save");
            // Initialisation of the map (hexa grid)
            slider.value = startGameData.slider;
            hexaGrid = new HexaGrid();
            if (startGameData.mapChosen < 4){
                switch (startGameData.mapChosen)
                {
                    case 0:
                        {
                            Debug.Log("Creating map from ruins file");
                            hexaGrid.createGridFromFile(Application.streamingAssetsPath + "/Data/Map/ruins");
                            tileW = hexaGrid.w;
                            tileH = hexaGrid.h;
                            ruinsMap.SetActive(true);
                            foreach (Hexa hexa in hexaGrid.hexaList)
                            {
                                if (hexa.type == HexaType.VOID || hexa.type == HexaType.WALL)
                                {
                                    hexa.go.GetComponent<Renderer>().enabled = false;
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            Debug.Log("Creating map from arene file");
                            hexaGrid.createGridFromFile(Application.streamingAssetsPath + "/Data/Map/arene");
                            tileW = hexaGrid.w;
                            tileH = hexaGrid.h;
                            areneMap.SetActive(true);
                            foreach (Hexa hexa in hexaGrid.hexaList)
                            {
                                if (hexa.type == HexaType.VOID || hexa.type == HexaType.WALL)
                                {
                                    hexa.go.GetComponent<Renderer>().enabled = false;
                                }
                            }
                            InitializePortals();
                            bonusFixe = true;
                            break;
                        }
                    case 2:
                        {
                            Debug.Log("Creating map from foret file");
                            hexaGrid.createGridFromFile(Application.streamingAssetsPath + "/Data/Map/foret");
                            tileW = hexaGrid.w;
                            tileH = hexaGrid.h;
                            foretMap.SetActive(true);
                            foreach (Hexa hexa in hexaGrid.hexaList)
                            {
                                if (hexa.type == HexaType.VOID || hexa.type == HexaType.WALL)
                                {
                                    hexa.go.GetComponent<Renderer>().enabled = false;
                                }
                            }
                            bonusFixe = true;
                            break;
                        }
                    case 3:
                        {
                            Debug.Log("Creating empty map");
                            hexaGrid.createRectGrid(34, 30);
                            tileW = hexaGrid.w;
                            tileH = hexaGrid.h;
                            break;
                        }
                    
                }
            }
            
        //Added by Mariana Duarte L3Q1 04/2025
        else{
            Debug.Log("Instatiation de la carte sauvegardee");

            string pathToMapObjects = Application.persistentDataPath+"/Save/Maps/"+startGameData.mapName+".db";
            string pathToMapGrid = Application.persistentDataPath+"/Save/Grids/"+startGameData.mapName+"Grid.db";

            startGameData.map = SaveManager.createMapFromFile(pathToMapObjects, pathToMapGrid);
            hexaGrid.createGridFromFile(pathToMapGrid);
            tileW = hexaGrid.w;
            tileH = hexaGrid.h;

            foreach (MapObject obj in startGameData.map.objects){
                Vector3 objectPosition = new Vector3(obj.x,obj.y,obj.z);
                GameObject objectPrefab = Instantiate(obj.gameObject,objectPosition,obj.getRotation());
                objectPrefab.transform.SetParent(mapObjects.transform);

                MeshRenderer meshRenderer = objectPrefab.GetComponent<MeshRenderer>();
                if (meshRenderer != null){
                    meshRenderer.enabled = true;
                }
            }
        }

            if (!bonusFixe && !(startGameData.mapChosen == 4)) // Si mapChosen = 4, alors la carte est personnalisee par l'utilisateur
            {
                caseBonus = initBonus();
            }
            else
            {
                if (startGameData.mapChosen == 1 || startGameData.mapChosen == 2) // Si map arene ou foret, bonus fixe au milieu
                {
                    caseBonus = initFixedBonus(17, 15, 2);
                }
            }
            Initiative.SetActive(true);

            int nb = 0;
            // Put characters on the grid
            for (int i = 0; i < 5; i++)
            {
                
                if (i < startGameData.charsTeam1.Count)
                {
                    Debug.Log(startGameData.charsTeam1[i]);
                    hexaGrid.addChar(
                        startGameData.charsTeam1[i],
                        tileW / 2 - 4 + 2 + i,
                        tileH - 2,
                        0
                    );
                    nb++;
                }
                if (i < startGameData.charsTeam2.Count)
                {
                    hexaGrid.addChar(startGameData.charsTeam2[i], tileW / 2 - 4 + 2 + i, 2, 1);
                    nb++;
                }
            }

            endGameDataCharacter = createEndDataGameList(nb);

            hexaGrid.getCharList().Sort();

            foreach (Character c in hexaGrid.getCharList())
            {
                hexaGrid.getHexa(c.getX(), c.getY()).changeType(HexaType.GROUND);
            }

            for (int i = hexaGrid.getCharList().Count; i <= 10; i++)
                Initiative.transform.GetChild(i).transform.position = new Vector3(10000, 10000, 0);

            // Initialisation of the current character cursor
            currentCharControlledID = 0;
            currentCharControlled = hexaGrid.getCharList()[currentCharControlledID];
        }
        // Initialisation of the AI
        decisionSequence = new List<ActionAIPos>();
        decisionSequenceCPU = new List<(Character chr, ActionAIPos act)>();
        AI.hexaGrid = hexaGrid;

        AIHard.learn = false;
        AIUtil.hexaGrid = hexaGrid;

        mousePos = Input.mousePosition;
        hexaHovered = null;
        hexaHoveredOld = null;
        charHovered = null;

        countTurn = 0;

        pathFinderDisplay = new List<GameObject>();
        lineOfSightDisplay = new List<GameObject>();
        pathWalk = null;
        attackUsed = 0;
        pathWalkpos = 0;
        newTurn = 0;
        winner = -1;
        statsGame = new StatsGame();
        actionType = ActionType.MOVE;
        UICharTurns.SetActive(true);
        UIAction.SetActive(true);

        //Author : L3C1 CROUZET Oriane
        UITooltip.SetActive(false);

        { // Init character turn list UI
            int i = 0;
            foreach (Character c in hexaGrid.getCharList())
            {
                GameObject go = GameObject.Instantiate(UICharTurnTemplate, UICharTurns.transform);
                go.SetActive(false);
                go.transform.localPosition = new Vector3(200 + i, 0, 0);
                go.transform.GetChild(0).GetComponent<Image>().color =
                    (c.getTeam() == 0) ? Character.TEAM_1_COLOR : Character.TEAM_2_COLOR;
                go.transform.GetChild(1).GetComponent<RawImage>().texture = charCards[
                    (int)c.charClass
                ];
                UICharTurnsList.Add(go);
                i += 80;
            }
        }
        displayInitiative();

        // Initialisation de la caméra
        // Si au moins l'un des joueurs n'est pas humain, la caméra ne tournera pas automatiquement
        lockedCamera = (
            startGameData.player1Type != PlayerType.HUMAN
            || startGameData.player2Type != PlayerType.HUMAN
        );
                AIwaitingTime = lockedCamera ? 0 : 20;
        Debug.Log("Debut de la partie");
        // Si le premier personnage contrôlé est de l'équipe "opposée", rotation instantanée
        cameraController.setOldCharFocused(currentCharControlled);
        if (currentCharControlled.team == 1)
        {
            cameraController.GoToCharacter(currentCharControlled, true);
        }
    }

    

    // created by simon Sepiol L3L1
    public void saveGame(string path){
        // on enleve le badSaveNameText si il etait present
        badSaveNameText.SetActive(false);

        if(IsValidFileName(path)){
            print("nom ok");
            string savePath="/Save/"+path+".db";
            writeSaveGame(savePath);

            actualLoad=path;
            startGameData.loadSave=path;
            saveMenu=false;
            panelSave.SetActive(false);
        }
        else{
            print("nom pas ok");
            badSaveNameText.SetActive(true);
        }
        

    }


    static bool IsValidFileName(string fileName)
    {
        // Expression régulière pour autoriser uniquement les lettres, les chiffres, les espaces et les tirets
        Regex regex = new Regex("^[a-zA-Z0-9\\s-]*$");

        // Vérifie si le nom de fichier correspond à l'expression régulière
        return regex.IsMatch(fileName);
    }

    // Update is called once per frame
    //Author : ??
    //Edited by L3Q1 VALAT Thibault
    // Edited by L3C1 Youcef MEDILEH :
    // - ajout de la gestion de la zone de poison
    // - ajout de la capacité 2 des personnages
    // Edited by L3C1 Socrate Louis Deriza, ajout du bouton compétence 2
    //Edited by L3C1 Yuting HUANG, le 01/04/2023, ajout d'un temps limité
    //Edited by L3C1 CROUZET Oriane, 08/04/2023, ajout du bouton help
    void Update()
    {
        // Update mouse position
        mousePosOld = mousePos;
        mousePos = Input.mousePosition;


        //Change controlled character
        getHoveredHexa();
        if (
            Input.GetMouseButton(1)
            && charHovered != null
            && charHovered.PA > 0
            && charHovered.getTeam() == currentCharControlled.getTeam()
        )
        {
            currentCharControlled = charHovered;
        }
        frame++;

        if (currentCharControlled.isFreezed())
        {
            currentCharControlled.PA = 0;
            currentCharControlled.setFreezed(false);
            nextTurn();
        }

        HandleBushes();
        
        // mark poisoned hexas
        // Added by Youcef MEDILEH, L3C1, le 23/03/2023
        foreach (Hexa hexa in poisonnedHexas)
        {
            hexa.go.GetComponent<Renderer>().material.color = Color.green;
        }

        //Added by Julien D'aboville L3L1 2024

        if (currentCharControlled.getStunned() == true)
        {
            currentCharControlled.setPa(0);
            currentCharControlled.setStunned(false);
            nextTurn();
        }

        // PAUSE MENU

        if (saveMenu)
        {
                if (Input.GetMouseButtonDown(0))
                {
                    Transform child_retour = GameObject.Find("Menu Pause").transform.GetChild(1).GetChild(1);
                    Transform child_valider = GameObject.Find("Menu Pause").transform.GetChild(1).GetChild(0);
                    

                    if(IsPointerOver(child_retour.GetComponent<RectTransform>())){
                        saveMenu=false;
                        panelSave.SetActive(false);
                        print("retour");
                    }

                    if(IsPointerOver(child_valider.GetComponent<RectTransform>())){
                        saveGame(saveNameInput.text);
                        print("save");
                    }

                }
            }
        else if (pauseMenu)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Transform child_restart = GameObject
                    .Find("Menu Pause")
                    .transform.GetChild(0)
                    .GetChild(3);

                Transform child_save = GameObject
                    .Find("Menu Pause")
                    .transform.GetChild(0)
                    .GetChild(2);
                
                Transform child_quit = GameObject
                    .Find("Menu Pause")
                    .transform.GetChild(0)
                    .GetChild(4);

                Transform child_back = GameObject
                    .Find("Menu Pause Button")
                    .transform.GetChild(0);

                Transform child_rotate = GameObject
                    .Find("Menu Pause")
                    .transform.GetChild(0)
                    .GetChild(5);

                // Save the current game
                if (IsPointerOver(child_save.GetComponent<RectTransform>()))
                {
                    //print("genre le panel si besoin");
                    generateSaveGamePanelOrSave();
                }
                
                // To restart the game:
                else if (IsPointerOver(child_restart.GetComponent<RectTransform>()))
                {
                    Debug.Log("Restarting game");
                    
                    SceneManager.LoadScene(1);
                }
                // to quit the game:
                else if (IsPointerOver(child_quit.GetComponent<RectTransform>()))
                {
                    MainMenu.startGameData = new StartGameData();
                    MainMenu.startGameData.slider = slider.value;
                    SceneManager.LoadScene(0);
                }
                /// To go back:
                else if (IsPointerOver(child_back.GetComponent<RectTransform>()))
                {
                    pauseMenu = false;
                    UIPauseMenu.SetActive(false);
                    cameraController.IsUIOpen(false);
                }
                // To rotate the camera :
                else if (IsPointerOver(child_rotate.GetComponent<RectTransform>()))
                {
                    cameraController.ReverseCamera(true, false);
                }
             }

            
        }
        //When there is still no winner
        else if (winner == -1)
        {
            
            // OPEN PAUSE MENU
            // nouveau system bouton
            // ancienne coordonees : mousePos.x >= Screen.width / 2 - 160 && mousePos.x < Screen.width / 2 && mousePos.y >= Screen.height - 90 && mousePos.y < Screen.height

            // recupere le bouton pause avec les espaces
            Transform child_pauseMenuButton = GameObject
                .Find("Menu Pause Button")
                .transform.GetChild(0);

            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOver(child_pauseMenuButton.GetComponent<RectTransform>()))
                {
                    //print("clique sur le menu");
                    pauseMenu = true;
                    UIPauseMenu.SetActive(true);
                    cameraController.IsUIOpen(true);
                }
            }

            //Author :  L3C1 CROUZET Oriane, 08/04/2023
            //OPEN HELP MENU

            // ancienne coordonées :mousePos.x >= Screen.width / 2 && mousePos.x < Screen.width / 2 + 160 && mousePos.y >= Screen.height - 90 && mousePos.y < Screen.height
            // oui ils ont mis un espace apres le nom de l'objet
            Transform child_helpMenuButton = GameObject
                .Find("Menu Help Button ")
                .transform.GetChild(0);

            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOver(child_helpMenuButton.GetComponent<RectTransform>()))
                {
                    //helpMenu = true;
                    //UIHelpMenu.SetActive(true);
                    //cameraController.IsUIOpen(true);
                }
            }
        }

        //Edited by Socrate Louis Deriza L3C1
       
        // MAIN GAME LOOP
        if (winner == -1)
        {
           
   /*     if (pathWalk != null && charge)
            {
                //print("CHARGE");
              //charge
            }*/
            if (pathWalk != null)
            {
                // Walking animation when going from an hexa to another
                walkingAnimation();
            }
            else if (attackUsed > 0)
            {
                // Attack animation
                if (attackUsed == 1)
                {
                    useAttack();
                }
                attackUsed--;
                // Interaction with the game
            }
            else if (!pauseMenu)
            {
                PlayerType currentPlayerType = whoControlsThisChar(currentCharControlled);
                // ACTIONS FOR HUMAN PLAYERS
                if (currentPlayerType == PlayerType.HUMAN)
                {
                    // HOVER DETECTION : hovered hexa is stored in hexaHovered
                    getHoveredHexa();

                    // W Key : Move
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        Debug.Log("W RIZZ");
                        actionType = ActionType.MOVE;
                        updateMouseHover = true;
                        updateUI = true;
                    }

                    // X Key : Attack
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        actionType = ActionType.ATK1;
                        updateMouseHover = true;
                        updateUI = true;
                    }

                    // C Key : Skill
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        if (currentCharControlled.skillAvailable)
                            actionType = ActionType.ATK2;

                        updateMouseHover = true;
                        updateUI = true;
                    }

                    // V Key : Skill2
                    // Added by Youcef MEDILEH, L3C1
                    //Edited by L3C1 CROUZET Oriane
                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        if (currentCharControlled.skillAvailable2)
                            actionType = ActionType.ATK3;

                        updateMouseHover = true;
                        updateUI = true;
                    }


                    /*      if (Input.GetKeyDown(KeyCode.F) && currentCharControlled.getCharClass()== CharClass.FORGERON)
                          {
                              //print("Forgeron charge");

                              if (currentCharControlled.skillAvailable3)
                                  actionType = ActionType.MOVE;

                              updateMouseHover = true; updateUI = true;
                          }*/

                    // F Key : Skill3
                    // Added :Julien D'aboville L3L1 2024
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (currentCharControlled.isSkill3Up())
                            actionType = ActionType.ATK4;

                        updateMouseHover = true; updateUI = true;

                    }

                    // B Key : Skip
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        currentCharControlled.PA = 0;
                        nextTurn();
                    }

                    // Left Click : action (Move / Attack) or change action Type (UI) or skip turn
                    // Click on the left top buttons, under characters card
                    //Edited by Socrate Louis Deriza L3C1
                    


                    //IsPointerOver(child_mouvement.GetComponent<RectTransform>() )
                    // mouvement mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 835 && mousePos.y < Screen.height - 835 + 24 * 1.8
                    //attack : mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 890 && mousePos.y < Screen.height - 890 + 24 * 1.8
                    //competence 1 : mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 945 && mousePos.y < Screen.height - 945 + 24 * 1.8
                    //competence 2 : mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 1000 && mousePos.y < Screen.height - 1000 + 24 * 1.8
                    //skip : mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 1055 && mousePos.y < Screen.height - 1055 + 24 * 1.8



                    Transform child_attaque_basique = UIAction.transform.GetChild(0);
                    Transform child_competence1 = UIAction.transform.GetChild(1);
                    Transform child_competence2 = UIAction.transform.GetChild(2);
                    Transform child_competence3 = UIAction.transform.GetChild(3);
                    Transform child_skip = UIAction.transform.GetChild(4);
                    Transform child_mouvement = UIAction.transform.GetChild(5);
                    if (
                        !cameraController.cameraMoved
                        && Input.GetMouseButtonUp(0)
                                            )
                    {
                        // Deplacement
                        if (IsPointerOver(child_mouvement.GetComponent<RectTransform>()))
                        {
                            actionType = ActionType.MOVE;
                        }
                        // Attack
                        else if (IsPointerOver(child_attaque_basique.GetComponent<RectTransform>()))
                        {
                            actionType = ActionType.ATK1;
                        }
                        // Special attack 1
                        else if (IsPointerOver(child_competence1.GetComponent<RectTransform>()))
                        {
                            if (currentCharControlled.skillAvailable)
                                actionType = ActionType.ATK2;
                        }
                        // Special attack 2
                        else if (IsPointerOver(child_competence2.GetComponent<RectTransform>()))
                        {
                            if (currentCharControlled.skillAvailable2)
                                actionType = ActionType.ATK3;
                        }
                        else if (IsPointerOver(child_competence3.GetComponent<RectTransform>()))
                        {
                            if (currentCharControlled.skillAvailable3)
                                actionType = ActionType.ATK4;
                        }
                        // Next turn
                        else if (IsPointerOver(child_skip.GetComponent<RectTransform>()))
                        {
                            currentCharControlled.PA = 0;
                            nextTurn();
                        }
                        else
                        {
                            switch (actionType)
                            {
                                case ActionType.MOVE:

                                    //Added by Socrate Louis Deriza, L3C1
                                    //   getEndGameDataCharacterFromTheList(currentCharControlled).addAction(ActionType.MOVE);
                                    int nbCase = actionMove(hexaHovered);
                                    //      new Hexa(HexaType.GROUND, 10, 10)
                                    //     getEndGameDataCharacterFromTheList(currentCharControlled).addMovement(nbCase);
                                    break;
                                case ActionType.ATK1:
                                    int dmg = actionUseAttack(actionType, hexaHovered);
                                    //Added by Socrate Louis Deriza, L3C1

                                    //debugage de attaque envouteur
                                    //print(currentCharControlled.getName());
                                    //print(hexaHovered);
                                    //print(dmg);
                                    getEndGameDataCharacterFromTheList(currentCharControlled)
                                        .addAction(ActionType.ATK1);
                                    saveDataBasicAttack(currentCharControlled, hexaHovered, dmg);
                                    /*
                                    if (currentCharControlled.getName() == "Soigneur")
                                    {
                                        if (hexaHovered.charOn != null)
                                        {
                                            getEndGameDataCharacterFromTheList(hexaHovered.charOn).addAnHealingAction(dmg);
                                        }
                                    }
                                    else if (currentCharControlled.getName() == "Valkyrie" || currentCharControlled.getName() == "Mage")
                                    {
                                        List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(currentCharControlled, 1);
                                        for (int i = 0; i < listCharacter.Count; i++)
                                        {
                                            getEndGameDataCharacterFromTheList(currentCharControlled).addDamage(dmg);
                                        }
                                    }
                                    else
                                    {
                                        if(currentCharControlled.getName() != "Envouteur")
                                        {
                                            
                                            getEndGameDataCharacterFromTheList(currentCharControlled).addDamage(dmg);
                                            
                                        }
                                    }*/
                                    break;
                                case ActionType.ATK2:
                                    //Added by Socrate Louis Deriza, L3C1
                                    int dmg2 = actionUseAttack(actionType, hexaHovered);
                                    getEndGameDataCharacterFromTheList(currentCharControlled)
                                        .addAction(ActionType.ATK2);
                                    saveDataSkill1(currentCharControlled, hexaHovered, dmg2);
                                    /*
                                    if (currentCharControlled.getName() == "Soigneur")
                                    {
                                        List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(currentCharControlled, 2);
                                        for (int i = 0; i < listCharacter.Count; i++)
                                        {
                                            getEndGameDataCharacterFromTheList(listCharacter[i]).addAnHealingAction(dmg2);
                                        }
                                    }
                                    else if (currentCharControlled.getName() == "Valkyrie" || currentCharControlled.getName() == "Mage")
                                    {
                                        List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(currentCharControlled, 2);
                                        for (int i = 0; i < listCharacter.Count; i++)
                                        {
                                            getEndGameDataCharacterFromTheList(listCharacter[i]).addDamage(dmg2);
                                        }
                                    }
                                    else
                                    {
                                        if (currentCharControlled.getName() != "Envouteur")
                                        {
                                            if (hexaHovered.charOn != null)
                                            {
                                                getEndGameDataCharacterFromTheList(hexaHovered.charOn).addDamage(dmg2);
                                            }
                                        }
                                    }*/
                                    break;

                                // Added :Julien D'aboville L3L1 2024
                                case ActionType.ATK4:
                                    int dmg4 = actionUseAttack(actionType, hexaHovered);
                                    getEndGameDataCharacterFromTheList(currentCharControlled)
                                        .addAction(ActionType.ATK4);
                                    saveDataSkill3(currentCharControlled, hexaHovered, dmg4);
                                    break;
                                case ActionType.ATK3:
                                    int dmg3 = actionUseAttack(actionType, hexaHovered);
                                    getEndGameDataCharacterFromTheList(currentCharControlled)
                                        .addAction(ActionType.ATK3);
                                    saveDataSkill2(currentCharControlled, hexaHovered, dmg3);
                                    break;
                                /*
                                if (currentCharControlled.getName() == "Soigneur")
                                {
                                    List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(currentCharControlled, 3);
                                    for (int i = 0; i < listCharacter.Count; i++)
                                    {
                                        getEndGameDataCharacterFromTheList(listCharacter[i]).addAnHealingAction(dmg3);
                                    }
                                }
                                else if (currentCharControlled.getName() == "Voleur")
                                {

                                    if (hexaHovered.charOn != null)
                                    {
                                        getEndGameDataCharacterFromTheList(currentCharControlled).addAnHealingAction(dmg3);
                                        getEndGameDataCharacterFromTheList(hexaHovered.charOn).addDamage(dmg3);
                                    }
                                }
                                else if (currentCharControlled.getName() == "Valkyrie" || currentCharControlled.getName() == "Mage" || currentCharControlled.getName() == "Archer" || currentCharControlled.getName() == "Guerrier")
                                {
                                    List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(currentCharControlled, 3);
                                    for (int i = 0; i < listCharacter.Count; i++)
                                    {
                                        getEndGameDataCharacterFromTheList(listCharacter[i]).addDamage(dmg3);
                                    }
                                }
                                else
                                {
                                    if (currentCharControlled.getName() != "Envouteur")
                                    {
                                        if (hexaHovered.charOn != null)
                                        {
                                            getEndGameDataCharacterFromTheList(hexaHovered.charOn).addDamage(dmg3);
                                        }
                                    }
                                }*/

                                // Added ATK3 by Youcef MEDILEH, L3C1
                                case ActionType.SKIP:
                                    //Added by Socrate Louis Deriza, L3C1
                                    currentCharControlled.PA = 0;
                                    nextTurn();
                                    getEndGameDataCharacterFromTheList(currentCharControlled)
                                        .addAction(ActionType.SKIP);
                                    break;
                            }
                        }
                    }
                }
                //If the player isnot human
                else
                {
                    if (newTurn > AIwaitingTime)
                    {
                        if (decisionSequence.Count == 0 && decisionSequenceCPU.Count == 0)
                        {
                            switch (currentPlayerType)
                            {
                                case PlayerType.HUMAN: // failsafe
                                case PlayerType.AI_CPU_Defense:
                                    decisionSequenceCPU = CPU.decideDefense(
                                        currentCharControlled.getTeam(),
                                        hexaGrid
                                    );
                                    break;
                                case PlayerType.AI_CPU_Offense:
                                    decisionSequenceCPU = CPU.decideOffense(
                                        currentCharControlled.getTeam(),
                                        hexaGrid,
                                        caseBonus
                                    );
                                    //print("DECIDE OFFENSE VALIDE");
                                    break;
                                case PlayerType.AI_CPU: decisionSequenceCPU = CPU.chooseStrategy(currentCharControlled.getTeam(), hexaGrid, caseBonus, startGameData);  break;


                                /*case PlayerType.AI_CPU: if(CPUStrategy == 0) decisionSequenceCPU = CPU.decideDefense(currentCharControlled.getTeam(), hexaGrid);  break;
                                 *					      else if(CPUStrategy == 1) decisionSequenceCPU = CPU.decideOffense(currentCharControlled.getTeam(), hexaGrid);  break;
                                 *					      else if(CPUStrategy == 2) decisionSequenceCPU = CPU.chooseStrategy(currentCharControlled.getTeam(), hexaGrid);  break;
                                */

                                case PlayerType.AI_EASY:
                                    decisionSequence = AIEasy.decide(currentCharControlled);
                                    break;
                                case PlayerType.AI_MEDIUM:
                                    decisionSequence = AIMedium.decide(
                                        currentCharControlled,
                                        statsGame
                                    );
                                    break; //change for the AI medium afterwards
                                case PlayerType.AI_HARD:
                                    decisionSequence = AIHard.decide(
                                        currentCharControlled,
                                        statsGame
                                    );
                                    break;
                            }
                            //print("CURRENT PLAYER TYPE CHOISIT");
                        }
                        else
                        {
                            if (decisionSequence.Count > 0 || decisionSequenceCPU.Count > 0)
                            {
                                ActionAIPos actionAIPos;
                                if (decisionSequence.Count > 0)
                                {
                                    actionAIPos = decisionSequence[0];
                                    decisionSequence.RemoveAt(0);
                                }
                                else
                                {
                                    actionAIPos = decisionSequenceCPU[0].act;
                                    decisionSequenceCPU.RemoveAt(0);
                                }
                                switch (actionAIPos.action)
                                {
                                    case ActionType.MOVE:
                                        int nbCase = actionMove(hexaGrid.getHexa(actionAIPos.pos));
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addMovement(nbCase);
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.MOVE);
                                        break;
                                    case ActionType.ATK1:
                                        int dmg = actionUseAttack(
                                            actionAIPos.action,
                                            hexaGrid.getHexa(actionAIPos.pos)
                                        );
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.ATK1);
                                        saveDataBasicAttack(
                                            currentCharControlled,
                                            hexaHovered,
                                            dmg
                                        );
                                        break;
                                    case ActionType.ATK2:
                                        int dmg2 = actionUseAttack(
                                            actionAIPos.action,
                                            hexaGrid.getHexa(actionAIPos.pos)
                                        );
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.ATK2);
                                        saveDataSkill1(currentCharControlled, hexaHovered, dmg2);
                                        break;
                                    case ActionType.ATK3:
                                        int dmg3 = actionUseAttack(
                                            actionAIPos.action,
                                            hexaGrid.getHexa(actionAIPos.pos)
                                        );
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.ATK3);
                                        saveDataSkill2(currentCharControlled, hexaHovered, dmg3);
                                        break;
                                    // Added :Julien D'aboville L3L1 2024
                                    case ActionType.ATK4:
                                        int dmg4 = actionUseAttack(
                                            actionAIPos.action,
                                            hexaGrid.getHexa(actionAIPos.pos)
                                        );
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.ATK4);
                                        saveDataSkill3(currentCharControlled, hexaHovered, dmg4);
                                        break;
                                    case ActionType.SKIP:
                                        currentCharControlled.PA = 0;
                                        nextTurn();
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.SKIP);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            /**TODO À confirmer qu'aucun problème
                            
                            else // decisionSequenceCPU.Count > 0
                            {
                                setControlledCharacter(decisionSequenceCPU[0].chr);
                                ActionAIPos actionAIPos = decisionSequenceCPU[0].act;
                                decisionSequenceCPU.RemoveAt(0);

                                Debug.Log("Hmmm " + currentCharControlled.getName());
                                switch (actionAIPos.action)
                                {
                                    
                                    case ActionType.MOVE:
                                        int nbCase = actionMove(hexaGrid.getHexa(actionAIPos.pos));
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addMovement(nbCase);
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.MOVE);
                                        break;
                                    case ActionType.ATK1:
                                        int dmg = actionUseAttack(
                                            actionAIPos.action,
                                            hexaGrid.getHexa(actionAIPos.pos)
                                        );
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.ATK1);
                                        saveDataBasicAttack(
                                            currentCharControlled,
                                            hexaHovered,
                                            dmg
                                        );
                                        break;
                                    case ActionType.ATK2:
                                        int dmg2 = actionUseAttack(
                                            actionAIPos.action,
                                            hexaGrid.getHexa(actionAIPos.pos)
                                        );
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.ATK2);
                                        saveDataSkill1(currentCharControlled, hexaHovered, dmg2);
                                        break;
                                    case ActionType.ATK3:
                                        int dmg3 = actionUseAttack(
                                            actionAIPos.action,
                                            hexaGrid.getHexa(actionAIPos.pos)
                                        );
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.ATK3);
                                        saveDataSkill2(currentCharControlled, hexaHovered, dmg3);
                                        break;
                                    // Added :Julien D'aboville L3L1 2024
                                    case ActionType.ATK4:
                                        int dmg4 = actionUseAttack(
                                            actionAIPos.action,
                                            hexaGrid.getHexa(actionAIPos.pos)
                                        );
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.ATK4);
                                        saveDataSkill3(currentCharControlled, hexaHovered, dmg4);
                                        break;
                                    case ActionType.SKIP:
                                        currentCharControlled.PA = 0;
                                        getEndGameDataCharacterFromTheList(currentCharControlled)
                                            .addAction(ActionType.SKIP);
                                        nextTurn();
                                        break;

                                    default:
                                        break;
                                }
                            }
                            **/
                        }
                    }
                }
            }
        }
        // DISPLAY WINNER if there is a winner
        else
        {
            UIPauseMenu.SetActive(false);
            //Edited by L3C1 CROUZET Oriane, 08/04/2023
            UICharTurns.SetActive(false);
            UITooltip.SetActive(false);
            UIAction.SetActive(false);
            UIVictoryScreen.SetActive(true);
            //Debug.Log("Fin de la partie");
            //for (int i = 0; i < 5; i++)
            //{
            //if (i < startGameData.charsTeam1.Count) Debug.Log(startGameData.charsTeam1[i].ToString());
            //if (i < startGameData.charsTeam2.Count) hexaGrid.addChar(startGameData.charsTeam2[i], tileW / 2 - 4 + 2 + i, 2, 1);
            //}
            displayAllEndData(endGameDataCharacter);
            UIVictoryScreen.transform.GetChild(1).GetComponent<Text>().text =
                "VICTOIRE DE L'EQUIPE " + ((winner == 0) ? "BLEUE" : "ROUGE");
            UIVictoryScreen.transform.GetChild(1).GetComponent<Text>().color = (
                (winner == 0) ? Character.TEAM_1_COLOR : Character.TEAM_2_COLOR
            );

            /*
            for(int i = 0; i< endGameDataCharacter.Length; i++)
            {
                Debug.Log("Nom: "+endGameDataCharacter[i].typeCharacter.ToString());
                Debug.Log("Team : " + endGameDataCharacter[i].getTeam()Character);
                Debug.Log("Dommage de son équipe: "+ endGameDataCharacter[i].getDamagePercentageOfTHisTeam());
                Debug.Log("Pourcentage de point de vie restaurés: " + endGameDataCharacter[i].getPercentageOfHpRestored());
                Debug.Log("Pourcentage d'activité du personnage: "+endGameDataCharacter[i].getPercentageOfActivity());
                Debug.Log("Nombre de cases parcourrus: "+ endGameDataCharacter[i].getnumberOfSlotCrossedByTheCharacter());
                Debug.Log("nombre d'ennemie tués: "+ endGameDataCharacter[i].numberOfOpponentsEliminated);
            }*/



            // Back to menu

            if (Input.GetMouseButtonDown(0))
            {
                if (
                    mousePos.x >= Screen.width / 2 - 205
                    && mousePos.x < Screen.width / 2
                    && mousePos.y >= Screen.height / 2 - 90
                    && mousePos.y < Screen.height / 2 - 90 + 40
                )
                {
                    // EVALUATE AI HARD
                    // to delete

                    statsGame.endGame(winner, hexaGrid);
                    if (AIHard.learn)
                        statsGame.evaluateGame();
                    statsGame.evaluateGame();
                    MainMenu.startGameData = new StartGameData();
                    MainMenu.startGameData.slider = slider.value;
                    endGameDataCharacter = null;
                    SceneManager.LoadScene(0);
                }
                //Author : L3C1 CROUZET Oriane, 07/05/2023
                else if (
                    mousePos.x >= Screen.width / 2
                    && mousePos.x < Screen.width / 2 + 205
                    && mousePos.y >= Screen.height / 2 - 90
                    && mousePos.y < Screen.height / 2 - 90 + 40
                )
                {
                    SceneManager.LoadScene(1);
                }
            }
        } //mousePos.x >= Screen.width / 2 - 100 && mousePos.x < Screen.width / 2 + 100 && mousePos.y >= Screen.height / 2 - 90 && mousePos.y < Screen.height / 2 - 90 + 40

        // - DISPLAYS -------------------------------------------------------------------------------


        // CENTER CHARACTER MODEL
        foreach (Character c in hexaGrid.getCharList())
        {
            if (c.go.transform.GetChild(1))
                c.go.transform.GetChild(1).transform.position = c.go.transform.position;
        }
        if (winner == -1)
        {
            // Display arrow above the current character controlled
            {
                float currentHeight =
                    (((newTurn % 60) < 30) ? (newTurn % 60) : (60 - (newTurn % 60))) / 60.0f * 0.2f;
                arrow.transform.position = new Vector3(
                    currentCharControlled.go.transform.position.x,
                    currentHeight + 2f,
                    currentCharControlled.go.transform.position.z
                );
                if (newTurn == 0)
                {
                    hexaHighlight.GetComponent<MeshFilter>().mesh = hexaHollowMesh;
                }
                else if (newTurn < 10)
                {
                    hexaHighlight.transform.localScale = new Vector3(
                        1 + (10 - newTurn) * 0.3f,
                        1 + (10 - newTurn) * 0.3f,
                        1
                    );
                }
                else if (newTurn == 10)
                {
                    hexaHighlight.transform.localScale = new Vector3(1, 1, 1);
                    hexaHighlight.GetComponent<MeshFilter>().mesh = hexaFilledMesh;
                }
                hexaHighlight.transform.position = new Vector3(
                    currentCharControlled.go.transform.position.x,
                    -0.013f,
                    currentCharControlled.go.transform.position.z
                );
                newTurn++;
            }

            if (updateMouseHover)
            {
                // Clear previous hexas displayed
                foreach (GameObject go in pathFinderDisplay)
                {
                    Destroy(go);
                }
                pathFinderDisplay = new List<GameObject>();
                foreach (GameObject go in lineOfSightDisplay)
                {
                    Destroy(go);
                }
                lineOfSightDisplay = new List<GameObject>();
                // Display hovered hexa
                displayHoveredHexa();
            }

            // Display path in green / line of sight in blue / AoE in red
            if (
                pathWalk == null
                && updateMouseHover
                && whoControlsThisChar(currentCharControlled) == PlayerType.HUMAN
            )
            {
                switch (actionType)
                {
                    case ActionType.MOVE:
                        displayPossiblePaths();
                        displaySortestPath();
                        if (Input.GetKey(KeyCode.LeftControl))
                            displayLineOfSight();
                        break; //now displays the attack range of the controlled character if we press "Left Control".
                    case ActionType.ATK1:
                    case ActionType.ATK2:
                    case ActionType.ATK3:
                    case ActionType.ATK4:
                        displayLineOfSight();
                        break;
                    case ActionType.SKIP:
                        break;
                }
            }

            // Display UI
            if (updateUI)
            {
                displayNewCharTurnList();
                displayActionButtons();
            }
        }

        updateMouseHover = false;
        updateUI = false;
    }

    // ##################################################################################################################################################
    // Functions used in main
    // ##################################################################################################################################################


    //Author : VALAT Thibault L3Q1
    //Initialize a random bonus between : center-left of the map, center, or center right
    Hexa initBonus()
    {
        int bonusPlace = UnityEngine.Random.Range(1, 4);
        int x = 0;
        int y = 0;

        //print("on cree la case bonus");

        switch (bonusPlace)
        {
            case 1:
                //print("cas 1");
                displayBonus(hexaGrid.findAllPaths(5, 16, 2));
                x = 5;
                y = 16;
                break;
            case 2:
            //print("cas 2");
                displayBonus(hexaGrid.findAllPaths(17, 15, 2));
                x = 17;
                y = 15;
                break;
            case 3:
            //print("cas 3");
                displayBonus(hexaGrid.findAllPaths(29, 15, 2));
                x = 29;
                y = 15;
                break;
        }
        return hexaGrid.getHexa(x, y);
    }

    // Added by Timothé MIEL - L3L1
    // Initialize a fixed bonus given x and y coordinates and radius
    Hexa initFixedBonus(int x, int y, int radius)
    {
        displayBonus(hexaGrid.findAllPaths(x, y, radius));
        return hexaGrid.getHexa(x, y);
    }

    //Added by Socrate Louis Deriza, L3C1
    void saveDataBasicAttack(Character currentCharControlled1, Hexa hexaHovered1, int dmg1)
    {
        if (currentCharControlled1.getName() == "Soigneur")
        {
            if(hexaHovered1 != null)            
            if (hexaHovered1.charOn != null)
            {
                
                getEndGameDataCharacterFromTheList(hexaHovered1.charOn).addAnHealingAction(dmg1);
            }
        }
        else if (
            currentCharControlled1.getName() == "Valkyrie"
            || currentCharControlled1.getName() == "Mage"
        )
        {
            List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(
                currentCharControlled1,
                1
            );
            for (int i = 0; i < listCharacter.Count; i++)
            {
                getEndGameDataCharacterFromTheList(currentCharControlled1).addDamage(dmg1);
            }
        }
        else
        {
            if (currentCharControlled1.getName() != "Envouteur")
            {
                getEndGameDataCharacterFromTheList(currentCharControlled1).addDamage(dmg1);
            }
        }
    }

    // Added by Soccrate Louis Deriza, L3C1

    void displayAllEndData(EndGameDataCharacter[] endGameDataCharacterToDisplay)
    {
        EndGameDataCharacter[] data1 = getEndData1(endGameDataCharacterToDisplay);
        EndGameDataCharacter[] data2 = getEndData2(endGameDataCharacterToDisplay);
        EndGameDataCharacter[] data3 = getEndData3(endGameDataCharacterToDisplay);
        EndGameDataCharacter[] data4 = getEndData4(endGameDataCharacterToDisplay);
        EndGameDataCharacter[] data5 = getEndData5(endGameDataCharacterToDisplay);

        for (int i = 0; i < 5; i++)
        {
            if (i == 0)
            {
                GameObject gData10 = GameObject.Find("statData1Team0");
                Text textGData10 = gData10.GetComponent<Text>();
                textGData10.text =
                    "Plus gros dêgâts de son équipe: "
                    + (data1[0].typeCharacter).ToString()
                    + "("
                    + (data1[0]).getDamagePercentageOfTHisTeam()
                    + " pourcents)";
                textGData10.color = new Color(0.125f, 0.125f, 1);
                GameObject gData11 = GameObject.Find("statData1Team1");
                Text textGData11 = gData11.GetComponent<Text>();
                textGData11.text =
                    "Plus gros dêgâts de son équipe: "
                    + (data1[1].typeCharacter).ToString()
                    + "("
                    + (data1[1]).getDamagePercentageOfTHisTeam()
                    + " pourcents)";
                textGData11.color = new Color(1, 0.125f, 0);
            }
            else if (i == 1)
            {
                GameObject gData20 = GameObject.Find("statData2Team0");
                Text textGData20 = gData20.GetComponent<Text>();
                textGData20.text =
                    "Plus gros tueur de son équipe: "
                    + (data2[0].typeCharacter).ToString()
                    + "("
                    + (data2[0]).numberOfOpponentsEliminated
                    + " kills)";
                textGData20.color = new Color(0.125f, 0.125f, 1);
                GameObject gData21 = GameObject.Find("statData2Team1");
                Text textGData21 = gData21.GetComponent<Text>();
                textGData21.text =
                    "Plus gros tueur de son équipe: "
                    + (data2[1].typeCharacter).ToString()
                    + "("
                    + (data2[1]).numberOfOpponentsEliminated
                    + " kills)";
                textGData21.color = new Color(1, 0.125f, 0);
            }
            else if (i == 2)
            {
                GameObject gData30 = GameObject.Find("statData3Team0");
                Text textGData30 = gData30.GetComponent<Text>();
                textGData30.text =
                    "Plus grosse activité de son équipe: "
                    + (data3[0].typeCharacter).ToString()
                    + "("
                    + (data3[0]).getPercentageOfActivity()
                    + " pourcents d'utilisation d'attaque  ou de compétences)";
                textGData30.color = new Color(0.125f, 0.125f, 1);
                GameObject gData31 = GameObject.Find("statData3Team1");
                Text textGData31 = gData31.GetComponent<Text>();
                textGData31.text =
                    "Plus grosse activité de son équipe: "
                    + (data3[1].typeCharacter).ToString()
                    + "("
                    + (data3[1]).getPercentageOfActivity()
                    + " pourcents d'utilisation d'attaque  ou de compétences)";
                textGData31.color = new Color(1, 0.125f, 0);
            }
            else if (i == 3)
            {
                GameObject gData40 = GameObject.Find("statData4Team0");
                Text textGData40 = gData40.GetComponent<Text>();
                textGData40.text =
                    "Plus grosse regéneration de son équipe: "
                    + (data4[0].typeCharacter).ToString()
                    + "("
                    + (data4[0]).getPercentageOfHpRestored()
                    + " pourcents de ses points de vie retrouvés)";
                textGData40.color = new Color(0.125f, 0.125f, 1);
                GameObject gData41 = GameObject.Find("statData4Team1");
                Text textGData41 = gData41.GetComponent<Text>();
                textGData41.text =
                    "Plus grosse regéneration de son équipe: "
                    + (data4[1].typeCharacter).ToString()
                    + "("
                    + (data4[1]).getPercentageOfHpRestored()
                    + " pourcents de ses points de vie retrouvés)";
                textGData41.color = new Color(1, 0.125f, 0);
            }
            else
            {
                GameObject gData50 = GameObject.Find("statData5Team0");
                Text textGData50 = gData50.GetComponent<Text>();
                textGData50.text =
                    "Plus gros déplacements de son équipe: "
                    + (data5[0].typeCharacter).ToString()
                    + "("
                    + (data5[0]).getnumberOfSlotCrossedByTheCharacter()
                    + " cases)";
                textGData50.color = new Color(0.125f, 0.125f, 1);
                GameObject gData51 = GameObject.Find("statData5Team1");
                Text textGData51 = gData51.GetComponent<Text>();
                textGData51.text =
                    "Plus gros déplacements de son équipe: "
                    + (data5[1].typeCharacter).ToString()
                    + "("
                    + (data5[1]).getnumberOfSlotCrossedByTheCharacter()
                    + " cases)";
                textGData51.color = new Color(1, 0.125f, 0);
            }
        }
    }

    //Added by Socrate Louis Deriza, L3C1
    EndGameDataCharacter[] getEndData1(EndGameDataCharacter[] endGameDataCharacterToDisplay)
    {
        EndGameDataCharacter[] endData1 = new EndGameDataCharacter[2];
        int limit = endGameDataCharacterToDisplay.Length / 2;
        float percentageDamageMax = 0.0f;
        EndGameDataCharacter firstElement = endGameDataCharacterToDisplay[0];
        for (int i = 0; i < limit; i++)
        {
            if (
                percentageDamageMax
                < endGameDataCharacterToDisplay[i].getDamagePercentageOfTHisTeam()
            )
            {
                percentageDamageMax = endGameDataCharacterToDisplay[i]
                    .getDamagePercentageOfTHisTeam();
                firstElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData1[0] = firstElement;
        percentageDamageMax = 0.0f;
        EndGameDataCharacter secondElement = endGameDataCharacterToDisplay[limit];
        for (int i = limit; i < endGameDataCharacterToDisplay.Length; i++)
        {
            if (
                percentageDamageMax
                < endGameDataCharacterToDisplay[i].getDamagePercentageOfTHisTeam()
            )
            {
                percentageDamageMax = endGameDataCharacterToDisplay[i]
                    .getDamagePercentageOfTHisTeam();
                secondElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData1[1] = secondElement;

        return endData1;
    }

    //Added by Socrate Louis Deriza, L3C1
    EndGameDataCharacter[] getEndData2(EndGameDataCharacter[] endGameDataCharacterToDisplay)
    {
        EndGameDataCharacter[] endData2 = new EndGameDataCharacter[2];
        int limit = endGameDataCharacterToDisplay.Length / 2;
        int maxKill = 0;
        EndGameDataCharacter firstElement = endGameDataCharacterToDisplay[0];
        for (int i = 0; i < limit; i++)
        {
            if (maxKill < endGameDataCharacterToDisplay[i].numberOfOpponentsEliminated)
            {
                maxKill = endGameDataCharacterToDisplay[i].numberOfOpponentsEliminated;
                firstElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData2[0] = firstElement;
        maxKill = 0;
        EndGameDataCharacter secondElement = endGameDataCharacterToDisplay[limit];
        for (int i = limit; i < endGameDataCharacterToDisplay.Length; i++)
        {
            if (maxKill < endGameDataCharacterToDisplay[i].numberOfOpponentsEliminated)
            {
                maxKill = endGameDataCharacterToDisplay[i].numberOfOpponentsEliminated;
                secondElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData2[1] = secondElement;

        return endData2;
    }

    //Added by Socrate Louis Deriza, L3C1
    EndGameDataCharacter[] getEndData3(EndGameDataCharacter[] endGameDataCharacterToDisplay)
    {
        EndGameDataCharacter[] endData3 = new EndGameDataCharacter[2];
        int limit = endGameDataCharacterToDisplay.Length / 2;
        float percentageActivityMax = 0.0f;
        EndGameDataCharacter firstElement = endGameDataCharacterToDisplay[0];
        for (int i = 0; i < limit; i++)
        {
            if (percentageActivityMax < endGameDataCharacterToDisplay[i].getPercentageOfActivity())
            {
                percentageActivityMax = endGameDataCharacterToDisplay[i].getPercentageOfActivity();
                firstElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData3[0] = firstElement;
        percentageActivityMax = 0.0f;
        EndGameDataCharacter secondElement = endGameDataCharacterToDisplay[limit];
        for (int i = limit; i < endGameDataCharacterToDisplay.Length; i++)
        {
            if (percentageActivityMax < endGameDataCharacterToDisplay[i].getPercentageOfActivity())
            {
                percentageActivityMax = endGameDataCharacterToDisplay[i].getPercentageOfActivity();
                secondElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData3[1] = secondElement;

        return endData3;
    }

    //Added by Socrate Louis Deriza, L3C1
    EndGameDataCharacter[] getEndData4(EndGameDataCharacter[] endGameDataCharacterToDisplay)
    {
        EndGameDataCharacter[] endData4 = new EndGameDataCharacter[2];
        int limit = endGameDataCharacterToDisplay.Length / 2;
        float percentageRegenerationMax = 0.0f;
        EndGameDataCharacter firstElement = endGameDataCharacterToDisplay[0];
        for (int i = 0; i < limit; i++)
        {
            if (
                percentageRegenerationMax
                < endGameDataCharacterToDisplay[i].getPercentageOfHpRestored()
            )
            {
                percentageRegenerationMax = endGameDataCharacterToDisplay[i]
                    .getPercentageOfHpRestored();
                firstElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData4[0] = firstElement;
        percentageRegenerationMax = 0.0f;
        EndGameDataCharacter secondElement = endGameDataCharacterToDisplay[limit];
        for (int i = limit; i < endGameDataCharacterToDisplay.Length; i++)
        {
            if (
                percentageRegenerationMax
                < endGameDataCharacterToDisplay[i].getPercentageOfHpRestored()
            )
            {
                percentageRegenerationMax = endGameDataCharacterToDisplay[i]
                    .getPercentageOfHpRestored();
                secondElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData4[1] = secondElement;

        return endData4;
    }

    //Added by Socrate Louis Deriza, L3C1
    EndGameDataCharacter[] getEndData5(EndGameDataCharacter[] endGameDataCharacterToDisplay)
    {
        EndGameDataCharacter[] endData5 = new EndGameDataCharacter[2];
        int limit = endGameDataCharacterToDisplay.Length / 2;
        int nbCase = 0;
        EndGameDataCharacter firstElement = endGameDataCharacterToDisplay[0];
        for (int i = 0; i < limit; i++)
        {
            if (nbCase < endGameDataCharacterToDisplay[i].getnumberOfSlotCrossedByTheCharacter())
            {
                nbCase = endGameDataCharacterToDisplay[i].getnumberOfSlotCrossedByTheCharacter();
                firstElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData5[0] = firstElement;
        nbCase = 0;
        EndGameDataCharacter secondElement = endGameDataCharacterToDisplay[limit];
        for (int i = limit; i < endGameDataCharacterToDisplay.Length; i++)
        {
            if (nbCase < endGameDataCharacterToDisplay[i].getnumberOfSlotCrossedByTheCharacter())
            {
                nbCase = endGameDataCharacterToDisplay[i].getnumberOfSlotCrossedByTheCharacter();
                secondElement = endGameDataCharacterToDisplay[i];
            }
        }
        endData5[1] = secondElement;

        return endData5;
    }

    //Added by Socrate Louis Deriza, L3C1
    void saveDataSkill1(Character currentCharControlled2, Hexa hexaHovered2, int dmg2)
    {
        if (currentCharControlled2.getName() == "Soigneur")
        {
            List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(
                currentCharControlled2,
                2
            );
            for (int i = 0; i < listCharacter.Count; i++)
            {
                getEndGameDataCharacterFromTheList(listCharacter[i]).addAnHealingAction(dmg2);
            }
        }
        else if (
            currentCharControlled2.getName() == "Valkyrie"
            || currentCharControlled2.getName() == "Mage"
        )
        {
            List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(
                currentCharControlled2,
                2
            );
            for (int i = 0; i < listCharacter.Count; i++)
            {
                getEndGameDataCharacterFromTheList(currentCharControlled2).addDamage(dmg2);
            }
        }
        else
        {
            if (currentCharControlled2.getName() != "Envouteur")
            {
                getEndGameDataCharacterFromTheList(currentCharControlled2).addDamage(dmg2);
            }
        }
    }

    //Added by Socrate Louis Deriza, L3C1
    //Edited by Julien D'aboville 2024 L3L1 (ajout Forgeron)
    void saveDataSkill2(Character currentCharControlled3, Hexa hexaHovered3, int dmg3)
    {
        if (currentCharControlled3.getName() == "Soigneur")
        {
            List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(
                currentCharControlled3,
                3
            );
            for (int i = 0; i < listCharacter.Count; i++)
            {
                getEndGameDataCharacterFromTheList(listCharacter[i]).addAnHealingAction(dmg3);
            }
        }
        else if (currentCharControlled3.getName() == "Voleur")
        {
            getEndGameDataCharacterFromTheList(currentCharControlled3).addAnHealingAction(dmg3);
            getEndGameDataCharacterFromTheList(currentCharControlled3).addDamage(dmg3);
        }
        else if (currentCharControlled3.getName() == "Valkyrie" || currentCharControlled3.getName() == "Mage" || currentCharControlled3.getName() == "Archer" || currentCharControlled3.getName() == "Guerrier" || currentCharControlled3.getName()=="Forgeron")
        {
            List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(
                currentCharControlled3,
                3
            );
            for (int i = 0; i < listCharacter.Count; i++)
            {
                getEndGameDataCharacterFromTheList(currentCharControlled3).addDamage(dmg3);
            }
        }
        else
        {
            if (currentCharControlled3.getName() != "Envouteur")
            {
                getEndGameDataCharacterFromTheList(currentCharControlled3).addDamage(dmg3);
            }
        }
    }

    //Added by Julien D'aboville, L3L1 2024(à revoir en fonction des compétences des personnages)

    void saveDataSkill3(Character currentCharControlled4, Hexa hexaHovered4, int dmg4)
    {
        if (currentCharControlled4.getName() == "Soigneur")
        {
            List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(currentCharControlled4, 3);
            for (int i = 0; i < listCharacter.Count; i++)
            {
                getEndGameDataCharacterFromTheList(listCharacter[i]).addAnHealingAction(dmg4);
            }
        }
        else if (currentCharControlled4.getName() == "Voleur")
        {
            getEndGameDataCharacterFromTheList(currentCharControlled4).addAnHealingAction(dmg4);
            getEndGameDataCharacterFromTheList(currentCharControlled4).addDamage(dmg4);
        }
        else if (currentCharControlled4.getName() == "Valkyrie" || currentCharControlled4.getName() == "Mage" || currentCharControlled4.getName() == "Archer" || currentCharControlled4.getName() == "Guerrier")
        {
            List<Character> listCharacter = getAllAllyOrEnnemyInRangeAttack(currentCharControlled4, 3);
            for (int i = 0; i < listCharacter.Count; i++)
            {
                getEndGameDataCharacterFromTheList(currentCharControlled4).addDamage(dmg4);
            }
        }
        else
        {
            if (currentCharControlled4.getName() != "Envouteur")
            {
                getEndGameDataCharacterFromTheList(currentCharControlled4).addDamage(dmg4);
            }
        }
    }

    //Added by Socrate Louis Deriza, L3C1
    public List<Character> getAllAllyOrEnnemyInRangeAttack(Character charac, int numberOfAttack)
    {
        List<Character> listInRange = new List<Character>();
        foreach (Character c in hexaGrid.getCharList())
        {
            if (charac.getName() == "Soigneur")
            {
                if (numberOfAttack == 3 && charac.getTeam() == c.getTeam())
                {
                    if (isInRangeToUseSkill2(charac, c) == true)
                    {
                        listInRange.Add(c);
                    }
                }
                if (numberOfAttack == 2 && charac.getTeam() == c.getTeam())
                {
                    if (isInRangeToUseSkill(charac, c) == true)
                    {
                        if (
                            c.getName() != charac.getName()
                            || c.getX() != charac.getX()
                            || c.getY() != charac.getY()
                        )
                        {
                            listInRange.Add(c);
                        }
                    }
                }
                if (numberOfAttack == 1 && charac.getTeam() == c.getTeam())
                {
                    if (isInRangeToAttack(charac, c))
                    {
                        if (
                            c.getName() != charac.getName()
                            || c.getX() != charac.getX()
                            || c.getY() != charac.getY()
                        )
                        {
                            listInRange.Add(c);
                        }
                    }
                }
            }
            else
            {
                if (numberOfAttack == 3 && charac.getTeam() != c.getTeam())
                {
                    if (isInRangeToUseSkill2(charac, c) == true)
                    {
                        listInRange.Add(c);
                    }
                }
                if (numberOfAttack == 2 && charac.getTeam() != c.getTeam())
                {
                    if (isInRangeToUseSkill(charac, c) == true)
                    {
                        listInRange.Add(c);
                    }
                }
                if (numberOfAttack == 1 && charac.getTeam() != c.getTeam())
                {
                    if (isInRangeToAttack(charac, c))
                    {
                        listInRange.Add(c);
                    }
                }
            }
        }
        return listInRange;
    }

    //Author : VALAT Thibault L3Q1
    //Display Bonus hexas in gray (neutral)
    void displayBonus(List<Point> points)
    {
        Debug.Log("displayBonus");
        if (points == null)
        {
            Debug.Log("points.Count = null");
        }
        Debug.Log("points.Count = " + points.Count);
        foreach (Point p in points)
        {
            Hexa point = hexaGrid.getHexa(p);
            if (point.type == HexaType.WALL)
            {
                point.changeType(HexaType.GROUND);
            }
            point.changeType(HexaType.BONUS);
            bonusPoints.Add(point);

            GameObject go = GameObject.Instantiate(hexaTemplate, hexasFolder.transform);
            go.SetActive(true);
            go.GetComponent<Transform>().position = Hexa.hexaPosToReal(p.getX(), p.getY(), -0.015f);
            go.GetComponent<MeshFilter>().mesh = hexaFilledMesh;
            go.GetComponent<Renderer>().sharedMaterial = bonusDisplayMat;
            go.GetComponent<Collider>().enabled = false;
            bonusDisplayMat.color = neutralBonusDisplayColor;
        }
    }

    //Author : VALAT Thibault L3Q1
    //Display Bonus hexas in gray (neutral), red or blue and change during the game
    void checkAndUpdateBonusControll()
    {
        bool redInBonusZone = false;
        bool blueInBonusZone = false;

        //Check if there is zero or one or two teams in the bonus zone
        foreach (Character c in hexaGrid.getCharList())
        {
            if (hexaGrid.getHexa(c.getX(), c.getY()).type == HexaType.BONUS && c.getTeam() == 0)
                blueInBonusZone = true;
            if (hexaGrid.getHexa(c.getX(), c.getY()).type == HexaType.BONUS && c.getTeam() == 1)
                redInBonusZone = true;
        }
        //Reset the bonus zone
        if ((!redInBonusZone && !blueInBonusZone) || (redInBonusZone && blueInBonusZone))
        {
            giveBonusValue(-1);
            changeBonusColor(neutralBonusDisplayColor);
        }
        //Give the bonus to the red team
        else if (redInBonusZone && !blueInBonusZone)
        {
            giveBonusValue(1);
            changeBonusColor(redBonusDisplayColor);
        }
        //Give the bonus to the blue team
        else if (!redInBonusZone && blueInBonusZone)
        {
            giveBonusValue(0);
            changeBonusColor(blueBonusDisplayColor);
        }
    }

    //Change the display of the bonus hexas and the bonus card
    //Author : VALAT Thibault L3Q1
    void changeBonusColor(Color teamColor)
    {
        Debug.Log("Change colors");
        //Change the color of the hexas
        bonusDisplayMat.color = teamColor;

        //Change the color of the bonus card
        switch (bonusTeam)
        {
            case -1:
                BonusCard.transform.GetChild(0).GetComponent<Image>().color = new Color(
                    0.5f,
                    0.5f,
                    0.5f
                );
                BonusCard.transform.GetChild(2).GetComponent<Text>().text = "Bonus";
                break;
            case 1:
                BonusCard.transform.GetChild(0).GetComponent<Image>().color = new Color(
                    1,
                    0.2f,
                    0.2f
                );
                BonusCard.transform.GetChild(2).GetComponent<Text>().text = "+1 HP";
                break;
            case 0:
                BonusCard.transform.GetChild(0).GetComponent<Image>().color = new Color(
                    0.2f,
                    0.2f,
                    1
                );
                BonusCard.transform.GetChild(2).GetComponent<Text>().text = "+1 HP";
                break;
        }
    }

    //Author Louis Deriza Socrate L3C1, 15/04/2023

    // edited simon sepiol L3L1
    // ne marche pas si on charge depuis une save car les perso ne sont pas a la meme place
    EndGameDataCharacter[] createEndDataGameList(int sizeEndGameData)
    {
        EndGameDataCharacter[] endGameDataTab = new EndGameDataCharacter[sizeEndGameData];
        int team1count=0;
        int team2count=sizeEndGameData/2;
        foreach( Character c in hexaGrid.getCharList()){
            if(c.getTeam()==0){
                endGameDataTab[team1count]= new EndGameDataCharacter(c, c.charClass, c.getTeam());
                team1count++;
            
            }
            else{
                endGameDataTab[team2count]= new EndGameDataCharacter(c, c.charClass, c.getTeam());
                team2count++;
                
            }
        }

        //for (int i = 0; i < 5; i++)
        //{
        //    if (i < startGameData.charsTeam1.Count)
        //    {
        //        // TODO : remplacer par une liste des perso
        //        Character c = hexaGrid.getCharacterOnHexa(tileW / 2 - 4 + 2 + i, tileH - 2);
        //        endGameDataTab[i] = new EndGameDataCharacter(c, startGameData.charsTeam1[i], 0);
//
        //        //hexaGrid.addChar(startGameData.charsTeam1[i], , , 0);
        //    }
        //    if (i < startGameData.charsTeam2.Count)
        //    {
        //        Character c = hexaGrid.getCharacterOnHexa(tileW / 2 - 4 + 2 + i, 2);
        //        endGameDataTab[((sizeEndGameData / 2) + i)] = new EndGameDataCharacter(
        //            c,
        //            startGameData.charsTeam2[i],
        //            1
        //        );
        //        //hexaGrid.addChar(startGameData.charsTeam2[i], , , 1);
        //    }
        //}
        return endGameDataTab;
    }

    //Added by Socrate Louis Deriza, L3C1
    EndGameDataCharacter getEndGameDataCharacterFromTheList(Character character)
    {
        for (int i = 0; i < endGameDataCharacter.Length; i++)
        {
            //Debug.Log(character.getName());
            //Debug.Log(endGameDataCharacter[i].character.getName());
            if (endGameDataCharacter[i].character == character)
            {
                return endGameDataCharacter[i];
            }
        }
        return null;
    }

    //Give the +1HP bonus to the team controlling the bonus
    //Author : VALAT Thibault L3Q1
    void giveBonusValue(int bonusControlledTeam)
    {
        //Reset the bonus value if the team which had it lost it
        if (bonusControlledTeam == -1 && bonusTeam != -1)
        {
            foreach (Character c in hexaGrid.getCharList())
                if (c.getTeam() == bonusTeam)
                    c.HP--;
            bonusTeam = -1;
        }
        //Give the bonus to the blue team
        else if (bonusControlledTeam == 0 && bonusTeam != 0)
        {
            foreach (Character c in hexaGrid.getCharList())
            {
                if (c.getTeam() == bonusTeam)
                    c.HP--;
                if (c.getTeam() == bonusControlledTeam)
                    c.HP++;
            }
            bonusTeam = 0;
        }
        //Give the bonus to the red team
        else if (bonusControlledTeam == 1 && bonusTeam != 1)
        {
            foreach (Character c in hexaGrid.getCharList())
            {
                if (c.getTeam() == bonusTeam)
                    c.HP--;
                if (c.getTeam() == bonusControlledTeam)
                    c.HP++;
            }
            bonusTeam = 1;
        }
    }

    // Return the team with the active bonus
    //Author : GOUVEIA Klaus L3Q1
    public static int getBonusTeam()
    {
        return bonusTeam;
    }

    // Return the playerType of character (humaun ...)
    //Author : ??
    PlayerType whoControlsThisChar(Character c)
    {
        return (c.getTeam() == 0) ? startGameData.player1Type : startGameData.player2Type;
    }

    //Stock the current hoverred hexa
    //Author : ??
    //Edited by Mariana Duarte L3Q1 04/2025 (to ignore the map objects layer)
    void getHoveredHexa()
    {
        RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        bool success = false;
        if (Physics.Raycast(ray.origin, ray.direction, out raycastHit, 100,  ~mapObjectsLayer))
            success = (raycastHit.transform.gameObject.tag == "Hexa");
        if (success)
        {
            Hexa hexaHit = raycastHit.transform.gameObject.GetComponent<HexaGO>().hexa;
            if (hexaHit != hexaHovered)
            {
                hexaHoveredOld = hexaHovered;
                hexaHovered = hexaHit;
                charHovered = hexaHovered.charOn;
                updateMouseHover = true;
                updateUI = true;
            }
        }
        else
        {
            if (hexaHovered != null)
            {
                hexaHoveredOld = hexaHovered;
                hexaHovered = null;
                charHovered = null;
            }
            updateMouseHover = true;
            updateUI = true;
        }
    }

    //get the controlled character
    //Author : ??
    void getControlledCharacter()
    {
        //stocks the hovered hexa
        getHoveredHexa();

        if (Input.GetMouseButton(0) && hexaHovered != null)
        {
            foreach (Character c in hexaGrid.getCharList())
            {
                if (c.getTeam() == currentCharControlled.getTeam())
                {
                    if (hexaHovered.getX() == c.getX() && hexaHovered.getY() == c.getY())
                    {
                        currentCharControlled = c;
                    }
                }
            }
        }
    }

    //Set the controlled character
    //Author : ??
    public void setControlledCharacter(Character c)
    {
        currentCharControlled = c;
    }

    //Move the character
    //Author : ??
    int actionMove(Hexa hexaDestination)
    {

        int i= 0;
        int saveY = hexaDestination.getY();
        //print("ACTION MOVE save y :"+ saveY);
        //print("ACTION MOVE avant hexadestination  x" + hexaDestination.getX()+ "hexadestination y :"+hexaDestination.getY());
        //Move the character when its possible
        if (
            hexaDestination != null
            && (hexaDestination.type == HexaType.GROUND || hexaDestination.type == HexaType.BONUS || hexaDestination.type == HexaType.PORTAL || hexaDestination.type == HexaType.BUSH)
        )
        {
            //print("ACTION MOVE can MOVE !!");
            //print("ACTION MOVE chemin avant");
            //print("Charge a true :"+charge);
            

            List<Point> path = hexaGrid.findShortestPath(currentCharControlled.getX(), currentCharControlled.getY(), hexaDestination.getX(), hexaDestination.getY(), currentCharControlled.PM);
            //print("ACTION MOVE apres hexadestination x" + hexaDestination.getX() + "hexadestination y :" + hexaDestination.getY());

            if (path ==null && charge)

            {
                path = new List<Point>();
                //print("Creation chemin charge");
                for(i=currentCharControlled.getX();i<= hexaDestination.getX(); i++)
                {
                    //print("position charge i" + i);
                    path.Add(new Point(i, saveY));

                }

            }

            //Debug.Log("fin méthode findShortestPath");
            //print("hexa type du joueur"+ hexaGrid.getHexa(hexaDestination.getX(), hexaDestination.getY()).type);
            
            //print("ACTION MOVE chemin apres");
// //print("Taille chemin :" + path.Count);
            /*for (i = 0; i < path.Count; i++)
            {
                //print("paths Action move" + i + " : x" + path[i].getX() + " y :" + path[i].getY());
            }*/
            if (path != null && path.Count > 1)
            {
                pathWalk = path;
                pathWalkpos = 0;
                return path.Count;
            }
            else
            {
                /*
                if (path == null)
                {
                    Debug.LogWarning("ActionMove Error(null): " + hexaDestination.getX() + " " + hexaDestination.getY() +
                    "\nHexa type : \t" + hexaDestination.type +
                    "\nOn hexa : \t" + charToString(hexaDestination.charOn));
                }
                if (path.Count == 1)
                {
                    Debug.LogWarning("ActionMove Error(=1): " + hexaDestination.getX() + " " + hexaDestination.getY() +
                    "\nHexa type : \t" + hexaDestination.type +
                    "\nOn hexa : \t" + charToString(hexaDestination.charOn));
                }
                if (path.Count < 1)
                {
                    Debug.LogWarning("ActionMove Error(>1): " + hexaDestination.getX() + " " + hexaDestination.getY() +
                    "\nHexa type : \t" + hexaDestination.type +
                    "\nOn hexa : \t" + charToString(hexaDestination.charOn));
                }*/
                return 0;
            }
        }
        else
        {
            Debug.LogWarning("ActionMove Error: No Hexa found.");
            return 0;
        }
    }

    //Attack with the current character, the hexa clicked
    //Author : ??
    // Edited BY Youcef MEDILEH, L3C1
    // - Added the possibility to use the second skill
    //Edited BY Julien D'aboville L3L1 2024 
    //Edited BY Simon Sepiol L3L1 2024 

    int actionUseAttack(ActionType attack, Hexa hexaDestination)
    {
        //Set the attack used
        CharsDB.Attack attackUsed_;
        if (attack == ActionType.ATK1)
            attackUsed_ = CharsDB.list[(int)currentCharControlled.charClass].basicAttack;
        else if (attack == ActionType.ATK2)
        {
            attackUsed_ = CharsDB.list[(int)currentCharControlled.charClass].skill_1;
        }
        else if (attack == ActionType.ATK3)
        {
            attackUsed_ = CharsDB.list[(int)currentCharControlled.charClass].skill_2;
        } //ATK3, by Youcef MEDILEH, L3C1
        else
        {
            attackUsed_ = CharsDB.list[(int)currentCharControlled.charClass].skill_3;
        }
        Debug.Log(CharsDB.list[(int)currentCharControlled.charClass]);
        //Use the attack if it's possible
        if (
            hexaDestination != null
            && hexaGrid.hexaInSight(
                currentCharControlled.getX(),
                currentCharControlled.getY(),
                hexaDestination.getX(),
                hexaDestination.getY(),
                attackUsed_.range
            )
        )
        {
            // edited by Simon Sepiol L3L1
            // on enleve les capacitées utilisées

            if (attack == ActionType.ATK2)
            {
                currentCharControlled.skillAvailable = false;
                currentCharControlled.attack1coolDownValue=currentCharControlled.getSkill1CoolDown();
                actionType = ActionType.ATK1;
            }
            else if (attack == ActionType.ATK3)
            {
                currentCharControlled.skillAvailable2 = false;
                currentCharControlled.attack2coolDownValue=currentCharControlled.getSkill2CoolDown();
                actionType = ActionType.ATK1;
            }
            else if (attack == ActionType.ATK4)
            {
                currentCharControlled.skillAvailable3 = false;
                currentCharControlled.attack3coolDownValue=currentCharControlled.getSkill3CoolDown();
                actionType = ActionType.ATK1;
            }
            // Attack animation
            Animator animator = currentCharControlled
                .go.transform.GetChild(1)
                .GetComponent<Animator>();
            if (
                animator
                && attack == ActionType.ATK4
                && currentCharControlled.charClass == CharClass.NETHERFANG
            ) // action de charge
            {
                Debug.Log("Animation Explosion");
                animator.SetTrigger("JumpKick");

                //Animation quand il fait la charge
            }
            else if (animator)
            {
                animator.SetTrigger("Attack1Trigger");
            }
            attackUsedAttack = attackUsed_;
            attackUsedTargetHexa = new Point(hexaDestination.getX(), hexaDestination.getY());
            attackUsed = 30; // Delay attack

            // Particles for soigneur
            if (currentCharControlled.charClass == CharClass.SOIGNEUR)
            {
                GameObject go = GameObject.Instantiate(particleHeal);
                go.transform.position = Hexa.hexaPosToReal(hexaDestination.getX(), hexaDestination.getY(), 0);
                go.transform.localScale *= 0.1f;
                Destroy(go, 5);
            }
            //Particles for mage
            else if (currentCharControlled.charClass == CharClass.MAGE)
            {
                GameObject go = GameObject.Instantiate(particleExplosion);
                go.transform.position = Hexa.hexaPosToReal(hexaDestination.getX(), hexaDestination.getY(), 0);
                go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                Destroy(go, 5);
            }

            // calculate the angle the character will be facing
            Vector3 v1 = Hexa.hexaPosToReal(hexaDestination.getX(), hexaDestination.getY(), 0);
            Vector3 v2 = Hexa.hexaPosToReal(currentCharControlled.getX(), currentCharControlled.getY(), 0);
            float x = v1.x - v2.x;
            float y = v1.z - v2.z;
            float d = Mathf.Sqrt(x * x + y * y);
            float cos_ = (x == 0) ? 0 : x / d;
            float angle = Mathf.Acos(cos_);
            if (y < 0)
                angle = -angle;
            int angleDegrees = (int)((angle * 180) / (Mathf.PI));
            if (angleDegrees < 0)
                angleDegrees = 360 + angleDegrees;
            angleDegrees = 360 - angleDegrees + 90;
            angleDegrees = (angleDegrees + 5) / 10 * 10;
            //Debug.Log(x + " " + y + " " + cos_ + " " + " " + angle + " " + angleDegrees);
            Transform charModel = currentCharControlled.go.transform.GetChild(1);
            if (charModel)
                charModel.eulerAngles = new Vector3(0, angleDegrees, 0);
            return attackUsed_.effectValue;
        }
        //When it's not possible
        else
        {
            if (hexaDestination == null)
            {
                Debug.Log("hexaDest null");
            }
            else if (
                !hexaGrid.hexaInSight(
                    currentCharControlled.getX(),
                    currentCharControlled.getY(),
                    hexaDestination.getX(),
                    hexaDestination.getY(),
                    attackUsed_.range
                )
            )
            {
                Debug.Log(
                    "hexa not in sight: from "
                        + currentCharControlled.getX()
                        + ","
                        + currentCharControlled.getY()
                        + " to "
                        + hexaDestination.getX()
                        + ","
                        + hexaDestination.getY()
                );
            }
        }
        updateMouseHover = true;
        updateUI = true;
        return 0;
    }

    //Use attack
    
    //edited by GOUVEIA Klaus, group: L3Q1
    //edited by MEDILEH Youcef, group: L3C1 14/03/2023
    // - Added the possibility to use the skill 2
    // - Added the MASSIVE_HEAL skill type
    // - Added the HEALTH_STEAL skill type
    // - Added the DAMAGE_OVER_TIME skill type
    // - Added the LIGHTNING skill type
    // - Created methods for each skill type
    //edited by CROUZET Oriane, group : L3C1 14/03/2023
    // - Added the RANDOM RANGE skill type
    // - Added the DOOM skill type
    // - Added the FREEZE skill type
    //Author : ?
    void useAttack()
    {
        //Get the characters hitted
        List<Character> hits = hexaGrid.getCharWithinRange(
            attackUsedTargetHexa.getX(),
            attackUsedTargetHexa.getY(),
            attackUsedAttack.rangeAoE
        );
        // Filter target(s)
        if (!attackUsedAttack.targetsEnemies)//target alliés
        {
            for (int i = 0; i < hits.Count; i++)
            {
                if (hits[i].getTeam() != currentCharControlled.getTeam())
                {
                    hits.RemoveAt(i);
                    i--;
                }
            }
        }
        //edited by L3L1 Julien D'aboville 2024 (pour ne pas supprmier si le perso se target lui meme)
        if (!attackUsedAttack.targetsAllies)//target enemies
        {
            for (int i = 0; i < hits.Count; i++)
            {
                if (hits[i].getTeam() == currentCharControlled.getTeam() && hits[i] != currentCharControlled) 
                {
                    hits.RemoveAt(i);
                    i--;
                }
            }
        }
        if (!attackUsedAttack.targetsSelf)
        {
            for (int i = 0; i < hits.Count; i++)
            {
                if (hits[i] == currentCharControlled)
                {
                    hits.RemoveAt(i);
                    i--;
                }
            }
        }

        if (attackUsedAttack.attackEffect == CharsDB.AttackEffect.DAMAGE_OVER_TIME)
        {
            HandlePoison();
        }
        //Added by L3L1 Julien D'aboville 2024(pas terminer)

        /*      if (attackUsedAttack.attackEffect == CharsDB.AttackEffect.CHARGE)
              {
                  //print("CHARGE 2");
                  //   getHoveredHexa();
             //     walkingAnimation();
                  //print("hexahovered charge x" + hexaHovered.getX() + "y" + hexaHovered.getY());
                  int nbCase = actionMove(hexaHovered);
                  //   currentCharControlled.updatePos(pathWalk[pathWalk.Count - 1].getX(), pathWalk[pathWalk.Count - 1].getY(), hexaGrid);
                  currentCharControlled.updatePos(10, 10, hexaGrid);
               //   walkingAnimation();
                  //Added by Socrate Louis Deriza, L3C1
                  //     getEndGameDataCharacterFromTheList(currentCharControlled).addAction(ActionType.MOVE);
                  //    getEndGameDataCharacterFromTheList(currentCharControlled).addMovement(nbCase);

                  //   int nbCase = actionMove(hexaHovered);


                  // getEndGameDataCharacterFromTheList(currentCharControlled).addAction(ActionType.MOVE);
                  //   getEndGameDataCharacterFromTheList(currentCharControlled).addMovement(nbCase);

                       charge = true;
              }*/


        //Added by L3L1 Julien D'aboville 2024
        if (attackUsedAttack.attackEffect == CharsDB.AttackEffect.TELEPORT)
        {
            ////print("TELEPORTATION");
            HandleTeleport();
        }


        int AttaqueBaseValue = attackUsedAttack.effectValue;


        //Give attack effects
        foreach (Character c in hits)
           {
        ////print("attack");


            //Added by L3L1 Julien D'aboville 2024
            passiveGuerrier(c, attackUsedAttack);
            passiveMage();
           // passiveArcher();
            DisableShield(c);
            passiveVoleur(c);
            passiveLifeweaver(attackUsedAttack);
            passiveValkyrie(c, attackUsedAttack);
            passiveNetherfang(c, attackUsedAttack);
            passiveDruide(c, attackUsedAttack);
            passiveForgeron(c);







            //If the attack effect is damage, give damages
            switch (attackUsedAttack.attackEffect)
        {
                //added by Julien D'aboville L3L1 2024

                case CharsDB.AttackEffect.HAMMER:
                    {
                        HandleHammer(c);
                    }
                    break;
                //Added by L3L1 Julien D'aboville 2024

                case CharsDB.AttackEffect.HAMMER_LAUNCH:
                    {
                        HandleHammerLaunch(c);
                    }
                    break;
                    /*
                case CharsDB.AttackEffect.CHARGE:
                    {
                        HandleCharge(c);
                    }
                    break;*/
                case CharsDB.AttackEffect.DAMAGE:

                    {
                        HandleDamage(c);
                    }
                    break;

                //Added by L3L1 Julien D'aboville 2024

                case CharsDB.AttackEffect.ABSORB:

                {
                    HandleAbsorption(c);


                }
                    break;

                //Added by L3L1 Julien D'aboville 2024

                case CharsDB.AttackEffect.SOUL_ABSORPTION:
                    {
                        HandleSoulAbsorption(c);

                    }
                break;

                //Added by L3L1 Julien D'aboville 2024

                case CharsDB.AttackEffect.INVISIBLE:

                    {
                        ////print("invisible2");
                        HandleInvisible(c);
                    }
                    break;
                //Added by L3L1 Julien D'aboville 2024

                case CharsDB.AttackEffect.STORM:

                    {
                        ////print("storm 2");
                        HandleStorm(c);
                    }
                    break;

                //Added by L3L1 Julien D'aboville 2024
                case CharsDB.AttackEffect.SHIELD:

                    {
                        EnableShield(c);
                    }
                    break;

                //Added by L3L1 Julien D'aboville 2024

                case CharsDB.AttackEffect.HOWL:

                    {
                        HandleHowl(c);
                    }
                    break;


                //Added by L3L1 Julien D'aboville 2024

                case CharsDB.AttackEffect.JUMP_EXPLOSION:
                    { HandleJumpExplosion(c); }
                    break;

                //Added by L3L1 Julien D'aboville 2024


                case CharsDB.AttackEffect.RAGE:

                    {
                        //print("RAGE EFFECT");
                        HandleRage(c);
                    }
                    break;

                // Author: CROUZET Oriane, group : L3C1
                // Date : 14/03/2023

                //If the attack effect is random damage, give random damages between 2 to 5 damage points in addition to the value of the original attack
                case CharsDB.AttackEffect.RANDOM_DAMAGE:

                    {
                        HandleRandomDamage(c);
                    }
                    break;

                // Author: CROUZET Oriane, group : L3C1
                // Date : 15/03/2023
                //If the effect is Doom, the character gets an aura that reduce the ennemy attack by two
                case CharsDB.AttackEffect.DOOM:

                    {
                        HandleDoom(c);
                    }
                    break;

                // Author: MEDILEH Youcef, group : L3C1
                // Date : 15/03/2023
                case CharsDB.AttackEffect.LIGHNING:

                    {
                        HandleLightning(c);
                    }
                    break;
                // Author: CROUZET Oriane, group : L3C1
                // Date : 15/03/2023
                //If the effect is freeze, freeze ennemies for one turn (no actions available)
                case CharsDB.AttackEffect.FREEZE:

                    {
                        HandleFreeze(c);
                    }
                    break;

                // Author: MEDILEH Youcef, groupe : L3C1
                // Date : 14/03/2023
                case CharsDB.AttackEffect.HEALTH_STEALING:

                    {
                        HandleHealthSteal(c);
                    }
                    break;
                //If the attack effect is healing, heal
                case CharsDB.AttackEffect.HEAL:

                    {
                        HandleHeal(c);
                    }
                    break;
                // Author: MEDILEH Youcef, groupe : L3C1
                // Date : 13/03/2023
                case CharsDB.AttackEffect.MASSIVE_HEAL:

                    {
                        HandleMassiveHeal();
                    }
                    break;
                //If the attack effect is PA buffing, buff it
                case CharsDB.AttackEffect.PA_BUFF:

                    {
                        HandlePABuff(c);
                    }
                    break;
                //If the attack effect is damage buffing, buff it
                case CharsDB.AttackEffect.DMG_BUFF:

                    {
                        HandleDMGBuff(c);
                    }
                    break;

                // Author : Sepiol SImon groupe L3L1 2024
                case CharsDB.AttackEffect.BURNING:

                    {
                        HandleBurning(c);
                    }
                    break;

                // Author : Sepiol SImon groupe L3L1 2024
                case CharsDB.AttackEffect.DISABLESKILL:

                    {
                        HandleDisableSkill(c);
                    }
                    break;
            }
            //Added by L3L1 Julien D'aboville 2024
            if (currentCharControlled.getName()=="Netherfang" && currentCharControlled.passiveNetherfangActif == true)
            {
                Color purple = new Color(0.5F, 0.5F, 0.5F, 1);
                ShowDamageValueDisplay("Soif de Sang", purple, c.getX(), c.getY());
                GameObject go = GameObject.Instantiate(particlePassiveNetherfang);
                go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);
            }
                    attackUsedAttack.effectValue = AttaqueBaseValue;

        }

        nextTurn();
    }


    //Author by L3L1 Julien D'aboville 2024
    //Lorque le LifeWeaver est tout seul dans son équipe sa compétence 'Absoption d'ame' a son cooldown réduit à 0

    public void EnableSkill1IllimitedLifeWeaver()
    {
        if (currentCharControlled.getName() == "Lifeweaver")

        {

            int nbPersoInTeam = 0;
            
            for (int i = 0; i < hexaGrid.getCharList().Count; i++)
            {
                if (hexaGrid.getCharList()[i].getTeam()== currentCharControlled.getTeam())
                {
                    nbPersoInTeam += 1;

                }
            }

            if (nbPersoInTeam == 1)
            {
                print("avant Cooldown attaque skill 1lifeweaver :" + currentCharControlled.getSkill1CoolDown());
                currentCharControlled.attack1coolDownValue = 0;
              //  currentCharControlled.attack1coolDownValue = currentCharControlled.getSkill1CoolDown();
                print("apres Cooldown attaque skill 1lifeweaver :" + currentCharControlled.getSkill1CoolDown());

            }

        }
    }



    //Author by L3L1 Julien D'aboville 2024


    //Protection Magique : La valkyrie reçoit 2 points de dégâts en moins des attaques magiques et elle récupère 34% en HP des dégats de son attaque.
    public void passiveValkyrie(Character c, CharsDB.Attack attack)
    {
        float pourcentageHpDeSonAttaque = 0.34f;
        int degatsMagiqueBloques = 2;
        if (c.getName()== "Valkyrie" && currentCharControlled.getName() == "Mage")
        {
            //print("Prenom : " + c.getName());
            //print("Avant : " + attackUsedAttack.effectValue);
            //print("HP actuelle :" + c.HP);
            //print("HP maxx   :" + c.getHPmax());
            attack.effectValue -= degatsMagiqueBloques;
            ShowDamageValueDisplay("Resistance de  de " + degatsMagiqueBloques + "degats magiques", Color.yellow, c.getX(), c.getY());





        }
        if (currentCharControlled.getName() == "Valkyrie")
        {

                int vieRecupereParAttaque = (int)(pourcentageHpDeSonAttaque * attack.effectValue);
                currentCharControlled.setHP(currentCharControlled.getHP() + vieRecupereParAttaque);
                ShowDamageValueDisplay("Gain de " + vieRecupereParAttaque, Color.yellow, currentCharControlled.getX(), currentCharControlled.getY());

            


        }


    }

    //Author by L3L1 Julien D'aboville 2024
    //   Siphon d'Âme : Le druide absorbe les points d'action des ennemis qu'il bat.

    public void passiveDruide(Character c, CharsDB.Attack attack)
    {
        //print("passiveDruide  non activé:" + currentCharControlled.getPA());

        if (currentCharControlled.getName() == "Druide")
        {
            print("passiveDruide activé :" + currentCharControlled.getPA());

            if (attack.effectValue >= c.getHP())
            {
                print("currentCharControlled.PA  :" + currentCharControlled.getPA());

                currentCharControlled.setPa(currentCharControlled.getPA()+ c.getCharacterPA());
                print("currentCharControlled.PA  :" + currentCharControlled.getPA());
                ShowDamageValueDisplay("Gain de " + c.characterPA + "PA", Color.yellow, currentCharControlled.getX(), currentCharControlled.getY());


            }

        }



    }

    //Author by L3L1 Julien D'aboville 2024
    //   Ascension Arcanique  : A partir de 15 tours l'envouteur gagne 2 points de base de PA supplémentaires.

    public void passiveEnvouteur()
    {
        int pointBonusPASupplemntaire = 2;
        int nombresDeToursApresActivation = 14;
        //print("passiveEnvouteur non activé:" + currentCharControlled.getPA()+" tour :"+tour);

        if (currentCharControlled.getName() == "Envouteur")
        {
     
            if (tour >= nombresDeToursApresActivation && !currentCharControlled.passiveEnvouteurActif)
            {
                currentCharControlled.bonusPassive = pointBonusPASupplemntaire;
                print("passiveEnvouteur  activé:" + currentCharControlled.getPA()+" tour :"+tour);

            
                currentCharControlled.setCharacterPA(currentCharControlled.getCharacterPA() + currentCharControlled.bonusPassive);


                ShowDamageValueDisplay("Gain de " + currentCharControlled.bonusPassive + " basePA et dégats sur ses compétences", Color.yellow, currentCharControlled.getX(), currentCharControlled.getY());

                currentCharControlled.passiveEnvouteurActif = true;

            }
        }



    }




    //Author by L3L1 Julien D'aboville 2024
    // Vol de Vie: Si Netherfang a moins de 50% de sa vie, il récupère la moitié des dégâts qu'il fait en points de vie de son attaque .
    public void passiveNetherfang(Character c, CharsDB.Attack attack)
    {

        if (currentCharControlled.getName() == "Netherfang" )
        {
            float degatBonusHowl = 0;
            float pourcentageVieActivation = 0.50f;
            float pourcentagePointDeVieRecupere = 0.50f;
            if (currentCharControlled.getHP() <= pourcentageVieActivation * currentCharControlled.getHPmax())
            {
                if (currentCharControlled.howl)
                {
                    degatBonusHowl = currentCharControlled.nbHowl * currentCharControlled.damageBuffHowl;
                }
                int vieRecupereParAttaque = (int)(pourcentagePointDeVieRecupere * (attack.effectValue + degatBonusHowl));
                print("vieRecupereParAttaque :" + vieRecupereParAttaque);
                print("pourcentagePointDeVieRecupere");
                
                currentCharControlled.setHP(currentCharControlled.getHP() + vieRecupereParAttaque);
                ShowDamageValueDisplay("Gain de " + vieRecupereParAttaque, Color.yellow, currentCharControlled.getX(), currentCharControlled.getY());
                currentCharControlled.passiveNetherfangActif = true;
            }
            else
            {
                currentCharControlled.passiveNetherfangActif = false;
            }


        }

    }

    //Author by L3L1 Julien D'aboville 2024
    //Attaque Surprise : Le voleur a 20% de chances d'étourdir son ennemi quand il l'attaque
    public void passiveVoleur(Character c)
    {

        if (currentCharControlled.getName() == "Voleur")
        {
            print("Prenom : " + c.getName());
            print("Avant : " + attackUsedAttack.effectValue);
            print("HP actuelle :" + c.getHP());
            print("HP maxx   :" + c.getHPmax());

            System.Random rnd = new();
            int randomNumber = rnd.Next(1, 6);
            //entre 1 et 5
            if (currentCharControlled.isInvisible() && randomNumber==1)
            {
                c.setStunned(true);
                ShowDamageValueDisplay("Charme latent" , Color.yellow, c.getX(), c.getY());

            }


        }

    }

    //Author by L3L1 Julien D'aboville 2024
    //Guérison Forestière : Si le bûcheron a moins de 25% de sa vie totale, il récupère 2 point de vie chaque attaque sur un adversaire et gagne un unique point d'action supplémentaire.
    public void passiveForgeron(Character c)
    {
        //print("currentCharControlled.getName() :" + currentCharControlled.getName());
        //print("passive forgeron desactivé ");
        //print("currentCharControlled.getHP() :" + currentCharControlled.getHP());


        if (currentCharControlled.getName() == "Forgeron")
        {
            float pourcentageVieActivation = 0.25f;

            print("passive Forgeron activé ");

            float pourcentageHpMax = pourcentageVieActivation * currentCharControlled.getHPmax();
            GameObject go=null;
            //entre 1 et 4
            int bonusPassiveForgeron = 2;
            print("pourcentageHpMax:" + pourcentageHpMax);
            print("currentCharControlled.getHP() :" + currentCharControlled.getHP());

            if (currentCharControlled.getHP()<= pourcentageHpMax)
            {
                go = Instantiate(particlePassiveForgeron);

                Vector3 targetPosition = Hexa.hexaPosToReal(currentCharControlled.getX(), currentCharControlled.getY(), 0);
                go.transform.position = targetPosition;



                go.transform.SetParent(currentCharControlled.go.transform, false);

                // Permet d'ajuster la position locale pour maintenir 'go' à la position ciblée par rapport à son parent currentCharControlled.go
                go.transform.localPosition = currentCharControlled.go.transform.InverseTransformPoint(targetPosition);
                ParticleSystem particleSystem = go.GetComponent<ParticleSystem>();



                print("CHARACTER.PA = " + currentCharControlled.getCharacterPA());
                print("currentCharControlled.myCharClass.basePA avant" + currentCharControlled.myCharClass.basePA);
                StartCoroutine(DisablePassiveForgeron(currentCharControlled, pourcentageHpMax, go, particleSystem));


         

                if (currentCharControlled.getCharacterPA() == 2)
                {
                    currentCharControlled.setCharacterPA(3);
                    

                }
              
              // print("currentCharControlled.myCharClass.basePA apres" + currentCharControlled.myCharClass.basePA);
                print("currentCharControlled.getCharacterPA() apres" + currentCharControlled.getCharacterPA());



                currentCharControlled.setHP(currentCharControlled.getHP()+ bonusPassiveForgeron);

                
                ShowDamageValueDisplay("Gain de " + bonusPassiveForgeron, Color.yellow, currentCharControlled.getX(), currentCharControlled.getY());

            }
            else if (currentCharControlled.getHP() > pourcentageHpMax)
            {
       
                currentCharControlled.setCharacterPA(2);
                Destroy(go);

            }


        }

    }
    //Author by L3L1 Julien D'aboville 2024
    //Description : Désactive le passive du forgeron
    IEnumerator DisablePassiveForgeron(Character currentCharControlled, float pourcentageHpMax,GameObject go, ParticleSystem particleSystem)
    {
        yield return new WaitUntil(() => currentCharControlled.getHP() >pourcentageHpMax);
        particleSystem.Stop();
        Destroy(go);


        ShowDamageValueDisplay("PASSIVE OFF", Color.gray, currentCharControlled.getX(), currentCharControlled.getY());


    }

    //Author by L3L1 Julien D'aboville 2024
    //Lien vital : Quand le Lifeweaver soigne quelqu'un, il reçoit aussi 40% de cette guérison (Pour la compétence Absorption il recoit seulement lors de la 1ere activation).
    public void passiveLifeweaver(CharsDB.Attack attack)
    {


        if (currentCharControlled.getName() == "Lifeweaver")
        {
            float pourcentageVieGuerrison = 0.40f;


            if (attack.attackEffect == CharsDB.AttackEffect.HEAL || attack.attackEffect == CharsDB.AttackEffect.ABSORB)
            {
              

                if (attack.attackEffect == CharsDB.AttackEffect.HEAL || attack.attackEffect == CharsDB.AttackEffect.ABSORB)
                {
                    int bonusDeSoin = (int)(attack.effectValue * pourcentageVieGuerrison);
                    if (bonusDeSoin + currentCharControlled.getHP() <= currentCharControlled.getHPmax())
                    {
                        GameObject go = GameObject.Instantiate(particleHeal);
                        go.transform.position = Hexa.hexaPosToReal(currentCharControlled.getX(), currentCharControlled.getY(), 0);
                        go.transform.localScale *= 0.1f;
                        currentCharControlled.setHP(currentCharControlled.getHP() + bonusDeSoin);
                        ShowDamageValueDisplay("Bonus de soin" + bonusDeSoin, Color.yellow, currentCharControlled.getX(), currentCharControlled.getY());
                        Destroy(go, 5);
                    }


                }


            }

        }
    }

    //Author by L3L1 Julien D'aboville 2024

    //Mur de fer : Renforcement de la défense du guerrier, qui diminue les dégâts subis de 1 point, lorsque sa santé est réduite de 50%.
    public void passiveGuerrier(Character c, CharsDB.Attack attack)
    {

        if (c.getName() == "Guerrier")
        {
            float pourcentageVieActivation = 0.50f;
            int nbPointDiminutionDegats = 1;
            //print("Prenom : " + c.getName());
            //print("Avant : " + attackUsedAttack.effectValue);
            //print("HP actuelle :" + c.HP);
            //print("HP maxx   :" + c.getHPmax());

            if (c.getHP() < c.getHPmax() * pourcentageVieActivation)
            {
                attack.effectValue -= nbPointDiminutionDegats;
                if (attack.effectValue <= 0)
                {
                    attack.effectValue = 0;
                }
                //print("Apres : " + attackUsedAttack.effectValue);

            }

        }

    }

    //Author by L3L1 Julien D'aboville 2024
    //Conduit arcanique : Diminution de 2 unités pour la récupération des cooldowns du mage lorsque des alliés à proximité utilisent leurs compétences.
    public void passiveMage()
    {
        int nbCooldownReductionBonus = 2;
        int distanceAllieAProximite = 10;
        //print("hexaGrid.getCharList().Count = " + hexaGrid.getCharList().Count);
        for (int i = 0; i < hexaGrid.getCharList().Count; i++)
        {
            Character c1 = hexaGrid.getCharList()[i];
            if (c1.getName()=="Mage" && currentCharControlled.getTeam() == c1.getTeam() && currentCharControlled.getName()!= "Mage")
            {
                if (hexaGrid.getDistance(currentCharControlled.getX(), currentCharControlled.getY(), c1.getX(),c1.getY()) <= distanceAllieAProximite)
                {
                    
                    if (c1.getAttack1coolDownValue() - nbCooldownReductionBonus >= 0)
                    {
                       
                
                        c1.setAttack1coolDownValue(c1.getAttack1coolDownValue() - nbCooldownReductionBonus);
                    }
                    else
                    {
              
                        c1.setAttack1coolDownValue(0);


                    }
                    if (c1.getAttack2coolDownValue() - nbCooldownReductionBonus >= 0)
                    {
    
                        c1.setAttack2coolDownValue(c1.getAttack2coolDownValue() - nbCooldownReductionBonus);

                        //    //print("cooldownRestantBonus = " + cooldownRestantBonus + "c1.getSkill2CoolDown() :" + c1.getSkill2CoolDown());
                    }
                    else
                    {
            
                        c1.setAttack2coolDownValue(0);


                    }
                    if (c1.getAttack3coolDownValue() - nbCooldownReductionBonus >= 0)
                    {
               
                        c1.setAttack3coolDownValue(c1.getAttack3coolDownValue() - nbCooldownReductionBonus);


                    }
                    else
                    {
                    
                        c1.setAttack3coolDownValue(0);

                    

                    }


                }
            }
        }

   

        }


    //Author by L3L1 Julien D'aboville 2024

    //Aura : Le soigneur active une guérison automatique de 1 point de vie pour tous les alliés situés à une distance de 2 hexagones ou moins à chaque point d'action.
    public void passiveSoigneur()
    {
        int distanceGuerisonAutomatique = 2;
        int nbPointDeVieAugmente = 1;
        foreach (Character c1 in hexaGrid.getCharList())
        {
            if (c1.getName() == "Soigneur" )
            {
                foreach (Character c2 in hexaGrid.getCharList())
                {
                    if (c2.getName() != "Soigneur" && c2.getTeam() == c1.getTeam())
                    {
                        //print("c2.getTeam() :" + c2.getTeam() + "nom c2:" + c2.getName());
                        //print("c1.getTeam() :" + c1.getTeam() + "nom c1:"+c1.getName());

                        if (hexaGrid.getDistance(c2.getX(), c2.getY(), c1.getX(), c1.getY()) <= distanceGuerisonAutomatique)
                        {
                            if (c2.getHP() + nbPointDeVieAugmente <= c2.getHPmax())
                            {
                                //print("VALIDE c2.getTeam() :" + c2.getTeam() + "nom c2:" + c2.getName());
                                //print("VALIDE c1.getTeam() :" + c1.getTeam() + "nom c1:" + c1.getName());
                                c2.setHP(c2.getHP() + nbPointDeVieAugmente);
                                GameObject go = GameObject.Instantiate(particleHeal);
                                go.transform.position = Hexa.hexaPosToReal(c2.getX(), c2.getY(), 0);
                                go.transform.localScale *= 0.1f;
                                Destroy(go, 5);
                            }
                            



                        }
                    }

                }
            }

                    
              
            }
        }




    //Author by L3L1 Julien D'aboville 2024

    //Oeil de faucon : Augmentation de la portée de tir de l'archer de 2 unités lorsque sa santé tombe sous 1/3 de ses points de vie max une seule fois.
    public void passiveArcher()
    {
        int distanceAugmentation = 2;
        float pourcentageVieActivation = 0.33f;
        if (currentCharControlled.getName() == "Archer" && currentCharControlled.passiveArcherActif == false)
        {
            //print("Prenom : " + c.getName());
            //print("Avant : " + attackUsedAttack.effectValue);
            //print("HP actuelle :" + c.HP);
            //print("HP maxx   :" + c.getHPmax());


            if (currentCharControlled.getHP() <= (currentCharControlled.getHPmax() * pourcentageVieActivation))
            {
                print("avant passif Range archer :" + currentCharControlled.getRange());
                //print("Avant range :" + c.getRange());

                currentCharControlled.setRange(currentCharControlled.getRange() + distanceAugmentation);
                currentCharControlled.passiveArcherActif = true;

                print("apres passif Range archer :" + currentCharControlled.getRange());

                //print("apres range :" + c.getRange());

            }

        }

    }


    // Author : Sepiol SImon groupe L3L1 2024
    private void HandleDisableSkill(Character c){
        c.wasSkill1Available=c.skillAvailable;
        c.wasSkill2Available=c.skillAvailable2;
        c.wasSkill3Available=c.skillAvailable3;
        //print("skill desactive");
        c.disabledSkill=attackUsedAttack.effectValue+1; // plus un car si on veux attendre 1 tour il faut 2 a cette valeur
    }

    // Author: MEDILEH Youcef, groupe : L3C1
    // edited by Simon Sepiol L3L1 2024
    private void HandlePoison()
    {
        foreach (
            Point p in hexaGrid.getHexasWithinRange(
                attackUsedTargetHexa.getX(),
                attackUsedTargetHexa.getY(),
                attackUsedAttack.rangeAoE
            )
        )
        {
            // get team of the character to set the turn of the poisonned hexas
            if (currentCharControlled.getTeam() == 0)
            {
                turnCounterDamageOverTimeTEAM1 = 3;
            }
            else
            {
                turnCounterDamageOverTimeTEAM2 = 3;
            }

            hexaGrid.getHexa(p.getX(), p.getY()).isPoisoned = true;
            // addded by Simon
            hexaGrid.getHexa(p.getX(), p.getY()).whoPoisonedIt= currentCharControlled;
            //
            hexaGrid.getHexa(p.getX(), p.getY()).poisonnedDamage = attackUsedAttack.effectValue;
            Debug.Log("hexa " + p.getX() + "," + p.getY() + " is poisonned");
            Debug.Log("Damage over time : " + attackUsedAttack.effectValue);
            poisonnedHexas.Add(hexaGrid.getHexa(p.getX(), p.getY()));
            // change the color of the hexa
            hexaGrid.getHexa(p.getX(), p.getY()).go.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    // Author: MEDILEH Youcef, groupe : L3C1
    private void HandleLightning(Character c)
    {
        Debug.Log("Lightning");
        HandleDamage(c);
        HandleFreeze(c);
        // affichage des noms des personnages touchés


        Debug.Log(c.getName());
    }

    private void HandleDMGBuff(Character c)
    {
        if (!c.dmgbuff)
        {
            c.dmgbuff = true;
           
            ShowDamageValueDisplay("+" + attackUsedAttack.effectValue, Color.yellow, c.getX(), c.getY());
        }
    }

    private void HandlePABuff(Character c)
    {
        Debug.Log("1: " + c.getPA() + "\n2: " + c.getClassData().basePA);
        if (c.characterPA == c.getClassData().basePA)
        {
            // Create object that shows pa buff
            
            ShowDamageValueDisplay("+" + attackUsedAttack.effectValue, Color.blue, c.getX(), c.getY());

            if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD)
                statsGame.addToDamageDealt(currentCharControlled, attackUsedAttack.effectValue);

            currentCharControlled.totalDamage += attackUsedAttack.effectValue;
            c.PA = c.characterPA + attackUsedAttack.effectValue;
        }
    }

    // Author: MEDILEH Youcef, groupe : L3C1
    // Permet de soigner tous les alliés
    private void HandleMassiveHeal()
    {
        // heal all allies units
        foreach (Character c2 in hexaGrid.getCharList())
        {
            if (c2.getTeam() == currentCharControlled.getTeam())
            {
                Debug.Log("Heal " + c2.getName() + " for " + attackUsedAttack.effectValue);
                // Create object that shows heal
             
                int heal = attackUsedAttack.effectValue;
                if (heal > c2.HPmax - c2.HP) heal = c2.HPmax - c2.HP;
               
                ShowDamageValueDisplay("+" + heal, Color.green, c2.getX(), c2.getY());


                if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD) statsGame.addToHeal(currentCharControlled, heal);

                currentCharControlled.totalDamage += heal;
                c2.HP += heal;
            }
        }
    }

   // Edited by L3L1 Julien D'aboville 2024

    private void HandleHeal(Character c)
    {
 
        InstantiateParticleHeal(c);

        int heal = attackUsedAttack.effectValue;
        if (heal > c.getHPmax() - c.HP) heal = c.getHPmax() - c.HP;

        ShowDamageValueDisplay("+"+heal, Color.green, c.getX(), c.getY());

        if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD) statsGame.addToHeal(currentCharControlled, heal);

        currentCharControlled.totalDamage += heal;
        c.setHP(c.getHP()+heal);
    }







    //added by Julien D'aboville L3L1 2024
    //Description : Compétence qui permet au personnage de se téléporter

    private void HandleTeleport()
    {
        //print("attackUsedTargetHexa X" + attackUsedTargetHexa.getX() + " attackUsedTargetHexa Y :" + attackUsedTargetHexa.getY());
        currentCharControlled.updatePos(attackUsedTargetHexa.getX(), attackUsedTargetHexa.getY(),hexaGrid);
        ShowDamageValueDisplay("TELEPORT", Color.blue, attackUsedTargetHexa.getX(), attackUsedTargetHexa.getY());
    
    }







    //added by Julien D'aboville L3L1 2024
    // Description ; Compétence qui permet au personnage de sauter sur sa position en infligeant des dégats de zone

    private void HandleJumpExplosion(Character c)
    {
        if (c.getTeam() != currentCharControlled.getTeam())
        {
            c.setHP(c.getHP() - attackUsedAttack.effectValue);
       
            hammerExplosionSource.Play();
            GameObject go = GameObject.Instantiate(particleSmoke);
            go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);
            Debug.Log("Jump Explosion");
            ShowDamageValueDisplay("-" + attackUsedAttack.effectValue, Color.red, c.getX(), c.getY());


        }


        bool isDead = IsDead(c);
        if (isDead)
        {
            getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
        }
    }


    //added by Julien D'aboville L3L1 2024 
    //Description : Attaque au marteau avec dégat de zone
    private void HandleHammer(Character c)
    {
        c.setHP(c.getHP() - attackUsedAttack.effectValue);

   
        hammerExplosionSource.Play();
        GameObject go = GameObject.Instantiate(particleSmoke);
        go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);
        ShowDamageValueDisplay("-" + attackUsedAttack.effectValue, Color.red, c.getX(), c.getY());

        bool isDead = IsDead(c);
        if (isDead)
        {
            getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
        }


    }

    //added by Julien D'aboville L3L1 2024 
    //Description : Compétence Hurlement qui augmente les PV et les dégats du personnage

    private void HandleHowl(Character c)
    {
        currentCharControlled.howl = true;
        currentCharControlled.damageBuffHowl = attackUsedAttack.effectValue;
        GameObject go = GameObject.Instantiate(particleHowl);
        go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);

        ShowDamageValueDisplay("HURLEMENT", Color.green, c.getX(), c.getY());

        int heal = attackUsedAttack.effectValue;
        


        if (heal > c.getHPmax() - c.getHP()) heal = c.getHPmax() - c.getHP();

        c.setHP(c.getHP() + heal);

        howlsource.Play();

        currentCharControlled.nbHowl += 1;
 

    }



    //added by Julien D'aboville L3L1 2024
    //Description : Lance un marteau sur un adversaire infligeant des dégats et une probabilté d'1/4 d'étourdir l'adversaire

    void HandleHammerLaunch(Character c)
    {
        System.Random rnd = new();
        int randomNumber = rnd.Next(1, 5);//entre 1 et 4
   
        if (randomNumber == 1)
        {
            c.setStunned(true);
            ShowDamageValueDisplay("Stunned (-"+attackUsedAttack.effectValue+")", Color.red, c.getX(), c.getY());

        }
        else {
            ShowDamageValueDisplay("-"+attackUsedAttack.effectValue,  Color.red, c.getX(), c.getY());
        }

        GameObject go = GameObject.Instantiate(particleHammerLaunch);
        go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);
        int hpRestant = c.getHP() - attackUsedAttack.effectValue;
        c.setHP(hpRestant);
        bool isDead = IsDead(c);
        if (isDead)
        {
            getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
        }
    }

    //added by Julien D'aboville L3L1 2024 
    //Description : Compétence d'invisibilité sur 4 tours avec dégat supplémentaire sur sa compétence de base

    private void HandleInvisible(Character c)
    {
        int dureeTours = 4;
        c.go.SetActive(false);
        
        ShowDamageValueDisplay("INVISIBLE", Color.black, c.getX(), c.getY());

        currentCharControlled.invisibiliteDamage = attackUsedAttack.effectValue;

        float tourInitial = tour;
        //print("TOUR Initial  :" + tourInitial);
        float tourFinal = tour + dureeTours;
   

        c.estInvisible = true;
        StartCoroutine(DisableInvisible( c, tourFinal, currentCharControlled.invisibiliteDamage));

        //print("invisible");
        
    }


    //added by Julien D'aboville L3L1 2024 
    IEnumerator DisableInvisible(Character c,float tourFinal,int damageBuff)
        
    {
        yield return new WaitUntil(() => tour == tourFinal);
   
        c.go.SetActive(true);

        c.estInvisible = false;



    }







    //added by Julien D'aboville L3L1 2024 
    //Description : Compétence tempête qui inflige des degats sur tous les ennemis dans la zone

    private void HandleStorm(Character c)
    {
        c.setHP(c.getHP()-attackUsedAttack.effectValue);

  
        Color purple = new Color(0.5F, 0.5F, 0.5F, 1);
        ShowDamageValueDisplay("STORM", purple, c.getX(), c.getY());
        GameObject go = GameObject.Instantiate(particleStormPrefab);
        go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);

  
        bool isDead = IsDead(c);
        if (isDead)
        {
            getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
        }
    }

    //added by Julien D'aboville L3L1 2024 
    //Attaque à distance qui inflige des dégats sur une cible

    private void HandleSoulAbsorption(Character c)
    {
        c.setHP(c.getHP() - attackUsedAttack.effectValue);


        Color purple = new Color(0.5F, 0.5F, 0.5F, 1);
        ShowDamageValueDisplay("Soul Absorption", purple, c.getX(), c.getY());
        GameObject go = GameObject.Instantiate(particleSoulAbsorptionPrefab);
        go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);

        Destroy(go, 3f);




        bool isDead = IsDead(c);
        if (isDead)
        {
            getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
        }
    }





    //added by Julien D'aboville L3L1 2024 
    //Description : permet d'annuler le shield dans le cas où il est actif

    private void DisableShield(Character c)
    {
     
        if (c.getShield() == true && currentCharControlled.getTeam()!=c.getTeam())//vérifie qu'il ne sont pas dans la meme type (pour pas qu'allié désactive son shield en lui donnant un heal)
        {
            //print("protéger");
            shieldBreaksource.Play();
            ShowDamageValueDisplay("SHIELD ACTIVE DESACTIVE", Color.blue, c.getX(), c.getY());
    
            c.setHP(c.getHP() + attackUsedAttack.effectValue);
            c.setShield(false);

        }
    }

    
    //added by Julien D'aboville L3L1 2024 
    //Description : permet d'activer le shield
    private void EnableShield(Character c)
    {
        ShowDamageValueDisplay("SHIELD ACTIVE", Color.blue, c.getX(), c.getY());

        //print("Active shied");
        c.setShield(true);
        //currentCharControlled.setShield(true); //christophe 3/24
    }

    





    //added by Julien D'aboville L3L1 2024
    /// <summary>
    /// Affiche la valeur des dégâts infligés
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void ShowDamageValueDisplay(string text, Color color, int x, int y)
    {
        GameObject dmgValueDisp = Instantiate(damageValueDisplay);
        dmgValueDisp.GetComponent<DamageValueDisplay>().camera_ = cameraPos;
        dmgValueDisp.GetComponent<DamageValueDisplay>().setValue(x, y, text, color, 60);
    }

    //added by Julien D'aboville 2024 L3L1
    // Description : compétence de Rage qui augmente les PV et les dégats de base pendant 4 tours
    private void HandleRage(Character c)
    {
        Color pink = new Color(1, 0.5F, 0.5F, 1);
        ShowDamageValueDisplay("RAGE", pink, c.getX(), c.getY());

        
        int heal = attackUsedAttack.effectValue; // permet d'aller au dessu de ces PV max
        currentCharControlled.damageInRage = attackUsedAttack.effectValue;

        //print("TOUR Initial  :" + tour);
        float tourFinal = tour + 4;

        c.setHP(c.getHP()+heal);
        //print("Affichage Rage");
        //print(c.getHP());
        c.raged = true;



        GameObject go = Instantiate(particleRagePrefab);

        Vector3 targetPosition = Hexa.hexaPosToReal(c.getX(), c.getY(), 0.5f);
        go.transform.position = targetPosition;



        go.transform.SetParent(c.go.transform, false);

        // Permet d'ajuster la position locale pour maintenir 'go' à la position ciblée par rapport à son parent c.go
        go.transform.localPosition = c.go.transform.InverseTransformPoint(targetPosition);

        ParticleSystem particleSystem = go.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        






        // go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);


        StartCoroutine(DisableRage(c, currentCharControlled.damageInRage, heal, tourFinal,go, particleSystem));
    }




    //added by Julien D'aboville L3L1 2024

    /// <summary>
    /// Désactive le sort de rage
    /// </summary>
    /// <param name="c"></param>
    /// <param name="damageInRage"></param>
    /// <param name="heal"></param>
    /// <param name="tourFinal"></param>
    /// <param name="go"></param>
    /// <param name="particleSystem"></param>
    /// <returns></returns>
    IEnumerator DisableRage(Character c, int damageInRage, int heal, float tourFinal, GameObject go, ParticleSystem particleSystem)
    {
        yield return new WaitUntil(() => tour >= tourFinal);
        particleSystem.Stop();
        Destroy(go);

        c.raged = false;

        //print("Rage désactivé:" + tour);
        c.HP -= heal; 
      

        ShowDamageValueDisplay("RAGE OFF", Color.gray, c.getX(), c.getY());

      
    }

    //added by Julien D'aboville L3L1 2024 
    /// <summary>
    ///  compétence qui augmente les pv d'un allié  l'instant t et après 3 tours suivant l'activation. (ou 2 si c'est sa dernière compétence  utilisée)  .
    /// </summary>
    /// <param name="c"></param>
    private void HandleAbsorption(Character c)
    {
        int nbToursAttente = 3;
        
      
        int heal = attackUsedAttack.effectValue;
        //    int heal = 10;
        if (heal > c.getHPmax() - c.getHP())
            heal = c.getHPmax() - c.getHP();
        

        ShowDamageValueDisplay("Absorption", Color.green, c.getX(), c.getY());
        float tourInitial = tour;
        //print("TOUR Initial  :" + tourInitial);
        float tourFinal = tour + nbToursAttente;
        c.setHP(c.getHP()+heal);
        //print("Affichage Absorption");
        //print(c.getHP());

        InstantiateParticleHeal(c);



        //print(c.myCharClass.basicAttack.effectValue);
        StartCoroutine(EnableAbsorb(c,heal, tourFinal));

    }

    //added by Julien D'aboville L3L1 2024
    /// <summary>
    /// Active l’absorption
    /// </summary>
    /// <param name="c"></param>
    public void InstantiateParticleHeal(Character c)
    {
        GameObject go = GameObject.Instantiate(particleHeal);
        go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);
        go.transform.localScale *= 0.1f;
        Destroy(go, 5);
    }

    //added by Julien D'aboville L3L1 2024
    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <param name="heal"></param>
    /// <param name="tourFinal"></param>
    /// <returns></returns>
    IEnumerator EnableAbsorb(Character c, int heal, float tourFinal)

    {
        yield return new WaitUntil(() => tour == tourFinal);

        InstantiateParticleHeal(c);
        if (heal > c.getHPmax() - c.getHP()) heal = c.getHPmax() - c.getHP();
        c.setHP(c.getHP()+heal);
       
       
    }



    // Author: MEDILEH Youcef, groupe : L3C1
    // Permet de voler de la vie à un ennemi
    private void HandleHealthSteal(Character c)
    {
        // Create object that shows damage dealt (health steal)
        GameObject healthStolenDisp = GameObject.Instantiate(damageValueDisplay);
        healthStolenDisp.GetComponent<DamageValueDisplay>().camera_ = cameraPos;
        c.setHP(c.getHP()-attackUsedAttack.effectValue);
        healthStolenDisp
            .GetComponent<DamageValueDisplay>()
            .setValue(c.getX(), c.getY(), "-" + attackUsedAttack.effectValue, Color.red, 60);
        Debug.Log(
            "###### Health stolen from "
                + c.getName()
                + " : "
                + attackUsedAttack.effectValue
                + " HP ######"
        );
        if (currentCharControlled.HP + attackUsedAttack.effectValue > currentCharControlled.HPmax)
        {
            currentCharControlled.HP = currentCharControlled.HPmax;
        }
        else
        {
            currentCharControlled.HP += attackUsedAttack.effectValue;
        }
        healthStolenDisp
            .GetComponent<DamageValueDisplay>()
            .setValue(
                currentCharControlled.getX(),
                currentCharControlled.getY(),
                "+" + attackUsedAttack.effectValue,
                Color.green,
                60
            );
        Debug.Log(
            "###### Health restored to "
                + currentCharControlled.getName()
                + " : "
                + attackUsedAttack.effectValue
                + " HP ######"
        );
        if (whoControlsThisChar(c) == PlayerType.AI_HARD)
            statsGame.addToDamageTaken(c, attackUsedAttack.effectValue);
        if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD)
            statsGame.addToDamageDealt(currentCharControlled, attackUsedAttack.effectValue);
        // Enemy dies
        bool isDead = IsDead(c);
        if (isDead)
        {
            getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
        }
    }

    // Author: CROUZET Oriane, groupe : L3C1
    // Permet de geler un ennemi
    //Edited by Julien D'aboville L3L1 2024
    private void HandleFreeze(Character c)
    {
        if (!c.freezed)
        {
            c.setFreezed(true);
            // Create object that shows freeze
            ShowDamageValueDisplay("Freezed", Color.blue, c.getX(), c.getY());
 
            GameObject go = GameObject.Instantiate(particleFreeze);
            go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 1f);


        }
    }

    // Author: CROUZET Oriane, groupe : L3C1
    private void HandleDoom(Character c)
    {
        if (!c.doomed)
        {
            c.setDoomed(true);
            // Create object that shows doom
            ShowDamageValueDisplay("Doomed", Color.red, c.getX(), c.getY());
        }
    }

    // Author: CROUZET Oriane, groupe : L3C1

    // si un personnage est doom les degats qu'il infliges sont reduits puis le doom part
    private void HandleRandomDamage(Character c)
    {
        System.Random rnd = new();
        int randomNumberDamage = rnd.Next(1, 5);

        if (currentCharControlled.dmgbuff)
        {
           
            ShowDamageValueDisplay("-" + (attackUsedAttack.effectValue + 1 + randomNumberDamage), Color.red, c.getX(), c.getY());

        }
        else if (currentCharControlled.doomed)
        {
            ShowDamageValueDisplay("-" + (attackUsedAttack.effectValue + randomNumberDamage - doomDamage), Color.red, c.getX(), c.getY());

          
        }
        else
        {
            ShowDamageValueDisplay("-" + (attackUsedAttack.effectValue + randomNumberDamage), Color.red, c.getX(), c.getY());

           
        }
        if (whoControlsThisChar(c) == PlayerType.AI_HARD)
            statsGame.addToDamageTaken(c, (attackUsedAttack.effectValue + randomNumberDamage));
        if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD)
            statsGame.addToDamageDealt(
                currentCharControlled,
                (attackUsedAttack.effectValue + randomNumberDamage)
            );

        if (currentCharControlled.dmgbuff)
        {
            currentCharControlled.totalDamage += (
                attackUsedAttack.effectValue + 1 + randomNumberDamage
            );
            c.HP -= (attackUsedAttack.effectValue + 1 + randomNumberDamage);
        }
        else if (currentCharControlled.doomed)
        {
            currentCharControlled.totalDamage += (
                attackUsedAttack.effectValue - doomDamage + randomNumberDamage
            );
            c.HP -= (attackUsedAttack.effectValue - doomDamage + randomNumberDamage);
            currentCharControlled.setDoomed(false);
        }
        else
        {
            currentCharControlled.totalDamage += (
                attackUsedAttack.effectValue + randomNumberDamage
            );
            c.HP -= (attackUsedAttack.effectValue + randomNumberDamage);
        }
        // Enemy dies
        bool isDead = IsDead(c);
        if (isDead)
        {
            getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
        }
    }

    // si un personnage est doom les degats qu'il infliges sont reduits puis le doom part
    //Edited by Julien D'aboville L3L1 2024 
    private void HandleDamage(Character c)
    {
        // Create object that shows damage dealt
        if (currentCharControlled.dmgbuff)
        {

            ShowDamageValueDisplay("-" + (attackUsedAttack.effectValue + 1 ), Color.red, c.getX(), c.getY());

        }
        else if (currentCharControlled.doomed)
        {
            ShowDamageValueDisplay("-" + (attackUsedAttack.effectValue- doomDamage), Color.red, c.getX(), c.getY());


        }
        //Added by Julien D'aboville L3L1 2024 

        else if (currentCharControlled.estEnRage())
        {
            ShowDamageValueDisplay("-" + (attackUsedAttack.effectValue + currentCharControlled.damageInRage), Color.red, c.getX(), c.getY());
            int damageBuff= (attackUsedAttack.effectValue + currentCharControlled.damageInRage);
            c.setHP(c.getHP() - damageBuff);
        }

        //Added by Julien D'aboville L3L1 2024 

        else if (currentCharControlled.isInvisible())
        {
            ShowDamageValueDisplay("-" + (attackUsedAttack.effectValue + currentCharControlled.invisibiliteDamage), Color.red, c.getX(), c.getY());
            int damageBuff = (attackUsedAttack.effectValue + currentCharControlled.invisibiliteDamage);
            c.setHP(c.getHP() - damageBuff);

        }
        //Added by Julien D'aboville L3L1 2024 

        else if (currentCharControlled.isHowling())
        {
            ShowDamageValueDisplay("-" + (attackUsedAttack.effectValue + (currentCharControlled.nbHowl* currentCharControlled.damageBuffHowl)), Color.red, c.getX(), c.getY());
            int damageBuff = (attackUsedAttack.effectValue + (currentCharControlled.nbHowl * currentCharControlled.damageBuffHowl));
                        c.setHP(c.getHP() - damageBuff);

        }


        else
        {
            ShowDamageValueDisplay("-" + attackUsedAttack.effectValue , Color.red, c.getX(), c.getY());


        }
        
        //if (whoControlsThisChar(c) == PlayerType.AI_HARD) statsGame.addToDamageTaken(c, attackUsedAttack.effectValue);
        //if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD) statsGame.addToDamageDealt(currentCharControlled, attackUsedAttack.effectValue);

        if (currentCharControlled.dmgbuff)
        {
            currentCharControlled.totalDamage += (attackUsedAttack.effectValue + 1);
            c.HP -= (attackUsedAttack.effectValue + 1);
        }
        else if (currentCharControlled.doomed)
        {
            currentCharControlled.totalDamage += (attackUsedAttack.effectValue - doomDamage);
            c.HP -= (attackUsedAttack.effectValue - doomDamage);
            currentCharControlled.setDoomed(false);
        }
        else
        {
            currentCharControlled.totalDamage += attackUsedAttack.effectValue;
            c.setHP(c.getHP()-attackUsedAttack.effectValue);
        }
        // Enemy dies
        bool isDead = IsDead(c);
        if (isDead)
        {
            getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
        }
    }

    // Author : Simon Sepiol L3L1 2024
    private void HandleBurning(Character c)
    {
        c.burning = 2;
        //print("le bruleur est "+ currentCharControlled.getName());
        c.whoBurnedIt=currentCharControlled;
        c.fireDamage=attackUsedAttack.effectValue/3;
        HandleDamage(c);
        GameObject go = GameObject.Instantiate(particleExplosion);
        go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);
        go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        Destroy(go, 5);

        
    }

    // Author: ??
    // Edited by Youcef MEDILEH, L3C1
    //Edited by Julien D'aboville L3L1 2024
    private bool IsDead(Character c)
    {
        if (c.getHP() <= 0)
        {
            if (whoControlsThisChar(c) == PlayerType.AI_HARD)
                statsGame.setDead(c, true);
            if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD)
                statsGame.addToKills(currentCharControlled, 1);
            c.HP = 0;
            hexaGrid.getHexa(c.getX(), c.getY()).charOn = null;
            GameObject.Destroy(c.go);
            for (int i = 0; i < hexaGrid.getCharList().Count; i++)
            {
                if (hexaGrid.getCharList()[i] == c)
                {
                    GameObject.Destroy(UICharTurnsList[i]);
                    UICharTurnsList.RemoveAt(i);
                    hexaGrid.getCharList().RemoveAt(i);
                }
            }
            // update currentCharControlled ID
            for (int i = 0; i < hexaGrid.getCharList().Count; i++)
            {
                if (hexaGrid.getCharList()[i] == currentCharControlled)
                    currentCharControlledID = i;
            }
            // force AI to make a new decision
            decisionSequence = new List<ActionAIPos>();
            // check if there is a winner
            int nbT1 = 0;
            int nbT2 = 0;
            foreach (Character c2 in hexaGrid.getCharList())
            {
                if (c2.getTeam() == 0)
                    nbT1++;
                else
                    nbT2++;
            }
            if (nbT1 == 0)
            {
                winner = 1;
                //getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
            }
            else if (nbT2 == 0)
            {
                winner = 0;
                //getEndGameDataCharacterFromTheList(currentCharControlled).hasKilledSomeOne();
            }

            //Added by Julien D'aboville L3L1 2024
            if (currentCharControlled.getName() == "Druide")
            {
                Color purple = new Color(0.5F, 0.5F, 0.5F, 1);
                ShowDamageValueDisplay("Siphon d'âme", purple, c.getX(), c.getY());
                GameObject go = GameObject.Instantiate(particlePassiveDruide);
                go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0);
            }
            else
            {

// ShowDamageValueDisplay("DEAD", Color.red, c.getX(), c.getY());
                GameObject go = GameObject.Instantiate(particleDead);
                go.transform.position = Hexa.hexaPosToReal(c.getX(), c.getY(), 0.7f);
            }
            

            return (true);
        }
        return (false);
    }

    //Display the walking animation
    //Author : ??
    //Edited by L3C1, Yuting HUANG, le 03/04/2023, MaxTimeNum
    void walkingAnimation()
    {
        //print("tour walking animation : "+tour);
        //print("pathWalkpos : " + pathWalkpos);
        for (int i =0;i< pathWalk.Count;i++)
        {
            //print("paths " + i + " : x" + pathWalk[i].getX()+" y :"+pathWalk[i].getY());
        }
        int speed = 6;
        if (pathWalkpos == 0)
        {
            Animator animator = currentCharControlled
                .go.transform.GetChild(1)
                .GetComponent<Animator>();
            if (animator)
            {
                animator.SetBool("Moving", true);
                animator.SetBool("Running", true);
            }
        }
        if (pathWalkpos < (pathWalk.Count - 1) * speed)
        {
            for (int i = 0; i < 6; i++)
            {
                Point p = HexaGrid.findPos(
                    pathWalk[pathWalkpos / speed].getX(),
                    pathWalk[pathWalkpos / speed].getY(),
                    (HexaDirection)i
                );
                if (
                    p.getX() == pathWalk[pathWalkpos / speed + 1].getX()
                    && p.getY() == pathWalk[pathWalkpos / speed + 1].getY()
                )
                {
                    currentCharControlled.setDirection((HexaDirection)i);
                    i = 6;
                }
            }
            //print("CHANGEMENT DE DIRECTION !!");

            float multiplier = (pathWalkpos % speed) / (float)speed;

            float x1 = pathWalk[pathWalkpos / speed].getX() * 0.75f;
            float x2 = pathWalk[pathWalkpos / speed + 1].getX() * 0.75f;
            float x = x1 * (1.0f - multiplier) + x2 * multiplier;

            float y1 =
                pathWalk[pathWalkpos / speed].getY() * -0.86f
                + (pathWalk[pathWalkpos / speed].getX() % 2) * 0.43f;
            float y2 =
                pathWalk[pathWalkpos / speed + 1].getY() * -0.86f
                + (pathWalk[pathWalkpos / speed + 1].getX() % 2) * 0.43f;
            float y = y1 * (1.0f - multiplier) + y2 * multiplier;
            currentCharControlled.go.transform.position = new Vector3(
                x + Hexa.offsetX,
                0,
                y + Hexa.offsetY
            );
            //new Vector3(x + Hexa.offsetX, 0, y + Hexa.offsetY);
            pathWalkpos++;
        }
        else
        {
            currentCharControlled.updatePos(
                pathWalk[pathWalk.Count - 1].getX(),
                pathWalk[pathWalk.Count - 1].getY(),
                hexaGrid
            );
            Animator animator = currentCharControlled
                .go.transform.GetChild(1)
                .GetComponent<Animator>();
            if (animator)
            {
                animator.SetBool("Moving", false);
                animator.SetBool("Running", false);
            }
            checkAndUpdatePortals(startGameData.mapChosen);
            nextTurn();
        }
    }

    // Author Simon Sepiol L3L1 2024
    void handleBurningCharacter()
    {
        List<Character> charListCopy = new List<Character>(hexaGrid.getCharList());

        foreach (Character c in charListCopy)
        {
            if (c.isBurning() == 1)
            {
                // Create object that shows damage
                //print("burning");
                Debug.Log("burning !!!!!!!!!!!!!!!!!!!!!!");
                GameObject dmgValueDisp = GameObject.Instantiate(damageValueDisplay);
                dmgValueDisp.GetComponent<DamageValueDisplay>().camera_ = cameraPos;
                dmgValueDisp
                    .GetComponent<DamageValueDisplay>()
                    .setValue(c.getX(), c.getY(), "brulé : -"+c.fireDamage.ToString()+" HP", Color.red, 240);

                if (whoControlsThisChar(c) == PlayerType.AI_HARD){
                    statsGame.addToDamageTaken(c, 1);
                }

                //print("ajoute des degats a "+ c.whoBurnedIt.getName());
                c.whoBurnedIt.totalDamage += c.fireDamage;

                c.HP -= c.fireDamage;
                bool isDead = IsDead(c);
                if (isDead)
                {
                    getEndGameDataCharacterFromTheList(c.whoBurnedIt).hasKilledSomeOne();
                }
                c.burning = 0;
                c.fireDamage=0;
            }
            else if(c.isBurning()>0){
                c.burning--;
            }
        }
    }

    void handlePoisonedHexa(){
        List<Character> charListCopy = new List<Character>(hexaGrid.getCharList());

        foreach (Character c in charListCopy)
        {
            // If the character is in a poisonned hexa
            if (poisonnedHexas.Contains(hexaGrid.getHexa(c.getX(), c.getY())))
            {
                // Create object that shows damage
                GameObject dmgValueDisp = GameObject.Instantiate(damageValueDisplay);
                dmgValueDisp.GetComponent<DamageValueDisplay>().camera_ = cameraPos;
                dmgValueDisp
                    .GetComponent<DamageValueDisplay>()
                    .setValue(
                        c.getX(),
                        c.getY(),
                        "-" + hexaGrid.getHexa(c.getX(), c.getY()).poisonnedDamage,
                        Color.red,
                        240
                    );

                if (whoControlsThisChar(c) == PlayerType.AI_HARD){
                    statsGame.addToDamageTaken(c,hexaGrid.getHexa(c.getX(), c.getY()).poisonnedDamage);
                }

                // est ce que ca ajoute les degats au bon character ?
                // non ca n'ajoute pas les degats au bon character
                // changement avec whoPoisonnedIt by Simon
                //print("ajoute des degats a " + hexaGrid.getHexa(c.getX(), c.getY()).whoPoisonedIt.getName());
                hexaGrid.getHexa(c.getX(), c.getY()).whoPoisonedIt.totalDamage += hexaGrid
                    .getHexa(c.getX(), c.getY())
                    .poisonnedDamage;
                c.HP -= hexaGrid.getHexa(c.getX(), c.getY()).poisonnedDamage;
                bool isDead = IsDead(c);
                if (isDead)
                {
                    getEndGameDataCharacterFromTheList(hexaGrid.getHexa(c.getX(), c.getY()).whoPoisonedIt)
                        .hasKilledSomeOne();
                }
            }
        }
    }

    void handleDisabledCharacter(){
        // valeur 0 on fait rien, valeur 1 on remet les skill, valeur autre on attend
            Character c = currentCharControlled;
            //print("current char : "+currentCharControlled.getName());
            //print("c : "+c.getName());
            //print("skill disabled : "+c.disabledSkill);
            if(c.disabledSkill!=0 && c.disabledSkill!=1){
                //print(" les skill sont disabled !!!!!!!!!!!!!!!!!!!! "+c.disabledSkill);

                GameObject dmgValueDisp = GameObject.Instantiate(damageValueDisplay);
                dmgValueDisp.GetComponent<DamageValueDisplay>().camera_ = cameraPos;
                dmgValueDisp
                    .GetComponent<DamageValueDisplay>()
                    .setValue(c.getX(), c.getY(), "skill disabled for "+(c.disabledSkill-1)+" tour ", Color.red, 240);

                c.skillAvailable=false;
                c.skillAvailable2=false;
                c.skillAvailable3=false;

                c.disabledSkill-=1;
            }
            else if(c.disabledSkill == 1 ){
                c.skillAvailable=c.wasSkill1Available;
                c.skillAvailable2=c.wasSkill2Available;
                c.skillAvailable3=c.wasSkill3Available;
                c.disabledSkill=0;
            }
            
        

    }

    void checkAndSetBackSkill(){
        if(currentCharControlled.attack1coolDownValue<=1){
            currentCharControlled.skillAvailable=true;
        }
        else{
            currentCharControlled.attack1coolDownValue--;
        }

        if(currentCharControlled.attack2coolDownValue<=1){
            currentCharControlled.skillAvailable2=true;
        }
        else{
            currentCharControlled.attack2coolDownValue--;
        }

        if(currentCharControlled.attack3coolDownValue<=1){
            currentCharControlled.skillAvailable3=true;
        }
        else{
            currentCharControlled.attack3coolDownValue--;
        }
    }

    //Initialize both portals
    //Author : MIEL Timothé - L3L1
    public void InitializePortals()
    {
        portal1 = new Hexa();
        portal2 = new Hexa();
        int numberOfPortal = 0;
        foreach (Hexa hexa in hexaGrid.hexaList)
        {
            if (hexa.type == HexaType.PORTAL)
            {
                switch (numberOfPortal)
                {
                    case 0:
                        portal1 = hexa;
                        numberOfPortal++;
                        break;
                    case 1:
                        portal2 = hexa;
                        numberOfPortal++;
                        break;
                    default:
                        numberOfPortal++;
                        Debug.Log(numberOfPortal + " portails détectés !");
                        break;
                }
            }
        }
        portal1.setOtherPortal(portal2);
        portal2.setOtherPortal(portal1);
        portalOpened = true;
    }

    public void checkAndUpdatePortals(int mapChosen)
    {
        if (mapChosen == 1) //L'update ne se fait que dans l'arène (seule map avec des portails)
        {
            bool isCharOnPortal1 = false;
            bool isCharOnPortal2 = false;

            if (tour - tourActivationPortail >= 3)
            {
                portalOpened = true;
                Debug.Log("Activation portail");
                UpdatePortalColor();
            }

            if (portalOpened)
            {
                foreach (Character c in hexaGrid.getCharList())
                {
                    if (hexaGrid.getHexa(c.getX(), c.getY()) == portal1)
                    {
                        Debug.Log("Perso sur portail détecté");
                        isCharOnPortal1 = true;
                    }
                    if (hexaGrid.getHexa(c.getX(), c.getY()) == portal2)
                    {
                        isCharOnPortal2 = true;
                        Debug.Log("Perso sur portail détecté");
                    }
                }
                if (isCharOnPortal1 ^ isCharOnPortal2)
                {
                    activatePortal();
                    portalOpened = false;
                    UpdatePortalColor();
                }
                tourActivationPortail = tour;
            }
        }
    }

    public void activatePortal()
    {
        Debug.Log("Appel activatePortal()");
        foreach (Character c in hexaGrid.getCharList())
        {
            if (hexaGrid.getHexa(c.getX(), c.getY()) == portal1)
            {
                c.go.transform.position = portal2.go.transform.position;
                c.updatePos(portal2.getX(), portal2.getY(), hexaGrid);
            }
            else if (hexaGrid.getHexa(c.getX(), c.getY()) == portal2)
            {
                c.go.transform.position = portal1.go.transform.position;
                c.updatePos(portal1.getX(), portal1.getY(), hexaGrid);
            }

        }
    }

    public void UpdatePortalColor()
    {
        if (portalOpened)
        {
            Debug.Log("Couleur ouverte");
            portal1.go.GetComponent<Renderer>().material.color = openedPortal;
            portal2.go.GetComponent<Renderer>().material.color = openedPortal;
        }
        else
        {
            Debug.Log("Couleur fermée");
            portal1.go.GetComponent<Renderer>().material.color = closedPortal;
            portal2.go.GetComponent<Renderer>().material.color = closedPortal;
        }
    }


    public void HandleBushes()
    {
        foreach (Character c in hexaGrid.getCharList())
        {
            if (c.GetHexa(hexaGrid).type == HexaType.BUSH)
            {
                c.go.SetActive(false);
                c.estDansUnBuisson = true;
            }
            else
            {
                if (c.isInvisible()) // On ne change pas l'invisibilité d'un personnage déjà invisible
                {

                }
                else
                {
                    c.go.SetActive(true);
                    c.estDansUnBuisson = false;
                }
            }
        }
    }

    //Proceed to the next turn
    //Author : ?
    // Edited and refactored by VALAT Thibault, L3Q1
    // Edited by Youcef MEDILEH, L3C1
    // Edited by Julien D'aboville, L3C1 2024

    // - gestion du poison
    // - debut reglage du bug de l'initiative
    void nextTurn()
    {
        //Added by Julien D'aboville, L3C1 2024

        






        checkAndUpdateBonusControll();
        displayInitiative();
        //print("currentCharControlled.PA avant:" + currentCharControlled.PA);

        currentCharControlled.PA--;

        //print("currentCharControlled.PA apres:"+ currentCharControlled.PA);


        //Added by Julien D'aboville, L3C1 2024
        //print("Passive");

        //il est activé à tous les points d'actions
        passiveSoigneur();
        
        //  passiveForgeron();




        // edited by Simon Sepiol L3L1 2024
        // si c'est 0 on remet l'attaque sinon on descend le coolDown et quand on utilise une attaque on met la valeur 
        // a celle donné dans le constructeur de l'attaque


        //When the controlled character has no more PA, his turn is over
        if (currentCharControlled.PA <= 0)
        {
            
            // gere les personnages qui sont en train de bruler
            // au debut pour prendre les degats de brulure au prochain tour
            

            Debug.Log("Le tour de " + currentCharControlled.getName() + " est fini");
            //print("NEXT TURN");
            //print("tour" + tour);
            tour += 1;
            if (turnCounterDamageOverTimeTEAM1 > 0 || turnCounterDamageOverTimeTEAM2 > 0)
            {
                turnCounterDamageOverTimeTEAM1--;
                turnCounterDamageOverTimeTEAM2--;
                // Check each character is they are in poisonned hexa
                handlePoisonedHexa();

            }
            else
            {
                // all poisonned hexa are removed
                poisonnedHexas.Clear();
            }

            //Reset priority
            do
            {
                hexaGrid.getCharList().Sort();
                foreach (Character c in hexaGrid.getCharList())
                {
                    if (c.priority >= 5)
                        c.priority = c.priority - UnityEngine.Random.Range(1, 3);
                    else
                        c.priority--;
                }

                currentCharControlled.resetPriority();
                hexaGrid.getCharList().Sort();
                currentCharControlled = hexaGrid.getCharList()[0];
                Debug.Log("Le prochain est " + currentCharControlled.getName());
                passiveArcher();
                passiveEnvouteur();
                EnableSkill1IllimitedLifeWeaver();

                //   currentCharControlled.PA = CharsDB
                //        .list[(int)currentCharControlled.charClass]
                //       .basePA;
                //Added by Julien d'aboville L3L1 2024
                currentCharControlled.PA = currentCharControlled.getCharacterPA();


            } while (currentCharControlled.HP <= 0);

            PlayerType currentPlayerType =
                (currentCharControlled.getTeam() == 0)
                    ? startGameData.player1Type
                    : startGameData.player2Type;




            //Stats to remove
            if (
                currentPlayerType == PlayerType.AI_HARD
                || currentPlayerType == PlayerType.AI_MEDIUM
            )
            {
                statsGame.nextTurn(currentCharControlled);
            }
            actionType = ActionType.MOVE;
            newTurn = 0;
            decisionSequence = new List<ActionAIPos>();
            if (!lockedCamera)
                {
                cameraController.GoToCharacter(currentCharControlled, false);
}
        }


        //Added by Julien D'aboville, L3C1 2024

        






        updateUI = true;
        updateMouseHover = true;
        pathWalk = null;
        displayInitiative();







        

        // gere la recuperation des competences
        checkAndSetBackSkill();
        // gere les personnages avec leurs capacitées bloquées
        // a la fin pour qu'on ai changer de currentcharacter
        handleDisabledCharacter();
        handleBurningCharacter();


       // 
      //  passiveArcher();
    }

    //Load a game
    //Author : ?
    //Edited by L3Q1, VALAT Thibault
    //Edited by L3C1, Yuting HUANG, ajoute un map non aleatoire

    // Edited by Simon L3L1 2024 : correction bug
    //Edited by Mariana Duarte L3Q1 04/2025

    // il doit manquer endGameData
    public void loadGame(string savePath)
    {
        print("ca load");
        print(savePath);
        hexaGrid = new HexaGrid();
        int nbChar = 0;
        string conn = "URI=file:" + Application.persistentDataPath + savePath; //Path to database.
        IDbConnection dbconn = (IDbConnection)new SqliteConnection(conn);
        dbconn.Open(); //Open connection to the database
        IDbCommand dbcmd = dbconn.CreateCommand();
        IDataReader reader;

        // added simon sepiol l3L1 2024
        dbcmd.CommandText = "SELECT mapNumber, mapName FROM game";
        reader = dbcmd.ExecuteReader();
        reader.Read();
        int map_type = reader.GetInt32(0);
        string map_name = reader.GetString(1);//Added by Mariana Duarte L3Q1 04/2025
        reader.Close(); //close the reader to execute a new command
        reader.Dispose();
        reader = null;

        if(map_type<4){
            switch (map_type)
            {
                case 0:
                    {
                        Debug.Log("Creating map from ruins file");
                        hexaGrid.createGridFromFile(Application.streamingAssetsPath + "/Data/Map/ruins");
                        tileW = hexaGrid.w;
                        tileH = hexaGrid.h;
                        ruinsMap.SetActive(true);
                        foreach (Hexa hexa in hexaGrid.hexaList)
                        {
                            if (hexa.type == HexaType.VOID || hexa.type == HexaType.WALL)
                            {
                                hexa.go.GetComponent<Renderer>().enabled = false;
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        Debug.Log("Creating map from arene file");
                        hexaGrid.createGridFromFile(Application.streamingAssetsPath + "/Data/Map/arene");
                        tileW = hexaGrid.w;
                        tileH = hexaGrid.h;
                        areneMap.SetActive(true);
                        foreach (Hexa hexa in hexaGrid.hexaList)
                        {
                            if (hexa.type == HexaType.VOID || hexa.type == HexaType.WALL)
                            {
                                hexa.go.GetComponent<Renderer>().enabled = false;
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        Debug.Log("Creating map from foret file");
                        hexaGrid.createGridFromFile(Application.streamingAssetsPath + "/Data/Map/foret");
                        tileW = hexaGrid.w;
                        tileH = hexaGrid.h;
                        foretMap.SetActive(true);
                        foreach (Hexa hexa in hexaGrid.hexaList)
                        {
                            if (hexa.type == HexaType.VOID || hexa.type == HexaType.WALL)
                            {
                                hexa.go.GetComponent<Renderer>().enabled = false;
                            }
                        }
                        break;
                    }
                case 3:
                    {
                        Debug.Log("Creating empty map");
                        hexaGrid.createRectGrid(34, 30);
                        tileW = hexaGrid.w;
                        tileH = hexaGrid.h;
                        break;
                    }
            }
         
        }
        //Added by Mariana Duarte L3Q1 04/2025
        else{
            Debug.Log("Instatiation de la carte sauvegardee");

            string pathToMapObjects = Application.persistentDataPath+"/Save/MapForGame/"+map_name+".db";
            string pathToMapGrid = Application.persistentDataPath+"/Save/MapForGame/"+map_name+"Grid.db";

            Map map = SaveManager.createMapFromFile(pathToMapObjects, pathToMapGrid);
            hexaGrid.createGridFromFile(pathToMapGrid);
            tileW = hexaGrid.w;
            tileH = hexaGrid.h;

            foreach (MapObject obj in map.objects){
                Vector3 objectPosition = new Vector3(obj.x,obj.y,obj.z);
                GameObject objectPrefab = Instantiate(obj.gameObject,objectPosition,obj.getRotation());
                objectPrefab.transform.SetParent(mapObjects.transform);

                MeshRenderer meshRenderer = objectPrefab.GetComponent<MeshRenderer>();
                if (meshRenderer != null){
                    meshRenderer.enabled = true;
                }
            }
        }

        if(map_type<4){
            //Load the bonus zone
            dbcmd.CommandText = "SELECT bonusCenterX, bonusCenterY, bonusColor FROM board";
            reader = dbcmd.ExecuteReader();
            reader.Read();

            caseBonus = hexaGrid.getHexa(reader.GetInt32(0), (int)reader.GetInt32(1));
            bonusTeam = (int)reader.GetInt32(2);
            displayBonus(hexaGrid.findAllPaths(caseBonus.getx(), caseBonus.gety(), 2));
            reader.Close(); //close the reader to execute a new command
            reader.Dispose();
            reader = null;
        }
        

        dbcmd.CommandText = "SELECT player1, player2, mapNumber, mapName, w, h, current, NBchar FROM game";
        reader = dbcmd.ExecuteReader();
        reader.Read();
        startGameData.player1Type = (PlayerType)reader.GetInt32(0);
        startGameData.player2Type = (PlayerType)reader.GetInt32(1);
        startGameData.mapChosen = (int)reader.GetInt32(2);
        startGameData.mapName = reader.GetString(3); 
        hexaGrid.w = (int)reader.GetInt32(4);
        hexaGrid.h = (int)reader.GetInt32(5);
        currentCharControlledID = reader.GetInt32(6);
        nbChar = reader.GetInt32(7);

        reader.Close(); //close the reader to execute a new command
        reader.Dispose();
        reader = null;

        dbcmd.CommandText =
            "SELECT class, team, x, y, pa, hp, skillA, directionF, priority FROM characters";
        reader = dbcmd.ExecuteReader();
        for (int i = 0; i < nbChar && reader.Read(); i++)
        {
            CharClass cCharClass = (CharClass)reader.GetInt32(0);
            int cTeam = (int)reader.GetInt32(1);
            int cX = reader.GetInt32(2);
            int cY = reader.GetInt32(3);
            Character c = new Character(
                cCharClass,
                cX,
                cY,
                cTeam
            );
            c.PA = (int)reader.GetInt32(4);
            c.HP = (int)reader.GetInt32(5);
            c.skillAvailable = (reader.GetInt32(6) == 0) ? false : true;
            c.directionFacing = (HexaDirection)reader.GetInt32(7);
            c.priority = (int)reader.GetInt32(8);
            hexaGrid.addChar(c);
        }
        reader.Close(); //close the reader to execute a new command
        reader.Dispose();
        reader = null;

        //added by simon sepiol L3L1
        endGameDataCharacter = createEndDataGameList(nbChar);

        currentCharControlled = hexaGrid.getCharList()[currentCharControlledID];
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Dispose();
        dbconn.Close();
        dbconn = null;

        hexaGrid.getCharList().Sort();
        for (int i = hexaGrid.getCharList().Count; i <= 10; i++)
            Initiative.transform.GetChild(i).transform.position = new Vector3(10000, 10000, 0);

        displayInitiative();

        // on supprime la save si c'etait une save temporaire pour afficher le guide
        if(savePath.Equals("/Save/temp pour guide.db")){
            print("save tmporaire aaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            File.Delete(Application.persistentDataPath + savePath);
            // on met actualLoad a 0 pour pouvoir nonmmer si on veut faire une sauvegarde
            actualLoad="0";
        }
    }

    //Save a game
    //Author : ?
    //Edited by L3Q1, VALAT Thibault

    // edited by Simon L3L1
    // Edited by Mariana Duarte L3Q1 04/2025
    public void writeSaveGame(string savePath)
    {
        
        //print("ca write "+Application.streamingAssetsPath+ savePath);
        if (File.Exists(Application.persistentDataPath + savePath)) {
        // Si le fichier existe, le supprimer
            File.Delete(Application.persistentDataPath + savePath);
            //print("fichier existe");
        }

        //Added by Mariana Duarte L3Q1 04/2025
        string newMapName = "";
        if(startGameData.map != null){
            newMapName = saveNameInput.text + "Map";
            startGameData.map.name = newMapName;
            SaveManager.saveMapObjects(startGameData.map, "/Save/MapForGame");
            SaveManager.saveMapGrid(startGameData.map, "/Save/MapForGame");
        }
        
        Mono.Data.Sqlite.SqliteConnection.CreateFile(Application.persistentDataPath + savePath);

        string conna = "URI=file:" + Application.persistentDataPath + savePath; //Path to database.
        IDbConnection dbconna = (IDbConnection)new SqliteConnection(conna);
        IDbCommand dbcmda = dbconna.CreateCommand();
        dbconna.Open(); //Open connection to the database.
        dbcmda.CommandText = "BEGIN TRANSACTION";
        dbcmda.ExecuteNonQuery();
        dbcmda.CommandText =
            "CREATE TABLE 'game' ('player1'INTEGER,'player2'INTEGER,'mapNumber'INTEGER,'mapName' TEXT,'w'INTEGER,'h'INTEGER,'current'INTEGER,'NBchar'INTEGER)";
        dbcmda.ExecuteNonQuery();
        dbcmda.CommandText =
            "CREATE TABLE 'characters' ('class'	INTEGER,'team'	INTEGER,'x'	INTEGER,'y'	INTEGER,'pa'	INTEGER,'hp'	INTEGER,'skillA'INTEGER,'directionF'	INTEGER, 'totalDamage'	INTEGER, 'priority'	INTEGER)";
        dbcmda.ExecuteNonQuery();
        dbcmda.CommandText =
            "CREATE TABLE 'board' ('IDhexa'	INTEGER UNIQUE,'type'  INTEGER,'bonusCenterX'  INTEGER,'bonusCenterY'  INTEGER,'bonusColor' INTEGER,PRIMARY KEY('IDhexa'))";
        dbcmda.ExecuteNonQuery();
        dbcmda.CommandText = "end";
        dbcmda.ExecuteNonQuery();
        dbcmda.Dispose();
        dbconna.Dispose();
        dbconna.Close();
        
        string conn = "URI=file:" + Application.persistentDataPath + savePath; //Path to database.

        IDbConnection dbconn = (IDbConnection)new SqliteConnection(conn);

        IDbCommand dbcmd = dbconn.CreateCommand();
        dbconn.Open(); //Open connection to the database.

        dbcmd.CommandText = "begin";
        dbcmd.ExecuteNonQuery();
        dbcmd.CommandText = "DELETE FROM game";
        dbcmd.ExecuteNonQuery();
        dbcmd.CommandText = "DELETE FROM board";
        dbcmd.ExecuteNonQuery();
        dbcmd.CommandText = "DELETE FROM characters";
        dbcmd.ExecuteNonQuery();
        dbcmd.CommandText = "DELETE FROM game";
        dbcmd.ExecuteNonQuery();
        dbcmd.CommandText = "DELETE FROM board";
        dbcmd.ExecuteNonQuery();
        dbcmd.CommandText = "INSERT INTO game (player1, player2, mapNumber, mapName, w, h, current, NBchar) VALUES (@player1, @player2, @mapNumber, @mapName, @w, @h, @current, @NBchar)";

        dbcmd.Parameters.Add(new SqliteParameter("@player1", (byte)startGameData.player1Type));
        dbcmd.Parameters.Add(new SqliteParameter("@player2", (byte)startGameData.player2Type));
        dbcmd.Parameters.Add(new SqliteParameter("@mapNumber", (byte)startGameData.mapChosen));
        dbcmd.Parameters.Add(new SqliteParameter("@mapName", newMapName)); 
        dbcmd.Parameters.Add(new SqliteParameter("@w", hexaGrid.w));
        dbcmd.Parameters.Add(new SqliteParameter("@h", hexaGrid.h));
        dbcmd.Parameters.Add(new SqliteParameter("@current", currentCharControlledID));
        dbcmd.Parameters.Add(new SqliteParameter("@NBchar", hexaGrid.getCharList().Count));
        dbcmd.ExecuteNonQuery();

        foreach (Character c in hexaGrid.getCharList())
        {
            dbcmd.CommandText =
                "INSERT INTO characters (class, team, x, y, pa, hp, skillA, directionF, totalDamage, priority) VALUES ("
                + (byte)c.charClass
                + ", "
                + (byte)c.getTeam()
                + ", "
                + c.getX()
                + ", "
                + c.getY()
                + ", "
                + (byte)c.PA
                + ", "
                + (byte)c.HP
                + ", "
                + (byte)(c.skillAvailable ? 1 : 0)
                + ", "
                + (byte)c.directionFacing
                + ", "
                + (byte)c.totalDamage
                + ", "
                + (byte)c.priority
                + ")";
            dbcmd.ExecuteNonQuery();
        }

        
        if (!(caseBonus == null)){
            int caseBonusX=caseBonus.getx();

            dbcmd.CommandText =
            "INSERT INTO board (bonusCenterX, bonusCenterY, bonusColor) VALUES ("
            + caseBonusX
            + ", "
            + (byte)caseBonus.gety()
            + ", "
            + (byte)bonusTeam
            + ")";
        dbcmd.ExecuteNonQuery();
        }
        //print("case bonus x = "+caseBonusX);

        
        




        dbcmd.CommandText = "end";
        dbcmd.ExecuteNonQuery();
        dbcmd.Dispose();
        dbconn.Dispose();
        dbconn.Close();
        dbconn = null;

    }

    public void debugVerifBonus(string savePath){
        string conn = "URI=file:" + Application.persistentDataPath + savePath; //Path to database.
        IDbConnection dbconn = (IDbConnection)new SqliteConnection(conn);
        dbconn.Open(); //Open connection to the database
        IDbCommand dbcmd = dbconn.CreateCommand();
        IDataReader reader;

        // added simon sepiol l3L1 2024
        dbcmd.CommandText = "SELECT bonusCenterX, bonusCenterY, bonusColor FROM board";
        reader = dbcmd.ExecuteReader();
        reader.Read();
        int map_type = reader.GetInt32(0);
        //print(map_type);

        reader.Close();   
        reader.Dispose(); 
        reader = null;    

        dbcmd.Dispose();  
        dbconn.Close();   
        dbconn.Dispose(); 
        dbconn = null;
    }
    
    public void generateSaveGamePanelOrSave()
    {   
        print("genere le paneau save");
        
        
        // si on est pas deja dans une sauvegarde
        if(actualLoad=="0"){

            
            saveMenu=true;
            panelSave.SetActive(true);

            
                    
        }
        // si on est dans une sauvegarde
        else{
            //print("on est dans save au moment de l'ecriture");
            writeSaveGame("/Save/"+actualLoad+".db");
        }
    }

    // Added by Timothé MIEL - L3L1
    public bool IsOnlyCPUGame()
    {
        if (startGameData == null)
        {
            return true;
        }
        else
        {
            return (startGameData.player1Type != PlayerType.HUMAN && startGameData.player2Type != PlayerType.HUMAN);
        }
    }

    // ##################################################################################################################################################
    // Display Functions used in main
    // ##################################################################################################################################################


    //Display the right character in the top left character card
    //Author : ?
    void displayNewCharTurnList()
    {
        //GameObject go = UICharTurnsList[0];
        UIEnemyChar.SetActive(false);

        UICurrentChar.transform.GetChild(3).GetComponent<Text>().text =
            currentCharControlled.PA + "";
        UICurrentChar.transform.GetChild(4).GetChild(3).GetComponent<Text>().text =
            currentCharControlled.HP + "/" + currentCharControlled.HPmax;
        UICurrentChar.transform.GetChild(4).GetComponent<Slider>().maxValue = currentCharControlled.HPmax;
        UICurrentChar.transform.GetChild(4).GetComponent<Slider>().value = currentCharControlled.HP;
        UICurrentChar.transform.GetChild(1).GetComponent<RawImage>().texture = charCards[
            (int)currentCharControlled.charClass
        ];
        UICurrentChar.transform.GetChild(0).GetComponent<Image>().color =
            (currentCharControlled.getTeam() == 0) ? Character.TEAM_1_COLOR : Character.TEAM_2_COLOR;
        if (charHovered != null && charHovered.getTeam() == currentCharControlled.getTeam())
        {
            UICurrentChar.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1);
            UICurrentChar.transform.GetChild(3).GetComponent<Text>().text = charHovered.PA + "";
            UICurrentChar.transform.GetChild(4).GetChild(3).GetComponent<Text>().text =
                charHovered.HP + "/" + charHovered.HPmax;
            UICurrentChar.transform.GetChild(4).GetComponent<Slider>().maxValue = charHovered.HPmax;
            UICurrentChar.transform.GetChild(4).GetComponent<Slider>().value = charHovered.HP;
            UICurrentChar.transform.GetChild(1).GetComponent<RawImage>().texture = charCards[
                (int)charHovered.charClass
            ];
        }
        else if (charHovered != null && charHovered.getTeam() != currentCharControlled.getTeam() && charHovered.estInvisible == false && charHovered.estDansUnBuisson == false)
        //On n'active pas la carte adverse si invisibilité
        {
            UIEnemyChar.SetActive(true);
            UIEnemyChar.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1);
            UIEnemyChar.transform.GetChild(3).GetComponent<Text>().text = charHovered.PA + "";
            UIEnemyChar.transform.GetChild(4).GetChild(3).GetComponent<Text>().text =
                charHovered.HP + "/" + charHovered.HPmax;
            UIEnemyChar.transform.GetChild(4).GetComponent<Slider>().maxValue = charHovered.HPmax;
            UIEnemyChar.transform.GetChild(4).GetComponent<Slider>().value = charHovered.HP;
            UIEnemyChar.transform.GetChild(1).GetComponent<RawImage>().texture = charCards[
                (int)charHovered.charClass
            ];
        }
    }

    //Display Initiative at the bottom on the screen
    //Author : VALAT Thibault, L3Q1
    void displayInitiative()
    {
        Initiative.transform.GetChild(hexaGrid.getCharList().Count).transform.position = new Vector3(
            10000,
            10000,
            0
        );
        //Display every Initiative case
        for (int i = 0; i < hexaGrid.getCharList().Count; i++)
            if (hexaGrid.getCharList()[i].HP > 0)
                displayOneInitiativeCase(i, hexaGrid.getCharList()[i]);
    }

    //Display on Initiative case
    //Author : VALAT Thibault, L3Q1
    // Edited by : Youcef MEDILEH, L3C1
    // - Ajout de la gestion de la couleur des cases d'initiative en fonction de l'état du personnage s'il est gelé ou non (freezed)
    void displayOneInitiativeCase(int i, Character c)
    {
        if (c.isFreezed())
        {
            Initiative.transform.GetChild(i).transform.GetChild(0).GetComponent<Image>().color =
                new Color(0.5f, 0.5f, 0.5f);
            Initiative.transform.GetChild(i).transform.GetChild(1).GetComponent<RawImage>().color =
                new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            Initiative.transform.GetChild(i).transform.GetChild(0).GetComponent<Image>().color =
                new Color(1f, 1f, 1f);
            Initiative.transform.GetChild(i).transform.GetChild(1).GetComponent<RawImage>().color =
                new Color(1f, 1f, 1f);
            Initiative
                .transform.GetChild(i)
                .transform.GetChild(1)
                .GetComponent<RawImage>()
                .texture = charSquares[(int)c.charClass];
            Initiative.transform.GetChild(i).transform.GetChild(0).GetComponent<Image>().color =
                (c.getTeam() == 0) ? Character.TEAM_1_COLOR : Character.TEAM_2_COLOR;
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

    //Display the action buttons on the bottom left
    //Edited by Socrate Louis Deriza L3C1
   


    void displayActionButtons()
    {
        int iChildrenToSelect;
        if ((actionType == ActionType.ATK1))
            iChildrenToSelect = 0;
        else if ((actionType == ActionType.ATK2))
            iChildrenToSelect = 1;
        else if ((actionType == ActionType.ATK3))
            iChildrenToSelect = 2;
        else if ((actionType == ActionType.ATK4))
            iChildrenToSelect = 3;
        else if ((actionType == ActionType.SKIP))
            iChildrenToSelect = 4;
        else if (actionType == ActionType.MOVE)
            iChildrenToSelect = 5;
        else
            iChildrenToSelect = 4;
        for (int i = 0; i < 6; i++)
        {
            if ((i == iChildrenToSelect))
                UIAction
                    .transform.GetChild(i)
                    .GetChild(0)
                    .gameObject.GetComponent<Text>()
                    .fontStyle = FontStyle.Bold;
            else
                UIAction
                    .transform.GetChild(i)
                    .GetChild(0)
                    .gameObject.GetComponent<Text>()
                    .fontStyle = FontStyle.Normal;
        }

        // edited by Simon Sepiol L3L1 2024
        UIAction.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = currentCharControlled.getAttackName() + " (X)";
        if (!currentCharControlled.skillAvailable)
        {
            if(currentCharControlled.disabledSkill>0){
                UIAction.transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = "Skill disabled";

            }
            else{
                UIAction.transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text =
                currentCharControlled.getSkill1CoolDown()-currentCharControlled.attack1coolDownValue + " / " +currentCharControlled.getSkill1CoolDown();
                //currentCharControlled.totalDamage + " / 10";
            }
            
        }
        else
        {
            UIAction.transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text =
                currentCharControlled.getSkill1Name()+" (C)";
        }

        if (!currentCharControlled.skillAvailable2)
        {
            if(currentCharControlled.disabledSkill>0){
                UIAction.transform.GetChild(2).transform.GetChild(0).GetComponent<Text>().text = "Skill disabled";

            }
            else{
            UIAction.transform.GetChild(2).transform.GetChild(0).GetComponent<Text>().text =
                currentCharControlled.getSkill2CoolDown()-currentCharControlled.attack2coolDownValue + " / " +currentCharControlled.getSkill2CoolDown();
            }
        }
        else
        {
            if(currentCharControlled.disabledSkill>0){
                UIAction.transform.GetChild(2).transform.GetChild(0).GetComponent<Text>().text = "Skill disabled";

            }
            else{
            UIAction.transform.GetChild(2).transform.GetChild(0).GetComponent<Text>().text =
                currentCharControlled.getSkill2Name()+" (V)";
            }
        }
        //Edited by Julien D'aboville L3L1 2024


        if (!currentCharControlled.skillAvailable3)
        {
            if(currentCharControlled.disabledSkill>0){
                UIAction.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text = "Skill disabled";

            }
            else{
            UIAction.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text =
                currentCharControlled.getSkill3CoolDown()-currentCharControlled.attack3coolDownValue + " / " +currentCharControlled.getSkill3CoolDown();
            }
        }
        else
        {
            UIAction.transform.GetChild(3).transform.GetChild(0).GetComponent<Text>().text =
                currentCharControlled.getSkill3Name()+" (F)";
        }

        //Author : L3C1 CROUZET Oriane, 06/05/2023
        // Modified by Simon Sepiol L3L1 2024
        //Tooltip
        Transform child_attaque_basique = UIAction.transform.GetChild(0);
        Transform child_competence1 = UIAction.transform.GetChild(1);
        Transform child_competence2 = UIAction.transform.GetChild(2);
        Transform child_competence3 = UIAction.transform.GetChild(3);
        Transform child_skip = UIAction.transform.GetChild(4);
        Transform child_mouvement = UIAction.transform.GetChild(5);

        if (IsPointerOver(child_attaque_basique.GetComponent<RectTransform>()))
        {
            UITooltip.SetActive(true);
            tooltipText.text =
                "Valeur de l'effet : "
                + CharsDB
                    .list[(int)currentCharControlled.charClass]
                    .basicAttack.effectValue.ToString()
                + "\n Portée : "
                + CharsDB.list[(int)currentCharControlled.charClass].basicAttack.range.ToString();
        }
        else if (IsPointerOver(child_competence1.GetComponent<RectTransform>()))
        {
            UITooltip.SetActive(true);
            tooltipText.text =
                "Valeur de l'effet : "
                + CharsDB.list[(int)currentCharControlled.charClass].skill_1.effectValue.ToString()
                + "\n Portée : "
                + CharsDB.list[(int)currentCharControlled.charClass].skill_1.range.ToString();
        }
        else if (IsPointerOver(child_competence2.GetComponent<RectTransform>()))
        {
            UITooltip.SetActive(true);
            tooltipText.text =
                "Valeur de l'effet : "
                + CharsDB.list[(int)currentCharControlled.charClass].skill_2.effectValue.ToString()
                + "\n Portée : "
                + CharsDB.list[(int)currentCharControlled.charClass].skill_2.range.ToString();
        }
        else if (IsPointerOver(child_competence3.GetComponent<RectTransform>()))
        {
            UITooltip.SetActive(true);
            tooltipText.text =
                "Valeur de l'effet : "
                + CharsDB.list[(int)currentCharControlled.charClass].skill_3.effectValue.ToString()
                + "\n Portée : "
                + CharsDB.list[(int)currentCharControlled.charClass].skill_3.range.ToString();
        }
        else if (IsPointerOver(child_mouvement.GetComponent<RectTransform>()))
        {
            UITooltip.SetActive(true);
            tooltipText.text =
                "deplacement de "
                + CharsDB.list[(int)currentCharControlled.charClass].basePM.ToString()
                + " cases.";
        }
        else if (IsPointerOver(child_skip.GetComponent<RectTransform>()))
        {
            UITooltip.SetActive(true);
            tooltipText.text = "passe le tour ";
        }
        else
        {
            UITooltip.SetActive(false);
        }

        //if (mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 835 && mousePos.y < Screen.height - 835 + 24 * 1.8) //Mouse over move button
        //{
        //    UITooltip.SetActive(true);
        //    tooltipText.text = "Déplacement de " + CharsDB.list[(int)currentCharControlled.charClass].basePM.ToString() + " cases.";
        //}
        //else if (mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 890 && mousePos.y < Screen.height - 890 + 24 * 1.8) //Mouse over attack button
        //{
        //    UITooltip.SetActive(true);
        //    tooltipText.text = "Valeur de l'effet : " + CharsDB.list[(int)currentCharControlled.charClass].basicAttack.effectValue.ToString() + "\n Portée : " + CharsDB.list[(int)currentCharControlled.charClass].basicAttack.range.ToString();
        //}
        //else if (mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 945 && mousePos.y < Screen.height - 945 + 24 * 1.8) //Mouse over special attack 1
        //{
        //    UITooltip.SetActive(true);
        //    tooltipText.text = "Valeur de l'effet : " + CharsDB.list[(int)currentCharControlled.charClass].skill_1.effectValue.ToString() + "\n Portée : " + CharsDB.list[(int)currentCharControlled.charClass].skill_1.range.ToString();
        //}
        //else if (mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 1000 && mousePos.y < Screen.height - 1000 + 24 * 1.8) //Mouse special attack 2
        //{
        //    UITooltip.SetActive(true);
        //    tooltipText.text = "Valeur de l'effet : " + CharsDB.list[(int)currentCharControlled.charClass].skill_2.effectValue.ToString() + "\n Portée : " + CharsDB.list[(int)currentCharControlled.charClass].skill_2.range.ToString();
        //}
        //
        //else if (mousePos.x >= 34 && mousePos.x < 34 + 140 * 1.5 && mousePos.y >= Screen.height - 1055 && mousePos.y < Screen.height - 1055 + 24 * 1.8) //Mouse special attack 3
        //{
        //    UITooltip.SetActive(true);
        //    tooltipText.text = "Valeur de l'effet : " + CharsDB.list[(int)currentCharControlled.charClass].skill_3.effectValue.ToString() + "\n Portée : " + CharsDB.list[(int)currentCharControlled.charClass].skill_3.range.ToString();
        //}
        //else
        //{
        //    UITooltip.SetActive(false);
        //}
    }

    //Display the current hovered hexa
    //Author : ?
    // Edited by Timothé MIEL - L3L1
    void displayHoveredHexa()
    {
        if (hexaHovered != null)
            hexaHovered.hoveredColor();
        if (hexaHoveredOld != null)
            if (hexaHoveredOld.type == HexaType.PORTAL) //On ne revient pas systématiquement à la couleur par défaut pour un portail
            {
                UpdatePortalColor();
            }
            else
            {
                hexaHoveredOld.defaultColor();
            }
    }

    //Display the possibles hexas to go to
    //Author : ?
    void displayPossiblePaths()
    {
        List<Point> path = hexaGrid.findAllPaths(
            currentCharControlled.getX(),
            currentCharControlled.getY(),
            currentCharControlled.PM
        );
        foreach (Point p in path)
        {
            GameObject go = GameObject.Instantiate(hexaTemplate, hexasFolder.transform);
            go.SetActive(true);
            go.GetComponent<Transform>().position = Hexa.hexaPosToReal(p.getX(), p.getY(), -0.015f);
            go.GetComponent<MeshFilter>().mesh = hexaFilledMesh;
            go.GetComponent<Renderer>().sharedMaterial = allPathsDisplayMat;
            go.GetComponent<Collider>().enabled = false;
            pathFinderDisplay.Add(go);
            //Debug.Log("Dernier hexa : " + pathFinderDisplay.Last<GameObject>());
        }
    }

    //Display the shortest path
    //Author : ?
    void displaySortestPath()
    {
        // Display path (create the green hexas)
        if (
            hexaHovered != null
            && (hexaHovered.type == HexaType.GROUND || hexaHovered.type == HexaType.BONUS || hexaHovered.type == HexaType.PORTAL || hexaHovered.type == HexaType.BUSH)
        )
        {
            List<Point> path = hexaGrid.findShortestPath(
                currentCharControlled.getX(),
                currentCharControlled.getY(),
                hexaHovered.getX(),
                hexaHovered.getY(),
                currentCharControlled.PM
            );
            if (path != null)
            {
                path.RemoveAt(0);
                foreach (Point p in path)
                {
                    GameObject go = GameObject.Instantiate(hexaTemplate, hexasFolder.transform);
                    go.SetActive(true);
                    go.GetComponent<Transform>().position = Hexa.hexaPosToReal(p.getX(), p.getY(), -0.014f);
                    go.GetComponent<MeshFilter>().mesh = hexaFilledMesh;
                    go.GetComponent<Renderer>().sharedMaterial = pathDisplayMat;
                    go.GetComponent<Collider>().enabled = false;
                    pathFinderDisplay.Add(go);
                }
            }
        }
    }

    //Display the sighted hexas
    //Author : ?
   
    void displayLineOfSight()
    {
        List<Point> hexasBlocked = null;
        // List<Point> pointList = hexaGrid.findHexasInSight(currentCharControlled.getX(), currentCharControlled.getY(), (actionType == ActionType.ATK1) ? currentCharControlled.getClassData().basicAttack.range : currentCharControlled.getClassData().skill_1.range, out hexasBlocked, currentCharControlled);
        // au lieu d'utilisr un operateur ternaire, on peut utiliser une condition if/else en incluant skill_2
        List<Point> pointList = null;

        if (actionType == ActionType.ATK1)
        {
            pointList = hexaGrid.findHexasInSight(
                currentCharControlled.getX(),
                currentCharControlled.getY(),
                currentCharControlled.getClassData().basicAttack.range,
                out hexasBlocked,
                currentCharControlled
            );
        }
        else if (actionType == ActionType.ATK2)
        {
            pointList = hexaGrid.findHexasInSight(
                currentCharControlled.getX(),
                currentCharControlled.getY(),
                currentCharControlled.getClassData().skill_1.range,
                out hexasBlocked,
                currentCharControlled
            );
        }
        else if (actionType == ActionType.ATK3)
        {
            pointList = hexaGrid.findHexasInSight(
                currentCharControlled.getX(),
                currentCharControlled.getY(),
                currentCharControlled.getClassData().skill_2.range,
                out hexasBlocked,
                currentCharControlled
            );
        }
        //Edited by Julien D'aboville L3L1 2024
        else if (actionType == ActionType.ATK4)
        {
            pointList = hexaGrid.findHexasInSight(
                currentCharControlled.getX(),
                currentCharControlled.getY(),
                currentCharControlled.getClassData().skill_3.range,
                out hexasBlocked,
                currentCharControlled
            );
        }

        bool hexaHoveredTargetable = false;
        // Display line of sight (Blue hexas)
        foreach (Point p in pointList)
        {
            if (hexaHovered != null && p.getX() == hexaHovered.getX() && p.getY() == hexaHovered.getY())
                hexaHoveredTargetable = true;
            GameObject go = GameObject.Instantiate(hexaTemplate, hexasFolder.transform);
            go.SetActive(true);
            go.GetComponent<Transform>().position = Hexa.hexaPosToReal(p.getX(), p.getY(), -0.015f);
            go.GetComponent<MeshFilter>().mesh = hexaFilledMesh;
            go.GetComponent<Renderer>().sharedMaterial = lineOfSightMat;
            go.GetComponent<Collider>().enabled = false;
            lineOfSightDisplay.Add(go);
        }
        // Display blocked hexas (transparent blue hexas)
        foreach (Point p in hexasBlocked)
        {
            GameObject go = GameObject.Instantiate(hexaTemplate, hexasFolder.transform);
            go.SetActive(true);
            go.GetComponent<Transform>().position = Hexa.hexaPosToReal(p.getX(), p.getY(), -0.015f);
            go.GetComponent<MeshFilter>().mesh = hexaFilledMesh;
            go.GetComponent<Renderer>().sharedMaterial = blockedSightMat;
            go.GetComponent<Collider>().enabled = false;
            lineOfSightDisplay.Add(go);
        }
        if (hexaHoveredTargetable)
        {
            // List<Point> hexaPos = hexaGrid.getHexasWithinRange(hexaHovered.getX(), hexaHovered.getY(), (actionType == ActionType.ATK1) ? currentCharControlled.getClassData().basicAttack.rangeAoE : currentCharControlled.getClassData().skill_1.rangeAoE);
            // if with skill_2
            List<Point> hexaPos = null;
            if (actionType == ActionType.ATK1)
            {
                hexaPos = hexaGrid.getHexasWithinRange(
                    hexaHovered.getX(),
                    hexaHovered.getY(),
                    currentCharControlled.getClassData().basicAttack.rangeAoE
                );
            }
            else if (actionType == ActionType.ATK2)
            {
                hexaPos = hexaGrid.getHexasWithinRange(
                    hexaHovered.getX(),
                    hexaHovered.getY(),
                    currentCharControlled.getClassData().skill_1.rangeAoE
                );
            }
            else if (actionType == ActionType.ATK3)
            {
                hexaPos = hexaGrid.getHexasWithinRange(
                    hexaHovered.getX(),
                    hexaHovered.getY(),
                    currentCharControlled.getClassData().skill_2.rangeAoE
                );
            }
            else if (actionType == ActionType.ATK4)
            {
                hexaPos = hexaGrid.getHexasWithinRange(
                    hexaHovered.getX(),
                    hexaHovered.getY(),
                    currentCharControlled.getClassData().skill_3.rangeAoE
                );
            }

            // Display AoE (red hexas)
            foreach (Point p in hexaPos)
            {
                GameObject go = GameObject.Instantiate(hexaTemplate, hexasFolder.transform);
                go.SetActive(true);
                go.GetComponent<Transform>().position = Hexa.hexaPosToReal(p.getX(), p.getY(), -0.014f);
                go.GetComponent<MeshFilter>().mesh = hexaFilledMesh;
                go.GetComponent<Renderer>().sharedMaterial = aoeMat;
                go.GetComponent<Collider>().enabled = false;
                lineOfSightDisplay.Add(go);
            }
        }
    }
}

//Author Socrate Louis Deriza, L3C1
public class EndGameDataCharacter
{
    public static int damageTeam0 = 0;
    public static int damageTeam1 = 0;

    public CharClass typeCharacter;
    public int teamCharacter;
    public int hpCharacter;

    public Character character;

    //dégât infligés par le personnage dans la partie
    public int damageDealt;

    //points de vies soignés au cours de la partie
    public int hpRestored;

    //nombre d'actions faite par le personnage au cours de la partie
    public int numberOfAction;

    //nombre d'actions dangereuse (attaque) faite par le personnage au cours de la partie (isolation des utilisation d'attaques et des compétences du reste)
    public int numberOfDangerousAction;

    //nombre de case parcouru par le personnage au cours de ces déplacements
    public int numberOfSlotCrossedByTheCharacter;

    public int numberOfOpponentsEliminated;

    public EndGameDataCharacter(Character charac, CharClass character, int team)
    {
        this.typeCharacter = character;
        this.teamCharacter = team;

        this.hpCharacter = charac.getHPmax();

        this.character = charac;
        this.numberOfAction = 0;
        this.damageDealt = 0;
        this.hpRestored = 0;
        this.numberOfAction = 0;
        this.numberOfDangerousAction = 0;
        this.numberOfSlotCrossedByTheCharacter = 0;
        this.numberOfOpponentsEliminated = 0;
    }

    public void addDamage(int damageCharacter)
    {
        this.damageDealt += damageCharacter;
        if (this.teamCharacter == 0)
        {
            EndGameDataCharacter.damageTeam0 += damageCharacter;
        }
        else
        {
            EndGameDataCharacter.damageTeam1 += damageCharacter;
        }
    }

    public void addAction(MainGame.ActionType newAction)
    {
        this.numberOfAction++;
        if (
            newAction == MainGame.ActionType.ATK1
            || newAction == MainGame.ActionType.ATK2
            || newAction == MainGame.ActionType.ATK3
            || newAction == MainGame.ActionType.ATK4
        )
        {
            this.numberOfDangerousAction++;
        }
    }

    public void addMovement(int numberHexa)
    {
        this.numberOfSlotCrossedByTheCharacter += numberHexa;
    }

    public void addAnHealingAction(int numberOfHpRestored)
    {
        this.hpRestored += numberOfHpRestored;
        /*for(int i = 0; i< numberOfHpRestored; i++)
        {
            this.hpRestored++;
        }*/
    }

    public float getPercentageOfActivity()
    {
        return ((((float)this.numberOfDangerousAction * 100.0f) / (float)this.numberOfAction));
    }

    public void hasKilledSomeOne()
    {
        this.numberOfOpponentsEliminated++;
    }

    public float getDamagePercentageOfTHisTeam()
    {
        if (teamCharacter == 0)
        {
            return ((((float)this.damageDealt * 100.0f) / (float)EndGameDataCharacter.damageTeam0));
        }
        else
        {
            return ((((float)this.damageDealt * 100.0f) / (float)EndGameDataCharacter.damageTeam1));
        }
    }

    public int getnumberOfSlotCrossedByTheCharacter()
    {
        return (this.numberOfSlotCrossedByTheCharacter);
    }

    public float getPercentageOfHpRestored()
    {
        return ((((float)this.hpRestored * 100.0f) / (float)this.hpCharacter));
    }
}
