using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexas;
using Mono.Data.Sqlite;
using System.Runtime.InteropServices;
using System.IO;
using System;


// ##################################################################################################################################################
// Characters
// Author : ?
// Edited by L3Q1, VALAT Thibault and GOUVEIA Klaus
//Edited by Julien D'aboville L3L1 2024
// Edited by Mariana Duarte L3Q1 2025
//Edited by Doralie Dorcal L3Q1 2024
// Commented by L3Q1, VALAT Thibault
// ##################################################################################################################################################

namespace Characters
{

    //Edited by L3C1 CROUZET Oriane
    //Edited by L3L1 Julien D'aboville 2024 
    public enum CharClass : byte { DRUIDE, MAGE, ARCHER, VOLEUR, ENVOUTEUR, SOIGNEUR, GUERRIER, VALKYRIE ,FORGERON ,NETHERFANG ,LIFEWEAVER, BASTION, MORTIFERE, AMAZONE, GEOMAGE, CUSTOM };

    //Author : ?
    //Edited by L3Q1, VALAT Thibault
    public class CharsDB
    {

        ///Edited by L3C1 CROUZET Oriane, MEDILEH Youcef
        // new AttackEffect DOOM, , HEALTH_STEALING, MASSIVE_HEAL, RANDOM_DAMAGE, FREEZE

        //Edited by L3L1 Julien D'aboville 2024
        // new AttackEffect SHIELD,HOWL,INVISIBLE,STORM,RAGE,ABSORB,HAMMER,HAMME_LAUNCH,JUMP_EXPLOSION,TELEPORT,SOUL_ABSORPTION

        public enum AttackEffect : byte
        {
            DAMAGE, RANDOM_DAMAGE, HEAL, MASSIVE_HEAL, PA_BUFF, DMG_BUFF, STUN, DOOM, SHIELD, HEALTH_STEALING, FREEZE, DAMAGE_OVER_TIME,
            LIGHNING,CHARGE,HOWL, INVISIBLE, STORM, RAGE, BURNING, DISABLESKILL,ABSORB, HAMMER, HAMMER_LAUNCH,JUMP_EXPLOSION,TELEPORT,SOUL_ABSORPTION,
            POSITION_EXCHANGE, DOMINATION, AWAKENING, METAMORPHOSIS, ROCK_WALL, SORCERY
        }
        // Attack 
        public class Attack
        {
            public int range;
            public int rangeAoE;
            public bool targetsEnemies;
            public bool targetsAllies;
            public bool targetsSelf;
            public AttackEffect attackEffect;
            public int effectValue;
            public int coolDown;
            public string name;

            //Initialize an attack
            public Attack(int range, int rangeAoE, bool targetsEnemies, bool targetsAllies, bool targetsSelf, AttackEffect attackEffect, int effectValue, int coolDown, string name)
            {
                this.range = range;
                this.rangeAoE = rangeAoE;
                this.targetsEnemies = targetsEnemies;
                this.targetsAllies = targetsAllies;
                this.targetsSelf = targetsSelf;
                this.attackEffect = attackEffect;
                this.effectValue = effectValue;
                this.coolDown=coolDown;
                this.name=name;
            }
        }
        // Char base stats per class and attacks
        public class CharacterDB
        {
            public int maxHP;
            public int basePA;
            public int basePM;
            public int basePriority;
            public Attack basicAttack;
            public Attack skill_1;
            //2 skills per class
            ///Edited by L3C1 MEDILEH Youcef
            public Attack skill_2;
            
            public Attack skill_3;



            // base stats list
            public CharacterDB(int maxHP, int basePA, int basePM, int priority, Attack basicAttack, Attack skill_1, Attack skill_2, Attack skill_3)
            {
                this.maxHP = maxHP;
                this.basePA = basePA;
                this.basePM = basePM;
                this.basePriority = priority;
                this.basicAttack = basicAttack;
                this.skill_1 = skill_1;
                //2 skills per class
                ///Edited by L3C1 MEDILEH Youcef
                this.skill_2 = skill_2;

                //Edited by L3L1 Julien

                this.skill_3 = skill_3;
            }
        }
        public static List<CharacterDB> list;

        public static string savePathPerso = Application.streamingAssetsPath+"/CharacterSave.db";


        public static void loadMonPerso(List<CharacterDB> list){
            if(File.Exists(savePathPerso)){
                int nbAttaque =4;
            string conn = "URI=file:" + savePathPerso ;
            SqliteConnection dbconn = new SqliteConnection(conn);
            dbconn.Open(); 
            SqliteCommand dbcmd = dbconn.CreateCommand();

            dbcmd.CommandText = "SELECT * FROM CommonInfo";
            SqliteDataReader reader = dbcmd.ExecuteReader();
            reader.Read();

            int pv = reader.GetInt32(0);
            int pa = reader.GetInt32(1);
            int pm = reader.GetInt32(2);
            int priority = reader.GetInt32(3);

            Debug.Log("pv et pa et pm et prio : "+pv+" "+pa+" "+pm+" "+priority);

            (int,int)[] tupleCharAttack = new (int,int)[nbAttaque];

            reader.Close();

            dbcmd.CommandText = "SELECT * FROM AttackInfo";
            reader = dbcmd.ExecuteReader();
            for(int i =0; i<4 && reader.Read(); i++){
                tupleCharAttack[i]=(reader.GetInt32(0),reader.GetInt32(1));
            }

            
            Attack[] attacks = new Attack[nbAttaque];

            for(int i=0; i<attacks.Length; i++){
                CharacterDB character = list[tupleCharAttack[i].Item1];

                Debug.Log("character avec "+character.basePA+" pa");
                Debug.Log("character no "+list[tupleCharAttack[i].Item1]);
                Debug.Log("avec attaque no "+tupleCharAttack[i].Item2);
                switch(tupleCharAttack[i].Item2){
                    case 0:
                        attacks[i]=character.basicAttack;
                    break;

                    case 1:
                        attacks[i]=character.skill_1;
                    break;

                    case 2:
                        attacks[i]=character.skill_2;
                    break;
                    case 3:
                        attacks[i]=character.skill_3;
                    break;
                }
            }
            Debug.Log(attacks[0].name);
            Debug.Log(attacks[1].name);
            Debug.Log(attacks[2].name);
            Debug.Log(attacks[3].name);

            list.Add(new CharacterDB(pv,pa,pm,priority,attacks[0],attacks[1],attacks[2],attacks[3]));
            }
            

        }

        //Initialize the characters for the game
        //edited by L3Q1, GOUVEIA Klaus and VALAT Thibault
        ///Edited by L3C1 CROUZET Oriane, MEDILEH Youcef
        //Edited by L3L1 Julien D'aboville 2024 
        //Author : ?
        public static void initCharsDB()
        {
            list = new List<CharacterDB>();
            //int range, int rangeAoE, bool targetsEnemies, bool targetsAllies, bool targetsSelf, AttackEffect attackEffect, int effectValue, int cooldown

            // DRUIDE
            list.Add(new CharacterDB(25, 2, 3, 4,
             new Attack(2, 0, true, false, false, AttackEffect.DAMAGE, 2, 1, "Coup basique"),
             new Attack(3, 0, true, false, false, AttackEffect.DAMAGE, 6, 8, "Coup de bâton"), // degats
              new Attack(1, 0, true, false, false, AttackEffect.FREEZE, 0, 4, "Gel"), // FREEZE
               new Attack(8, 8, true, false, false, AttackEffect.STORM, 3, 12, "Tempête"))); // TEMPETE

            // MAGE
            list.Add(new CharacterDB(25, 2, 3, 5,
             new Attack(3, 1, true, false, false, AttackEffect.DAMAGE, 1, 1, "Coup basique"),
              new Attack(5, 2, true, false, false, AttackEffect.DAMAGE, 3, 8, "Sort de dégats"), // degats
               new Attack(5, 2, true, false, false, AttackEffect.DAMAGE_OVER_TIME, 3, 15, "Empoisonement"), // empoisonement
                new Attack(1, 1, false, false, true, AttackEffect.SHIELD, 5, 4, "Bouclier"))); // bouclier

            // ARCHER
            list.Add(new CharacterDB(25, 2, 3, 5,
             new Attack(6, 0, true, false, false, AttackEffect.DAMAGE, 3, 1, "Coup basique"),
              new Attack(8, 0, true, false, false, AttackEffect.DAMAGE, 4, 4, "Flèche ameliorée"), // fleche ameliorée
               new Attack(15, 7, true, false, false, AttackEffect.DAMAGE, 4, 8, "Pluie de flèches"), // pluie de fleche
                new Attack(5, 1, true, false, false, AttackEffect.BURNING, 6, 15, "Flèche enflamée"))); // fleche enflamée

            // VOLEUR
            list.Add(new CharacterDB(35, 3, 3, 4,
             new Attack(1, 0, true, false, false, AttackEffect.DAMAGE, 3, 1, "Coup basique"),
              new Attack(2, 0, true, false, false, AttackEffect.DAMAGE, 5, 4, "Coup de dague"), // coup de dague
               new Attack(1, 0, true, false, false, AttackEffect.HEALTH_STEALING, 4, 8, "Vol de vie"),  // vole de vie
                new Attack(1, 1, false, false, true, AttackEffect.INVISIBLE, 3, 12, "Invisibilité")));  // invisibilité

            // ENVOUTEUR
            list.Add(new CharacterDB(30, 2, 3, 6,
             new Attack(4, 0, false, true, false, AttackEffect.DMG_BUFF, 3, 1, "Boost de dégâts"),
              new Attack(4, 2, false, true, false, AttackEffect.PA_BUFF, 3, 8, "Boost de PA"), // boost de PA
               new Attack(1, 1, true, false, false, AttackEffect.DOOM, 1, 4, "Malédiction"), // DOOM
                new Attack(1, 1, true, false, false, AttackEffect.DISABLESKILL, 10, 12, "Blocage des capacitées")));  // blocage capacité


            // SOIGNEUR
            list.Add(new CharacterDB(28, 2, 3, 6,
             new Attack(4, 0, false, true, false, AttackEffect.HEAL, 2, 1, "Soin basique"),
              new Attack(5, 4, false, true, false, AttackEffect.HEAL, 4, 4, "Soin"), // heal
               new Attack(1, 0, false, true, true, AttackEffect.MASSIVE_HEAL, 6, 12, "Soin massif"), // massive heal
                new Attack(6, 0, false, true, false, AttackEffect.ABSORB, 3, 8, "Absorption"))); // absorbtion


            // GUERRIER
            list.Add(new CharacterDB(40, 2, 3, 7,
              new Attack(1, 0, true, false, false, AttackEffect.DAMAGE, 1, 1, "Coup basique"),
              new Attack(1, 1, true, false, false, AttackEffect.RANDOM_DAMAGE, 4, 8, "Coup d'épée"), // coup d'epée
               new Attack(1, 1, true, false, false, AttackEffect.RANDOM_DAMAGE, 3, 4, "Coup aléatoire"), // coup aleatoire
                 new Attack(0, 0, false, false, true, AttackEffect.RAGE, 4, 12, "Rage")));  // rage


            // VALKYRIE
            list.Add(new CharacterDB(35, 2, 4, 4,
               new Attack(1, 1, true, false, false, AttackEffect.DAMAGE, 3, 1, "Coup basique"),
                new Attack(2, 1, true, false, false, AttackEffect.DAMAGE, 2, 4, "Coup de hache"), // degats
                 new Attack(3, 1, true, false, false, AttackEffect.LIGHNING, 0, 8, "Gel"), // Foudre
                  new Attack(1, 1, true, false, false, AttackEffect.DAMAGE, 3, 12, "Tourne hache"))); // tourne hache

            // FORGERON
            list.Add(new CharacterDB(35, 2, 3, 4,
             new Attack(2, 0, true, false, false, AttackEffect.DAMAGE, 1, 1, "Coup basique"),
              new Attack(0, 0, false, true, true, AttackEffect.HOWL, 1, 8, "Cri de guerre"), // cri de guerre
               new Attack(2, 3, true, false, false, AttackEffect.HAMMER, 3, 12, "Marteau explosif"), // Marteau explosif
                 new Attack(6, 0, true, false, false, AttackEffect.HAMMER_LAUNCH, 2, 4, "Lancer de marteau"))); // Lancer de marteau

            // NETHERFANG
            list.Add(new CharacterDB(40, 2, 3, 4,
             new Attack(1, 0, true, false, false, AttackEffect.DAMAGE, 1, 1, "Coup de griffe"),
              new Attack(0, 0, false, true, true, AttackEffect.HOWL, 1, 8, "Cri de guerre"), // cri de guerre
               new Attack(1, 0, true, false, false, AttackEffect.FREEZE, 0, 4, "Gel"), // FREEZE
                  new Attack(1, 5, true, false, true, AttackEffect.JUMP_EXPLOSION, 5, 12, "Saut"))); // Saut

            // LIFEWEAVER
            list.Add(new CharacterDB(25, 2, 3, 3,
             new Attack(6, 0, false, true, false, AttackEffect.HEAL, 3, 1, "Soin basique"),
              new Attack(5, 1, true, false, false, AttackEffect.SOUL_ABSORPTION, 3, 4, "Absorption d'âme"), // damage
               new Attack(6, 0, false, true, false, AttackEffect.ABSORB, 3, 8, "Absorption"), // absorbtion 
                new Attack(7, 0, false, true, true, AttackEffect.TELEPORT, 1, 12, "Téléportation"))); // TELEPORTATION

             // BASTION
            list.Add(new CharacterDB(30, 1, 3, 4, 
             new Attack(1, 1, true, false, false, AttackEffect.DAMAGE, 3, 1, "Frappe forteresse"), 
              new Attack(0, 0, false, false, true, AttackEffect.SHIELD, 2, 4, "Egide de titan"), 
               new Attack(10, 10, true, true, false, AttackEffect.DAMAGE, 3, 8, "Salve des roches"), 
                new Attack(2, 1, true, false, false, AttackEffect.DAMAGE, 1, 12, "Impact destructeur")));

            // MORTIFERE
            list.Add(new CharacterDB(30, 2, 4, 4, 
             new Attack(1, 0, true, false, false, AttackEffect.DAMAGE, 1, 1, "Embarassement de la mort"), 
              new Attack(0, 0, true, true, false, AttackEffect.HEAL, 2, 4, "Guérison ténébreuse"), 
               new Attack(2, 2, true, false, false, AttackEffect.HEALTH_STEALING, 1, 8, "Siphon des âmes"), 
                new Attack(3, 4, true, false, false, AttackEffect.SORCERY, 4, 12, "Malédiction léthargique"))); 

            // AMAZONE
            list.Add(new CharacterDB(30, 3, 1, 4,
             new Attack(2, 0, true, false, false, AttackEffect.DAMAGE, 1, 1, "Frappe sauvage"), 
              new Attack(0, 0, false, false, true, AttackEffect.HEAL, 1, 4, "Soin de la chausseuse"), 
               new Attack(0, 0, false, false, true, AttackEffect.RAGE, 2, 8, "Fougue furieuse"), 
                new Attack(4, 2, true, false, false, AttackEffect.DAMAGE, 3, 12, "Flèche perforante"))); 

            // GEOMAGE
            list.Add(new CharacterDB(30, 2, 4, 3, 
             new Attack(1, 1, true, false, false, AttackEffect.DAMAGE, 2, 1, "Séisme"), 
              new Attack(0, 0, false, true, true, AttackEffect.SHIELD, 3, 4, "Rempart géologique"), 
               new Attack(2, 5, true, true, false, AttackEffect.RANDOM_DAMAGE, 3, 8, "Choc sismique"), 
                new Attack(10, 1, true, true, true, AttackEffect.ROCK_WALL, 4, 12, "Barrière de roche"))); 



            // une fois la liste remplit par les capacités
            // on peut les utiliser pour le perso custom
            loadMonPerso(list);
        }

       
    }



    //Class character, to instatiate characters
    //Author : ?
    //Edited by L3Q1, Valat Thibault and Klaus Gouveia
    public class Character : System.IEquatable<Character>, System.IComparable<Character>
    {
        public static GameObject characterTemplate;
        public static List<GameObject> characterTemplateModels;
        public static GameObject charactersFolder;
        public static Color TEAM_1_COLOR = new Color(0.125f, 0.125f, 1);
        public static Color TEAM_2_COLOR = new Color(1, 0.125f, 0);
        public int totalDamage;
        public CharClass charClass;
        public int team;
        public int HPmax;
        public int HP;
        public int characterPA;
        public int PA;
        public int PM;
        public int priority;
        public int basePriority;
        public int x;
        public int y;
        public bool skillAvailable;
        ///Edited by L3C1 CROUZET Oriane
        public bool skillAvailable2;
        public bool doomed;
        public bool freezed;



        //Edited by L3L1 Julien D'aboville 2024 
        public bool shield;
        public bool stunned;
        public bool skillAvailable3;
        public bool howl;
        public bool raged;
        public bool estInvisible;
        public int nbHowl;
        public int invisibiliteDamage;
        public int damageInRage;
        public int damageBuffHowl;
        public int bonusPassive;
        public bool passiveEnvouteurActif ;
        public bool passiveArcherActif;
        public bool passiveNetherfangActif;
        public CharsDB.CharacterDB myCharClass;

        //L3L2
        public bool metamorphosis;
        public bool sorcery;
        public bool dominated;



        // Added by Timothé MIEL - L3L1
        public bool estDansUnBuisson;




        public int burning;
        public int fireDamage;
        public Character whoBurnedIt;

        // author Simon Sepiol L3L1 2024
        public int disabledSkill; // nb de tour a etre sans skill

        // on sauvegarde quel skill etait present pour pouvoir les remettre
        public bool wasSkill1Available;
        public bool wasSkill2Available;
        public bool wasSkill3Available;

        public bool dmgbuff;
        public HexaDirection directionFacing;
        public GameObject go;

        public int attack1coolDownValue;
        public int attack2coolDownValue;
        public int attack3coolDownValue;


        
        

        //Constructor
        //Author : ?
        //Edited by L3Q1, Klaus GOUVEIA and VALAT Thibault
       
        public Character(CharClass charClass, int x, int y, int team)
        {
            this.charClass = charClass;
            myCharClass = CharsDB.list[(int)charClass];
            HPmax = myCharClass.maxHP; HP = HPmax;
            PA = myCharClass.basePA;
            characterPA = myCharClass.basePA;
            PM = myCharClass.basePM;
            priority = myCharClass.basePriority;
            basePriority = myCharClass.basePriority;
            totalDamage = 0;
            this.x = x;
            this.y = y;
            this.team = team;
            this.skillAvailable = true;
            this.skillAvailable2 = true;
           
            //Edited by CROUZET Oriane, 15/03/2023
            this.doomed = false;
            this.freezed = false;
            this.dmgbuff = false;

            //Edited by L3L1 Julien D'aboville  2024
            this.stunned = false;
            this.skillAvailable3 = true;
            this.shield = false;
            this.raged = false;
            this.estInvisible = false;
            this.howl = false;
           this.nbHowl = 0;
            this.bonusPassive = 0;
            this.damageInRage = 0;
            this.damageBuffHowl = 0;
            this.invisibiliteDamage = 0;
            this.passiveEnvouteurActif = false;
            this.passiveArcherActif = false;
            this.passiveNetherfangActif = false;

            //L3L2
            this.metamorphosis = false;
            this.dominated = false;


            this.burning=0;
            

            


            this.go = GameObject.Instantiate(characterTemplate, charactersFolder.transform);
            this.go.SetActive(true);
            this.go.transform.position = Hexa.hexaPosToReal(x, y, 0);
            this.go.GetComponent<CharacterGO>().character = this;
            this.setColorByTeam();
            this.setDirection(HexaDirection.DOWN);

            this.attack1coolDownValue=0;
            this.attack2coolDownValue=0;
            this.attack3coolDownValue=0;
        }

    

        // No GameObject (console mode)
        ///Edited by L3C1 CROUZET Oriane

        public Character(CharClass charClass, int x, int y, int team, bool a)
        {
            this.charClass = charClass;
            CharsDB.CharacterDB myCharClass = CharsDB.list[(int)charClass];
            HPmax = myCharClass.maxHP; HP = HPmax;
            PA = myCharClass.basePA;
            characterPA = myCharClass.basePA;

            PM = myCharClass.basePM;
            priority = myCharClass.basePriority;
            basePriority = myCharClass.basePriority;

            this.x = x;
            this.y = y;
            this.team = team;
            this.skillAvailable = true;
            this.skillAvailable2 = true;



            //Edited by L3L1 Julien D'aboville2024

            this.skillAvailable3 = true;
            this.shield = false;
            this.stunned = false;
            this.raged = false;
            this.estInvisible = false;







            //Edited by L3C1 CROUZET Oriane
            this.doomed = false;
            this.freezed = false;

            this.go = null;


        }




        //Added by L3L1 Julien D'aboville 2024
        //retourne la valeur de la team(0 ou 1)
        public int getTeam()
        {
            return team;
        }

        //Added by L3L1 Julien D'aboville 2024
        //retourne la base de PA du character
        public int getCharacterPA()
            
        {
                return this.characterPA;
        }
        //Added by L3L1 Julien D'aboville 2024
        //Modifie la valeur de la base de PA du character.
        public void setCharacterPA(int newbasePA)
            {
                this.characterPA = newbasePA;
            }

        
        public int getAttack1coolDownValue()
        {
            return this.attack1coolDownValue;

        }

        public int getAttack2coolDownValue()
        {
            return this.attack2coolDownValue;

        }

        public int getAttack3coolDownValue()
        {
            return this.attack3coolDownValue;

        }
       

        public void setAttack1coolDownValue(int attack1coolDownValue)
        {
            this.attack1coolDownValue = attack1coolDownValue;

        }
     
   
        public void setAttack2coolDownValue(int attack2coolDownValue)
        {
            this.attack2coolDownValue = attack2coolDownValue;

        }
       

        public void setAttack3coolDownValue(int attack3coolDownValue)
        {
            this.attack3coolDownValue = attack3coolDownValue;

        }
        //Added by L3L1 Julien D'aboville 2024
        //Modifie la disponibilité du skill3
        public void setSkillAvailable3(bool skillAvailable3)
        {
            this.skillAvailable3 = skillAvailable3;
        }
        //Added by L3L1 Julien D'aboville 2024
        //Modifie la disponibilité du skill2
        public void setSkillAvailable2(bool skillAvailable2)
        {
            this.skillAvailable2 = skillAvailable2;
        }
        //Added by L3L1 Julien D'aboville 2024
        //Modifie la disponibilité du skill1
        public void setSkillAvailable1(bool skillAvailable1)
        {
            this.skillAvailable = skillAvailable1;
        }


        //Added by L3L1 Julien D'aboville 2024

        public void setStunned(bool stunned)
        {
            this.stunned = stunned;


        }
        //Added by L3L1 Julien D'aboville 2024

        public bool getStunned()
        {
            return this.stunned;
        }

        //Added by L3L1 Julien D'aboville 2024
        //Retourne la valeur des PA du character
        public int getPa()
        {
            return this.PA;
        }

        //Added by L3L1 Julien D'aboville 2024
        //change la valeur des PA du character
        public void setPa(int newPa)
        {
            this.PA = newPa;
        }

       // Added by Timothé MIEL - L3L1
        public Hexa GetHexa(HexaGrid hexaGrid)
        {
            return(hexaGrid.getHexa(x, y));
        }


        //Override of the equals method
        // /!\ Needed to sort characters 
        //Author : VALAT Thibault
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Character objAsCharacter = obj as Character;
            if (objAsCharacter == null) return false;
            else return Equals(objAsCharacter);
        }

        //Decide how to sort characters : according to the priority
        // /!\ Needed to sort characters
        //Author : VALAT Thibault
        public int CompareTo(Character compareChar)
        {
            if (compareChar == null)
                return 1;
            else
                return this.priority.CompareTo(compareChar.priority);
        }

        //Return the attribut to sort the characters
        // /!\ Needed to sort characters
        //Author : VALAT Thibault
        public override int GetHashCode()
        {
            return priority;
        }

        //To compare two characters
        // /!\ Needed to sort characters
        //Author : VALAT Thibault
        public bool Equals(Character other)
        {
            if (other == null) return false;
            return (this.priority.Equals(other.priority));
        }

        //Reset the priority to its basic value 
        //Author : VALAT Thibault
        // Edited by Youcef MEDILEH
        // - debut reglage de la priorité des personnages
        public void resetPriority()
        {
            // Debug.Log("#### resetPriority " + this.getName() + " ####");
            // Debug.Log("# basePriority : " + basePriority + "#");
            // Debug.Log("# priority : " + priority + "#");
            // Debug.Log("###############################");
            if (basePriority == 4)
            {
                this.priority = (basePriority + UnityEngine.Random.Range(0, 2));
            }
            else if (basePriority >= 6)
            {
                this.priority = (basePriority - UnityEngine.Random.Range(0, 2));
            }
            else
            {
                this.priority = basePriority;
            }
        }

        //Update the position of the character on thr grid
        //Author : ?
        public void updatePos(int newX, int newY, HexaGrid hexaGrid)
        {
            hexaGrid.getHexa(x, y).charOn = null;
            x = newX;
            y = newY;
            hexaGrid.getHexa(x, y).charOn = this;
            this.go.transform.position = Hexa.hexaPosToReal(x, y, 0);
        }

        // Console mode
        public void updatePos2(int newX, int newY, HexaGrid hexaGrid)
        {
            hexaGrid.getHexa(x, y).charOn = null;
            x = newX;
            y = newY;
            hexaGrid.getHexa(x, y).charOn = this;
        }

        //Clear the position of the character on thr grid
        //Author : ?
        public void clearPos(HexaGrid hexaGrid)
        {
            hexaGrid.getHexa(x, y).charOn = null;
        }

        //x coordinate getter
        //Author : ?
        public int getX()
        {
            return x;
        }

        //y coordinate getter
        //Author : ?
        public int getY()
        {
            return y;
        }

        //total damage getter
        //Author : ?
        public int getTotalDamage()
        {
            return totalDamage;
        }

        //total damage setter
        //Author : ?
        public void setTotalDamage(int totalDamage)
        {
            this.totalDamage = totalDamage;
        }

        //Pa getter
        //Author : ?
        public int getPA()
        {
            return PA;
        }


        //Pa setter
        //Author : ?
        public void setPA(int PA)
        {
            this.PA = PA;
        }

        //Hp getter
        //Author : ?
        public int getHP()
        {
            return HP;
        }

        public int getHPmax()
        {
            return HPmax;
        }

        //Pa setter
        //Author : ?
        public void setHP(int HP)
        {
            this.HP = HP;
        }

        //Char class getter
        //Author : ?
        public CharClass getCharClass()
        {
            return charClass;
        }
        
        //DamageBuff getter
        //Author : ?
        public bool getDamageBuff()
        {
            return dmgbuff;
        }

        //DamageBuff setter
        //Author : ?
        public void setDamageBuff(bool dmgbuff)
        {
            this.dmgbuff = dmgbuff;
        }

        //Damage getter
        //Author : ?
        public int getDamage()
        {
            if (dmgbuff == true) return CharsDB.list[(int)charClass].basicAttack.effectValue + 1;
            else return CharsDB.list[(int)charClass].basicAttack.effectValue;
        }

        //Range getter
        //Author : ?
        public int getRange()
        {
            return this.getClassData().basicAttack.range;
        }


        // AUTHOR : Julien D'aboville, group : L3L1
        //change la valeur du coolDown du skill 1
        public void setSkill1CoolDown(int newCooldown1)
        {
             CharsDB.list[(int)charClass].skill_1.coolDown= newCooldown1;
        }

        // AUTHOR : Julien D'aboville, group : L3L1
        //change la valeur du coolDown du skill 2
        public void setSkill2CoolDown(int newCooldown2)
        {
             CharsDB.list[(int)charClass].skill_2.coolDown= newCooldown2;
        }

        // AUTHOR : Julien D'aboville, group : L3L1
        //change la valeur du coolDown du skill 3
        public void setSkill3CoolDown(int newCooldown3)
        {
             CharsDB.list[(int)charClass].skill_3.coolDown= newCooldown3;
        }


        // AUTHOR : Julien D'aboville, group : L3L1
        //change  la valeur de la variable range de la basicAttack
        public void setRange(int newRange)
        {
            this.getClassData().basicAttack.range= newRange;
        }

        //Skill damage getter
        //Author : ?
        public int getSkillDamage()
        {
            if (dmgbuff == true) return CharsDB.list[(int)charClass].skill_1.effectValue + 1;
            else return CharsDB.list[(int)charClass].skill_1.effectValue;
        }

        ///Author : L3C1 MEDILEH Youcef
        public int getSkill2Damage()
        {
            if (dmgbuff == true) return CharsDB.list[(int)charClass].skill_2.effectValue + 1;
            else return CharsDB.list[(int)charClass].skill_2.effectValue;
        }

        

        public int getSkill3Damage()
        {
            if (dmgbuff == true) return CharsDB.list[(int)charClass].skill_3.effectValue + 1;
            else return CharsDB.list[(int)charClass].skill_3.effectValue;
        }

        public int getSkill1CoolDown(){
            return CharsDB.list[(int)charClass].skill_1.coolDown;
        }

        public int getSkill2CoolDown(){
            return CharsDB.list[(int)charClass].skill_2.coolDown;
        }

        public int getSkill3CoolDown(){
            return CharsDB.list[(int)charClass].skill_3.coolDown;
        }

        //Skill range getter
        //Author : ?
        //récupère la range du skill 1
        public int getSkillRange()
        {
            return this.getClassData().skill_1.range;
        }

        // AUTHOR : MEDILEH Youcef, group : L3C1
        //récupère la range du skill 2
        public int getSkill2Range()
        {
            return this.getClassData().skill_2.range;
        }





        // AUTHOR : Julien D'aboville, group : L3L1
        //récupère la range du skill 3
        public int getSkill3Range()
        {
            return this.getClassData().skill_3.range;
        }

        // AUTHOR : Julien D'aboville, group : L3L1
        //récupère la AoE range du skill 1
        public int getSkill1AoERange()
        {
            return this.getClassData().skill_1.rangeAoE;
        }
        // AUTHOR : Julien D'aboville, group : L3L1
        //récupère la AoE range du skill 2
        public int getSkill2AoERange()
        {
            return this.getClassData().skill_2.rangeAoE;
        }

        // AUTHOR : Julien D'aboville, group : L3L1
        //récupère la AoE range du skill 3
        public int getSkill3AoERange()
        {
            return this.getClassData().skill_3.rangeAoE;
        }

        public string getAttackName()
        {
            return this.myCharClass.basicAttack.name;
        }

        public string getSkill1Name(){

            return this.myCharClass.skill_1.name;

        }

        public string getSkill2Name(){

            return this.myCharClass.skill_2.name;

        }

        public string getSkill3Name(){

            return this.myCharClass.skill_3.name;

        }


        //Returns if the skill is ready or not
        //Author : ?
        public bool isSkill1Up()
        {
            return skillAvailable;
        }

        // AUTHOR : MEDILEH Youcef, group : L3C1
        public bool isSkill2Up()
        {
            return skillAvailable2;
        }

        // AUTHOR : Julien D'aboville, group : L3L1
        //permet de vérifier si le skill 3 est disponible
        public bool isSkill3Up()
        {
            return skillAvailable3;
        }

      

        // Author: CROUZET Oriane, group : L3C1
        // Date : 15/03/2023
        //Returns if the character is doomed or not
        public bool isDoomed()
        {
            return doomed;
        }
        // Author: CROUZET Oriane, group : L3C1
        // Date : 15/03/2023
        //Set the state of doomed
        public void setDoomed(bool doomed)
        {
            this.doomed = doomed;
            Debug.Log("Le perso est " + this.doomed);
        }
        // Author: CROUZET Oriane, group : L3C1
        // Date : 15/03/2023
        //Returns if the character is freezed or not
        public bool isFreezed()
        {
            return freezed;
        }

        // Author Simon Sepiol L3L1 2024

        public int isBurning(){
            return burning;
        }


        // AUTHOR : Julien D'aboville 2024, group : L3L1
        //Vérifie si la compétence "Invisibilité" du personnage est activée.
        public bool isInvisible()
        {
            return estInvisible;
        }

        // AUTHOR : Julien D'aboville 2024, group : L3L1
        //  Vérifie si la compétence "Rage" du personnage est activée.
        public bool estEnRage()
        {
            return raged;
        }

        // AUTHOR : Julien D'aboville 2024, group : L3L1
        //Vérifie si la compétence "Hurlement" du personnage est activée.
        public bool isHowling()
        {
            return howl;
        }



        // AUTHOR : Julien D'aboville 2024, group : L3L1
        //permet de changer la valeur de la variable booléenne shield
        public void setShield(bool shield)
        {
            if (shield && this.shield == false)                                                        //christophe3/24
            {
                Debug.Log("avant");
                GameObject childObject = MainGame.EFFETS[0];
                Debug.Log("apres");
                GameObject.Instantiate(childObject, this.go.transform);
                childObject.transform.SetParent(this.go.transform);
            } else if (!shield && this.shield){ // Doralie Dorcal L3Q1 2025
                Debug.Log("Désactivation du shield");
                foreach (Transform child in this.go.transform)
                {
                    if (child.name.StartsWith("ForceField"))
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }
            }
            this.shield = shield;
        }

        /*public void shieldDesactivate()
        {
            if (this.getShield())
            {
                this.setShield(false);
                if (this.go != null)
                {

                    Transform parentTransform = this.go.transform;

                    foreach (Transform childTransform in parentTransform)
                    {

                        if (childTransform.name.Split("(")[0] == "ForceField")
                        {
                            GameObject.Destroy(childTransform.gameObject);
                        }
                    }
                }

            }
        }*/

        // AUTHOR : Julien D'aboville 2024, group : L3L1
        //permet de vérifier si le shield est activé
        public bool getShield()
        {
            return shield;

        }


        // Author: CROUZET Oriane, group : L3C1
        // Date : 15/03/2023
        //Set the state of freezed
        public void setFreezed(bool freezed)
        {
            this.freezed = freezed;
        }

        //Set the color of the characters depending on the team
        //Author : ?
        public void setColorByTeam()
        {

            switch (team)
            {
                case 0: this.go.transform.GetChild(0).GetComponent<Renderer>().material.color = TEAM_1_COLOR; break;
                case 1: this.go.transform.GetChild(0).GetComponent<Renderer>().material.color = TEAM_2_COLOR; break;
                default: break;
            }
            GameObject.Instantiate(characterTemplateModels[(int)this.charClass], go.transform);
        }

      


        //Set the direction of the character
        //Author : ?
        public void setDirection(HexaDirection newDirection)
        {
            this.directionFacing = newDirection;
            Transform charModel = this.go.transform.GetChild(1);
            if (charModel) charModel.eulerAngles = new Vector3(0, (int)newDirection * 60, 0);
        }

        //Returns the name of the class of the character
        //Author : ?
        public string getName()
        {
            switch (this.charClass)
            {
                case CharClass.GUERRIER: return "Guerrier";
                case CharClass.VOLEUR: return "Voleur";
                case CharClass.ARCHER: return "Archer";
                case CharClass.MAGE: return "Mage";
                case CharClass.SOIGNEUR: return "Soigneur";
                case CharClass.ENVOUTEUR: return "Envouteur";
                case CharClass.VALKYRIE: return "Valkyrie";
                case CharClass.DRUIDE: return "Druide";
                case CharClass.FORGERON: return "Forgeron";
                case CharClass.NETHERFANG: return "Netherfang";
                case CharClass.LIFEWEAVER: return "Lifeweaver";
                case CharClass.AMAZONE : return "Amazone";
                case CharClass.BASTION : return "Bastion";
                case CharClass.MORTIFERE : return "Mortifere";
                case CharClass.GEOMAGE : return "Geomage";
                default: return "None";
            }
        }

        //Returns the data of the class of the character
        //Author : ?
        public CharsDB.CharacterDB getClassData()
        {
            return CharsDB.list[(int)charClass];
        }
    }

}
