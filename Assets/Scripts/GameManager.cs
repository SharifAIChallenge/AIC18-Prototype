using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GameManager: MonoBehaviour {

    public int roadNumber = 1;
	public int[] roadLength;
    public GameObject attackerScript;
    public GameObject defenderScript;
    public Text DefenderGoldText;
    public Text AttackerGoldText;
	public Text ScoreText;
    public Text TurnText;
    public static int elementCount = 6;
	public static GameManager gm;
	public int defenderGold = 10;
	public int defenderIncome = 1;
	public int laser = 3;
	public int defenderScore = 0;
	public int attackerGold = 10;
	public int attackerIncome = 1;
	public int nuke = 3;
	//not used till now
	public int attackerScore = 0;
	//each phase takes this number of ticks
	public int ticksPerTurn = 10;
	public int currentTick = -1;
	[HideInInspector]
	public RoadController[][] Road;
	public GameObject Map;
	public int row;
	public int col;
    public int lightUnitPerLevel = 100, heavyUnitPerLevel = 14;
    public string[] resistanceRef = new string[elementCount];
    public float[][] resistances = new float[elementCount][];
    public GameObject[] towers;
    public GameObject[] creeps;
	public String[][] vision;
	public String[] showVision;
    bool towerOkay = true, unitOkay = true;
    bool ended = false;
    public int lightUnitLevel = 0;
    public int heavyUnitLevel = 0;
	int lightUnitCount = 0, heavyUnitCount = 0;
	int prevTick;
	public float tickLength;
	public GameObject[][] realMap;
	IEnumerator ticker()
	{
		while (true) {
			yield return new WaitForSeconds (tickLength);
			currentTick++;
		}
	}
    void Awake()
    {
        if (gm == null)
            gm = this.GetComponent<GameManager>();
		vision = new string[row] [];
		showVision = new string[row];
		for (int i = 0; i < row; i++)
			vision [i] = new string[col];
    }

    // Use this for initialization
    void Start () {
		roadLength = new int[roadNumber];
		roadLength = Map.GetComponent<MapMaker> ().roadLength;
		realMap = Map.GetComponent<MapMaker> ().tiles;
        Road = new RoadController[roadNumber][];
		for(int i = 0; i < roadNumber; i++) 
			GameManager.gm.Road[i] = new RoadController[roadLength[i]];
		StartCoroutine (ticker ());
        changeAttackerGold(0);
        changeDefenderGold(0);
        for(int i = 0; i < elementCount; i++)
        {
            resistances[i] = new float[elementCount];
            string[] row = resistanceRef[i].Split(',');
            for(int j = 0; j < elementCount; j++)
            {
                resistances[i][j] = (float) Double.Parse(row[j]);
            }
        }
	}

    // Update is called once per frame
    void Update()
    {
		if (currentTick!= prevTick)
        {
			updateVision ();
			prevTick = currentTick;
			if (currentTick % 10 == 0) {
				defenderScript.GetComponent<Defender> ().whatToDo ();
				TurnText.text = "Tower TODO Turn";
			} else if (currentTick % 2 == 0) {
				defenderScript.GetComponent<Defender> ().updateGame ();
				TurnText.text = "Tower UPDATE Turn";

			}
			if(currentTick % 10 ==1)
            {
                attackerScript.GetComponent<Attacker>().whatToDo();
                TurnText.text = "Unit TODO Turn";
            }
			else if(currentTick % 2 ==1)
			{
				attackerScript.GetComponent<Attacker>().updateGame();
				TurnText.text = "Unit UPDATE Turn";
			}
            MonoBehaviour.print(currentTick);
        }
        if (defenderScore == 0 && !ended)
        { 
            EditorUtility.DisplayDialog("End", "Game Over!", "OK");
            ended = true;
        }
        if (currentTick % (2 * ticksPerTurn) == 2 && unitOkay)
        {
            unitOkay = false;
            //EditorUtility.DisplayDialog("Unit Making Time!", "Make Some Units Before Going to Next Tick!", "OK");
        }
        if (currentTick % (2 * ticksPerTurn) != 2)
            unitOkay = true;
        if (currentTick % (2 * ticksPerTurn) == 1 && towerOkay)
        {
            towerOkay = false;
            //EditorUtility.DisplayDialog("Tower Building Time!", "Build Some Towers Before Going to Next Tick!", "OK");
            changeDefenderGold(defenderIncome);
            changeAttackerGold(attackerIncome);
        }
        if (currentTick % (2 * ticksPerTurn) != 1)
            towerOkay = true;
    }

    public float resistance(Element attacker, Element defender)
    {
        return resistances[(int)attacker][(int)defender];
    }

	public void changeScore(int amount)
	{
		defenderScore -= amount;
		ScoreText.text = " Score: " + defenderScore;
	}

    public void changeDefenderGold(int amount)
    {
        defenderGold += amount;
        DefenderGoldText.text = " Defender Gold: " + defenderGold;
    }

    public void changeAttackerGold(int amount)
    {
        attackerGold += amount;
        AttackerGoldText.text = " Attacker Gold: " + attackerGold;
    }

    public Tower createTower(int y, int x, string name)
    {
		if (y >= row || x >= col) {
			print ("wrong coordinates for tower!");
			return null;
		}
        GameObject output = null;
        GameObject tile = Map.GetComponent<MapMaker>().tiles[y][x];
        if (tile.tag.Equals("Grass"))
        {
            if (tile.GetComponent<TowerPlaceManager>().hasTower)
                return null;
			if (tile.GetComponent<TowerPlaceManager> ().canPlaceTower == false) {
				print ("This tile has been lasered!");
				return null;
			}
            GameObject theTower = null;
            foreach(GameObject tower in towers)
            {
                if (tower.name.Equals(name))
                {
                    theTower = tower;
                    break;
                }
            }
            if(theTower != null)
			{
				if (defenderGold < theTower.GetComponent<Tower> ().baseCost)
					return null;
				changeDefenderGold (-theTower.GetComponent<Tower> ().baseCost);
                output = GameObject.Instantiate(theTower);
				if (output != null) {
					output.GetComponent<Tower>().setXY(x, y);
					tile.GetComponent<TowerPlaceManager> ().setTower (output);
					output.GetComponent<Tower> ().initialize ();
					return output.GetComponent<Tower>();
				}

            }

        }
		return null;
    }
    public Unit createUnit(string name, int roadNum)
    {
		if (roadNum >= roadNumber) {
			print ("wrong road number!");
			return null;
		}
        GameObject theCreep = null, output = null;
        foreach (GameObject creep in creeps)
        {
            if (creep.name.Equals(name))
            {
                theCreep = creep;
                break;
            }
        }
        if (theCreep != null)
        {
			if(theCreep.name.Equals("LightUnit")){
				if (GameManager.gm.attackerGold < theCreep.GetComponent<Unit>().baseCost + (theCreep.GetComponent<Unit>().costPlus * lightUnitLevel))
				{
					return null;
				}
				changeAttackerGold(-(theCreep.GetComponent<Unit>().baseCost + (theCreep.GetComponent<Unit>().costPlus * lightUnitLevel)));
				attackerIncome += theCreep.GetComponent<Unit>().income;
			}
			if(theCreep.name.Equals("HeavyUnit")){
				if (GameManager.gm.attackerGold < theCreep.GetComponent<Unit>().baseCost + (theCreep.GetComponent<Unit>().costPlus * lightUnitLevel))
				{
					return null;
				}
				changeAttackerGold(-(theCreep.GetComponent<Unit>().baseCost + (theCreep.GetComponent<Unit>().costPlus * lightUnitLevel)));
				attackerIncome += theCreep.GetComponent<Unit>().income;
			}
            output = GameObject.Instantiate(theCreep);
            output.GetComponent<Unit>().roadNumber = roadNum;
			attackerScript.GetComponent<Attacker> ().addUnit (output.GetComponent<Unit>());
            if (output.tag.Equals("LightUnit"))
            {
                output.GetComponent<Unit>().level = lightUnitLevel;
                lightUnitCount++;
                if (lightUnitCount >= lightUnitPerLevel)
                {
                    lightUnitCount -= lightUnitPerLevel;
                 	lightUnitLevel++;
                }
            }
            else
            {
                output.GetComponent<Unit>().level = heavyUnitLevel;
                heavyUnitCount++;
                if (heavyUnitCount >= heavyUnitPerLevel)
                {
                    heavyUnitCount -= heavyUnitPerLevel;
					heavyUnitLevel++;
                }
            }
			output.GetComponent<Unit>().initialize();
			return output.GetComponent<Unit>();
        }
        return null;
    }

	public bool useNuke(int y,int x){
		if (nuke == 0) {
			print ("You don't have any nukes!");
			return false;
		}
		if((y<1||y>=row-1)||(x<1||x>=col-1)){
			print ("wrong coordinates for nuke!");
			return false;
		}
		nuke--;
		List<GameObject> targets;
		for (int i = y - 1; i <= y + 1; i++)
			for (int j = x - 1; j <= x + 1; j++) {
				if (realMap [i] [j].tag == "Road") {
					targets = realMap [i] [j].GetComponent<RoadController> ().getUnits ();
					for (int k = 0; k < targets.Count; k++)
						targets [k].GetComponent<Unit> ().health = 0;
				}
			}
		return true;
	}
		
	public bool useLaser (int y, int x){
		if(laser==0){
			print ("You don't have any lasers!");
			return false;
		}
		if(y>=row || x>=col){
			print ("wrong coordinates for laser!");
			return false;
		}
		if (realMap [y] [x].tag == "Grass") {
			DestroyObject (realMap [y] [x].GetComponent<TowerPlaceManager> ().tower);
			realMap [y] [x].GetComponent<TowerPlaceManager> ().removeTower();
			realMap [y] [x].GetComponent<TowerPlaceManager> ().canPlaceTower = false;
			laser--;
			return true;
		}
		return false;
	}

	void updateVision(){
		string curr;
		for (int i = 0; i < row; i++) {
			curr = "";
			for (int j = 0; j < col; j++) {
				curr += vision [i] [j] + ",";
			}
			showVision [i] = curr;
		}
	}
}
