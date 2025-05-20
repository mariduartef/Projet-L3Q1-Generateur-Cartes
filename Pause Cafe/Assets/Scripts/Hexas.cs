using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Misc;
using Characters;
using CharactersCPU;
using static UtilCPU.UtilCPU;
using System;

// ##################################################################################################################################################
// Hexa : the grid and its functions
// The grid is made if hexas
// Author : ?
// Edited by L3Q1, VALAT Thibault and GOUVEIA Klaus
//Edited by Julien D'aboville L3L1 2024
// Commented by L3Q1, VALAT Thibault
// Edited by Mariana Duarte L3Q1 2025 - Addition of the destructible objects
// ##################################################################################################################################################

// ##################################################################################################################################################
// Map aléatoire : Création et Amélioration des tuiles non traversables générées aléatoirement 
// (indiquées par des couleurs sombres)
// Date : 03/22/2023
// Edited by L3C1, Yuting HUANG
// Commence par la ligne 299
// Les méthodes concernantes :  createRandomRectGrid(), createRandomRectGrid2(), getWallNumber()
// ##################################################################################################################################################




namespace Hexas
{

	public enum HexaType : byte { GROUND, VOID, WALL, BONUS, PORTAL, BUSH, DESTRUCTIBLE_OBJECT };
	public enum HexaDirection : byte { UP, UP_RIGHT, DOWN_RIGHT, DOWN, DOWN_LEFT, UP_LEFT };

	//Class of an hexa
	//Author : ?
	//Edited by L3Q1, VALAT Thibault
	public class Hexa
	{
        //chen christophe l3q1 25/03/2025
        public bool IsAvailable = true;
        




        public static GameObject hexasFolder;
		public static GameObject mapHexaFolder;
		public static Mesh hexaFilledMesh;
		public static Mesh hexaHollowMesh;
		public static Mesh hexaWallMesh;
		public static GameObject hexaTemplate;

		public static float offsetX = 0;
		public static float offsetY = 0;

		public HexaType type;
		public int x;
		public int y;
		public Character charOn;
		public GameObject go;
		public bool isPoisoned; // true if the hexa is poisoned -- Added by MEDILEH Youcef, L3C1

		public int poisonnedDamage { get; set; }

		// Author Simon Sepiol L3L1 2024
		// sert a attribuer les degats infligés a la bonne personne
		public Character whoPoisonedIt;

		//Added by Timothé MIEL - L3L1
		public Hexa otherPortal;

		//Constructor
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public Hexa(HexaType type, int x, int y, bool inMapFolder)
		{
			this.type = type;
			this.x = x;
			this.y = y;
			this.charOn = null;
			if (inMapFolder)
				this.go = GameObject.Instantiate(hexaTemplate, mapHexaFolder.transform);
			else
				this.go = GameObject.Instantiate(hexaTemplate, hexasFolder.transform);
			this.go.SetActive(true);
			this.go.transform.position = hexaPosToReal(x, y, 0);
			switch (this.type)
			{
				case HexaType.GROUND: this.go.GetComponent<MeshFilter>().mesh = hexaHollowMesh; break;
				case HexaType.VOID: this.go.GetComponent<MeshFilter>().mesh = hexaFilledMesh; break;
				case HexaType.WALL: 
				case HexaType.DESTRUCTIBLE_OBJECT : 
					this.go.GetComponent<MeshFilter>().mesh = hexaWallMesh; break;
				case HexaType.BONUS: this.go.GetComponent<MeshFilter>().mesh = hexaHollowMesh; break;
				case HexaType.PORTAL: this.go.GetComponent<MeshFilter>().mesh = hexaFilledMesh; break;
				case HexaType.BUSH: this.go.GetComponent<MeshFilter>().mesh = hexaHollowMesh; break;
			}
			this.defaultColor();
			this.go.GetComponent<HexaGO>().hexa = this;
			this.isPoisoned = false;
		}


		// No GameObject (console mode)
		/**public Hexa(HexaType type, int x, int y, bool a)
		{
			this.type = type;
			this.x = x;
			this.y = y;
			this.charOn = null;
			this.go = null;
			this.isPoisoned = false;
		}
		**/

		// Void constructor
		// Added by Timothé MIEL - L3L1
		public Hexa() { }


		//To change the type of an hexa
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public void changeType(HexaType newType)
		{
			type = newType;
			switch (this.type)
			{
				case HexaType.GROUND: this.go.GetComponent<MeshFilter>().mesh = hexaHollowMesh; break;
				case HexaType.VOID: this.go.GetComponent<MeshFilter>().mesh = hexaFilledMesh; break;
				case HexaType.WALL: 
				case HexaType.DESTRUCTIBLE_OBJECT : 
					this.go.GetComponent<MeshFilter>().mesh = hexaWallMesh; break;
				case HexaType.BONUS: this.go.GetComponent<MeshFilter>().mesh = hexaHollowMesh; break;
				case HexaType.PORTAL: this.go.GetComponent<MeshFilter>().mesh = hexaFilledMesh; break;
				case HexaType.BUSH: this.go.GetComponent<MeshFilter>().mesh = hexaHollowMesh; break;
			}
			this.defaultColor();
		}

		// Console mode
		public void changeType2(HexaType newType)
		{
			type = newType;
		}

		//X coordinate getter
		//Author : ?
		public int getX() { return x; }

		//Y coordinate getter
		//Author : ?
		public int getY() { return y; }


	

		//To change the color of an hexa
		//Author : ?	
		public void changeColor(Color color)
		{
			this.go.GetComponent<Renderer>().material.color = color;
		}

		//The default color of each hexa type
		//Author : ?
		//Edited by L3Q1, VALAT Thibault	
		public void defaultColor()
		{
			switch (this.type)
			{
				case HexaType.GROUND: this.go.GetComponent<Renderer>().material.color = new Color(0.25f, 0.2f, 0.075f, 0.5f); break;
				case HexaType.VOID: this.go.GetComponent<Renderer>().material.color = new Color(0.25f, 0.2f, 0.075f); break;
				case HexaType.WALL: 
				case HexaType.DESTRUCTIBLE_OBJECT :
					this.go.GetComponent<Renderer>().material.color = new Color(0.25f, 0.2f, 0.075f); break;
				case HexaType.BONUS: this.go.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f, 1f); break;
				case HexaType.PORTAL: this.go.GetComponent<Renderer>().material.color = new Color(0.98f, 0.06f, 0.06f); break;
				case HexaType.BUSH: this.go.GetComponent<Renderer>().material.color = new Color(0.25f, 0.2f, 0.075f); break;
			}
		}

		//The hovered color of each hexa type
		//Author : ?
		//Edited by L3Q1, VALAT Thibault	
		public void hoveredColor()
		{
			switch (this.type)
			{
				case HexaType.GROUND: this.go.GetComponent<Renderer>().material.color = new Color(1, 1, 1); break;
				case HexaType.VOID: this.go.GetComponent<Renderer>().material.color = new Color(0.25f, 0.25f, 0.25f); break;
				case HexaType.WALL: 
				case HexaType.DESTRUCTIBLE_OBJECT : 
					this.go.GetComponent<Renderer>().material.color = new Color(0.25f, 0.25f, 4.25f); break;
				case HexaType.BONUS: this.go.GetComponent<Renderer>().material.color = new Color(1, 1, 1); break;
				case HexaType.PORTAL: this.go.GetComponent<Renderer>().material.color = new Color(1f, 0.63f, 0.62f); break;
				case HexaType.BUSH: this.go.GetComponent<Renderer>().material.color = new Color(0.25f, 0.25f, 0.25f); break;
			}
		}

		//Get the name of each hexa type
		//Author : ?
		//Edited by L3Q1, VALAT Thibault	
		public string getName()
		{
			switch (this.type)
			{
				case HexaType.GROUND: return "Ground";
				case HexaType.VOID: return "Void";
				case HexaType.WALL: return "Wall";
				case HexaType.DESTRUCTIBLE_OBJECT : return "Destructible Object";
				case HexaType.BONUS: return "Bonus";
				case HexaType.PORTAL: return "Portal";
				case HexaType.BUSH: return "Bush";

				default: return "None";
			}
		}

		//To translate an (x,y) position to a Vector3 postion
		//Author : ?
		public static Vector3 hexaPosToReal(int x, int y, float height)
		{
			return new Vector3(x * 0.75f + offsetX, height, y * -0.86f + (x % 2) * 0.43f + offsetY);
		}

		//Return the x coordinate
		//Author : VALAT Thibault, L3Q1
		public int getx()
		{
			return x;
		}

		//Return the y coordinate
		//Author : VALAT Thibault, L3Q1
		public int gety()
		{
			return y;
		}

		// Set the poison state of the hexa
		// Author : MEDILEH Youcef, L3C1
		public void setPoisoned(bool b)
		{
			this.isPoisoned = b;
		}

		// Return the poison state of the hexa
		// Author : MEDILEH Youcef, L3C1
		public bool getPoisoned()
		{
			return this.isPoisoned;
		}

		//Other portal setter
		//Author : L3L1 - MIEL Timothé - 2024
		public void setOtherPortal(Hexa otherPortal) { this.otherPortal = otherPortal; }
	}



	//The grid of the map
	//Author : ?
	//Edited by L3Q1, VALAT Thibault and GOUVEIA KLAUS
	[Serializable]
	public class HexaGrid
	{
		public List<Hexa> hexaList;
		public List<Character> charList;
		public List<Hexa> bonusList;
		public int w;
		public int h;

		//Constructor
		public HexaGrid()
		{
			hexaList = new List<Hexa>();
			charList = new List<Character>();
			bonusList = new List<Hexa>();
			w = 0;
			h = 0;
		}

        
		


        //Added by Julien D'aboville L3L1 2024
        public List<Character> getCharList()
        {
			return charList;

		}

		//To create the map from a binary file
		//Author : ?
		// debugage save : on a map not found 2 pour ruin
		public void createGridFromFile(string filePath)
		{
			Debug.Log(filePath);
			if (File.Exists(filePath))
			{
				using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
				{
					w = reader.ReadInt32();
					h = reader.ReadInt32();
					Hexa.offsetX = -((w - 1) * 0.75f) / 2;
					Hexa.offsetY = -((h - 1) * -0.86f + ((w - 1) % 2) * 0.43f) / 2;
					for (int j = 0; j < h; j++)
					{
						for (int i = 0; i < w; i++)
						{
							hexaList.Add(new Hexa((HexaType)reader.ReadByte(), i, j, true));
						}
					}
				}
			}
			else
			{
				Debug.Log("Map not found 1");
				createRectGrid(34, 30);
			}
			// test sans ces lignes pour debugage
			//Debug.Log("Map not found 2");
			//bonusList = new List<Hexa>(hexaList.FindAll(isHexaBonus));

		}

		// Console mode
		/**public void createGridFromFile2(string filePath)
		{
			if (File.Exists(filePath))
			{
				using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
				{
					w = reader.ReadInt32();
					h = reader.ReadInt32();
					Hexa.offsetX = -((w - 1) * 0.75f) / 2;
					Hexa.offsetY = -((h - 1) * -0.86f + ((w - 1) % 2) * 0.43f) / 2;
					for (int j = 0; j < h; j++)
					{
						for (int i = 0; i < w; i++)
						{
							hexaList.Add(new Hexa((HexaType)reader.ReadByte(), i, j, true));
						}
					}
				}
			}
			else
			{
				createRectGrid2(34, 30);
			}
		}
		**/

		//To create a the grid composed of hexas
		//Author : ?
		public void createRectGrid(int w, int h)
		{
			Hexa.offsetX = -((w - 1) * 0.75f) / 2;
			Hexa.offsetY = -((h - 1) * -0.86f + ((w - 1) % 2) * 0.43f) / 2;
			this.w = w;
			this.h = h;
			for (int j = 0; j < h; j++)
			{
				for (int i = 0; i < w; i++)
				{
					hexaList.Add(new Hexa(HexaType.GROUND, i, j, true));
				}
			}
		}


		// Console mode
		/**public void createRectGrid2(int w, int h)
		{
			Hexa.offsetX = -((w - 1) * 0.75f) / 2;
			Hexa.offsetY = -((h - 1) * -0.86f + ((w - 1) % 2) * 0.43f) / 2;
			this.w = w;
			this.h = h;
			for (int j = 0; j < h; j++)
			{
				for (int i = 0; i < w; i++)
				{
					hexaList.Add(new Hexa(HexaType.GROUND, i, j, true));
				}
			}
		}
		**/


		//Returns the list of hexas of type 'bonus'
		//Author : L3Q1, VALAT Thibault
		public List<Hexa> getBonusList()
		{
			return bonusList;
		}

		//Add a character on an hexa
		//Author : ?
		public void addChar(CharClass charClass, int x, int y, int team)
		{
			Hexa hexa = getHexa(x, y);
			if (hexa != null && hexa.charOn == null)
			{
				Character c = new Character(charClass, x, y, team);
				hexa.charOn = c;
				charList.Add(c);
			}
		}

		//Author : Socrate Louis Deriza, L3C1
		public Character getCharacterOnHexa(int x, int y)
		{
			return (getHexa(x, y).charOn);
		}

		// Console mode
		public void addChar2(CharClass charClass, int x, int y, int team)
		{
			Hexa hexa = getHexa(x, y);
			if (hexa != null && hexa.charOn == null)
			{
				Character c = new Character(charClass, x, y, team, true);
				hexa.charOn = c;
				charList.Add(c);
			}
		}

		//Add a character on an hexa
		//Author : ?
		public void addChar(Character c)
		{
			Hexa hexa = getHexa(c.x, c.y);
			if (hexa != null && hexa.charOn == null)
			{
				hexa.charOn = c;
				charList.Add(c);
			}
		}

		//Finds the position of an adjacent hexa
		//Author : ?
		public static Point findPos(int x, int y, HexaDirection direction)
		{
			switch (direction)
			{
				case HexaDirection.UP: return new Point(x, y - 1);
				case HexaDirection.UP_RIGHT: return new Point(x + 1, (x % 2 == 0) ? y : y - 1);
				case HexaDirection.DOWN_RIGHT: return new Point(x + 1, (x % 2 == 0) ? y + 1 : y);
				case HexaDirection.DOWN: return new Point(x, y + 1);
				case HexaDirection.DOWN_LEFT: return new Point(x - 1, (x % 2 == 0) ? y + 1 : y);
				case HexaDirection.UP_LEFT: return new Point(x - 1, (x % 2 == 0) ? y : y - 1);
				default: return null;
			}
		}


		//Returns if a character can walks on an hexa given
		//Author : L3Q1, VALAT Thibault
		public bool canWalk(Hexa point)
		{
			return point.type == HexaType.GROUND || point.type == HexaType.BONUS || point.type == HexaType.PORTAL || point.type == HexaType.BUSH;
		}

		//Gives the (x,y) hexa
		//Author : ?
		public Hexa getHexa(int x, int y)
		{
			if (x >= 0 && x < w && y >= 0 && y < h)
			{
				if (hexaList.Count == 0)
				{
					Debug.Log("GetHexa(" + x + ", " + y + ") return null  --> hexaList.Count == 0");
					return null;
				}
				//	Debug.Log("GetHexa("+x+", "+y+")");
				//	Debug.Log("avant ");

				//	Debug.Log("hexaList x = "+hexaList[x + y * w].x +", y = "+hexaList[x + y * w].y + ")");
				//	Debug.Log("après ");
				return hexaList[x + y * w];

			}
			else
			{
				Debug.Log("GetHexa(" + x + ", " + y + ") return null");
				return null;
			}
		}

		//Gives the hexa according to the point given
		//Author : ?
		public Hexa getHexa(Point p)
		{
			if (p != null && p.x >= 0 && p.x < w && p.y >= 0 && p.y < h)
			{
				//		Debug.Log("getHexa(Point p) Not NUll");
				return hexaList[p.x + p.y * w];
			}
			else
			{
				//	Debug.Log("getHexa(Point p)  NUll");
				return null;
			}
		}

		//Returns the ID of a character
		//Author : ?
		public int getCharID(Character c)
		{
			for (int i = 0; i < charList.Count; i++)
			{
				if (c == charList[i]) return i; //change that one 
			}
			return -1;
		}

		//Class of a temporary hexa
		//Author : ?
		public class HexaTemp
		{
			public int x, y, nbSteps;
			public HexaTemp(int x, int y, int nbSteps)
			{
				this.x = x; this.y = y; this.nbSteps = nbSteps;
			}
		}

		//Finds the shortest path between point (x1,y1) and (x2,y2) within maxSteps steps.
		//Returns either null if there is no path or the list of hexas to take from (x2,y2) to (x1,y1)
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public List<Point> findShortestPath(int x1, int y1, int x2, int y2, int maxSteps)
		{
			//If the star hexa or end hexa is a wall
			if (getHexa(x1, y1).type == HexaType.WALL || getHexa(x2, y2).type == HexaType.WALL || getHexa(x1, y1).type == HexaType.DESTRUCTIBLE_OBJECT || getHexa(x2, y2).type == HexaType.DESTRUCTIBLE_OBJECT)
				return null;

			//Temporary list of tempHexa
			List<HexaTemp> hexaList2 = new List<HexaTemp>();

			foreach (Hexa hexa in hexaList)
			{
				hexaList2.Add(new HexaTemp(hexa.x, hexa.y, maxSteps + 1));
			}

			List<HexaTemp> toCheck = new List<HexaTemp>();
			toCheck.Add(new HexaTemp(x1, y1, 0));
			hexaList2[x1 + y1 * w].nbSteps = 0;
			int minSteps = maxSteps + 1;

			//Add the walkable points
			while (toCheck.Count > 0)
			{
				HexaTemp p = toCheck[0];
				toCheck.RemoveAt(0);
				if (p.nbSteps < maxSteps && p.nbSteps < minSteps)
				{
					for (int i = 0; i < 6; i++)
					{
						HexaDirection hexaDirectionI = (HexaDirection)i;
						Point p2 = findPos(p.x, p.y, hexaDirectionI);
						Hexa h = getHexa(p2);
						if (h != null && canWalk(h) && h.charOn == null && hexaList2[p2.x + p2.y * w].nbSteps > p.nbSteps + 1)
						{
							hexaList2[p2.x + p2.y * w].nbSteps = p.nbSteps + 1;
							if (p2.x == x2 && p2.y == y2) minSteps = p.nbSteps + 1;
							toCheck.Add(new HexaTemp(p2.x, p2.y, p.nbSteps + 1));
						}
					}
				}
			}

			//Fulfill the list according to maxSteps
			if (hexaList2[x2 + y2 * w].nbSteps != maxSteps + 1)
			{
				List<Point> l = new List<Point>();
				int nbSteps = hexaList2[x2 + y2 * w].nbSteps - 1;
				Point p = new Point(x2, y2);
				l.Add(p);
				while (!(p.x == x1 && p.y == y1))
				{
					for (int i = 0; i < 6; i++)
					{
						HexaDirection hexaDirectionI = (HexaDirection)i;
						Point p2 = findPos(p.x, p.y, hexaDirectionI);
						if (p2 != null && p2.x >= 0 && p2.x < w && p2.y >= 0 && p2.y < h && hexaList2[p2.x + p2.y * w].nbSteps == nbSteps)
						{
							i = 6;
							p = p2;
							l.Add(p);
							nbSteps--;
						}
					}
				}
				// Flip the list
				List<Point> l2 = new List<Point>();
				for (int i = l.Count - 1; i >= 0; i--) l2.Add(l[i]);
				return l2;
			}
			else
			{
				return null;
			}
		}

		//Finds all the possible paths from (x,y).
		//Returns the list of hexas.
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		// edited by simon L3L1
		public List<Point> findAllPaths(int x, int y, int maxSteps)
		{

			// on teste pour sauvegarde

			if (getHexa(x, y).type == HexaType.WALL || getHexa(x, y).type == HexaType.DESTRUCTIBLE_OBJECT)
			{
				Debug.Log("veut faire findAllPath pour un mur");
				return null;
			}

			//if the point is a wall
			//if (getHexa(x, y).type == HexaType.WALL) return null;

			//creates new list of hexas
			List<HexaTemp> hexaList2 = new List<HexaTemp>();
			foreach (Hexa hexa in hexaList)
			{
				hexaList2.Add(new HexaTemp(hexa.x, hexa.y, maxSteps + 1));
			}
			List<HexaTemp> toCheck = new List<HexaTemp>();
			toCheck.Add(new HexaTemp(x, y, 0));
			hexaList2[x + y * w].nbSteps = 0;

			//Add the walkable points
			while (toCheck.Count > 0)
			{
				HexaTemp p = toCheck[0];
				toCheck.RemoveAt(0);
				if (p.nbSteps < maxSteps)
				{
					for (int i = 0; i < 6; i++)
					{
						HexaDirection hexaDirectionI = (HexaDirection)i;
						Point p2 = findPos(p.x, p.y, hexaDirectionI);
						Hexa h = getHexa(p2);
						if (h != null && canWalk(h) && h.charOn == null && hexaList2[p2.x + p2.y * w].nbSteps > p.nbSteps + 1)
						{
							hexaList2[p2.x + p2.y * w].nbSteps = p.nbSteps + 1;
							toCheck.Add(new HexaTemp(p2.x, p2.y, p.nbSteps + 1));
						}
					}
				}
			}

			List<Point> pList = new List<Point>();
			foreach (HexaTemp ht in hexaList2)
			{
				if (ht.nbSteps <= maxSteps) pList.Add(new Point(ht.x, ht.y));
			}
			return pList;
		}



		//(private) Used by findHexasInSight
		//Author : ?
		private bool isSightBlockedByHexa(int x1, int y1, int x2, int y2, int hexaX, int hexaY)
		{
			return (Geometry.line_intersects_line(x1, y1, x2, y2, hexaX - 1, hexaY - 2, hexaX + 1, hexaY - 2) ||
					Geometry.line_intersects_line(x1, y1, x2, y2, hexaX + 1, hexaY - 2, hexaX + 2, hexaY) ||
					Geometry.line_intersects_line(x1, y1, x2, y2, hexaX + 2, hexaY, hexaX + 1, hexaY + 2) ||
					Geometry.line_intersects_line(x1, y1, x2, y2, hexaX + 1, hexaY + 2, hexaX - 1, hexaY + 2) ||
					Geometry.line_intersects_line(x1, y1, x2, y2, hexaX - 1, hexaY + 2, hexaX - 2, hexaY) ||
					Geometry.line_intersects_line(x1, y1, x2, y2, hexaX - 2, hexaY, hexaX - 1, hexaY - 2));
		}

		//Finds all hexas in sight from the position (x,y) within maxRange.
		//Returns the list of ground-type hexa positions that are in sight.
		//The list of hexas blocked is returned in out blocked.
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public List<Point> findHexasInSight(int x, int y, int maxRange, out List<Point> blocked, Character currentCharControlled)
		{
			if (x >= 0 && x < w && y >= 0 && y < h && hexaList[x + y * w].type != HexaType.WALL && hexaList[x + y * w].type != HexaType.DESTRUCTIBLE_OBJECT)
			{
				int x2 = x;
				int y2 = y;
				List<Point> toCheck = new List<Point>();
				List<Point> wallList = new List<Point>();
				List<Character> charList_ = new List<Character>();
				List<Point> charWallList = new List<Point>();
				List<Point> inSight = new List<Point>();
				blocked = new List<Point>();

				//Check for each hexa in the range if it is in sight
				for (int j = 0; j <= maxRange; j++)
				{
					int iMin = -maxRange + ((j + ((x + 1) % 2)) / 2);
					int iMax = maxRange - ((j + (x % 2)) / 2);
					if (x2 >= 0 && x2 < w)
					{
						for (int i = iMin; i <= iMax; i++)
						{
							if (y2 + i >= 0 && y2 + i < h)
							{
								switch (hexaList[x2 + (y2 + i) * w].type)
								{
									case HexaType.BONUS:
									case HexaType.PORTAL:
									case HexaType.BUSH:
									case HexaType.GROUND:
										toCheck.Add(new Point(x2, y2 + i));
										if ((x2 != x || y2 + i != y) && hexaList[x2 + (y2 + i) * w].charOn != null) charList_.Add(hexaList[x2 + (y2 + i) * w].charOn); break;
									case HexaType.VOID: break;
									case HexaType.WALL: 
									case HexaType.DESTRUCTIBLE_OBJECT : 
										wallList.Add(new Point(x2, y2 + i)); break;
								}
							}
						}
					}
					x2++;
				}

				//Check for each hexa in the range if it is in sight
				x2 = x - 1;
				for (int j = 1; j <= maxRange; j++)
				{
					int iMin = -maxRange + ((j + ((x + 1) % 2)) / 2);
					int iMax = maxRange - ((j + (x % 2)) / 2);
					if (x2 >= 0 && x2 < w)
					{
						for (int i = iMin; i <= iMax; i++)
						{
							if (y2 + i >= 0 && y2 + i < h)
							{
								switch (hexaList[x2 + (y2 + i) * w].type)
								{
									case HexaType.BONUS:
									case HexaType.PORTAL:
									case HexaType.BUSH:
									case HexaType.GROUND:
										toCheck.Add(new Point(x2, y2 + i));
										if ((x2 != x || y2 + i != y) && hexaList[x2 + (y2 + i) * w].charOn != null) charList_.Add(hexaList[x2 + (y2 + i) * w].charOn); break;
									case HexaType.VOID: break;
									case HexaType.WALL: wallList.Add(new Point(x2, y2 + i)); break;
								}
							}
						}
					}
					x2--;
				}

				//Set x and y values to each wall
				foreach (Point p in wallList)
				{
					int cx = p.x * 3;
					int cy = p.y * -4 + (p.x % 2) * 2;
					p.x = cx;
					p.y = cy;
				}

				foreach (Character p in charList_)
				{
					int cx = p.x * 3;
					int cy = p.y * -4 + (p.x % 2) * 2;
					charWallList.Add(new Point(cx, cy));
				}
				int myPosx = x * 3;
				int myPosy = y * -4 + (x % 2) * 2;

				//Check each point
				foreach (Point p in toCheck)
				{
					int px = p.x * 3;
					int py = p.y * -4 + (p.x % 2) * 2;
					bool hexaInSight = true;
					if (currentCharControlled.charClass != CharClass.ARCHER)
					{
						for (int i = 0; i < wallList.Count; i++)
						{
							if (isSightBlockedByHexa(myPosx, myPosy, px, py, wallList[i].x, wallList[i].y))
							{
								i = wallList.Count;
								hexaInSight = false;
							}
						}
						if (hexaInSight)
						{
							Character chara = getHexa(p).charOn;
							for (int i = 0; i < charWallList.Count; i++)
							{
								if (charList_[i] != chara)
								{
									if (isSightBlockedByHexa(myPosx, myPosy, px, py, charWallList[i].x, charWallList[i].y))
									{
										i = charWallList.Count;
										hexaInSight = false;
									}
								}
							}
						}
					}
					if (hexaInSight) inSight.Add(p);
					else blocked.Add(p);
				}
				return inSight;
			}
			else
			{
				blocked = new List<Point>();
				return new List<Point>();
			}
		}

		//Finds all hexas in sight from the position (x,y) within maxRange.
		//Returns the list of ground-type hexa positions that are in sight.
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public List<Point> findHexasInSight2(int x, int y, int maxRange)
		{
			if (x >= 0 && x < w && y >= 0 && y < h && hexaList[x + y * w].type != HexaType.WALL && hexaList[x + y * w].type != HexaType.DESTRUCTIBLE_OBJECT)
			{
				int x2 = x;
				int y2 = y;
				List<Point> toCheck = new List<Point>();
				List<Point> wallList = new List<Point>();
				List<Character> charList_ = new List<Character>();
				List<Point> charWallList = new List<Point>();
				List<Point> inSight = new List<Point>();

				//Check for each hexa in the range if it is in sight
				for (int j = 0; j <= maxRange; j++)
				{
					int iMin = -maxRange + ((j + ((x + 1) % 2)) / 2);
					int iMax = maxRange - ((j + (x % 2)) / 2);
					if (x2 >= 0 && x2 < w)
					{
						for (int i = iMin; i <= iMax; i++)
						{
							if (y2 + i >= 0 && y2 + i < h)
							{
								switch (hexaList[x2 + (y2 + i) * w].type)
								{
									case HexaType.BONUS:
									case HexaType.PORTAL:
									case HexaType.BUSH:
									case HexaType.GROUND:
										toCheck.Add(new Point(x2, y2 + i));
										if ((x2 != x || y2 + i != y) && hexaList[x2 + (y2 + i) * w].charOn != null) charList_.Add(hexaList[x2 + (y2 + i) * w].charOn); break;
									case HexaType.VOID: break;
									case HexaType.WALL: 
									case HexaType.DESTRUCTIBLE_OBJECT :
										wallList.Add(new Point(x2, y2 + i)); break;
								}
							}
						}
					}
					x2++;
				}

				x2 = x - 1;
				//Check for each hexa in the range if it is in sight
				for (int j = 1; j <= maxRange; j++)
				{
					int iMin = -maxRange + ((j + ((x + 1) % 2)) / 2);
					int iMax = maxRange - ((j + (x % 2)) / 2);
					if (x2 >= 0 && x2 < w)
					{
						for (int i = iMin; i <= iMax; i++)
						{
							if (y2 + i >= 0 && y2 + i < h)
							{
								switch (hexaList[x2 + (y2 + i) * w].type)
								{
									case HexaType.BONUS:
									case HexaType.PORTAL:
									case HexaType.BUSH:
									case HexaType.GROUND:
										toCheck.Add(new Point(x2, y2 + i));
										if ((x2 != x || y2 + i != y) && hexaList[x2 + (y2 + i) * w].charOn != null) charList_.Add(hexaList[x2 + (y2 + i) * w].charOn); break;
									case HexaType.VOID: break;
									case HexaType.WALL: 
									case HexaType.DESTRUCTIBLE_OBJECT :
										wallList.Add(new Point(x2, y2 + i)); break;
								}
							}
						}
					}
					x2--;
				}

				//Set x and y values to each wall
				foreach (Point p in wallList)
				{
					int cx = p.x * 3;
					int cy = p.y * -4 + (p.x % 2) * 2;
					p.x = cx;
					p.y = cy;
				}

				foreach (Character p in charList_)
				{
					int cx = p.x * 3;
					int cy = p.y * -4 + (p.x % 2) * 2;
					charWallList.Add(new Point(cx, cy));
				}

				int myPosx = x * 3;
				int myPosy = y * -4 + (x % 2) * 2;

				//Check each point
				foreach (Point p in toCheck)
				{
					int px = p.x * 3;
					int py = p.y * -4 + (p.x % 2) * 2;
					bool hexaInSight = true;
					for (int i = 0; i < wallList.Count; i++)
					{
						if (isSightBlockedByHexa(myPosx, myPosy, px, py, wallList[i].x, wallList[i].y))
						{
							i = wallList.Count;
							hexaInSight = false;
						}
					}
					if (hexaInSight)
					{
						Character chara = getHexa(p).charOn;
						for (int i = 0; i < charWallList.Count; i++)
						{
							if (charList_[i] != chara)
							{
								if (isSightBlockedByHexa(myPosx, myPosy, px, py, charWallList[i].x, charWallList[i].y))
								{
									i = charWallList.Count;
									hexaInSight = false;
								}
							}
						}
					}
					if (hexaInSight) inSight.Add(p);
				}
				return inSight;
			}
			else
			{
				return new List<Point>();
			}
		}

		//Returns true if the hexa at position (hexaX,hexaY) is in sight from (x,y)
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public bool hexaInSight(int x, int y, int hexaX, int hexaY, int maxRange)
		{
			if (getDistance(x, y, hexaX, hexaY) <= maxRange && x >= 0 && x < w && y >= 0 && y < h && hexaList[x + y * w].type != HexaType.WALL && hexaList[x + y * w].type != HexaType.DESTRUCTIBLE_OBJECT && hexaX >= 0 && hexaX < w && hexaY >= 0 && hexaY < h && (hexaList[hexaX + hexaY * w].type == HexaType.GROUND || hexaList[hexaX + hexaY * w].type == HexaType.BONUS || hexaList[hexaX + hexaY * w].type == HexaType.PORTAL || hexaList[hexaX + hexaY * w].type == HexaType.BUSH))
			{
				int x2 = x;
				int y2 = y;
				List<Point> wallList = new List<Point>();
				List<Character> charList_ = new List<Character>();
				List<Point> charWallList = new List<Point>();

				bool found = false;
				for (int j = 0; j <= maxRange; j++)
				{
					int iMin = -maxRange + ((j + ((x + 1) % 2)) / 2);
					int iMax = maxRange - ((j + (x % 2)) / 2);
					if (x2 >= 0 && x2 < w)
					{
						for (int i = iMin; i <= iMax; i++)
						{
							if (y2 + i >= 0 && y2 + i < h)
							{
								switch (hexaList[x2 + (y2 + i) * w].type)
								{
									case HexaType.BONUS:
									case HexaType.PORTAL:
									case HexaType.BUSH:
									case HexaType.GROUND:
										if ((x2 != x || y2 + i != y) && hexaList[x2 + (y2 + i) * w].charOn != null) charList_.Add(hexaList[x2 + (y2 + i) * w].charOn);
										if (!found && x2 == hexaX && y2 + i == hexaY) found = true; break;
									case HexaType.VOID: break;
									case HexaType.DESTRUCTIBLE_OBJECT :
									case HexaType.WALL: wallList.Add(new Point(x2, y2 + i)); break;
								}
							}
						}
					}
					x2++;
				}

				x2 = x - 1;
				for (int j = 1; j <= maxRange; j++)
				{
					int iMin = -maxRange + ((j + ((x + 1) % 2)) / 2);
					int iMax = maxRange - ((j + (x % 2)) / 2);
					if (x2 >= 0 && x2 < w)
					{
						for (int i = iMin; i <= iMax; i++)
						{
							if (y2 + i >= 0 && y2 + i < h)
							{
								switch (hexaList[x2 + (y2 + i) * w].type)
								{
									case HexaType.BONUS:
									case HexaType.PORTAL:
									case HexaType.BUSH:
									case HexaType.GROUND:
										if ((x2 != x || y2 + i != y) && hexaList[x2 + (y2 + i) * w].charOn != null) charList_.Add(hexaList[x2 + (y2 + i) * w].charOn);
										if (!found && x2 == hexaX && y2 + i == hexaY) found = true; break;
									case HexaType.VOID: break;
									case HexaType.DESTRUCTIBLE_OBJECT :
									case HexaType.WALL: wallList.Add(new Point(x2, y2 + i)); break;
								}
							}
						}
					}
					x2--;
				}
				if (!found) return false;

				foreach (Point p in wallList)
				{
					int cx = p.x * 3;
					int cy = p.y * -4 + (p.x % 2) * 2;
					p.x = cx;
					p.y = cy;
				}

				foreach (Character p in charList_)
				{
					int cx = p.x * 3;
					int cy = p.y * -4 + (p.x % 2) * 2;
					charWallList.Add(new Point(cx, cy));
				}

				int myPosx = x * 3;
				int myPosy = y * -4 + (x % 2) * 2;
				int px = hexaX * 3;
				int py = hexaY * -4 + (hexaX % 2) * 2;

				if (!(maxRange >= 7))
				{
					for (int i = 0; i < wallList.Count; i++)
					{
						if (isSightBlockedByHexa(myPosx, myPosy, px, py, wallList[i].x, wallList[i].y))
						{
							return false;
						}
					}
					Character chara = getHexa(hexaX, hexaY).charOn;
					for (int i = 0; i < charWallList.Count; i++)
					{
						if (charList_[i] != chara)
						{
							if (isSightBlockedByHexa(myPosx, myPosy, px, py, charWallList[i].x, charWallList[i].y))
							{
								return false;
							}
						}
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		//Returns the list of hexas pos within AoERange of position (x,y)
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public List<Point> getHexasWithinRange(int x, int y, int AoERange)
		{
			int x2 = x;
			int y2 = y;
			List<Point> pList = new List<Point>();

			//Check for each hexa in the range if it is a valid one
			for (int j = 0; j <= AoERange; j++)
			{
				int iMin = -AoERange + ((j + ((x + 1) % 2)) / 2);
				int iMax = AoERange - ((j + (x % 2)) / 2);
				if (x2 >= 0 && x2 < w)
				{
					for (int i = iMin; i <= iMax; i++)
					{
						if (y2 + i >= 0 && y2 + i < h)
						{
							switch (hexaList[x2 + (y2 + i) * w].type)
							{
								case HexaType.DESTRUCTIBLE_OBJECT :
								case HexaType.WALL: break;
								case HexaType.VOID: break;
								case HexaType.BONUS:
								case HexaType.PORTAL:
								case HexaType.BUSH:
								case HexaType.GROUND:
									pList.Add(new Point(x2, y2 + i));
									break;
							}
						}
					}
				}
				x2++;
			}

			//Check for each hexa in the range if it is a valid one
			x2 = x - 1;
			for (int j = 1; j <= AoERange; j++)
			{
				int iMin = -AoERange + ((j + ((x + 1) % 2)) / 2);
				int iMax = AoERange - ((j + (x % 2)) / 2);
				if (x2 >= 0 && x2 < w)
				{
					for (int i = iMin; i <= iMax; i++)
					{
						if (y2 + i >= 0 && y2 + i < h)
						{
							switch (hexaList[x2 + (y2 + i) * w].type)
							{
								case HexaType.BONUS:
								case HexaType.PORTAL:
								case HexaType.BUSH:
								case HexaType.GROUND: pList.Add(new Point(x2, y2 + i)); break;
								case HexaType.VOID: break;
								case HexaType.DESTRUCTIBLE_OBJECT :
								case HexaType.WALL: break;
							}
						}
					}
				}
				x2--;
			}
			return pList;
		}

		//Returns the list of characters within AoERange of position (x,y)
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public List<Character> getCharWithinRange(int x, int y, int AoERange)
		{
			int x2 = x;
			int y2 = y;
			List<Character> charList_ = new List<Character>();

			//Check for each hexa in the range if it is a valid one
			for (int j = 0; j <= AoERange; j++)
			{
				int iMin = -AoERange + ((j + ((x + 1) % 2)) / 2);
				int iMax = AoERange - ((j + (x % 2)) / 2);
				if (x2 >= 0 && x2 < w)
				{
					for (int i = iMin; i <= iMax; i++)
					{
						if (y2 + i >= 0 && y2 + i < h)
						{
							switch (hexaList[x2 + (y2 + i) * w].type)
							{
								case HexaType.BONUS:
								case HexaType.PORTAL:
								case HexaType.BUSH:
								case HexaType.GROUND: if (hexaList[x2 + (y2 + i) * w].charOn != null) charList_.Add(hexaList[x2 + (y2 + i) * w].charOn); break;
								case HexaType.VOID: break;
								case HexaType.DESTRUCTIBLE_OBJECT :
								case HexaType.WALL: break;
							}
						}
					}
				}
				x2++;
			}

			//Check for each hexa in the range if it is a valid one
			x2 = x - 1;
			for (int j = 1; j <= AoERange; j++)
			{
				int iMin = -AoERange + ((j + ((x + 1) % 2)) / 2);
				int iMax = AoERange - ((j + (x % 2)) / 2);
				if (x2 >= 0 && x2 < w)
				{
					for (int i = iMin; i <= iMax; i++)
					{
						if (y2 + i >= 0 && y2 + i < h)
						{
							switch (hexaList[x2 + (y2 + i) * w].type)
							{
								case HexaType.BONUS:
								case HexaType.PORTAL:
								case HexaType.BUSH:
								case HexaType.GROUND: if (hexaList[x2 + (y2 + i) * w].charOn != null) charList_.Add(hexaList[x2 + (y2 + i) * w].charOn); break;
								case HexaType.VOID: break;
								case HexaType.DESTRUCTIBLE_OBJECT :
								case HexaType.WALL: break;
							}
						}
					}
				}
				x2--;
			}
			return charList_;
		}

		//Returns the distance between two hexas
		//Author : ?
		public int getDistance(int x1, int y1, int x2, int y2)
		{
			int distance = 0;
			while (x1 != x2)
			{
				if (y1 > y2)
				{
					if (x1 > x2)
					{
						if (x1 % 2 == 1) y1--;
						x1--;
					}
					else
					{
						if (x1 % 2 == 1) y1--;
						x1++;
					}
				}
				else if (y1 < y2)
				{
					if (x1 > x2)
					{
						if (x1 % 2 == 0) y1++;
						x1--;
					}
					else
					{
						if (x1 % 2 == 0) y1++;
						x1++;
					}
				}
				else
				{
					return distance + ((x1 > x2) ? (x1 - x2) : (x2 - x1));
				}
				distance++;
			}
			return distance + ((y1 > y2) ? (y1 - y2) : (y2 - y1));
		}

		//Author : Yuting H, L3C1
		public int getDistance(Point p1, Point p2)
		{
			int x1 = p1.getX();
			int x2 = p2.getX();
			int y1 = p2.getY();
			int y2 = p2.getY();
			return getDistance(x1, y1, x2, y2);
		}


		// Returns the number of steps between two hexas
		//Author : ?
		//Edited by L3Q1, VALAT Thibault
		public int getWalkingDistance(int x1, int y1, int x2, int y2)
		{
			int maxSteps = 300;
			//check if one of the hexa is a wall
			if (this.getHexa(x1, y1).type == HexaType.WALL || this.getHexa(x2, y2).type == HexaType.WALL || this.getHexa(x1, y1).type == HexaType.DESTRUCTIBLE_OBJECT || this.getHexa(x2, y2).type == HexaType.DESTRUCTIBLE_OBJECT)
				return maxSteps;

			//temp list of hexas
			List<HexaTemp> hexaList2 = new List<HexaTemp>();
			foreach (Hexa hexa in this.hexaList)
			{
				hexaList2.Add(new HexaGrid.HexaTemp(hexa.x, hexa.y, maxSteps + 1));
			}

			List<HexaTemp> toCheck = new List<HexaTemp>();
			toCheck.Add(new HexaTemp(x1, y1, 0));
			hexaList2[x1 + y1 * this.w].nbSteps = 0;
			int minSteps = maxSteps + 1;

			//Check each hexa of the list
			while (toCheck.Count > 0)
			{
				HexaTemp p = toCheck[0];
				toCheck.RemoveAt(0);
				if (p.nbSteps < maxSteps && p.nbSteps < minSteps)
				{
					for (int i = 0; i < 6; i++)
					{
						HexaDirection hexaDirectionI = (HexaDirection)i;
						Point p2 = findPos(p.x, p.y, hexaDirectionI);
						Hexa h = this.getHexa(p2);
						if (h != null && ((h.x == x2 && h.y == y2) || (canWalk(h) && h.charOn == null)) && hexaList2[p2.x + p2.y * this.w].nbSteps > p.nbSteps + 1)
						{
							hexaList2[p2.x + p2.y * this.w].nbSteps = p.nbSteps + 1;
							if (p2.x == x2 && p2.y == y2) minSteps = p.nbSteps + 1;
							toCheck.Add(new HexaTemp(p2.x, p2.y, p.nbSteps + 1));
						}
					}
				}
			}

			return (hexaList2[x2 + y2 * this.w].nbSteps == maxSteps) ? maxSteps : hexaList2[x2 + y2 * this.w].nbSteps;
		}





		//Gives the walking distance of the CPU player
		//Author : ?
		public int getWalkingDistanceCPU(int x1, int y1, int x2, int y2)
		{
			List<Hexa> hexasSoonToBeMovedOn = new List<Hexa>();
			int walkingDistance;

			// Adding where the CPU plans to move characters
			// This is purely so CPU characters don't run into each other
			foreach (CharacterCPU cpu in CharactersCPU.CharacterCPU.charCPUList)
				if (getHexa(cpu.X, cpu.Y).charOn == null)
				{
					hexasSoonToBeMovedOn.Add(getHexa(cpu.X, cpu.Y));

					// Adding a dummy character
					getHexa(cpu.X, cpu.Y).charOn = cpu.TempChar;
				}

			walkingDistance = getWalkingDistance(x1, y1, x2, y2);

			// Removing all the invisible characters added in
			foreach (Hexa h in hexasSoonToBeMovedOn)
				h.charOn = null;

			return walkingDistance;
		}

		//Author : Yuting H, L3C1
		//Fonctions : nextPoint(), getShortestDistanceCPU()
		public Point nextPoint(Point p, List<Point> liste)
		{
			int index = liste.IndexOf(p);
			return liste[index + 1];

		}

		public int getShortestDistanceCPU(int x1, int y1, int x2, int y2)
		{
			List<Hexa> hexasSoonToBeMovedOn = new List<Hexa>();
			int walkShortestDistanceCPU = 0;

			foreach (CharacterCPU cpu in CharactersCPU.CharacterCPU.charCPUList)
				if (getHexa(cpu.X, cpu.Y).charOn == null)
				{
					hexasSoonToBeMovedOn.Add(getHexa(cpu.X, cpu.Y));


					getHexa(cpu.X, cpu.Y).charOn = cpu.TempChar;
				}
			foreach (Point p in findShortestPath(x1, y1, x2, y2, 300))
				walkShortestDistanceCPU += getDistance(p, nextPoint(p, findShortestPath(x1, y1, x2, y2, 300)));
			foreach (Hexa h in hexasSoonToBeMovedOn)
				h.charOn = null;

			return walkShortestDistanceCPU;
		}

		// Initialize both portals
		// Added by Timothé MIEL - L3L1
		public void InitializePortals()
		{
			Hexa portal1 = new Hexa();
			Hexa portal2 = new Hexa();
			int numberOfPortal = 0;
			foreach (Hexa hexa in hexaList)
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

		}


	}

}
