using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyUnitDetail : MonoBehaviour {
	public static HeavyUnitDetail hud;
	public int baseHealth;
	public float healthCoef;
	//tick per tile, move speed
	public int tpt;
	public int baseCost;
	public int costPlus;
	public int income;
	//gold gained for killing
	public int baseReward;
	public int rewardPlus;
	//damage dealt if reach end of the road
	public int damage = 1;
	public int visionRange = 5;
	public GameObject detail;

	void Awake(){
		if (hud == null)
			hud = this.GetComponent<HeavyUnitDetail>();
	}
	// Use this for initialization
	void Start () {
		detail.GetComponent<Unit> ().baseCost = baseCost;
		detail.GetComponent<Unit> ().costPlus = costPlus;
		detail.GetComponent<Unit> ().baseHealth = baseHealth;
		detail.GetComponent<Unit> ().healthCoef = healthCoef;
		detail.GetComponent<Unit> ().tpt = tpt;
		detail.GetComponent<Unit> ().income = income;
		detail.GetComponent<Unit> ().baseReward = baseReward;
		detail.GetComponent<Unit> ().rewardPlus = rewardPlus;
		detail.GetComponent<Unit> ().damage = damage;
		detail.GetComponent<Unit> ().visionRange = visionRange;
	}


	public int cost(int level){
		int ans = baseCost;
		ans += (level * costPlus);
		return ans;
	}
	// Update is called once per frame
	void Update () {

	}
}
