using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters;
using CharactersCPU;
using static UtilCPU.UtilCPU;
using static MainGame;
using AI_Class;
using AI_Util;
using Hexas;
using Misc;
using System;

//Edited by Julien D'aboville L3L1 2024
//Edited by Mariana Duarte L3Q1 2025

namespace ScriptsCPU
{
    public class ScriptsCPU
    {
        /// <summary>
        /// Makes all the characters wait on their turn.
        /// </summary>
        /// <param name="currentTeam">The current team in play</param>
        /// <param name="hexas">The game board</param>
        /// <returns>A list of ActionAIPos turn skips</returns>
        public static List<(Character, ActionAIPos)> prone(int currentTeam, HexaGrid hexas)
        {
            Debug.Log("Script called : PRONE");

            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();

            foreach (CharacterCPU c in teamList)
                actions.Add(c.wait());

            return actions;
        }

        /// <summary>
        /// Makes the CPU team gang up on one character.
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns>A (Character, ActionAIPos) list describing the actions to take</returns>
        public static List<(Character, ActionAIPos)> offense(int currentTeam, HexaGrid hexas, List<CharacterCPU> enemyList)
        {
            Debug.Log("Script called : OFFENSE");

            List<CharacterCPU> teamList = order(getTeamList(currentTeam, hexas),
                CharClass.SOIGNEUR, CharClass.ENVOUTEUR);

            List<(Character chr, ActionAIPos act)> actions = new List<(Character, ActionAIPos)>();
            CharacterCPU victim = null;

            foreach (CharacterCPU c in teamList)
                for (int countPA = 0; countPA < c.Character.getPa(); countPA++)
                {
                    // When all enemies are defeated
                    if (enemyList.Count < 1)
                        actions.Add(c.wait());

                    else
                    {
                        if (victim == null)
                            victim = enemyList[0];

                        switch (c.Character.charClass)
                        {
                            case CharClass.SOIGNEUR:
                                // Heals the ally with the least HP
                                HealerCPU h = (HealerCPU)c;
                                actions.Add(h.findHealingPriority(teamList));
                                break;
                            case CharClass.ENVOUTEUR:
                                // Targets allies in this order : archers > thieves > mages > warriors
                                SorcererCPU s = (SorcererCPU)c;
                                actions.Add(s.findBuffingPriority(teamList, actions));
                                break;
                            default:
                                // Plainly targets the current appointed unit to focus on
                                actions.Add(c.target(victim));

                                if (victim.HP <= 0)
                                {
                                    enemyList.Remove(victim);
                                    victim = null;
                                }

                                break;
                        }
                    }

                    // If the current character skipped this turn, close this for loop 
                    if (actions[actions.Count - 1].act.action == ActionType.SKIP)
                        countPA = c.Character.getPa();
                }

            return actions;
        }

        /// <summary>
        /// Gets the unit with the least HP to move towards the healer, then gets them healed.
        /// </summary>
        /// <param name="currentTeam">The team currently in control</param>
        /// <param name="hexas">The game board</param>
        /// <returns>A (Character, ActionAIPos) list describing the actions to take</returns>
        public static List<(Character, ActionAIPos)> patchUpUnit(int currentTeam, HexaGrid hexas, List<CharacterCPU> enemyList)
        {
            Debug.Log("Script called : PATCHUPUNIT");

            List<CharacterCPU> teamList = getHealingFocusedList(currentTeam, hexas);

            List<(Character chr, ActionAIPos act)> actions = new List<(Character, ActionAIPos)>();
            CharacterCPU victim = null;

            foreach (CharacterCPU c in teamList)
                for (int countPA = 0; countPA < c.Character.getPa(); countPA++)
                {
                    // When all enemies are defeated
                    if (enemyList.Count < 1)
                        actions.Add(c.wait());

                    else
                    {
                        if (victim == null)
                            victim = enemyList[0];

                        // Team member to focus healing on
                        if (teamList.IndexOf(c) == 0 && gotHealerInTeam(currentTeam) && teamList.Count > 1)
                        {
                            // Can this unit get an attack in before needing to walk to the healer?
                            if (hexas.getWalkingDistance(c.X, c.Y, teamList[1].X, teamList[1].Y) <
                                (c.Character.getPa() - countPA - 1) * c.Character.getClassData().basePM
                                + teamList[1].Character.getClassData().basicAttack.range &&
                                (c.SkillAvailable ? isInRangeToUseSkill(c, victim) : isInRangeToAttack(c, victim)))
                            {
                                actions.Add(c.target(victim));

                                if (victim.HP <= 0)
                                {
                                    enemyList.Remove(victim);
                                    victim = null;
                                }
                            }
                            else
                                actions.Add(c.moveTowards(teamList[1]));
                        }
                        else switch (c.Character.charClass)
                            {
                                case CharClass.SOIGNEUR:
                                    // Heals the ally in the first slot of teamList
                                    actions.Add(c.target(teamList[0]));
                                    break;
                                case CharClass.ENVOUTEUR:
                                    // Targets allies in this order : archers > thieves > mages > warriors
                                    SorcererCPU s = (SorcererCPU)c;
                                    actions.Add(s.findBuffingPriority(teamList, actions));
                                    break;
                                default:
                                    // Plainly targets the current appointed unit to focus on
                                    actions.Add(c.target(victim));

                                    if (victim.HP <= 0)
                                    {
                                        enemyList.Remove(victim);
                                        victim = null;
                                    }

                                    break;
                            }
                    }

                    // If the current character skipped this turn, close this for loop 
                    if (actions[actions.Count - 1].act.action == ActionType.SKIP)
                        countPA = c.Character.getPa();
                }

            return actions;
        }

        /// <summary>
        /// The team called moves as one towards the enemy.
        /// </summary>
        /// <param name="currentTeam">The team currently in control</param>
        /// <param name="hexas">The game board</param>
        /// <returns>A (Character, ActionAIPos) list describing the actions to take</returns>
        public static List<(Character, ActionAIPos)> formation(int currentTeam, HexaGrid hexas)
        {
            Debug.Log("Script called : FORMATION");

            List<CharacterCPU> teamList = order(getTeamList(currentTeam, hexas),
                CharClass.GUERRIER, CharClass.VALKYRIE, CharClass.VOLEUR, CharClass.MAGE, CharClass.ARCHER);
            List<(Character chr, ActionAIPos act)> actions = new List<(Character, ActionAIPos)>();

            CharacterCPU target = getTeamList(currentTeam == 0 ? 1 : 0, hexas)[0];
            CharacterCPU leader = teamList[0];

            foreach (CharacterCPU c in teamList)
                for (int countPA = 0; countPA < c.Character.getPa(); countPA++)
                {
                    // Leader call
                    if (c == leader)
                    {
                        // Limiting leader movement to 2 PA so leading thieves don't lose the other members
                        if (countPA >= 2)
                            actions.Add(c.wait());
                        else
                            actions.Add(c.moveTowards(target));
                    }
                    // Follower call
                    else
                        actions.Add(c.moveTowards(getTacticalFormation(target, leader, c.Character.getCharClass(), hexas)));

                    // If the current character skipped this turn, close this for loop 
                    if (actions[actions.Count - 1].act.action == ActionType.SKIP)
                        countPA = c.Character.getPa();
                }


            return actions;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//*//
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Heal the closest ally wich health point aren't full
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        /// Edited by Julien D'aboville L3L1 2024
        public static List<(Character, ActionAIPos)> heal(CharacterCPU healer, int currentTeam, HexaGrid hexas)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            CharacterCPU targetAlly;

            //Debug.Log("HEALER PLAYING");

            Character allyInPrio = identifyAllyClassPriority(currentTeam, hexas);

            if (allyInNeed(currentTeam, hexas))
            {
                // If an ally is in need
                Debug.Log("On est ici ");

                if (healer.Character.isSkill1Up() && canAOEHeal(currentTeam, hexas, healer.Character))
                {
                    Debug.Log("HEALER SKILL AVAILABLE ");

                    Debug.Log(", on est là");
                    // If spell is ready, and find a good way to use it
                    Point p = bestPointForAOEHeal(currentTeam, hexas, healer.Character);
                    ActionAIPos ciblage = new ActionAIPos(ActionType.ATK2, new Point(p.getX(), p.getY()));
                    actions.Add((healer.Character, ciblage));
                }
                //Added by Julien D'aboville L3L1 2024
                else if (healer.Character.isSkill3Up() && isInRangeToUseSkill3(healer.Character, allyInPrio))
                {
                    Debug.Log("HEALER SKILL AVAILABLE 3 AND isInRangeToUseSkill3 ");

                    ActionAIPos ciblage = new ActionAIPos(ActionType.ATK4, new Point(allyInPrio.getX(), allyInPrio.getY()));
                    actions.Add((healer.Character, ciblage));
                }


                // Choosing target to heal/follow
                targetAlly = choosingAlly(allyRankedByMissingHP(currentTeam, hexas), currentTeam, hexas, healer.Character);
                Debug.Log("heal =>" + targetAlly.Character.charClass);
                actions.Add(healer.target(targetAlly));
            }

            else if (allTeamFullLife(currentTeam, hexas) && !atLeastOnInRange(currentTeam, hexas))
            {
                // If no one is in need, and no one in range, get closer to an ally
                Debug.Log("first elif");
                actions.Add(healer.moveTowards(allyInPrio));
                return actions;
            }
            else if (!allyInNeed(currentTeam, hexas) && moveTowardsWithoutRisk(currentTeam, hexas, allyInPrio) && AIUtil.hexaGrid.getWalkingDistance(healer.Character.getX(), healer.Character.getY(), allyInPrio.getX(), allyInPrio.getY()) > 2)
            {
                Debug.Log("second elif");
                actions.Add(healer.moveTowards(allyInPrio));
                return actions;
            }

            else actions.Add(healer.wait());
            Debug.Log(" else");
            return actions;
        }


        /// <summary>
        /// Boost the closest ally 
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        /// Edited by Julien D'aboville 2024 L3L1

        public static List<(Character, ActionAIPos)> envouteur(CharacterCPU envouteur, int currentTeam, HexaGrid hexas)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            //Character toBoost = null;




            //Added by Julien D'aboville 2024 L3L1
            if (envouteur.Character.isSkill3Up())
            {
                List<Character> enemyTeam = getEnemyCharacterTeamList(currentTeam, hexas);
                Character character = null;
                foreach (Character cEnemy in enemyTeam)
                {
                    if (cEnemy.getName() != "Soigneur" && cEnemy.getName() != "Envouteur")
                    {
                        if (isInRangeToUseSkill3(envouteur.Character, cEnemy))
                        {
                            character = cEnemy;

                            ActionAIPos to_spell = new ActionAIPos(ActionType.ATK4, new Point(cEnemy.getX(), cEnemy.getY()));
                            actions.Add((envouteur.Character, to_spell));
                            Debug.Log("Cpu --> Utilisation compétence 3 envoûteur");
                            return actions;

                        }
                    }

                }


            }
            if (envouteur.Character.isSkill1Up() && atLeastOnInRange(currentTeam, hexas))
            {
                // If the spell is ready, find the best way to use it
                Debug.Log("AOE BOOST ");
                Point p = bestPointForAOEBoost(currentTeam, hexas, envouteur.Character);
                ActionAIPos ciblage = new ActionAIPos(ActionType.ATK2, new Point(p.getX(), p.getY()));
                actions.Add((envouteur.Character, ciblage));
                Debug.Log("Cpu --> Utilisation compétence 1 envoûteur");

                return actions;
            }






            if (envouteur.Character.isSkill2Up())
            {
                List<Character> enemyTeam = getEnemyCharacterTeamList(currentTeam, hexas);
                Character lessImportantCharacterToSpelled = null;
                foreach (Character cEnemy in enemyTeam)
                {
                    if (cEnemy.getName() != "Soigneur" && cEnemy.getName() != "Envouteur")
                    {
                        if (isInRangeToUseSkill2(envouteur.Character, cEnemy))
                        {
                            lessImportantCharacterToSpelled = cEnemy;
                            if (cEnemy.getName() == "Guerrier" || cEnemy.getName() == "Valkyrie")
                            {
                                ActionAIPos to_spell = new ActionAIPos(ActionType.ATK3, new Point(cEnemy.getX(), cEnemy.getY()));
                                actions.Add((envouteur.Character, to_spell));
                                Debug.Log("Cpu --> Utilisation compétence 2 envoûteur");
                                return actions;
                            }
                        }
                    }

                }
                if (lessImportantCharacterToSpelled != null)
                {
                    ActionAIPos to_spell = new ActionAIPos(ActionType.ATK3, new Point(lessImportantCharacterToSpelled.getX(), lessImportantCharacterToSpelled.getY()));
                    actions.Add((envouteur.Character, to_spell));
                    Debug.Log("Cpu --> Utilisation compétence 2 envoûteur");
                    return actions;
                }

            }
            if (atLeastOnInRange(currentTeam, hexas))
            {
                Debug.Log("Cpu --> Utilisation target envoûteur");

                // If an ally is in range, boost the best ally possible
                actions.Add(envouteur.target(allyToBoost(currentTeam, hexas)));
                return actions;
            }
            if (!atLeastOnInRange(currentTeam, hexas))
            {
                Debug.Log("Cpu --> Utilisation movetowards  envoûteur");

                // If no ally is in range, get closer to an one
                actions.Add(envouteur.moveTowards(identifyAllyClassPriority(currentTeam, hexas)));
                return actions;
            }

            return actions;
        }

        /// <summary>
        /// The currentChar will flee the enemy, and play around his healer
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        public static List<(Character, ActionAIPos)> fleeWithHealer(CharacterCPU currentChar, HexaGrid hexas)
        {
            Debug.Log("Flee With Healer appelé");
            Character character = currentChar.Character;
            CharClass charClass = character.charClass;
            int team = character.team;
            Point pos = new Point(character.getX(), character.getY());

            // If the currentChar is a healer AND someone is in need, the healer won't flee
            if (allyInNeed(team, hexas))
            {
                if (charClass == CharClass.SOIGNEUR)
                    return heal(currentChar, team, hexas);
                if (charClass == CharClass.LIFEWEAVER)
                    return lifeWeaver(currentChar, team, hexas);
            }

            // If the currentChar is a healer AND no one is in need, the healer will flee
            else if (!allyInNeed(team, hexas))
            {
                if (charClass == CharClass.SOIGNEUR || charClass == CharClass.LIFEWEAVER)
                    return flee(currentChar, hexas);
            }

            // If an ally is already fleeing, the currentChar won't flee
            if (alreadyAtRisk(team, hexas, currentChar.Character))
                return attack(team, hexas);

            List<Character> teamList = getCharacterTeamList(currentChar.Character.team, hexas);
            List<ActionAIPos> mouvement = new List<ActionAIPos>();
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Hexa goTo;
            Point healerPos = null;
            List<Point> healers = new List<Point>();

            // Find healer position
            foreach (Character c in teamList)
                if (c.charClass == CharClass.SOIGNEUR || c.charClass == CharClass.LIFEWEAVER)
                {
                    healerPos = new Point(c.getX(), c.getY());
                    healers.Add(new Point(c.getX(), c.getY()));
                };
            Point closestHealer = healers[0];
            foreach(Point p in healers)
                if (hexas.getDistance(pos, p) < hexas.getDistance(pos, closestHealer))
                    closestHealer = p;

            // For every point around the healer, we check if this point, is not on enemy range
            foreach (Point p in pointsAroundHexa(closestHealer, 3))
                if (!isPointInEnemyTeamRangeAttack(p, currentChar.Character.team))
                {
                    goTo = hexas.getHexa(p.getX(), p.getY());
                    if (goTo != null && (goTo.type == HexaType.GROUND || goTo.type == HexaType.BONUS || goTo.type == HexaType.BUSH || goTo.type == HexaType.PORTAL) && goTo.charOn == null)
                    {
                        mouvement.AddRange(AIUtil.findSequencePathToHexa(character, goTo.getX(), goTo.getY()));
                        actions.Add((character, mouvement[0]));
                        return actions;
                    }
                }

            // The currentChar couldn't find a good way to flee, he will attack
            Debug.Log("Can't flee");
            return attack(team, hexas);
        }

        /// <summary>
        /// The currentChar will flee the enemy, 
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        public static List<(Character, ActionAIPos)> flee(CharacterCPU currentChar, HexaGrid hexas)
        {
            Character character = currentChar.Character;
            CharClass charClass = character.charClass;
            int team = character.team;
            int x = character.getX();
            int y = character.getY();


            if (charClass == CharClass.SOIGNEUR)
                return heal(currentChar, team, hexas);

            List<ActionAIPos> mouvement = new List<ActionAIPos>();
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Hexa goTo;

            // For every point around the currentChar, we check if this point, is not on enemy range
            foreach (Point p in hexas.findAllPaths(x, y, 3))
                if (!isPointInEnemyTeamRangeAttack(p, team))
                {
                    Debug.Log("hehe ici on flee 1");
                    goTo = hexas.getHexa(p.getX(), p.getY());
                    if (goTo != null && (goTo.type == HexaType.GROUND || goTo.type == HexaType.BONUS || goTo.type == HexaType.BUSH || goTo.type == HexaType.PORTAL) && goTo.charOn == null)
                    {
                        mouvement.AddRange(AIUtil.findSequencePathToHexa(character, goTo.getX(), goTo.getY()));
                        actions.Add((character, mouvement[0]));
                        Debug.Log("hehe ici on flee 2");
                        return actions;
                    }
                }

            // The currentChar couldn't find a good way to flee, he will attack
            Debug.Log("Can't flee");
            return attack(currentChar.Character.team, hexas);
        }



        //edited by GOUVEIA Klaus, group: L3Q1
        //Author : ?
        /// <summary>
        /// Makes the CPU team gang up on one character.
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        public static List<(Character, ActionAIPos)> attack(int currentTeam, HexaGrid hexas)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;

            foreach (CharacterCPU c in teamList)
                for (int countPA = 0; countPA < c.Character.getPa(); countPA++)
                {
                    //victim = enemyInMostAllyRange(currentTeam, c.Character, hexas);
                    victim = easierTargetToKill(currentTeam, c.Character, hexas);

                    // If the character canno't attack this turn, he won't put himself at risk
                    if (c.Character.getPA() == 1 && getEnemyTargetsAvailable(c.Character).Count == 0 && !moveWithoutRisk(currentTeam, hexas, c.Character))
                        actions.Add(c.wait());
                    else
                        actions.Add(c.target(victim));

                }


            return actions;
        }




        /// <summary>
        /// Druide decision
        /// Added by Socrate Louis Deriza
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        /// Edited By Julien D'aboville L3L1 2024
        public static List<(Character, ActionAIPos)> druide(CharacterCPU druide, int currentTeam, HexaGrid hexas)
        {
            List<Character> enemyTeam = getEnemyCharacterTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            
            Debug.Log("DRUIDE actions PA--> " + druide.Character.getPa());
            for (int countPA = 0; countPA < druide.Character.getPa(); countPA++)
            {
          //      bool cantAttack = false;
                /*
                Added By Julien D'aboville L3L1 2024
                bool cantAttack = false;
                if (druide.Character.isSkill3Up() == true)
                {
                    if (cantAttack == false)
                    {
                        cantAttack = true;
                    }
                    actions.Add((druide.Character, new ActionAIPos(ActionType.ATK4, new Point(druide.Character.getX(), druide.Character.getY()))));
                    druide.Character.setSkillAvailable3(false);
                    countPA++;
                }
                if(countPA>= druide.Character.getPa())
                {
                    return actions;
                }
                */




                foreach (Character cEnemy in enemyTeam)
                {

                    Debug.Log("---" + cEnemy.getName() + "----");

                    //Added By Julien D'aboville L3L1 2024
                    if (druide.Character.isSkill3Up() == true)
                    {
                     /*   if (cantAttack == false)
                        {
                            cantAttack = true;
                        }*/
                        if (druide.Character.getSkill3AoERange() >= hexas.getDistance(cEnemy.getX(), cEnemy.getY(), druide.Character.getX(), druide.Character.getY()))
                        {
                            actions.Add((druide.Character, new ActionAIPos(ActionType.ATK4, new Point(druide.Character.getX(), druide.Character.getY()))));
                       //     druide.Character.setSkillAvailable3(false);
                            Debug.Log("Skill3");
                            //return actions;
                            break;


                        }


                    }


                    else if (druide.Character.isSkill1Up() == true)
                    {
                        Debug.Log("available");
                        /*   if (cantAttack == false)
                           {
                               cantAttack = true;
                           }*/
                        if (isInRangeToUseSkill(druide.Character, cEnemy))
                        {
                            actions.Add((druide.Character, new ActionAIPos(ActionType.ATK2, new Point(cEnemy.getX(), cEnemy.getY()))));

                            Debug.Log("Skill1");
                            //return actions;
                            break;



                        }
                    }
                    else if (druide.Character.isSkill2Up())
                    {
                        Debug.Log("available2");
                        /*   if (cantAttack == false)
                           {
                               cantAttack = true;
                           }*/
                        if (isInRangeToUseSkill2(druide.Character, cEnemy))
                        {
                            actions.Add((druide.Character, new ActionAIPos(ActionType.ATK3, new Point(cEnemy.getX(), cEnemy.getY()))));
                            Debug.Log("Skill2");
                            //return actions;
                            break;
                        }
                        else
                        {
                            if (druide.Character.isSkill1Up() == false)
                            {
                                Debug.Log("moove Druide/skillAvaible == false");
                                actions.Add(druide.target(cEnemy));
                                break;
                            }
                        }
                    }


                    if (isInRangeToAttack(druide.Character, cEnemy))
                    {
                        /*
                        if (cantAttack == false)
                        {
                            cantAttack = true;
                        }*/
                        actions.Add((druide.Character, new ActionAIPos(ActionType.ATK1, new Point(cEnemy.getX(), cEnemy.getY()))));
                        Debug.Log("attack1");
                        //return actions;
                        break;
                    }
                    else 
                    {
                        Debug.Log("moove Druide");
                        actions.Add(druide.target(cEnemy));
                        Debug.Log("fin itération first for");
                        //return actions;
                        break;
                    }

                    /*
                    else
                    {
                        Debug.Log("isRnageAttack :"+isInRangeToAttack(druide.Character, cEnemy));
                        actions.Add((druide.Character, new ActionAIPos(ActionType.SKIP, null)));
                        Debug.Log("Erreur Druide");
                        break;
                    }*/
                    


                }

            }
            Debug.Log("DRUIDE actions --> " + actions.Count);
            /*           Debug.Log("DRUIDE actions --> " + actions[0].GetType());
                       Debug.Log("DRUIDE actions --> " + actions[0]);*/
            //            Debug.Log("DRUIDE actions --> " + actions[1]);



            return actions;
        }

        /*
        List<Character> enemyTeam = getEnemyCharacterTeamList(currentTeam, hexas);

        foreach (cEnemy in enemyTeam)
        {
            if (druide.Character.isSkill1Up())
            {
                if (isInRangeToUseSkill())
                {
                    return
                }
            }
            if (druide.Character.isSkill1Up())
            {

            }
        }
        */

        /// <summary>
        /// Mage decision
        /// Edited by Socrate Louis Deriza
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        /// Edited By Julien D'aboville 2024 L3L1
        public static List<(Character, ActionAIPos)> mage(CharacterCPU mage, int currentTeam, HexaGrid hexas)
        {
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<CharacterCPU> ennemyList = getEnemyTeamList(currentTeam, hexas);


            Character victim = null;

            Debug.Log("MAGE actions START PA--> " + mage.Character.getPa());
            Debug.Log("MAGE START skillAvailble3 cooldown--> " + mage.Character.attack3coolDownValue);
            Debug.Log("MAGE START skillAvailble2 cooldown--> " + mage.Character.attack2coolDownValue);
            Debug.Log("MAGE START skillAvailble1 cooldown--> " + mage.Character.attack1coolDownValue);





            //Debug.Log("MAGE PLAYING");
            // If the mage spell is ready, he check if can find  agood way to use it, if yes he'll use it
            if (mage.Character.isSkill1Up() && canAOEDamage(currentTeam, hexas, mage.Character.getX(), mage.Character.getY(), 5, 2) /*&& !betterAOEDamage(currentTeam, hexas, mage.Character.getX(), mage.Character.getY(), 3)*/)
            {
                Debug.Log("MAGE canAOEDamage--> ");

                //Debug.Log("AOE DAMAGE ");
                Point p = bestPointForAOEDamage(currentTeam, hexas, mage.Character.getX(), mage.Character.getY(), 5, 2);
                ActionAIPos ciblage = new ActionAIPos(ActionType.ATK2, new Point(p.getX(), p.getY()));
                actions.Add((mage.Character, ciblage));
                Debug.Log("MAGE actions --> " + actions.Count);
                //    mage.Character.setSkillAvailable1(false);
                //                actions.Add((mage.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));

                //            return actions;
            }


            //Debug.Log("MAGE PLAYING");
            // If the mage spell is ready, he check if can find  agood way to use it, if yes he'll use it
            if (mage.Character.isSkill2Up() && canAOEDamage(currentTeam, hexas, mage.Character.getX(), mage.Character.getY(), 5, 2) /*&& !betterAOEDamage(currentTeam, hexas, mage.Character.getX(), mage.Character.getY(), 3)*/)
            {
                Debug.Log("MAGE canAOEDamage--> ");

                //Debug.Log("Poison DAMAGE ");
                Point p = bestPointForAOEDamage(currentTeam, hexas, mage.Character.getX(), mage.Character.getY(), 5, 2);
                ActionAIPos ciblage = new ActionAIPos(ActionType.ATK3, new Point(p.getX(), p.getY()));
                actions.Add((mage.Character, ciblage));
                Debug.Log("MAGE actions --> " + actions.Count);
                //     mage.Character.setSkillAvailable2(false);
                //           actions.Add((mage.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));


                //          return actions;
            }
            int init = actions.Count;
            for (int countPA = init; countPA < mage.Character.getPa(); countPA++)
            {
                //victim = enemyInMostAllyRange(currentTeam, c.Character, hexas);
                victim = easierTargetToKill(currentTeam, mage.Character, hexas);
                HexaGrid h = new HexaGrid();

                Debug.Log("nombre de pas entre mage et la victime :" + h.getDistance(mage.Character.getX(), mage.Character.getY(), victim.getX(), victim.getY()));
                int distanceEntreMageVictime = h.getDistance(mage.Character.getX(), mage.Character.getY(), victim.getX(), victim.getY());

                if (distanceEntreMageVictime <= 10 && mage.Character.isSkill3Up())
                {

                    Debug.Log("MAGE skillAvailble3 cooldown--> " + mage.Character.attack3coolDownValue);

                    actions.Add((mage.Character, new ActionAIPos(ActionType.ATK4, new Point(mage.Character.getX(), mage.Character.getY()))));
                    mage.Character.setSkillAvailable3(false);


                }

                // If the character canno't attack this turn, he won't put himself at risk
                else if (mage.Character.getPA() == 1 && getEnemyTargetsAvailable(mage.Character).Count == 0 && !moveWithoutRisk(currentTeam, hexas, mage.Character))
                {
                    Debug.Log("MAGE wait");

                    actions.Add(mage.wait());
                }
                else if (isInRangeToAttack(mage.Character, victim))
                {
                    Debug.Log("MAGE rangeToAttack");
                    actions.Add((mage.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));
                }
                else
                {
                    Debug.Log("Move Towards");

                    actions.Add(mage.moveTowards(victim));//RUN vers l'adversaire 
                }

            }
            Debug.Log("MAGE actions --> " + actions.Count);

            return actions;
        }



        //edited by GOUVEIA Klaus, group: L3Q1
        //Author : ?
        /// <summary>
        /// Rogue decision
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        public static List<(Character, ActionAIPos)> voleur(CharacterCPU voleur, int currentTeam, HexaGrid hexas, Hexa caseBonus)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;
            List<Character> enemysList = getEnemyTargetsAvailable(voleur.Character);
            Debug.Log("Enemy Tragets Available :" + enemysList.Count);
            List<Character> currentCharacter = new List<Character>();
            currentCharacter.Add(voleur.Character);

            Point centreBonus = new Point(caseBonus.getX(), caseBonus.getY());

            for (int countPA = 0; countPA < voleur.Character.getPa(); countPA++)
            {

                victim = null;

                foreach (Character c in enemysList)
                {
                    if (c.getCharClass() == CharClass.SOIGNEUR)
                    {
                        if (isInTotalRange(voleur.Character, c))
                        {
                            victim = c;
                        }
                    }
                }
                if (victim != null)
                {
                    Debug.Log("Voleur Target Soigneur");
                    actions.Add(voleur.target(victim));
                }


                else if (enemysList.Count != 0 && countPA == 0 && moveWithoutRisk(currentTeam, hexas, voleur.Character) && actions.Count == 0)
                {
                    Debug.Log("Voleur PA = 0");
                    if (canGroupKill(currentCharacter, enemysList[0]))
                    {
                        Debug.Log("Voleur Kill Ennemy");
                        actions.Add(voleur.target(enemysList[0]));
                        actions.Add(voleur.target(enemysList[0]));
                        actions.Add(voleur.target(enemysList[0]));
                        return actions;
                    }

                    else if (isInTotalRange(voleur.Character, enemysList[0]))
                    {

                        actions.Add(voleur.target(enemysList[0]));
                        actions.Add(voleur.target(enemysList[0]));
                        Debug.Log("Voleur Hit and Flee 1");
                        actions.Add(flee(voleur, hexas)[0]);
                        Debug.Log("Fin Voleur Hit and Flee 1");
                        return actions;
                    }
                    else
                    {

                        actions.Add(voleur.target(enemysList[0]));
                        Debug.Log("Voleur Hit and Flee 2 f");
                        actions.Add(flee(voleur, hexas)[0]);
                        Debug.Log("Fin Voleur Hit and Flee 2--> first  flee");
                        Debug.Log("Voleur Hit and Flee 2 s");
                        actions.Add(flee(voleur, hexas)[0]);
                        Debug.Log("Fin Voleur Hit and Flee 2--> second flee");
                        return actions;
                    }
                }

                else if (enemysList.Count != 0 && countPA == 1 && moveWithoutRisk(currentTeam, hexas, voleur.Character) && actions.Count == 1)
                {
                    Debug.Log("Voleur PA = 1");
                    if (canGroupKill(currentCharacter, enemysList[0]))
                    {
                        Debug.Log("Voleur Kill Ennemy");
                        actions.Add(voleur.target(enemysList[0]));
                        actions.Add(voleur.target(enemysList[0]));
                        return actions;
                    }
                    else
                    {
                        Debug.Log("Voleur Hit and Flee3");
                        actions.Add(voleur.target(enemysList[0]));
                        actions.Add(flee(voleur, hexas)[0]);
                        Debug.Log("Fin Voleur Hit and Flee3");
                        return actions;
                    }
                }

                else if (getBonusTeam() != currentTeam && hexas.getHexa(voleur.X, voleur.Y).type == HexaType.BONUS)
                {
                    Debug.Log("Voleur Attack Zone");
                    victim = closestEnemy(currentTeam, voleur.Character, hexas);
                    actions.Add(voleur.target(victim));
                }

                else if (getBonusTeam() != currentTeam)
                {
                    Debug.Log("Voleur Move Towards Zone");
                    Point bonus = new Point(caseBonus.getX(), caseBonus.getY());
                    actions.Add(voleur.moveTowards(bonus));
                }


                else
                {
                    //if (notToFarAway(currentTeam, hexas, voleur.Character) && moveWithoutRisk(currentTeam, hexas, voleur.Character)){
                    victim = easierTargetToKill(currentTeam, voleur.Character, hexas);
                    if (getEnemyTargetsAvailable(voleur.Character).Count == 0 && !moveWithoutRisk(currentTeam, hexas, voleur.Character))
                    {
                        actions.Add(voleur.wait());
                        Debug.Log("Voleur Wait");
                    }
                    else
                    {
                        Debug.Log("Voleur Target Enemy");
                        actions.Add(voleur.target(victim));
                    }
                }
                //if (notToFarAway(currentTeam, hexas, voleur.Character) && moveWithoutRisk(currentTeam, hexas, voleur.Character))
                //{
                //    //victim = enemyInMostAllyRange(currentTeam, voleur.Character, hexas);
                //    victim = easierTargetToKill(currentTeam, voleur.Character, hexas);
                //    actions.Add(voleur.target(victim));
                //}
                //else
                //    actions.Add(voleur.wait());
                //***
            }
            HexaGrid h = new HexaGrid();

            Debug.Log("nb actions du voleur -> " + actions.Count);
            //       Debug.Log("nombre de pas entre guerrier et la victime :" + h.getDistance(voleur.Character.getX(), voleur.Character.getY(), victim.getX(), victim.getY()));
            //       int distanceEntreVoleurVictime = h.getDistance(voleur.Character.getX(), voleur.Character.getY(), victim.getX(), victim.getY());
            Debug.Log("voleur.Character.getPa() =" + voleur.Character.getPa());
            if (voleur.Character.getPa() <= 0)
            {
                Debug.Log("SKIP voleur.Character.getPa() =" + voleur.Character.getPa());

                List<(Character, ActionAIPos)> lo = new List<(Character, ActionAIPos)>();
                lo.Add(voleur.wait());
                return lo;
            }


            return actions;
        }


        //edited by GOUVEIA Klaus, group: L3Q1
        //Author : ?
        /// <summary>
        /// Archer behaviour
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        public static List<(Character, ActionAIPos)> archer(CharacterCPU archer, int currentTeam, HexaGrid hexas)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;
            List<Character> enemysList = getEnemyTargetsAvailable(archer.Character);
            Debug.Log("Enemy Tragets Available :" + enemysList.Count);

            for (int countPA = 0; countPA < archer.Character.getPa(); countPA++)
            {

                victim = easierTargetToKill(currentTeam, archer.Character, hexas);
                actions.Add(archer.target(victim));
                Debug.Log("Archer Attack2");

            }

            Debug.Log("Archer Actions => " + actions.Count);
            return actions;
        }


        //edited by GOUVEIA Klaus, group: L3Q1
        //Edited by Soocrate Louis Deriza L3C1
        //Author : Klaus GOUVEIA
        /// Edited By Julien D'aboville 2024 L3L1
        public static List<(Character, ActionAIPos)> valkyrie(CharacterCPU valkyrie, int currentTeam, HexaGrid hexas, Hexa caseBonus)
        {
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            Character victim = null;
            int theoretical_hp;
            theoretical_hp = -1;
            for (int countPA = 0; countPA < valkyrie.Character.getPa(); countPA++)
            {
                //victim = enemyInMostAllyRange(currentTeam, c.Character, hexas);
                victim = easierTargetToKill(currentTeam, valkyrie.Character, hexas);
                if (countPA == 0)
                {
                    theoretical_hp = victim.getHP();
                }
                // If the character canno't attack this turn, he won't put himself at risk
                if (valkyrie.Character.getPA() == 1 && getEnemyTargetsAvailable(valkyrie.Character).Count == 0 && !moveWithoutRisk(currentTeam, hexas, valkyrie.Character))
                {
                    actions.Add(valkyrie.wait());
                }
                else
                {
                    if (theoretical_hp <= 0)
                    {
                        actions.Add(valkyrie.wait());
                    }
                    else
                    {
                        actions.Add(valkyrie.target(victim));
                    }
                }


                if (actions[actions.Count - 1].Item2.action == ActionType.ATK1)
                {
                    theoretical_hp -= valkyrie.Character.getDamage();
                }
                if (actions[actions.Count - 1].Item2.action == ActionType.ATK2)
                {
                    theoretical_hp -= valkyrie.Character.getSkillDamage();
                }
                if (actions[actions.Count - 1].Item2.action == ActionType.ATK3)
                {
                    theoretical_hp -= 2 + ((valkyrie.Character.dmgbuff == true) ? 1 : 0);
                }

                if (actions[actions.Count - 1].Item2.action == ActionType.ATK4)
                {
                    theoretical_hp -= valkyrie.Character.getSkill3Damage()
;
                }



            }
            return actions;



        }

        /// <summary>
        /// Guerrier decision
        /// Edited Socrate Louis Deriza
        /// Edited by Julien D'aboville 2024 L3L1
        /// permet de choisir le coup à utiliser du guerrier
        /// </summary>
        /// <param name="currentChar">The character currently in control</param>
        /// <param name="charList">The list of all the characters on the board</param>
        /// <returns></returns>
        public static List<(Character, ActionAIPos)> guerrier(CharacterCPU guerrier, int currentTeam, HexaGrid hexas)
        {
           
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;
            int distanceEntreGuerrierVictime = 0;
            int distanceActivationRage = 5;

            for (int countPA = 0; countPA < guerrier.Character.getPa(); countPA++)
            {

                victim = easierTargetToKill(currentTeam, guerrier.Character, hexas);//victime à selectionner la plus facile à tuer
                Debug.Log("caractere guerrier PA :" + guerrier.Character.getPa() + " countPA :" + countPA);

                HexaGrid h = new HexaGrid();

                Debug.Log("nombre de pas entre guerrier et la victime :" + h.getDistance(guerrier.Character.getX(), guerrier.Character.getY(), victim.getX(), victim.getY()));
                distanceEntreGuerrierVictime = h.getDistance(guerrier.Character.getX(), guerrier.Character.getY(), victim.getX(), victim.getY());
                if (victim != null)
                {

                    if (guerrier.Character.isSkill1Up() && isInRangeToUseSkill(guerrier.Character, victim)) //  skillAvailable est pour attaque puissante( 5 de dégat)
                    {
                        actions.Add((guerrier.Character, new ActionAIPos(ActionType.ATK2, new Point(victim.getX(), victim.getY()))));
                        guerrier.Character.setSkillAvailable1(false);
                    }


                    else if (guerrier.Character.isSkill3Up() && distanceEntreGuerrierVictime <= distanceActivationRage)
                    {
                        Debug.Log("SKILL 3 GUERRIER SCRIPT CPU");
                        actions.Add((guerrier.Character, new ActionAIPos(ActionType.ATK4, new Point(guerrier.Character.getX(), guerrier.Character.getY())))); //sur lui meme
                        guerrier.Character.setSkillAvailable3(false);

                    }
                    // Si la victime est à portée du skill2
                    else if (guerrier.Character.isSkill2Up() && isInRangeToUseSkill2(guerrier.Character, victim))
                    {
                        actions.Add((guerrier.Character, new ActionAIPos(ActionType.ATK3, new Point(victim.getX(), victim.getY()))));
                        guerrier.Character.setSkillAvailable2(false);

                    }




                    else
                    {
                        if (isInRangeToAttack(guerrier.Character, victim))
                        {
                            actions.Add((guerrier.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));//attaque de base
                        }
                        else
                        {
                            actions.Add(guerrier.moveTowards(victim));//avance
                        }
                    }
                }
          
            }
            Debug.Log("guerrier Actions => " + actions.Count);
            Debug.Log("guerrier Action 0" + actions[0]);



            return actions;
        }



        //Added By Julien D'aboville 2024 L3L1
        /// <summary>
        /// permet de choisir le coup à utiliser du bûcheron
        /// </summary>
        /// <param name="forgeron"></param>
        /// <param name="currentTeam"></param>
        /// <param name="hexas"></param>
        /// <param name="caseBonus"></param>
        /// <returns></returns>

        public static List<(Character, ActionAIPos)> forgeron(CharacterCPU forgeron, int currentTeam, HexaGrid hexas, Hexa caseBonus)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<CharacterCPU> enemyList = getEnemyTeamList(currentTeam, hexas);

            Debug.Log("FORGERON enemyList :" + enemyList.Count);
            Debug.Log("FORGERON teamList :" + teamList.Count);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;

            Debug.Log("skill 3 cool down forgeron ->" + forgeron.Character.attack3coolDownValue);

            for (int countPA = 0; countPA < forgeron.Character.getPa(); countPA++)
            {

                victim = easierTargetToKill(currentTeam, forgeron.Character, hexas);//victime à selectionner la plus facile à tuer
                Debug.Log("caractere forgeron PA :" + forgeron.Character.getPa() + " countPA :" + countPA);
                Debug.Log("forgeron.Character.isSkill1Up() : " + forgeron.Character.isSkill1Up());
                Debug.Log("forgeron.Character.isSkill2Up() : " + forgeron.Character.isSkill2Up());
                Debug.Log("forgeron.Character.isSkill3Up() : " + forgeron.Character.isSkill3Up());
                if (victim != null)
                {

                    if (forgeron.Character.isSkill1Up()) //  skillAvailable est pour HOWL
                    {

                        actions.Add((forgeron.Character, new ActionAIPos(ActionType.ATK2, new Point(forgeron.Character.getX(), forgeron.Character.getY()))));
                        forgeron.Character.setSkillAvailable1(false);
                    }

                    // Si la victime est à portée de HAMMER
                    else if (forgeron.Character.isSkill2Up() && isInRangeToUseSkill2(forgeron.Character, victim))
                    {
                        actions.Add((forgeron.Character, new ActionAIPos(ActionType.ATK3, new Point(victim.getX(), victim.getY()))));
                        forgeron.Character.setSkillAvailable2(false);

                    }
                    else if (forgeron.Character.isSkill3Up() && isInRangeToUseSkill3(forgeron.Character, victim))
                    {
                        actions.Add((forgeron.Character, new ActionAIPos(ActionType.ATK4, new Point(victim.getX(), victim.getY()))));
                        forgeron.Character.setSkillAvailable3(false);

                    }
                    else
                    {
                        Debug.Log("Script FORGERON victime est invisible : " + victim.isInvisible());

                        if (isInRangeToAttack(forgeron.Character, victim) && !victim.isInvisible())
                        {
                            Debug.Log("forgeron.Character.attaquebasique: " + forgeron.Character.isSkill3Up());

                            actions.Add((forgeron.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));//attaque de base
                        }
                        else
                        {
                            Debug.Log("forgeron.Character.deplacement: " + forgeron.Character.isSkill3Up());

                            actions.Add(forgeron.moveTowards(victim));//avance
                        }
                    }
                }

            }
            Debug.Log("Forgeron Actions => " + actions.Count);
            //            Debug.Log("forgeron Action 0" + actions[0].GetType());
            if (actions.Count == 0)
            {
                List<(Character, ActionAIPos)> a = new List<(Character, ActionAIPos)>();
                a.Add((forgeron.Character, new ActionAIPos(ActionType.SKIP, null)));
                return a;
            }

            return actions;
        }



        //Added By Julien D'aboville 2024 L3L1
        /// <summary>
        /// permet de choisir le coup à utiliser du netherFang
        /// </summary>
        /// <param name="netherfang"></param>
        /// <param name="currentTeam"></param>
        /// <param name="hexas"></param>
        /// <param name="caseBonus"></param>
        /// <returns></returns>

        public static List<(Character, ActionAIPos)> netherFang(CharacterCPU netherfang, int currentTeam, HexaGrid hexas, Hexa caseBonus)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;

            Debug.Log("skill 3 cool down netherFang ->" + netherfang.Character.attack3coolDownValue);

            for (int countPA = 0; countPA < netherfang.Character.getPa(); countPA++)
            {

                victim = easierTargetToKill(currentTeam, netherfang.Character, hexas);//victime à selectionner la plus facile à tuer
                Debug.Log("caractere netherfang PA :" + netherfang.Character.getPa() + " countPA :" + countPA);
                Debug.Log("netherfang.Character.isSkill1Up() : " + netherfang.Character.isSkill1Up());
                Debug.Log("netherfang.Character.isSkill2Up() : " + netherfang.Character.isSkill2Up());
                Debug.Log("netherfang.Character.isSkill3Up() : " + netherfang.Character.isSkill3Up());
                if (victim != null)
                {

                    if (netherfang.Character.isSkill1Up()) //  skillAvailable est pour HOWL
                    {

                        actions.Add((netherfang.Character, new ActionAIPos(ActionType.ATK2, new Point(netherfang.Character.getX(), netherfang.Character.getY()))));
                        netherfang.Character.setSkillAvailable1(false);
                    }

                    // Si la victime est à portée de freeze
                    else if (netherfang.Character.isSkill2Up() && isInRangeToUseSkill2(netherfang.Character, victim))
                    {
                        actions.Add((netherfang.Character, new ActionAIPos(ActionType.ATK3, new Point(victim.getX(), victim.getY()))));
                        netherfang.Character.setSkillAvailable2(false);

                    }

                    //Saut explosion
                    else if (netherfang.Character.isSkill3Up() && netherfang.Character.getSkill3AoERange() >= hexas.getDistance(victim.getX(), victim.getY(), netherfang.Character.getX(), netherfang.Character.getY()))
                    {
                        Debug.Log("Saut explosion");
                        actions.Add((netherfang.Character, new ActionAIPos(ActionType.ATK4, new Point(netherfang.Character.getX(), netherfang.Character.getY()))));
                        netherfang.Character.setSkillAvailable3(false);

                    }
                    else
                    {
                        Debug.Log("Script de netherfang victime est invisible : " + victim.isInvisible());

                        if (isInRangeToAttack(netherfang.Character, victim) && !victim.isInvisible())
                        {
                           

                            actions.Add((netherfang.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));//attaque de base
                        }
                        else
                        {
                            Debug.Log("netherfang.Character.deplacement: " + netherfang.Character.isSkill3Up());

                            actions.Add(netherfang.moveTowards(victim));//avance
                        }
                    }
                }

                /*     
                     {
                         actions.Add(netherfang.waitOrMoveStrategically());
                     }*/
            }
            Debug.Log("netherfang Actions => " + actions.Count);
            Debug.Log("netherfang Action 0" + actions[0].GetType());
            if (actions.Count == 0)
            {
                List<(Character, ActionAIPos)> a = new List<(Character, ActionAIPos)>();
                a.Add((netherfang.Character, new ActionAIPos(ActionType.SKIP, null)));
                return a;
            }

            return actions;
        }








        //Added By Julien D'aboville 2024 L3L1
        /// <summary>
        /// permet de choisir le coup à utiliser du lifeWeaver
        /// </summary>
        /// <param name="lifeWeaver"></param>
        /// <param name="currentTeam"></param>
        /// <param name="hexas"></param>
        /// <returns></returns>
        public static List<(Character, ActionAIPos)> lifeWeaver(CharacterCPU lifeWeaver, int currentTeam, HexaGrid hexas)
        {
            int distanceSuperieurAllie = 4;
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<CharacterCPU> enemyList = getEnemyTeamList(currentTeam, hexas);

            Debug.Log("enemyList :" + enemyList.Count);
            Debug.Log("teamList :" + teamList.Count);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            CharacterCPU targetAlly;

            //Debug.Log("lifeWeaver PLAYING");
    
            List<int> distances = new List<int>();
            Character allyInPrio = identifyAllyClassPriority(currentTeam, hexas);

 
            Character victim = easierTargetToKill(currentTeam, lifeWeaver.Character, hexas);
        
            if (teamList.Count == 1)
            {
                actions.Add(lifeWeaver.moveTowards(victim));
                lifeWeaver.Character.setSkillAvailable1(true);//active le skill de damge quand le lifeweaver n'a plus d'alliée
            }
            if (allyInNeed(currentTeam, hexas))
            {
                // If an ally is in need
                Debug.Log("On est ici ");

               

                //chosit une victime ou il peut envoyer sa première compétence
                Character victimChosit = null;
                int minHP = 1000;

                //chosit la victime qui a les points de vie le plus bas
                for (int i = 0; i < enemyList.Count - 1; i++)
                {
                    if (isInRangeToUseSkill(lifeWeaver.Character, enemyList[i].Character))
                    {
                        if (enemyList[i].Character.getHP() < minHP)
                        {
                            minHP = enemyList[i].Character.getHP();
                            victimChosit = enemyList[i].Character;
                        }
                    }
                }
                if (lifeWeaver.Character.isSkill3Up() && hexas.getDistance(allyInPrio.getX(), allyInPrio.getY(), lifeWeaver.Character.getX(), lifeWeaver.Character.getY()) >= distanceSuperieurAllie)
                {
                    Point choisit = null;
                    List<Point> targetArea = hexas.getHexasWithinRange(lifeWeaver.X, lifeWeaver.Y, lifeWeaver.Character.getRange());
                    Debug.Log("NAME allyInPrio :" + allyInPrio.getName());
                    Debug.Log("Coordonnes de targetArea :");
                    int minDistance = 1000;
                    Debug.Log("targetArea.Count :" + targetArea.Count);

                    for (int i = 0; i < targetArea.Count - 1; i++)
                    {
                        int distance = hexas.getDistance(targetArea[i].getX(), targetArea[i].getY(), allyInPrio.getX(), allyInPrio.getY());

                        distances.Add(distance);
                        Debug.Log("Point n°" + i + " Coordonnes de targetArea x:" + targetArea[i].getX() + ", targetArea y:" + targetArea[i].getY());
                        //   Debug.Log("Distance ["+i+"] ="+distance[i]);
                        if (distance <= minDistance)
                        {
                            minDistance = distance;
                            choisit = targetArea[i];
                        }
                    }
                    Debug.Log("Minimum distance :" + minDistance);
                    //choisit une zone valide aleatoire pour se teleporter
                    //     Point choisit = targetArea[UnityEngine.Random.Range(0, targetArea.Count - 1)];
                    Debug.Log(" Point choisit : (" + choisit.getX() + ", " + choisit.getY() + ")");
                    Hexa goTo = hexas.getHexa(choisit.getX(), choisit.getY());
                    if (goTo != null && hexas.canWalk(goTo) && goTo.charOn == null)
                    {
                        actions.Add((lifeWeaver.Character, new ActionAIPos(ActionType.ATK4, new Point(choisit.getX(), choisit.getY()))));
                        lifeWeaver.Character.setSkillAvailable3(false);
                    }
                    lifeWeaver.Character.setSkillAvailable3(false);
                }
                //SKILL1 : ATTAQUE
                else if (lifeWeaver.Character.isSkill1Up() && victimChosit != null)
                {
                    if (isInRangeToUseSkill(lifeWeaver.Character, victimChosit))
                    {
                        ActionAIPos ciblage = new ActionAIPos(ActionType.ATK2, new Point(victimChosit.getX(), victimChosit.getY()));
                        actions.Add((lifeWeaver.Character, ciblage));
                        lifeWeaver.Character.setSkillAvailable1(false);
                    }
                }
                // Choosing target to heal/follow
                targetAlly = choosingAlly(allyRankedByMissingHP(currentTeam, hexas), currentTeam, hexas, lifeWeaver.Character);
                Debug.Log("heal =>" + targetAlly.Character.charClass);
                actions.Add(lifeWeaver.target(targetAlly));
            }
            else if (lifeWeaver.Character.isSkill2Up() && isInRangeToUseSkill2(lifeWeaver.Character, allyInPrio))
            {
                Debug.Log("HEALER SKILL AVAILABLE 2 AND isInRangeToUseSkill2 ");

                ActionAIPos ciblage = new ActionAIPos(ActionType.ATK3, new Point(allyInPrio.getX(), allyInPrio.getY()));
                actions.Add((lifeWeaver.Character, ciblage));
                lifeWeaver.Character.setSkillAvailable2(false);
            }
            else if (teamList.Count == 1)
            {
                actions.Add(lifeWeaver.moveTowards(victim));
            }
            else if (allTeamFullLife(currentTeam, hexas) && !atLeastOnInRange(currentTeam, hexas))
            {
                // If no one is in need, and no one in range, get closer to an ally
                Debug.Log("first elif");
                actions.Add(lifeWeaver.moveTowards(allyInPrio));
                return actions;
            }
            else if (!allyInNeed(currentTeam, hexas) && moveTowardsWithoutRisk(currentTeam, hexas, allyInPrio) && AIUtil.hexaGrid.getWalkingDistance(lifeWeaver.Character.getX(), lifeWeaver.Character.getY(), allyInPrio.getX(), allyInPrio.getY()) > 2)
            {
                Debug.Log("second elif");
                actions.Add(lifeWeaver.moveTowards(allyInPrio));
                return actions;
            }

            else actions.Add(lifeWeaver.wait());
            Debug.Log(" else");
            return actions;
        }

        //Added By Mariana Duarte L3Q1 2025
        /// <summary>
        /// permet de choisir le coup à utiliser du bastion
        /// </summary>
        /// <param name="bastion"></param>
        /// <param name="currentTeam"></param>
        /// <param name="hexas"></param>
        /// <param name="caseBonus"></param>
        /// <returns></returns>

        public static List<(Character, ActionAIPos)> bastion(CharacterCPU bastion, int currentTeam, HexaGrid hexas)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<CharacterCPU> enemyList = getEnemyTeamList(currentTeam, hexas);

            Debug.Log("BASTION enemyList :" + enemyList.Count);
            Debug.Log("BASTION teamList :" + teamList.Count);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;

            Debug.Log("skill 3 cool down bastion ->" + bastion.Character.attack3coolDownValue);

            for (int countPA = 0; countPA < bastion.Character.getPa(); countPA++)
            {

                victim = easierTargetToKill(currentTeam, bastion.Character, hexas);//victime à selectionner la plus facile à tuer
                Debug.Log("caractere bastion PA :" + bastion.Character.getPa() + " countPA :" + countPA);
                Debug.Log("bastion.Character.isSkill1Up() : " + bastion.Character.isSkill1Up());
                Debug.Log("bastion.Character.isSkill2Up() : " + bastion.Character.isSkill2Up());
                Debug.Log("bastion.Character.isSkill3Up() : " + bastion.Character.isSkill3Up());
                if (victim != null)
                {

                    if (bastion.Character.isSkill1Up()) //  skillAvailable est pour HOWL
                    {

                        actions.Add((bastion.Character, new ActionAIPos(ActionType.ATK2, new Point(bastion.Character.getX(), bastion.Character.getY()))));
                        bastion.Character.setSkillAvailable1(false);
                    }

                    // Si la victime est à portée de HAMMER
                    else if (bastion.Character.isSkill2Up() && isInRangeToUseSkill2(bastion.Character, victim))
                    {
                        actions.Add((bastion.Character, new ActionAIPos(ActionType.ATK3, new Point(victim.getX(), victim.getY()))));
                        bastion.Character.setSkillAvailable2(false);

                    }
                    else if (bastion.Character.isSkill3Up() && isInRangeToUseSkill3(bastion.Character, victim))
                    {
                        actions.Add((bastion.Character, new ActionAIPos(ActionType.ATK4, new Point(victim.getX(), victim.getY()))));
                        bastion.Character.setSkillAvailable3(false);

                    }
                    else
                    {
                        Debug.Log("Script BASTION victime est invisible : " + victim.isInvisible());

                        if (isInRangeToAttack(bastion.Character, victim) && !victim.isInvisible())
                        {
                            Debug.Log("bastion.Character.attaquebasique: " + bastion.Character.isSkill3Up());

                            actions.Add((bastion.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));//attaque de base
                        }
                        else
                        {
                            Debug.Log("bastion.Character.deplacement: " + bastion.Character.isSkill3Up());

                            actions.Add(bastion.moveTowards(victim));//avance
                        }
                    }
                }

            }
            Debug.Log("Bastion Actions => " + actions.Count);
            //            Debug.Log("forgeron Action 0" + actions[0].GetType());
            if (actions.Count == 0)
            {
                List<(Character, ActionAIPos)> a = new List<(Character, ActionAIPos)>();
                a.Add((bastion.Character, new ActionAIPos(ActionType.SKIP, null)));
                return a;
            }

            return actions;
        }

        //Added By Mariana Duarte L3Q1 2025
        /// <summary>
        /// permet de choisir le coup à utiliser d'amazone
        /// </summary>
        /// <param name="amazone"></param>
        /// <param name="currentTeam"></param>
        /// <param name="hexas"></param>
        /// <param name="caseBonus"></param>
        /// <returns></returns>

        public static List<(Character, ActionAIPos)> amazone(CharacterCPU amazone, int currentTeam, HexaGrid hexas)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<CharacterCPU> enemyList = getEnemyTeamList(currentTeam, hexas);

            Debug.Log("AMAZONE enemyList :" + enemyList.Count);
            Debug.Log("AMAZONE teamList :" + teamList.Count);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;

            Debug.Log("skill 3 cool down amazone ->" + amazone.Character.attack3coolDownValue);

            for (int countPA = 0; countPA < amazone.Character.getPa(); countPA++)
            {

                victim = easierTargetToKill(currentTeam, amazone.Character, hexas);//victime à selectionner la plus facile à tuer
                Debug.Log("caractere amazone PA :" + amazone.Character.getPa() + " countPA :" + countPA);
                Debug.Log("amazone.Character.isSkill1Up() : " + amazone.Character.isSkill1Up());
                Debug.Log("amazone.Character.isSkill2Up() : " + amazone.Character.isSkill2Up());
                Debug.Log("amazone.Character.isSkill3Up() : " + amazone.Character.isSkill3Up());
                if (victim != null)
                {

                    if (amazone.Character.isSkill1Up()) //  skillAvailable est pour HOWL
                    {

                        actions.Add((amazone.Character, new ActionAIPos(ActionType.ATK2, new Point(amazone.Character.getX(), amazone.Character.getY()))));
                        amazone.Character.setSkillAvailable1(false);
                    }

                    // Si la victime est à portée de HAMMER
                    else if (amazone.Character.isSkill2Up() && isInRangeToUseSkill2(amazone.Character, victim))
                    {
                        actions.Add((amazone.Character, new ActionAIPos(ActionType.ATK3, new Point(victim.getX(), victim.getY()))));
                        amazone.Character.setSkillAvailable2(false);

                    }
                    else if (amazone.Character.isSkill3Up() && isInRangeToUseSkill3(amazone.Character, victim))
                    {
                        actions.Add((amazone.Character, new ActionAIPos(ActionType.ATK4, new Point(victim.getX(), victim.getY()))));
                        amazone.Character.setSkillAvailable3(false);

                    }
                    else
                    {
                        Debug.Log("Script AMAZONE victime est invisible : " + victim.isInvisible());

                        if (isInRangeToAttack(amazone.Character, victim) && !victim.isInvisible())
                        {
                            Debug.Log("amazone.Character.attaquebasique: " + amazone.Character.isSkill3Up());

                            actions.Add((amazone.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));//attaque de base
                        }
                        else
                        {
                            Debug.Log("amazone.Character.deplacement: " + amazone.Character.isSkill3Up());

                            actions.Add(amazone.moveTowards(victim));//avance
                        }
                    }
                }

            }
            Debug.Log("Amazone Actions => " + actions.Count);
            //            Debug.Log("forgeron Action 0" + actions[0].GetType());
            if (actions.Count == 0)
            {
                List<(Character, ActionAIPos)> a = new List<(Character, ActionAIPos)>();
                a.Add((amazone.Character, new ActionAIPos(ActionType.SKIP, null)));
                return a;
            }

            return actions;
        }

        //Added By Mariana Duarte L3Q1 2025
        /// <summary>
        /// permet de choisir le coup à utiliser du geomage
        /// </summary>
        /// <param name="geomage"></param>
        /// <param name="currentTeam"></param>
        /// <param name="hexas"></param>
        /// <param name="caseBonus"></param>
        /// <returns></returns>

        public static List<(Character, ActionAIPos)> geomage(CharacterCPU geomage, int currentTeam, HexaGrid hexas)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<CharacterCPU> enemyList = getEnemyTeamList(currentTeam, hexas);

            Debug.Log("GEOMAGE enemyList :" + enemyList.Count);
            Debug.Log("GEOMAGE teamList :" + teamList.Count);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;

            Debug.Log("skill 3 cool down geomage ->" + geomage.Character.attack3coolDownValue);

            for (int countPA = 0; countPA < geomage.Character.getPa(); countPA++)
            {

                victim = easierTargetToKill(currentTeam, geomage.Character, hexas);//victime à selectionner la plus facile à tuer
                Debug.Log("caractere geomage PA :" + geomage.Character.getPa() + " countPA :" + countPA);
                Debug.Log("geomage.Character.isSkill1Up() : " + geomage.Character.isSkill1Up());
                Debug.Log("geomage.Character.isSkill2Up() : " + geomage.Character.isSkill2Up());
                Debug.Log("geomage.Character.isSkill3Up() : " + geomage.Character.isSkill3Up());
                if (victim != null)
                {

                    if (geomage.Character.isSkill1Up()) //  skillAvailable est pour HOWL
                    {

                        actions.Add((geomage.Character, new ActionAIPos(ActionType.ATK2, new Point(geomage.Character.getX(), geomage.Character.getY()))));
                        geomage.Character.setSkillAvailable1(false);
                    }

                    // Si la victime est à portée de HAMMER
                    else if (geomage.Character.isSkill2Up() && isInRangeToUseSkill2(geomage.Character, victim))
                    {
                        actions.Add((geomage.Character, new ActionAIPos(ActionType.ATK3, new Point(victim.getX(), victim.getY()))));
                        geomage.Character.setSkillAvailable2(false);

                    }
                    else if (geomage.Character.isSkill3Up() && isInRangeToUseSkill3(geomage.Character, victim))
                    {
                        actions.Add((geomage.Character, new ActionAIPos(ActionType.ATK4, new Point(victim.getX(), victim.getY()))));
                        geomage.Character.setSkillAvailable3(false);

                    }
                    else
                    {
                        Debug.Log("Script GEOMAGE victime est invisible : " + victim.isInvisible());

                        if (isInRangeToAttack(geomage.Character, victim) && !victim.isInvisible())
                        {
                            Debug.Log("geomage.Character.attaquebasique: " + geomage.Character.isSkill3Up());

                            actions.Add((geomage.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));//attaque de base
                        }
                        else
                        {
                            Debug.Log("geomage.Character.deplacement: " + geomage.Character.isSkill3Up());

                            actions.Add(geomage.moveTowards(victim));//avance
                        }
                    }
                }

            }
            Debug.Log("Geomage Actions => " + actions.Count);
            //            Debug.Log("forgeron Action 0" + actions[0].GetType());
            if (actions.Count == 0)
            {
                List<(Character, ActionAIPos)> a = new List<(Character, ActionAIPos)>();
                a.Add((geomage.Character, new ActionAIPos(ActionType.SKIP, null)));
                return a;
            }

            return actions;
        }

        //Added By Mariana Duarte L3Q1 2025
        /// <summary>
        /// permet de choisir le coup à utiliser du mortifere
        /// </summary>
        /// <param name="mortifere"></param>
        /// <param name="currentTeam"></param>
        /// <param name="hexas"></param>
        /// <param name="caseBonus"></param>
        /// <returns></returns>

        public static List<(Character, ActionAIPos)> mortifere(CharacterCPU mortifere, int currentTeam, HexaGrid hexas)
        {
            List<CharacterCPU> teamList = getTeamList(currentTeam, hexas);
            List<CharacterCPU> enemyList = getEnemyTeamList(currentTeam, hexas);

            Debug.Log("MORTIFERE enemyList :" + enemyList.Count);
            Debug.Log("MORTIEFER teamList :" + teamList.Count);
            List<(Character, ActionAIPos)> actions = new List<(Character, ActionAIPos)>();
            Character victim = null;

            Debug.Log("skill 3 cool down mortifere ->" + mortifere.Character.attack3coolDownValue);

            for (int countPA = 0; countPA < mortifere.Character.getPa(); countPA++)
            {

                victim = easierTargetToKill(currentTeam, mortifere.Character, hexas);//victime à selectionner la plus facile à tuer
                Debug.Log("caractere mortifere PA :" + mortifere.Character.getPa() + " countPA :" + countPA);
                Debug.Log("mortifere.Character.isSkill1Up() : " + mortifere.Character.isSkill1Up());
                Debug.Log("mortifere.Character.isSkill2Up() : " + mortifere.Character.isSkill2Up());
                Debug.Log("mortifere.Character.isSkill3Up() : " + mortifere.Character.isSkill3Up());
                if (victim != null)
                {

                    if (mortifere.Character.isSkill1Up()) //  skillAvailable est pour HOWL
                    {

                        actions.Add((mortifere.Character, new ActionAIPos(ActionType.ATK2, new Point(mortifere.Character.getX(), mortifere.Character.getY()))));
                        mortifere.Character.setSkillAvailable1(false);
                    }

                    // Si la victime est à portée de HAMMER
                    else if (mortifere.Character.isSkill2Up() && isInRangeToUseSkill2(mortifere.Character, victim))
                    {
                        actions.Add((mortifere.Character, new ActionAIPos(ActionType.ATK3, new Point(victim.getX(), victim.getY()))));
                        mortifere.Character.setSkillAvailable2(false);

                    }
                    else if (mortifere.Character.isSkill3Up() && isInRangeToUseSkill3(mortifere.Character, victim))
                    {
                        actions.Add((mortifere.Character, new ActionAIPos(ActionType.ATK4, new Point(victim.getX(), victim.getY()))));
                        mortifere.Character.setSkillAvailable3(false);

                    }
                    else
                    {
                        Debug.Log("Script MORTIFERE victime est invisible : " + victim.isInvisible());

                        if (isInRangeToAttack(mortifere.Character, victim) && !victim.isInvisible())
                        {
                            Debug.Log("mortifere.Character.attaquebasique: " + mortifere.Character.isSkill3Up());

                            actions.Add((mortifere.Character, new ActionAIPos(ActionType.ATK1, new Point(victim.getX(), victim.getY()))));//attaque de base
                        }
                        else
                        {
                            Debug.Log("mortifere.Character.deplacement: " + mortifere.Character.isSkill3Up());

                            actions.Add(mortifere.moveTowards(victim));//avance
                        }
                    }
                }

            }
            Debug.Log("Mortifere Actions => " + actions.Count);
            //            Debug.Log("forgeron Action 0" + actions[0].GetType());
            if (actions.Count == 0)
            {
                List<(Character, ActionAIPos)> a = new List<(Character, ActionAIPos)>();
                a.Add((mortifere.Character, new ActionAIPos(ActionType.SKIP, null)));
                return a;
            }

            return actions;
        }
    }    
}

