using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc;
using Characters;
using Classifiers;
using Hexas;

namespace Stats {

///<summary>
/// StatsGame class
/// Calculate the fitness of a classifier
///</summary>
public class StatsGame {
	public List<StatsTurn> statsTurn;
	public int winner;
	public List<Character> survivors; // at the end of the game (to evaluate how close the game was)
	
	public StatsGame(){
		statsTurn = new List<StatsTurn>();
		winner = -1;
	}
	
	public void nextTurn(Character currentCharTurn){
		statsTurn.Add(new StatsTurn(currentCharTurn));
	}
	
	public void endGame(int winner,HexaGrid hexagrid){
		this.winner = winner;
		survivors = new List<Character>();
		foreach (Character c in hexagrid.charList){
			survivors.Add(c);
		}
	}
	
	public StatsTurn getStatsTurn(Character c){
		for (int i=statsTurn.Count-1;i>=0;i--) 
			if (statsTurn[i].character == c) 
				return statsTurn[i];
		return null;
	}
	
	public void addToDamageTaken(Character c,int n){
		StatsTurn st = getStatsTurn(c);
		if (st != null){
			st.damageTaken += n;
			st.HP -= n;
			if (st.HP < 0) st.HP = 0;
		}
	}
	
	public void addToDamageDealt(Character c,int n){
		StatsTurn st = getStatsTurn(c);
		if (st != null) st.damageDealt += n;
	}
	
	public void addToHeal(Character c,int n){
		StatsTurn st = getStatsTurn(c);
		if (st != null) st.heal += n;
	}

	public void addToKills(Character c,int n){
		StatsTurn st = getStatsTurn(c);
		if (st != null) st.kills += n;
	}
	
	public void setDead(Character c,bool dead){
		StatsTurn st = getStatsTurn(c);
		if (st != null) st.dead = dead;
	}
	
	public void addToRules(Character c,Classifier classifier){
		StatsTurn st = getStatsTurn(c);
		if (st != null) st.rulesUsed.Add(classifier);
	}
	
	public string disp(){
		string str = "";
		foreach (StatsTurn st in statsTurn){
			str += st.disp()+"\n";
		}
		return str;
	}
	
	public void evaluateGame(){
		
		//for (int i=0;i<statsTurn.Count;i++) evaluateTurn(i,1);
		foreach (StatsTurn st in statsTurn) 
			st.evaluateTurn();
		// The reward/punishment for winning/losing the game is increased for every character that has survived and how much HP they have left.
		float reward = 0.0f; 
		foreach (Character c in survivors) 
			if (c.team == winner) 
				reward += 0.1f + ((float)c.HP / (float)c.HPmax);
		//reward *= 5.0f;
		
		foreach (StatsTurn st in statsTurn){
			foreach (Classifier c in st.rulesUsed){
				c.addToFitness((st.character.team == winner) ? reward : (-reward));
				c.useCount++;
				c.lastUse = 0;
			}
		}
	}
	
	/** returns the score of the game. */
	public float evaluateGame(float expectedScore){
		//foreach (StatsTurn st in statsTurn) st.evaluateTurn();
		//for (int i=0;i<statsTurn.Count;i++) evaluateTurn(i,1);
		
		// The reward/punishment for winning/losing the game is increased for every character that has survived and how much HP they have left.
		float reward = 0.0f; foreach (Character c in survivors) if (c.team == winner) reward += 0.02f + (((float)c.HP / (float)c.HPmax) * 0.03f);
		//reward *= 5.0f;
		
		foreach (StatsTurn st in statsTurn){
			foreach (Classifier c in st.rulesUsed){
				c.addToFitness((st.character.team == winner) ? reward-expectedScore : (-reward-expectedScore));
				c.useCount++;
				c.lastUse = 0;
			}
		}
		
		return reward;
	}
	
	/** Evaluates every turn with the outcome of the following turns*/
	public void evaluateTurn(int turnID,int nbTurns){
		float goodness = 0;
		int nbTurnsReal = 0;
		for (int i=turnID;i<statsTurn.Count && nbTurns > nbTurnsReal;i++){
			StatsTurn st = statsTurn[i];
			goodness += st.calculate(1.0f,1.0f,1.0f,10.0f,10.0f);
			nbTurnsReal++;
		}
		float g = goodness / (float)(nbTurnsReal);
		//if (g < 0) g /= 2.0f; // remove later
		
		if (g != 0.0f){
			foreach (Classifier c in statsTurn[turnID].rulesUsed){
				c.addToFitness2(g);
			}
		}
	}
}

public class StatsTurn {
	public Character character;
	public int HP;
	public int heal; //heal to allies
	public int damageDealt;
	public int damageTaken; // Damage taken AFTER their turn (between the end of their turn and their next turn)
	public int kills;
	public bool dead;
	public List<Classifier> rulesUsed;
	
	public StatsTurn(Character c){
		character = c;
		HP = c.HP;
		damageDealt = 0;
		damageTaken = 0;
		kills = 0;
		heal=0;
		dead = false;
		rulesUsed = new List<Classifier>();
	}
	
	public string disp(){
		return character.charClass + " " + character.team + " : " + HP + " HP, " + damageDealt + " dealt, " +  damageTaken + " taken, " + kills + " kills, dead : " + dead + " nb Rules : " + rulesUsed.Count;
	}
	
	public float calculate(float healValue, float damageDealtValue,float damageTakenValue,float killsValue,float deathValue){
		return damageDealt*damageDealtValue - damageTaken*damageTakenValue + kills*killsValue - ((dead) ? deathValue : 0)+ heal * healValue;
	}
	
	public void evaluateTurn(){
		float goodness = this.calculate(1.0f,1.0f,1f,10.0f,10.0f);
		
		if (goodness != 0){
			foreach (Classifier c in rulesUsed){
				c.addToFitness(goodness);
                //c.modified = true;
			}
		}
	}
}

}
