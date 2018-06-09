using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTowerDetail : MonoBehaviour {
	public static ArcherTowerDetail atd;
	public int baseCost;
	public int baseUpgrade;
	public float upgradeCoef;
	public int range;
	public int baseDamage;
	public float damageCoef;
	//tick per attack, attack speed
	public int tpa;
	//attack all enemies in a tile if true
	public bool isAOE;
	public GameObject detail;

	void Awake(){
		if (atd == null)
			atd = this.GetComponent<ArcherTowerDetail>();
	}
	// Use this for initialization
	void Start () {
		detail.GetComponent<Tower> ().baseCost = baseCost;
		detail.GetComponent<Tower> ().baseUpgrade = baseUpgrade;
		detail.GetComponent<Tower> ().upgradeCoef = upgradeCoef;
		detail.GetComponent<Tower> ().range = range;
		detail.GetComponent<Tower> ().baseDamage = baseDamage;
		detail.GetComponent<Tower> ().damageCoef = damageCoef;
		detail.GetComponent<Tower> ().tpa = tpa;
		detail.GetComponent<Tower> ().isAOE = isAOE;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
