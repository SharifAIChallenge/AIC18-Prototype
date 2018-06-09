using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Tower : MonoBehaviour {
	public int totalCost;
	public int baseCost;
	public int upgradeCost;
	public int baseUpgrade;
	public float upgradeCoef;
	public int range;
	public int damage;
	public int baseDamage;
	public float damageCoef;
	//tick per attack, attack speed
	public int tpa;
	//attack all enemies in a tile if true
	public bool isAOE;
    // Use this for initialization
    //[HideInInspector]
    public int level = 0;
    public int x = 2;
    int prevX = -1;
	//[HideInInspector]
	public int y = 1;
    int prevY = -1, timer = 1;
    public Element attackElement;
	List<GameObject> target;
    bool ticked = false, moneyTaken = false, timerUpdated = false;
	public int rta = 0;
    void Start () {
        /*if (GameManager.gm.defenderGold < cost[level])
        {
            //EditorUtility.DisplayDialog("Low Gold!", "Dear Defender, You Don't Have Enough Gold!", "OK");
            DestroyObject(this.gameObject);
            return;
        }*/
		//loadInfo ();
        moneyTaken = true;
      //  GameManager.gm.changeDefenderGold(-cost[level]);
		refreshPlace();
		this.GetComponent<SpriteRenderer> ().enabled = true;
	}
	public void initialize(){
		damage = baseDamage;
		upgradeCost = baseUpgrade;
		totalCost = baseCost;

	}
    public void refreshPlace()
    {
		GameObject tile = GameManager.gm.Map.GetComponent<MapMaker>().tiles[y][x];
		if (tile == null)
			EditorUtility.DisplayDialog ("fuck", "SHIT", "WTF");
        tile.GetComponent<TowerPlaceManager>().setTower(this.gameObject);
        this.transform.parent = GameObject.Find("Towers").transform;
        this.transform.position = tile.transform.position;
    }

	// Update is called once per frame
	void FixedUpdate () {
        /*if ((prevX != x || prevY != y) && x < GameManager.gm.Map.GetComponent<MapMaker>().numOfCols && x > -1 && y < MapMaker.numberOfRows && y > -1)
        {
            GameManager.gm.Map.GetComponent<MapMaker>().tiles[x][y].GetComponent<TowerPlaceManager>().removeTower();
            refreshPlace();
            prevX = y;
            prevY = x;
            this.GetComponent<SpriteRenderer>().enabled = true;
        }*/
        if (GameManager.gm.currentTick % 2 != 0 && !timerUpdated)
        {
            timerUpdated = true;
            timer++;
        }
        if (GameManager.gm.currentTick % 2 == 0)
            timerUpdated = false;
        if (timer % tpa != 0)
        {
            ticked = false;
            return;
        }
        else if (timer % tpa == 0 && !ticked)
        {
            ticked = true;
            target = nearest();
			if (target != null && target.Count >0)
            {
				//EditorUtility.DisplayDialog ("Damage", "damage", "ok");
                if (isAOE)
                {
                    for (int i = 0; i < target.Count; i++)
                        target[i].GetComponent<Unit>().takeDamage(damage, this.attackElement);
                }
                else
                {
					for (int i = 0; i < target.Count; i++) {
						if (target [i].GetComponent<Unit> ().tag == "HeavyUnit") {
							target [i].GetComponent<Unit> ().takeDamage (damage, this.attackElement);
							return;
						}
					}
					target[0].GetComponent<Unit>().takeDamage(damage, this.attackElement);
                }
            }
        }
	}
    List<GameObject> nearest() {
        List<GameObject> output = null;
		int furthestTile = int.MaxValue;
        int furthestRoad = 0;
		for (int i = 0; i < GameManager.gm.roadNumber; i++) {
			for (int j = GameManager.gm.Road [i].GetLength (0) - 1; j >=0 ; j--) {
				RoadController currentRoad = GameManager.gm.Road [i] [j];
				if (Mathf.Abs (currentRoad.y - y) + Mathf.Abs (currentRoad.x - x) > range)
					continue;
				if (currentRoad.getUnits ().Count > 0 && GameManager.gm.Road[i].GetLength(0) - j < furthestTile) {
					furthestRoad = i;
					furthestTile = j;
					output = currentRoad.getUnits ();
				}
			}
		}
        return output;
	}
		
    public void setXY(int x, int y)
    {
		this.x = x;
		this.y = y;
    }

    public string getInfo()
    {
        return this.name + ", Damage: " + damage + ", Range: " + range + ", TurnPerAttack: " + tpa + ", AOE: " + isAOE + ", Timer: " + timer;
    }

    public bool levelUp()
    {
        bool output = false;
       
            if(GameManager.gm.defenderGold >= upgradeCost)
            {
                GameManager.gm.changeDefenderGold(-upgradeCost);
                this.level++;
				totalCost += upgradeCost;
				upgradeCost =(int)((float)upgradeCost * upgradeCoef);
				damage =(int)((float)damage * damageCoef);
                output = true;
            }
            else
            {
                print("Not Enough Gold for Upgrade!");
            }
        return output;
    }
}
