using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MonPersoGestionScript : MonoBehaviour
{
    public static Dictionary<string, List<string>> AttackNames = new Dictionary<string, List<string>>()
    {
        { "DRUIDE", new List<string>{ "Coup basique", "Coup de bâton", "Gel", "Tempête" } },
        { "MAGE", new List<string>{ "Coup basique", "Sort de dégâts", "Empoisonement", "Bouclier" } },
        { "ARCHER", new List<string>{ "Coup basique", "Flèche améliorée", "Pluie de flèches", "Flèche enflamée" } },
        { "VOLEUR", new List<string>{ "Coup basique", "Coup de dague", "Vol de vie", "Invisibilité" } },
        { "ENVOUTEUR", new List<string>{ "Boost de dégâts", "Boost de PA", "Malédiction", "Blocage des capacités" } },
        { "SOIGNEUR", new List<string>{ "Soin basique", "Soin", "Soin massif", "Absorption" } },
        { "GUERRIER", new List<string>{ "Coup basique", "Coup d'épée", "Coup aléatoire", "Rage" } },
        { "VALKYRIE", new List<string>{ "Coup basique", "Coup de hache", "Gel", "Tourne hache" } },
        { "FORGERON", new List<string>{ "Coup basique", "Cri de guerre", "Marteau explosif", "Lancer de marteau" } },
        { "NETHERFANG", new List<string>{ "Coup basique", "Cri de guerre", "Gel", "Saut" } },
        { "LIFEWEAVER", new List<string>{ "Soin basique", "Coup de griffe", "Absorption", "Téléportation" } },
        { "BASTION", new List<string>{ "Frappe forteresse","Egide de titan" ,"Salve des roches", "Imapct desctructeur" } },
        { "AMAZONE", new List<string>{ "Frappe sauvage", "Soin de la chausseuse", "Fougue furieuse", "Flèche perforante" } },
        { "GEOMAGE", new List<string>{ "Séisme", "Rempart géologique", "Choc sismique", "Barrière de roche" } },
        { "MORTIFERE", new List<string>{ "Emabarassement de la mort", "Guérison ténébreuse", "Siphon des âmes", "Malédiction léthargique" } }

    };

    static int nbAttaque=4;

    public static string savePathPerso = Application.streamingAssetsPath + "/CharacterSave.db";
    public TMP_Dropdown[] characterDropdown;
    public TMP_Dropdown[] attackDropdown;
    public TMP_InputField pvInput;
    public TMP_InputField paInput;
    public TMP_InputField pmInput;
    public TMP_InputField priorityInput;

    public TMP_Text textError;

    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    void FillCharacterDropdown(int i)
    {
        List<string> characterNames = new List<string>(AttackNames.Keys);

        characterDropdown[i].ClearOptions();
        characterDropdown[i].AddOptions(characterNames);
    }

    void FillAttackDropdown(int i)
    {
        string selectedCharacter = characterDropdown[i].options[characterDropdown[i].value].text;

        List<string> attackNames = AttackNames[selectedCharacter];

        attackDropdown[i].ClearOptions();
        attackDropdown[i].AddOptions(attackNames);
    }

    public void TrySavePerso()
    {
        (int, int)[] attacks = new (int character, int attack)[attackDropdown.Length];
        for (int i = 0; i < attackDropdown.Length; i++)
        {
            attacks[i] = (characterDropdown[i].value, attackDropdown[i].value);
        }

        int pa;
        int pv;
        int pm;
        int priority;

        int.TryParse(paInput.text, out pa);
        int.TryParse(pvInput.text, out pv);
        int.TryParse(pmInput.text, out pm);
        int.TryParse(priorityInput.text, out priority);


        if(pa<=0 || pm<=0 || pv<=0 || priority<=0){
            textError.gameObject.SetActive(true);
        }
        else{
            textError.gameObject.SetActive(false);
            WriteSavePerso(pv, pa,pm,priority, attacks);
        }
        
    }

    public void WriteSavePerso(int pv, int pa,int pm,int priority, (int, int)[] attacks)
    {
        if (File.Exists(savePathPerso))
        {
            File.Delete(savePathPerso);
        }
        SqliteConnection.CreateFile(savePathPerso);

        string conn = "URI=file:" + savePathPerso;
        SqliteConnection dbconn = new SqliteConnection(conn);
        dbconn.Open();
        SqliteCommand dbcmd = dbconn.CreateCommand();

        dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS CommonInfo (PV INTEGER, PA INTEGER, PM INTEGER, PRIORITY INTEGER)";
        dbcmd.ExecuteNonQuery();

        dbcmd.CommandText = "INSERT INTO CommonInfo (PV, PA, PM, PRIORITY) VALUES (@PV, @PA, @PM, @PRIORITY)";
        dbcmd.Parameters.AddWithValue("@PV", pv);
        dbcmd.Parameters.AddWithValue("@PA", pa);
        dbcmd.Parameters.AddWithValue("@PM", pm);
        dbcmd.Parameters.AddWithValue("@PRIORITY", priority);
        dbcmd.ExecuteNonQuery();

        dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS AttackInfo (CharacterIndex INTEGER, AttackIndex INTEGER)";
        dbcmd.ExecuteNonQuery();

        foreach ((int, int) attack in attacks)
        {
            dbcmd.CommandText = "INSERT INTO AttackInfo (CharacterIndex, AttackIndex) VALUES (@CharacterIndex, @AttackIndex)";
            dbcmd.Parameters.AddWithValue("@CharacterIndex", attack.Item1);
            dbcmd.Parameters.AddWithValue("@AttackIndex", attack.Item2);
            dbcmd.ExecuteNonQuery();
            print("attaque : char : "+attack.Item1+" attack : "+attack.Item2);
        }

        dbconn.Close();
    }

    void Start()
    {
        for (int i = 0; i < nbAttaque; i++)
        {
            FillCharacterDropdown(i);
            FillAttackDropdown(i);
            // variable local sinon le delegate sera le meme pour tout les listener
            int index = i; 
            characterDropdown[i].onValueChanged.AddListener(delegate { FillAttackDropdown(index); });
        }
    }

    void Update()
    {
    }
}
