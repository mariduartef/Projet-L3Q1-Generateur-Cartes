using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Characters;
using AI_Class;
using Mono.Data.Sqlite;
using System.Data;
using System;
using static Characters.CharsDB;
using System.ComponentModel;
using Maps;


public enum PlayerType : byte { HUMAN, AI_CPU_Offense, AI_CPU_Defense, AI_CPU, AI_EASY,  AI_MEDIUM, AI_HARD};

// Data structure used to send game info when switching scenes
public class StartGameData
{
    public string loadSave=null;
    public List<CharClass> charsTeam1 = new List<CharClass>();
    public List<CharClass> charsTeam2 = new List<CharClass>();
    public PlayerType player1Type;
    public PlayerType player2Type;
    public int mapChosen; // 0 = Ruins, 1 = Random
    public string mapName; //Added by Mariana Duarte L3Q1
    public Map map; //Added by Mariana Duarte L3Q1
    public int nbGames;
    public float slider;
    public StartGameData() { }
}

///<summary>
///Functions and methods for the Main menu
///</summary>
// //edited by Julien D'aboville L3L1 2024
// Edited by Mariana Duarte L3Q1 04/2025
public class MainMenu : MonoBehaviour
{
    public static StartGameData startGameData;
    public Texture secondBackground;
    public Texture firstBackground;
    int maxTeamSize = 5; //nb of characters (1 to 5)
    public GameObject background;
    public GameObject mainMenu;
    public GameObject CharSelectMenu;
    public GameObject advancedOptionsMenu;
    public List<Texture> charCards;
    public GameObject charsTeam1Display;
    public GameObject charsTeam2Display;
    public GameObject teamSelectHighlight;
    public GameObject credits;
    public GameObject guide;
    public Text errorLoad;
    // Main menu buttons
    public Button buttonPlay;
    public Button buttonQuit;
    public Button buttonCredits;
    public Button buttonGuide;
    // Char select menu buttons
    public List<Button> buttonCharCards;
    public List<Button> buttonTeam1Cards;
    public List<Button> buttonTeam2Cards;
    public Button buttonBackTeam1;
    public Button buttonBackTeam2;
    public Button buttonReadyTeam1;
    public Button buttonReadyTeam2;
    public Button buttonToAdvancedOptions;
    // Advanced options menu buttons
    public Button buttonToCharSelect;
    public Button v1;
    public Button v2;
    public Button v3;
    public Button v4;
    public Button v5;
    public Toggle toggleConsoleMode;
    public Slider sliderNbGames;
    public GameObject textNbGames;

    bool v5Pressed = false;
    bool v4Pressed = false;
    bool v3Pressed = false;
    bool v2Pressed = false;
    bool v1Pressed = false;
    bool buttonPlayPressed = false;
    bool buttonQuitPressed = false;
    bool buttonCreditsPressed = false;
    //bool buttonGuidePressed = false;
    bool buttonStatsPressed = false;



    //edited by Julien D'aboville L3L1 2024 edited by Chen Christophe L3Q1 2025
    const int nbPersos=16;
    bool[] buttonCharCardsPressed = new bool[nbPersos] {  false,false,false,false,false, false,false,false,false,false,false,false,false ,false,false,false};
    bool[] buttonTeam1Enable = new bool[nbPersos] { true,true,true,true,true, true, true, true,true,true,true ,true,true,true,true,true};
    bool[] buttonTeam2Enable = new bool[nbPersos] { true,true,true,true,true, true, true, true, true,true,true,true,true,true,true,true};



    bool[] buttonTeam1CardsPressed = new bool[5] { false, false, false, false, false };
    bool[] buttonTeam2CardsPressed = new bool[5] { false, false, false, false, false };
    bool buttonBackTeam1Pressed = false;
    bool buttonBackTeam2Pressed = false;
    bool buttonReadyTeam1Pressed = false;
    bool buttonReadyTeam2Pressed = false;
    bool buttonToAdvancedOptionsPressed = false;
    bool buttonToCharSelectPressed = false;

    public Slider slider;
    public Dropdown dropdownPlayer1Type;
    public Dropdown dropdownPlayer2Type;
    public Dropdown dropdownMap;
    PlayerType player1Type;
    PlayerType player2Type;

    List<CharClass> charsTeam1 = new List<CharClass>();
    List<CharClass> charsTeam2 = new List<CharClass>();
    int currentTeam = 0;
    bool consoleMode;
    int nbGames;

    //Awake is called before Start
    void Awake()
    {
        Application.targetFrameRate = 75;
        QualitySettings.vSyncCount = 0;
    }

    // Start is called before the first frame update
    //Edited by Socrate Louis Deriza L3C1
    //edited by Julien D'aboville L3L1 2024
    // edited by simon L3L1
    void Start()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Save/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Save/");
        int noCustomCharacter=15;
        if(isThereCustomCharacter()){
            buttonCharCards[noCustomCharacter].gameObject.SetActive(true);
        }
        else{
            buttonCharCards[noCustomCharacter].gameObject.SetActive(false);
        }

        mainMenu.SetActive(true);

        CreateListenersMenu();

        charsTeam1Display.transform.GetChild(1).gameObject.SetActive(true);

        //replaceCharacter();

        CreateListenersSelection(nbPersos);

        consoleMode = true;
        nbGames = 1;
        // Read Options from options file
        if (File.Exists(Application.streamingAssetsPath + "/Data/Options/options"))
        {
            loadOptions();
        }
        else
        {
            saveOptions();
        }
        toggleConsoleMode.isOn = consoleMode;
        sliderNbGames.value = Mathf.Sqrt((float)nbGames);
        if (startGameData != null)
        {
            slider.value = startGameData.slider;
        }
    }

    public void removeIAdropdown(){
        List<String> options = new List<string>();
        options.Add("Humain");
        switch(currentTeam){
            case 0:
                
                dropdownPlayer1Type.ClearOptions();
                dropdownPlayer1Type.AddOptions(options);
            break;

            case 1:
                dropdownPlayer2Type.ClearOptions();
                dropdownPlayer2Type.AddOptions(options);
            break;
        }
    }

    public bool isThereCustomCharacter(){
        if(File.Exists(savePathPerso)){
            return true;
        }
        return false;
    }

    public void addIAdropdown(){
        List<String> options = new List<string>();
        options.Add("Humain");
        options.Add("CPU");
        switch(currentTeam){
            case 0:
                
                dropdownPlayer1Type.ClearOptions();
                dropdownPlayer1Type.AddOptions(options);
            break;

            case 1:
                dropdownPlayer2Type.ClearOptions();
                dropdownPlayer2Type.AddOptions(options);
            break;
        }
    }
    
    void Update()
    {
        //***********
        // MAIN MENU
        //***********

        if (mainMenu.activeInHierarchy)
        {
            // Go to character selection menu
            if (buttonPlayPressed)
            {
                credits.SetActive(false);
                guide.SetActive(false);
                mainMenu.SetActive(false);
                CharSelectMenu.SetActive(true);
                initCharSelectMenu();
                buttonPlayPressed = false;
                background.GetComponent<RawImage>().texture = secondBackground;
            }
            
            // Show credits
            if (buttonCreditsPressed)
            {
                credits.SetActive(!credits.activeInHierarchy);
                buttonCreditsPressed = false;
            }

            // Stats
            if (buttonStatsPressed)
            {
                SceneManager.LoadScene(2);
                buttonStatsPressed = false;
            }

            // Quit game
            if (buttonQuitPressed)
            {
                quitGame();
            }


        }

        // *************************
        // CHARACTER SELECTION MENU
        // *************************

        else if (CharSelectMenu.activeInHierarchy)
        {
        


            // Back to main menu
            if (buttonBackTeam1Pressed)
            {
                for (int i = 0; i < nbPersos; i++)//Edited by L3C1 Y,H , L3L1 Julien
                {
                    buttonCharCards[i].gameObject.SetActive(true);
                    buttonTeam1Enable[i] = true;
                    buttonTeam2Enable[i] = true;
                }
                mainMenu.SetActive(true);
                background.GetComponent<RawImage>().texture = firstBackground;
                CharSelectMenu.SetActive(false);
                buttonBackTeam1Pressed = false;
            }

            // Back to team 1
            if (buttonBackTeam2Pressed)
            {

                charSelectMenuPreviousPlayer();
                buttonBackTeam2Pressed = false;
            }

            List<CharClass> charsTeam = (currentTeam == 0) ? charsTeam1 : charsTeam2;
            GameObject charsTeamDisplay = (currentTeam == 0) ? charsTeam1Display : charsTeam2Display;
            bool[] buttonTeamCardsPressed = (currentTeam == 0) ? buttonTeam1CardsPressed : buttonTeam2CardsPressed;

            //Display 1, 2, 3, 4 or 5 character slot
            if (v5Pressed) { maxTeamSize = 5; v5Pressed = SlotActivation(maxTeamSize); }
            if (v4Pressed) { maxTeamSize = 4; v4Pressed = SlotActivation(maxTeamSize); }
            if (v3Pressed) { maxTeamSize = 3; v3Pressed = SlotActivation(maxTeamSize); }
            if (v2Pressed) { maxTeamSize = 2; v2Pressed = SlotActivation(maxTeamSize); }
            if (v1Pressed) { maxTeamSize = 1; v1Pressed = SlotActivation(maxTeamSize); }

            // CHARACTER SELECTION Edited by L3C1 Y,H , Edited By Julien D'aboville L3L1 2024
            for (int i = 0; i < nbPersos; i++)
            {
                if (buttonCharCardsPressed[i])
                {
                    print("pressedfffffffffffffff");
                    if (charsTeam.Count < maxTeamSize)
                    {

                        charsTeam.Add((CharClass)i);
                        
                        // si le perso custom, de numero 11 est selectione
                        // on enleve la possibilité d'etre une IA si il y a le perso custom dans l'equipe
                        if(i==15){
                            removeIAdropdown();
                        }

                        if (currentTeam == 0)
                        {
                            buttonTeam1Enable[i] = false;
                            
                        }
                        else
                        {
                            buttonTeam2Enable[i] = false;
                        }
                        charsTeamDisplay.transform.GetChild(0).transform.GetChild(charsTeam.Count - 1).GetComponent<RawImage>().texture = charCards[i];

                        testAndDisplayCharRoster();
                        
                    }

                    buttonCharCardsPressed[i] = false;
                }
            }

            // REMOVE CHARACTER FROM TEAM
            for (int i = 0; i < 5; i++)
            {
                if (buttonTeamCardsPressed[i])
                {
                    if (charsTeam.Count > i)
                    {
                        
                        int numCard = (int)charsTeam[i];

                        //si on deselectionne le perso custom on remet IA en possibilité

                        if(numCard==15){
                            addIAdropdown();
                        }

                        if (currentTeam == 0)
                        {
                            buttonTeam1Enable[numCard] = true;
                        }
                        else
                        {
                            buttonTeam2Enable[numCard] = true;
                        }
                        charsTeam.RemoveAt(i);
                        for (int j = i; j < charsTeam.Count; j++)
                        {
                            charsTeamDisplay.transform.GetChild(0).transform.GetChild(j).GetComponent<RawImage>().texture = charCards[(int)charsTeam[j]];
                        }
                        charsTeamDisplay.transform.GetChild(0).transform.GetChild(charsTeam.Count).GetComponent<RawImage>().texture = charCards[nbPersos];//Edited by L3C1 Y,H

                        testAndDisplayCharRoster();
                    }
                    buttonTeamCardsPressed[i] = false;
                }
            }

            //Verifie si le joueur 2 n'ait pas autant de personnages que le joueur 1
            if (currentTeam == 1 && maxTeamSize != charsTeam1.Count)
            {
                charSelectMenuPreviousPlayer();
            }

            // READY
            if (charsTeam.Count == maxTeamSize)
            {
                if (currentTeam == 0) charsTeam1Display.transform.GetChild(2).gameObject.SetActive(true);
                else charsTeam2Display.transform.GetChild(2).gameObject.SetActive(true);
            }
            else
            {
                if (currentTeam == 0) charsTeam1Display.transform.GetChild(2).gameObject.SetActive(false);
                else charsTeam2Display.transform.GetChild(2).gameObject.SetActive(false);
            }
            if (buttonReadyTeam1Pressed)
            {
                charSelectMenuNextPlayer();
                buttonReadyTeam1Pressed = false;
            }
            else if (buttonReadyTeam2Pressed)
            {
                // Give Main all the info

                /*MainGame.startGameData = new StartGameData
                {
                    loadSave = null,
                    
                    charsTeam1 = charsTeam1,
                    charsTeam2 = charsTeam2,
                    player1Type = (PlayerType)dropdownPlayer1Type.value,
                    player2Type = (PlayerType)dropdownPlayer2Type.value,
                    mapChosen = dropdownMap.value,
                    nbGames = nbGames,
                    slider = slider.value
                };*/
                // Added by Mariana Duarte L3Q1 04/2025
                MapChoiceManagement.startGameData = new StartGameData
                {
                    loadSave = null,
                    charsTeam1 = charsTeam1,
                    charsTeam2 = charsTeam2,
                    player1Type = (PlayerType)dropdownPlayer1Type.value,
                    player2Type = (PlayerType)dropdownPlayer2Type.value,
                    mapChosen = -1,
                    mapName = "",
                    nbGames = nbGames,
                    slider = slider.value
                };
                buttonReadyTeam2Pressed = false;
                print("map value : "+dropdownMap.value);

                //Activation du mode console si difficulté d'IA choisie ??
                // Mode Console deactivated
           /*     if (MainGame.startGameData.player1Type != PlayerType.HUMAN && MainGame.startGameData.player2Type != PlayerType.HUMAN &&
                    MainGame.startGameData.player1Type != PlayerType.AI_CPU_Offense && MainGame.startGameData.player2Type != PlayerType.AI_CPU_Offense && consoleMode &&
                    MainGame.startGameData.player1Type != PlayerType.AI_CPU_Defense && MainGame.startGameData.player2Type != PlayerType.AI_CPU_Defense && consoleMode)
                {
                    // Load Console Mode scene
                    SceneManager.LoadScene(3);
                }*/
               

                /*{
                    SceneManager.LoadScene(1);
                }*/
                {
                    SceneManager.LoadScene("Assets/Scenes/Map Choice.unity");
                }

            }

            if (buttonToAdvancedOptionsPressed)
            {
                advancedOptionsMenu.SetActive(true);
                CharSelectMenu.SetActive(false);
                buttonToAdvancedOptionsPressed = false;
            }
        }
        else if (advancedOptionsMenu.activeInHierarchy)
        {
            if (buttonToCharSelectPressed)
            {
                advancedOptionsMenu.SetActive(false);
                CharSelectMenu.SetActive(true);
                buttonToCharSelectPressed = false;
                saveOptions();
            }
            consoleMode = toggleConsoleMode.isOn;
            nbGames = (int)(sliderNbGames.value * sliderNbGames.value);
            textNbGames.GetComponent<Text>().text = "(IA vs IA) nombre de parties : " + nbGames;
        }
    }

    void testAndDisplayCharRoster()
    {	//Edited by L3C1 Y,H , Julien D'aboville L3L1 2024,
        // edited by simon sepiol L3L1 2024
        Boolean displayRoster = false;
        int noCustomCharacter=15;
        if (currentTeam == 0 && charsTeam1.Count < maxTeamSize)
            displayRoster = true;
        else if (currentTeam == 1 && charsTeam2.Count < maxTeamSize)
            displayRoster = true;

        for (int i = 0; i < nbPersos; i++){
            if(i==noCustomCharacter){
                if(isThereCustomCharacter()){
                    buttonCharCards[i].gameObject.SetActive(displayRoster);
                }
                else{
                    buttonCharCards[i].gameObject.SetActive(false);
                }
            }
            else{
                buttonCharCards[i].gameObject.SetActive(displayRoster);

            }

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

    



    void initCharSelectMenu()
    {	//Edited by L3C1 Y,H ,Julien D'aboville L3L1 2024
        charsTeam1 = new List<CharClass>();
        charsTeam2 = new List<CharClass>();
        for (int i = 0; i < 5; i++) charsTeam1Display.transform.GetChild(0).transform.GetChild(i).GetComponent<RawImage>().texture = charCards[nbPersos];
        for (int i = 0; i < 5; i++) charsTeam2Display.transform.GetChild(0).transform.GetChild(i).GetComponent<RawImage>().texture = charCards[nbPersos];

        for (int i = 0; i < 5; i++) charsTeam1Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = true;
        for (int i = 0; i < 5; i++) charsTeam2Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = false;

        charsTeam1Display.transform.GetChild(1).gameObject.SetActive(true);
        charsTeam1Display.transform.GetChild(2).gameObject.SetActive(false);

        charsTeam2Display.transform.GetChild(1).gameObject.SetActive(false);
        charsTeam2Display.transform.GetChild(2).gameObject.SetActive(false);
    }

    void charSelectMenuNextPlayer()
    {
        currentTeam = 1;
        for (int i = 0; i < 5; i++) charsTeam1Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = false;
        for (int i = 0; i < 5; i++) charsTeam2Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = true;

        charsTeam1Display.transform.GetChild(1).gameObject.SetActive(false);
        charsTeam1Display.transform.GetChild(2).gameObject.SetActive(false);

        charsTeam2Display.transform.GetChild(1).gameObject.SetActive(true);
        charsTeam2Display.transform.GetChild(2).gameObject.SetActive(false);

        testAndDisplayCharRoster();

        teamSelectHighlight.transform.localPosition = new Vector3(0, 36 - 183, 0);
    }

    void charSelectMenuPreviousPlayer()
    {
        currentTeam = 0;
        for (int i = 0; i < 5; i++) charsTeam1Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = true;
        for (int i = 0; i < 5; i++) charsTeam2Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = false;

        charsTeam1Display.transform.GetChild(1).gameObject.SetActive(true);
        charsTeam1Display.transform.GetChild(2).gameObject.SetActive(true);

        charsTeam2Display.transform.GetChild(1).gameObject.SetActive(false);
        charsTeam2Display.transform.GetChild(2).gameObject.SetActive(false);

        testAndDisplayCharRoster();

        teamSelectHighlight.transform.localPosition = new Vector3(0, 36, 0);
    }


    void saveOptions()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(Application.streamingAssetsPath + "/Data/Options/options", FileMode.Create)))
        {
            writer.Write((int)((consoleMode) ? 1 : 0));
            writer.Write(nbGames);
        }
    }

    void loadOptions()
    {
        using (BinaryReader reader = new BinaryReader(File.Open(Application.streamingAssetsPath + "/Data/Options/options", FileMode.Open)))
        {
            consoleMode = (reader.ReadInt32() == 0) ? false : true;
            nbGames = reader.ReadInt32();
        }
    }
    
    public void quitGame()
    {
        Application.Quit();
        Debug.Log("You Quit The Game");
    }

    //Added by Timothé - L3L1
    private void CreateListenersMenu()
    {
        buttonPlay.onClick.AddListener(buttonPlayPressed_);
        buttonQuit.onClick.AddListener(buttonQuitPressed_);
        buttonCredits.onClick.AddListener(buttonCreditsPressed_);
        //buttonGuide.onClick.AddListener(buttonGuidePressed_);
    }

    //Added by Timothé - L3L1
    private void CreateListenersSelection(int nombrePersonnages)
    {
        for (int numListener = 0; numListener < nombrePersonnages; numListener++)
        {
            AddCharButtonListener(numListener);
        }

        for (int numListener = 0; numListener < maxTeamSize; numListener++)
        {
            AddTeam1ButtonListener(numListener);
            AddTeam2ButtonListener(numListener);
        }
        buttonBackTeam1.onClick.AddListener(buttonBackTeam1Pressed_);
        buttonBackTeam2.onClick.AddListener(buttonBackTeam2Pressed_);
        buttonReadyTeam1.onClick.AddListener(buttonReadyTeam1Pressed_);
        buttonReadyTeam2.onClick.AddListener(buttonReadyTeam2Pressed_);
        buttonToAdvancedOptions.onClick.AddListener(buttonToAdvancedOptionsPressed_);

        v5.onClick.AddListener(button5v5Pressed_);
        v4.onClick.AddListener(button4v4Pressed_);
        v3.onClick.AddListener(button3v3Pressed_);
        v2.onClick.AddListener(button2v2Pressed_);
        v1.onClick.AddListener(button1v1Pressed_);
        buttonToCharSelect.onClick.AddListener(buttonToCharSelectPressed_);
    }


    //Added by Timothé - L3L1
    //Based on L3C1 Y,H & Julien D'aboville L3L1 2024 edits
    private bool SlotActivation(int teamSize)
    {
        for (int removeIndex = 4; removeIndex > teamSize - 1; removeIndex--)
        {
            charsTeam1Display.transform.GetChild(0).transform.GetChild(removeIndex).GetComponent<RawImage>().texture = charCards[nbPersos];
            charsTeam2Display.transform.GetChild(0).transform.GetChild(removeIndex).GetComponent<RawImage>().texture = charCards[nbPersos];

            if (charsTeam1.Count > removeIndex)
            {
                buttonTeam1Enable[(int)charsTeam1[removeIndex]] = true;
                charsTeam1.RemoveAt(removeIndex);
            }
            if (charsTeam2.Count > removeIndex)
            {
                buttonTeam2Enable[(int)charsTeam2[removeIndex]] = true;
                charsTeam2.RemoveAt(removeIndex);
            }
        }
        for (int activate = 1; activate < teamSize; activate++)
        {
            buttonTeam1Cards[activate].gameObject.SetActive(true);
            buttonTeam2Cards[activate].gameObject.SetActive(true);
        }
        for (int deactivate = 4; deactivate > teamSize - 1; deactivate--)
        {
            buttonTeam1Cards[deactivate].gameObject.SetActive(false);
            buttonTeam2Cards[deactivate].gameObject.SetActive(false);
        }

        testAndDisplayCharRoster();

        return false;
    }

    // Events
    void button5v5Pressed_() { v5Pressed = true; }
    void button4v4Pressed_() { v4Pressed = true; }
    void button3v3Pressed_() { v3Pressed = true; }
    void button2v2Pressed_() { v2Pressed = true; }
    void button1v1Pressed_() { v1Pressed = true; }
    void buttonPlayPressed_() { buttonPlayPressed = true; }
    void buttonQuitPressed_() { buttonQuitPressed = true; }
    void buttonCreditsPressed_() { buttonCreditsPressed = true; }
    //void buttonGuidePressed_() { buttonGuidePressed = true; }
    void buttonStatsPressed_() { buttonStatsPressed = true; }
    void buttonCharCardsPressed_(int i) { buttonCharCardsPressed[i] = true; }
    void buttonTeam1CardsPressed_(int i) { buttonTeam1CardsPressed[i] = true; }
    void buttonTeam2CardsPressed_(int i) { buttonTeam2CardsPressed[i] = true; }
    void buttonBackTeam1Pressed_() { buttonBackTeam1Pressed = true; }
    void buttonBackTeam2Pressed_() { buttonBackTeam2Pressed = true; }
    void buttonReadyTeam1Pressed_() { buttonReadyTeam1Pressed = true; }
    void buttonReadyTeam2Pressed_() { buttonReadyTeam2Pressed = true; }
    void buttonToAdvancedOptionsPressed_() { buttonToAdvancedOptionsPressed = true; }
    void buttonToCharSelectPressed_() { buttonToCharSelectPressed = true; }
    void AddCharButtonListener(int index) { buttonCharCards[index].onClick.AddListener(() => buttonCharCardsPressed_(index)); } //Added by Timothé - L3L1
    void AddTeam1ButtonListener(int index) { buttonTeam1Cards[index].onClick.AddListener(() => buttonTeam1CardsPressed_(index)); } // Added by Timothé - L3L1
    void AddTeam2ButtonListener(int index) { buttonTeam2Cards[index].onClick.AddListener(() => buttonTeam2CardsPressed_(index)); } // Added by Timothé - L3L1
}
