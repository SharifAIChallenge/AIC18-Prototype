using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Unit : MonoBehaviour {
    public int health;
	public int baseHealth;
	public float healthCoef;
    //tick per tile, move speed
    public int tpt;
    public int cost;
	public int baseCost;
	public int costPlus;
	public int income;
	//gold gained for killing
	public int reward;
	public int baseReward;
	public int rewardPlus;
	//damage dealt if reach end of the road
	public int damage = 1;
    public int roadNumber = 0;
    public Element armorElement;
	public int visionRange = 5;
    public int level = 0;
	int CurrentTile;
    bool moved = false, ticked = false;
    int timer = 1;
	int x;
	int y;
	// Use this for initialization
	void Start () {
        //initialize();
	}

    public void initialize()
    {
		//loadInfo ();
		health = baseHealth;
		cost = baseCost;
		reward = baseReward;
		for (int i = 0; i < level; i++) {
			health =(int)((float)health * healthCoef);
			cost += costPlus;
			reward += rewardPlus;
		}
        CurrentTile = 0;
        GameManager.gm.Road[roadNumber][CurrentTile].addUnit(this.gameObject);
        this.transform.parent = GameObject.Find("Creeps").transform;
    }


	// Update is called once per frame
	void FixedUpdate () {
		updateVision ();
        if (GameManager.gm.currentTick % 2 == 0 && !ticked)
        {
            ticked = true;
            timer++;
        }
        if(GameManager.gm.currentTick % 2 != 0)
            ticked = false;
        if (health <= 0) {
            GameManager.gm.changeDefenderGold(reward);
            removeMe();
		}
		else if (timer % tpt == 0 && !moved) {
            moved = true;
			forward ();
		}
        else if (timer % tpt != 0)
            moved = false;
        /*
		if (tick % GameManager.gm.numOfTicks == 0) {
			GameManager.gm.Road [roadNumber][CurrentTile].GetComponent<RoadController> ().removeUnit (this.gameObject);
			DestroyObject (this.gameObject);
		}
        */
	}

    public void takeDamage(float amount, Element element)
    {
        this.health -= (int) (amount * GameManager.gm.resistance(element, armorElement));
    }


	//moving forward in path
	void forward(){
		GameManager.gm.Road [roadNumber][CurrentTile].removeUnit (this.gameObject);
		CurrentTile++;
		if (CurrentTile == GameManager.gm.Road[roadNumber].GetLength(0)) {
			GameManager.gm.changeScore (damage);
			CurrentTile--;
            removeMe();
		} else {
			GameManager.gm.Road [roadNumber][CurrentTile].addUnit (this.gameObject);
			x = GameManager.gm.Road[roadNumber][CurrentTile].x;
			y =  GameManager.gm.Road[roadNumber][CurrentTile].y;
		}
	}
	void updateVision(){
		for (int i = y - visionRange; i <= y + visionRange; i++) {
			if (i < 0 || i >= GameManager.gm.row)
				continue;
			for (int j = x - visionRange; j <= x + visionRange; j++) {
				if (j < 0 || j >= GameManager.gm.col)
					continue;
				if (Mathf.Abs(i - y) + Mathf.Abs(j - x) > visionRange)
					continue;
				if (GameManager.gm.Map.GetComponent<MapMaker>().tiles[i][j].tag.Equals("Grass")) {
					if (GameManager.gm.Map.GetComponent<MapMaker> ().tiles [i] [j].GetComponent<TowerPlaceManager> ().hasTower == false) {
						GameManager.gm.vision [i] [j] = "g";
					} else {
						if (GameManager.gm.Map.GetComponent<MapMaker> ().tiles [i] [j].GetComponent<TowerPlaceManager> ().tower.GetComponent<Tower>().isAOE == true)
							GameManager.gm.vision [i] [j] = "c";
						else if(GameManager.gm.Map.GetComponent<MapMaker> ().tiles [i] [j].GetComponent<TowerPlaceManager> ().tower.GetComponent<Tower>().isAOE == false)
							GameManager.gm.vision [i] [j] = "a";
						GameManager.gm.vision [i] [j]+= GameManager.gm.Map.GetComponent<MapMaker> ().tiles [i] [j].GetComponent<TowerPlaceManager> ().tower.GetComponent<Tower>().level.ToString();
					}
				}
			}
		}
	}
    public string getInfo()
    {
        return this.name + ", Health: " + health + ", Damage: " + damage + ", TurnPerMove: " + tpt + ", Timer: " + timer + ",Level: " + level;
    }

    public void removeMe()
    {
        GameManager.gm.attackerScript.GetComponent<Attacker>().removeUnit(this);
        GameManager.gm.Road[roadNumber][CurrentTile].removeUnit(this.gameObject);
        DestroyObject(this.gameObject);
    }

    public int getHealth()
    {
        return health;
    }
}
