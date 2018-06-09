using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Defender : MonoBehaviour
{

	LightUnitDetail lud;
	HeavyUnitDetail hud;
	CannonTowerDetail ctd;
	ArcherTowerDetail atd;
	List<Pair<Pair<int, int>, int>> cannonTowerPlaces = new List<Pair<Pair<int, int>, int>>();
	List<Pair<Pair<int, int>, int>> archerTowerPlaces = new List<Pair<Pair<int, int>, int>>();
	List<List<Pair<Tower, int>>> cannonTowers = new List<List<Pair<Tower, int>>>();
	List<List<Pair<Tower, int>>> archerTowers = new List<List<Pair<Tower, int>>>();
	List<TowerDetail> cannonTowerDetails = new List<TowerDetail>();
	List<TowerDetail> archerTowerDetails = new List<TowerDetail>();
	Tower[][] towers;
	string[][] map;
	bool started = false;
	float cannonBuildingChance = 0.35f;
	float archerMultiplicant = 1f / 2f, cannonMultiplicant = 1f / 3f;
	int cannonPlaceListIndex = 0;
	int archerPlaceListIndex = 0;
	bool worthyCannonPlaces = true, worthyArcherPlaces = true;
	List<Road> roads = new List<Road>();
	int start_counter = 0;

	void preload()
	{
		lud = LightUnitDetail.lud;
		hud = HeavyUnitDetail.hud;
		ctd = CannonTowerDetail.ctd;
		atd = ArcherTowerDetail.atd;
		map = GameManager.gm.vision;
		towers = new Tower[map.GetLength(0)][];
		for (int i = 0; i < map.GetLength(0); i++)
			towers[i] = new Tower[20];
		for(int i = 0; i < GameManager.gm.roadNumber; i++)
		{
			roads.Add(new Road(GameManager.gm.Road[i], this));
		}
		cannonTowers.Add(new List<Pair<Tower, int>>());
		archerTowers.Add(new List<Pair<Tower, int>>());
	}

	public void whatToDo()
	{
		GameManager.gm.createTower (14, 13, "ArcherTower");
		if (!started)
		{
			preload();
			findGoodPlaces();
			started = true;
		}
		else
		{
			if (start_counter < 3)
				start_counter++;
			else
			{
				foreach (Road road in roads)
					road.refreshDangerList();
				/*float randomOrNot = (float)new System.Random().NextDouble();
                if (randomOrNot < 0.3)
                    tryBuildingTower();
                else*/
				HeavyTryBuildingTower();
			}
		}
	}

	public void updateGame()
	{
		foreach (Road road in roads)
			road.refreshDangerList();
		toNukeOrNotToNuke();
	}

	public void removeTower(Tower tower)
	{
		if (tower.tag.Equals("ArcherTower")) {
			for (int i = 0; i < archerTowers.Count; i++)
				for (int j = 0; j < archerTowers[i].Count; j++)
				{
					if (archerTowers[i][j].x.Equals(tower))
						archerTowers[i].RemoveAt(j);
				}
			for (int i = 0; i < archerTowerDetails.Count; i++)
			{
				if (archerTowerDetails[i].tower.Equals(tower))
				{
					archerTowerDetails.RemoveAt(i);
				}
			}
		}
	}

	void findGoodPlaces()
	{
		List<Pair<Pair<int, int>, int>> tempCannonPlaces = new List<Pair<Pair<int, int>, int>>(), tempArcherPlaces = new List<Pair<Pair<int, int>, int>>();
		for (int i = 0; i < GameManager.gm.row; i++)
		{
			for (int j = 0; j < GameManager.gm.col; j++)
			{
				Pair<bool, int> answer = countAttackedRoadTiles(i, j, ctd.range);
				if (answer.x)
					tempCannonPlaces.Add(new Pair<Pair<int, int>, int>(new Pair<int, int>(i, j), answer.y));
				answer = countAttackedRoadTiles(i, j, atd.range);
				if (answer.x)
					tempArcherPlaces.Add(new Pair<Pair<int, int>, int>(new Pair<int, int>(i, j), answer.y));
			}
		}

		var points = from element in tempCannonPlaces
			orderby -element.y
			select element;
		foreach (Pair<Pair<int, int>, int> element in points)
		{
			cannonTowerPlaces.Add(element);
		}

		points = from element in tempArcherPlaces
			orderby -element.y
			select element;
		foreach (Pair<Pair<int, int>, int> element in points)
		{
			archerTowerPlaces.Add(element);
		}
	}

	Pair<bool, int> countAttackedRoadTiles(int x, int y, int range)
	{
		int roadCount = 0;
		if (!map[x][y].Equals("r"))
		{
			for (int i = y - range; i <= y + range; i++)
			{
				if (i < 0 || i >= GameManager.gm.row)
					continue;
				for (int j = x - range; j <= x + range; j++)
				{
					if (j < 0 || j >= GameManager.gm.col)
						continue;
					if (Mathf.Abs(i - y) + Mathf.Abs(j - x) > range)
						continue;
					if (map[i][j].Equals("r"))
					{
						roadCount++;
					}
				}
			}
			return new Pair<bool, int>(true, roadCount);
		}
		return new Pair<bool, int>(false, 0);
	}

	void tryBuildingTower()
	{
		System.Random rnd = new System.Random();
		float cannonOrArcher = (float)rnd.NextDouble();
		float whichLevel = (float)rnd.NextDouble();
		if (cannonOrArcher < cannonBuildingChance && GameManager.gm.defenderGold >= ctd.baseUpgrade)
		{
			bool upgradedATower = false;
			float sumOfChances = 0f;
			for (int i = cannonTowers.Count - 1; i >= 0; i--)
			{
				sumOfChances = cannonUpgradeChance(i + 1);
				if (whichLevel < sumOfChances && cannonTowers[i].Count > 0 && GameManager.gm.defenderGold >= calculateCannonCost(i + 1))
				{
					upgradedATower = true;
					Pair<Tower, int> upgradedTower = cannonTowers[i][0];
					upgradedTower.x.levelUp();
					foreach(TowerDetail td in cannonTowerDetails)
					{
						if (td.tower.Equals(upgradedTower.x))
						{
							td.RefreshDetails();
							break;
						}
					}
					if (i + 1 >= cannonTowers.Count)
						cannonTowers.Add(new List<Pair<Tower, int>>());
					cannonTowers[i].RemoveAt(0);
					cannonTowers[i + 1].Add(upgradedTower);
					cannonTowers[i + 1] = sortTowerList(cannonTowers[i + 1]);
				}
			}
			if (!upgradedATower)
			{
				Pair<Pair<int, int>, int> placeAndPoint = null;
				bool foundAPlace = false;
				while (!foundAPlace && worthyCannonPlaces)
				{
					print("cpi: " + cannonPlaceListIndex);
					placeAndPoint = cannonTowerPlaces[cannonPlaceListIndex];
					cannonPlaceListIndex++;
					if (towers[placeAndPoint.x.x][placeAndPoint.x.y] != null)
					{
						continue;
					}
					if (placeAndPoint.y <= 0)
					{
						worthyCannonPlaces = false;
						break;
					}
					foundAPlace = true;
					Tower createdTower = GameManager.gm.createTower(placeAndPoint.x.x, placeAndPoint.x.y, "CannonTower");
					towers[placeAndPoint.x.x][placeAndPoint.x.y] = createdTower;
					print(createdTower);
					cannonTowerDetails.Add(new TowerDetail(createdTower, this));
					cannonTowers[0].Add(new Pair<Tower, int>(createdTower, placeAndPoint.y));
					cannonTowers[0] = sortTowerList(cannonTowers[0]);
				}
			}
		}
		else if (GameManager.gm.defenderGold >= atd.baseUpgrade)
		{
			bool upgradedATower = false;
			float sumOfChances = 0f;
			for (int i = archerTowers.Count - 1; i >= 0; i--)
			{
				sumOfChances = archerUpgradeChance(i + 1);
				if (whichLevel < sumOfChances && archerTowers[i].Count > 0 && GameManager.gm.defenderGold >= calculateArcherCost(i + 1))
				{
					upgradedATower = true;
					Pair<Tower, int> upgradedTower = archerTowers[i][0];
					upgradedTower.x.levelUp();
					foreach (TowerDetail td in archerTowerDetails)
					{
						if (td.tower.Equals(upgradedTower.x))
						{
							td.RefreshDetails();
							break;
						}
					}
					if (i + 1 >= archerTowers.Count)
						archerTowers.Add(new List<Pair<Tower, int>>());
					archerTowers[i].RemoveAt(0);
					archerTowers[i + 1].Add(upgradedTower);
					archerTowers[i + 1] = sortTowerList(archerTowers[i + 1]);
				}
			}
			if (!upgradedATower)
			{
				Pair<Pair<int, int>, int> placeAndPoint = null;
				bool foundAPlace = false;
				while (!foundAPlace && worthyArcherPlaces)
				{
					placeAndPoint = archerTowerPlaces[archerPlaceListIndex];
					archerPlaceListIndex++;
					if (towers[placeAndPoint.x.x][placeAndPoint.x.y] != null)
					{
						continue;
					}
					if (placeAndPoint.y <= 0)
					{
						worthyArcherPlaces = false;
						break;
					}
					foundAPlace = true;
					Tower createdTower = GameManager.gm.createTower(placeAndPoint.x.x, placeAndPoint.x.y, "ArcherTower");
					towers[placeAndPoint.x.x][placeAndPoint.x.y] = createdTower;
					archerTowerDetails.Add(new TowerDetail(createdTower, this));
					archerTowers[0].Add(new Pair<Tower, int>(createdTower, placeAndPoint.y));
					archerTowers[0] = sortTowerList(archerTowers[0]);
				}
			}
		}
	}

	float archerUpgradeChance(int level)
	{
		float output = 1f - Mathf.Atan(level * archerMultiplicant) / Mathf.PI * 2;
		return output;
	}

	float cannonUpgradeChance(int level)
	{
		float output = 1f - Mathf.Atan(level * cannonMultiplicant) / Mathf.PI * 2;
		return output;
	}

	void toNukeOrNotToNuke()
	{

	}

	void HeavyTryBuildingTower()
	{
		bool cannonPossible = true, archerPossible = true;
		while (cannonPossible || archerPossible)
		{
			double archerOrCannon = new System.Random().NextDouble();
			float bestBuildingPoint = 0f, bestUpgradingPoint = 0f;
			int bestUpgradeIndex = -1;
			Pair<int, int> bestBuildingPlace = null;
			print ("chance:" + archerOrCannon);
			if (!cannonPossible || (archerOrCannon < 0.8 && archerPossible))
			{
				for (int i = 0; i < archerTowerDetails.Count; i++)
				{
					TowerDetail td = archerTowerDetails[i];
					if (calculateArcherCost(td.tower.level + 1) <= GameManager.gm.defenderGold)
					{
						print("gonna find best upgrading point. td: " + td.roadNIndex.Count);
						float currentUpgradePoint = 0f;
						HashSet<Pair<int, int>> countedPlaces = new HashSet<Pair<int, int>>();
						for (int j = 0; j < td.roadNIndex.Count; j++)
						{
							Pair<Road, int> pair = td.roadNIndex[j];
							Pair<int, int> place = pair.x.roadPlaces[pair.y];
							print("tile x: " + place.x + " tile y: " + place.y);
							if (!countedPlaces.Contains(place))
							{
								currentUpgradePoint += pair.x.heavyDangerList[pair.y];
							}
						}
						//This is linear for Archer Towers but Exponentail for Cannons
						currentUpgradePoint *= Mathf.Pow(2, td.targetingRoads.Count - 1);
						currentUpgradePoint /= Mathf.Pow(4, Mathf.Atan(calculateArcherCost(td.tower.level + 1)));
						currentUpgradePoint *= Mathf.Pow(7, Mathf.Atan(calculateArcherDmg(td.tower.level + 1) - calculateArcherDmg(td.tower.level)));
						if (currentUpgradePoint > bestUpgradingPoint)
						{
							bestUpgradingPoint = currentUpgradePoint;
							bestUpgradeIndex = i;
						}
					}
				}
				Pair<Pair<int, int>, int> placeAndPoint = null;
				bool foundAPlace = false;
				int totalTargettingTiles = 0, totalRoads = 0;
				if (calculateArcherCost(0) <= GameManager.gm.defenderGold)
				{
					int tempArcherPlaceListIndex = archerPlaceListIndex;
					bool tempWorthyArcherPlaces = worthyArcherPlaces;
					print(tempWorthyArcherPlaces + " and index is: " + tempArcherPlaceListIndex);
					while (tempWorthyArcherPlaces)
					{
						totalRoads = 0;
						totalTargettingTiles = 0;
						float currentBuildPoint = 0f;
						placeAndPoint = archerTowerPlaces[tempArcherPlaceListIndex];
						tempArcherPlaceListIndex++;
						if (towers[placeAndPoint.x.x][placeAndPoint.x.y] != null)
						{
							continue;
						}
						if (placeAndPoint.y <= 0 || tempArcherPlaceListIndex >= archerTowerPlaces.Count)
						{
							tempWorthyArcherPlaces = false;
							break;
						}
						foundAPlace = true;
						if (bestBuildingPlace == null)
							bestBuildingPlace = placeAndPoint.x;
						Pair<int, int> xy = placeAndPoint.x;
						int range = atd.range, tpa = atd.tpa, x = xy.x, y = xy.y;
						for (int roadIndex = 0; roadIndex < roads.Count; roadIndex++)
						{
							Road road = roads[roadIndex];
							bool foundInThisRoad = false;
							for (int i = y - range; i <= y + range; i++)
							{

								if (i < 0 || i >= GameManager.gm.row)
									continue;
								for (int j = x - range; j <= x + range; j++)
								{

									if (j < 0 || j >= GameManager.gm.col)
										continue;
									if (Mathf.Abs(i - y) + Mathf.Abs(j - x) > range)
										continue;

									for (int k = 0; k < road.roadLength; k++)
									{
										//print("x: " + road.roadPlaces[k].x + " y: " + road.roadPlaces[k].y + " j: " + j + " i: " + i);
										if (road.roadPlaces[k].x == j && road.roadPlaces[k].y == i)
										{
											totalTargettingTiles++;
											currentBuildPoint += road.heavyDangerList[k];
											if(road.heavyDangerList[k] > 0)
												//print("HDL k: " + road.heavyDangerList[k]);
												foundInThisRoad = true;
											break;
										}
									}
								}
							}
							if (foundInThisRoad)
								totalRoads++;
						}
						currentBuildPoint *= Mathf.Pow(2, totalRoads - 1);
						currentBuildPoint /= Mathf.Pow(4, Mathf.Atan (calculateArcherCost(0)));
						currentBuildPoint *= Mathf.Pow(6, Mathf.Atan(calculateArcherDmg(0)));
						if (bestBuildingPoint < currentBuildPoint)
						{
							bestBuildingPoint = currentBuildPoint;
							bestBuildingPlace = xy;
						}
					}
				}
				print("Archer BBP: " + bestBuildingPoint + " BUI: " + bestUpgradeIndex);
				if (bestBuildingPoint > bestUpgradingPoint && bestBuildingPlace != null)
				{
					Tower createdTower = GameManager.gm.createTower(bestBuildingPlace.y, bestBuildingPlace.x, "ArcherTower");
					if (createdTower != null)
					{
						print("the tower was: " + createdTower);
						towers[bestBuildingPlace.x][bestBuildingPlace.y] = createdTower;
						archerTowerDetails.Add(new TowerDetail(createdTower, this));
						archerTowers[0].Add(new Pair<Tower, int>(createdTower, totalTargettingTiles));
						archerTowers[0] = sortTowerList(archerTowers[0]);
					}
				}
				else if ((archerTowerDetails.Count>0)&&(bestUpgradingPoint > bestBuildingPoint && bestUpgradeIndex >= 0))
				{
					int i = archerTowerDetails[bestUpgradeIndex].tower.level;
					archerTowerDetails[bestUpgradeIndex].tower.levelUp();
					archerTowerDetails[bestUpgradeIndex].RefreshDetails();
					if (i + 1 >= archerTowers.Count)
						archerTowers.Add(new List<Pair<Tower, int>>());
					int index = -1;
					for (int j = 0; j < archerTowers[i].Count; j++)
					{
						if (archerTowers[i][j].x.Equals(archerTowerDetails[bestUpgradeIndex].tower))
						{
							index = j;
							break;
						}
					}
					archerTowers[i].RemoveAt(index);
					archerTowers[i + 1].Add(new Pair<Tower, int>(archerTowerDetails[bestUpgradeIndex].tower, totalTargettingTiles));
					archerTowers[i + 1] = sortTowerList(archerTowers[i + 1]);
				}
				else if (bestUpgradeIndex >= 0 || bestBuildingPlace != null)
				{
					tryBuildingTower();
					return;
				}
				else
					archerPossible = false;
			}
			else if (cannonPossible)
			{
				for (int i = 0; i < cannonTowerDetails.Count; i++)
				{
					TowerDetail td = cannonTowerDetails[i];
					if (calculateCannonCost(td.tower.level + 1) <= GameManager.gm.defenderGold)
					{
						float currentUpgradePoint = 0f;
						HashSet<Pair<int, int>> countedPlaces = new HashSet<Pair<int, int>>();
						for (int j = 0; j < td.roadNIndex.Count; j++)
						{
							Pair<Road, int> pair = td.roadNIndex[j];
							Pair<int, int> place = pair.x.roadPlaces[pair.y];
							if (!countedPlaces.Contains(place))
							{
								currentUpgradePoint += pair.x.lightDangerList[pair.y];
							}
						}
						//This is linear for Archer Towers but Exponentail for Cannons
						currentUpgradePoint *= Mathf.Pow(4, td.targetingRoads.Count - 1);
						currentUpgradePoint /= Mathf.Pow(4, Mathf.Atan(calculateCannonCost(td.tower.level + 1)));
						currentUpgradePoint *= Mathf.Pow(7, Mathf.Atan(calculateCannonDmg(td.tower.level + 1) - calculateCannonDmg(td.tower.level)));
						if (currentUpgradePoint > bestUpgradingPoint)
						{
							bestUpgradingPoint = currentUpgradePoint;
							bestUpgradeIndex = i;
						}
					}
				}
				Pair<Pair<int, int>, int> placeAndPoint = null;
				bool foundAPlace = false;
				int totalTargettingTiles = 0, totalRoads = 0;
				if (calculateCannonCost(0) <= GameManager.gm.defenderGold)
				{
					int tempCannonPlaceListIndex = cannonPlaceListIndex;
					bool tempWorthyCannonPlaces = worthyCannonPlaces;
					while (tempWorthyCannonPlaces)
					{
						totalTargettingTiles = 0;
						totalRoads = 0;
						float currentBuildPoint = 0f;
						placeAndPoint = cannonTowerPlaces[tempCannonPlaceListIndex];
						tempCannonPlaceListIndex++;
						if (towers[placeAndPoint.x.x][placeAndPoint.x.y] != null)
						{
							continue;
						}
						if (placeAndPoint.y <= 0)
						{
							tempWorthyCannonPlaces = false;
							break;
						}
						foundAPlace = true;
						if (bestBuildingPlace == null)
							bestBuildingPlace = placeAndPoint.x;
						Pair<int, int> xy = placeAndPoint.x;
						int range = atd.range, tpa = atd.tpa, x = xy.x, y = xy.y;
						for (int roadIndex = 0; roadIndex < roads.Count; roadIndex++)
						{
							Road road = roads[roadIndex];
							bool foundInThisRoad = false;
							for (int i = y - range; i <= y + range; i++)
							{
								if (i < 0 || i >= GameManager.gm.row)
									continue;
								for (int j = x - range; j <= x + range; j++)
								{
									if (j < 0 || j >= GameManager.gm.col)
										continue;
									if (Mathf.Abs(i - y) + Mathf.Abs(j - x) > range)
										continue;
									for (int k = 0; k < road.roadLength; k++)
									{
										if (road.roadPlaces[k].x == j && road.roadPlaces[k].y == i)
										{
											totalTargettingTiles++;
											currentBuildPoint += road.lightDangerList[k];
											foundInThisRoad = true;
											break;
										}
									}
								}
							}
							if (foundInThisRoad)
								totalRoads++;
						}
						currentBuildPoint *= Mathf.Pow(4, totalRoads - 1);
						currentBuildPoint /= Mathf.Pow(4, Mathf.Atan(calculateCannonCost(0)));
						currentBuildPoint *= Mathf.Pow(6, Mathf.Atan(calculateCannonDmg(0)));
						if (bestBuildingPoint < currentBuildPoint)
						{
							bestBuildingPoint = currentBuildPoint;
							bestBuildingPlace = xy;
						}
					}
				}
				print("Cannon BBP: " + bestBuildingPoint + " BUI: " + bestUpgradeIndex);
				if (bestBuildingPoint >= bestUpgradingPoint && bestBuildingPlace != null)
				{
					print(bestBuildingPlace.x + " " + bestBuildingPlace.y);
					Tower createdTower = GameManager.gm.createTower(bestBuildingPlace.x, bestBuildingPlace.y, "CannonTower");
					towers[bestBuildingPlace.x][bestBuildingPlace.y] = createdTower;
					cannonTowerDetails.Add(new TowerDetail(createdTower, this));
					cannonTowers[0].Add(new Pair<Tower, int>(createdTower, totalTargettingTiles));
					cannonTowers[0] = sortTowerList(cannonTowers[0]);
				}
				else if (bestUpgradingPoint >= bestBuildingPoint && bestUpgradeIndex >= 0)
				{
					int i = cannonTowerDetails[bestUpgradeIndex].tower.level;
					cannonTowerDetails[bestUpgradeIndex].RefreshDetails();
					cannonTowerDetails[bestUpgradeIndex].tower.levelUp();
					if (i + 1 >= cannonTowers.Count)
						cannonTowers.Add(new List<Pair<Tower, int>>());
					int index = -1;
					for (int j = 0; j < cannonTowers[i].Count; j++)
					{
						if (cannonTowers[i][j].x.Equals(cannonTowerDetails[bestUpgradeIndex].tower))
						{
							index = j;
							break;
						}
					}
					cannonTowers[i].RemoveAt(index);
					cannonTowers[i + 1].Add(new Pair<Tower, int>(cannonTowerDetails[bestUpgradeIndex].tower, totalTargettingTiles));
					cannonTowers[i + 1] = sortTowerList(cannonTowers[i + 1]);
				}
				else if (bestUpgradeIndex >= 0 || bestBuildingPlace != null)
				{
					tryBuildingTower();
					return;
				}
				else
					cannonPossible = false;
			}
		}
	}

	List<Pair<Tower, int>> sortTowerList(List<Pair<Tower, int>> unsorted)
	{
		List<Pair<Tower, int>> sorted = new List<Pair<Tower, int>>();
		var towers = from element in unsorted
			orderby -element.y
			select element;
		foreach (Pair<Tower, int> element in towers)
		{
			sorted.Add(element);
		}
		return sorted;
	}

	int calculateCannonCost(int level)
	{
		if (level == 0)
			return ctd.baseCost;
		int ans = ctd.baseUpgrade;
		for (int i = 1; i < level; i++)
			ans = (int)(ans * ctd.upgradeCoef);
		return ans;
	}

	int calculateArcherCost(int level)
	{
		if (level == 0)
			return atd.baseCost;
		int ans = atd.baseUpgrade;
		for (int i = 1; i < level; i++)
			ans = (int)(ans * atd.upgradeCoef);
		return ans;
	}

	int calculateArcherDmg(int level)
	{
		int ans = atd.baseDamage;
		for (int i = 0; i < level; i++)
			ans += (int)(ans * atd.damageCoef);
		return ans;
	}

	int calculateCannonDmg(int level)
	{
		int ans = ctd.baseDamage;
		for (int i = 0; i < level; i++)
			ans += (int)(ans * ctd.damageCoef);
		return ans;
	}

	int calculateLightHp(int level)
	{
		int ans = lud.baseHealth;
		for(int i = 0; i < level; i++)
		{
			ans = (int)(ans * lud.healthCoef);
		}
		return ans;
	}

	int calculateHeavyHp(int level)
	{
		int ans = hud.baseHealth;
		for (int i = 0; i < level; i++)
		{
			ans = (int)(ans * hud.healthCoef);
		}
		return ans;
	}

	class Pair<T, F>
	{
		public T x;
		public F y;

		public Pair(T x, F y)
		{
			this.x = x;
			this.y = y;
		}

		public override bool Equals(object other)
		{

			if (!other.GetType().Equals(typeof(Pair<T, F>)))
				return false;
			return ((Pair<T,F>)other).x.Equals(x) && ((Pair<T, F>)other).y.Equals(y);
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() * 10000 + y.GetHashCode();
		}
	}

	class TowerDetail
	{
		public Defender defender;
		public Tower tower;
		public List<Road> targetingRoads = new List<Road>();
		public int[] pointPerRoad;
		public string type;
		public List<Pair<Road, int>> roadNIndex = new List<Pair<Road, int>>();
		public HashSet<Pair<int, int>> xySet = new HashSet<Pair<int, int>>();

		public TowerDetail(Tower theTower, Defender defender)
		{
			pointPerRoad = new int[defender.roads.Count];
			this.defender = defender;
			for (int i = 0; i < pointPerRoad.GetLength(0); i++)
				pointPerRoad[i] = 0;
			tower = theTower;
			if (theTower.isAOE)
				type = "AOE";
			else
				type = "SingleTarget";
			RefreshDetails();
		}

		public void RefreshDetails()
		{
			int range = tower.range, tpa = tower.tpa, x = tower.x, y = tower.y;
			int totalTargetingTiles = 0;
			for(int roadIndex = 0; roadIndex < defender.roads.Count; roadIndex++)
			{
				Road road = defender.roads[roadIndex];
				for (int i = y - range; i <= y + range; i++)
				{
					if (i < 0 || i >= GameManager.gm.row)
						continue;
					for (int j = x - range; j <= x + range; j++)
					{
						if (j < 0 || j >= GameManager.gm.col)
							continue;
						if (Mathf.Abs(i - y) + Mathf.Abs(j - x) > range)
							continue;
						for (int k = 0; k < road.roadLength; k++)
						{
							if (road.roadPlaces[k].x == j && road.roadPlaces[k].y == i)
							{
								if (!targetingRoads.Contains(road))
									targetingRoads.Add(road);
								xySet.Add(new Pair<int, int>(j, i));
								roadNIndex.Add(new Pair<Road, int>(road, k));
								pointPerRoad[roadIndex]++;
								totalTargetingTiles++;
								break;
							}
						}
					}
				}
			}
			float AttackingTileCount = (float)xySet.Count / tower.tpa;
			int dmgPerTile;
			if (type.Equals("AOE"))
				dmgPerTile = (int)(tower.damage * AttackingTileCount / xySet.Count);
			else
				dmgPerTile = (int)(tower.damage * AttackingTileCount / totalTargetingTiles);

			foreach (Road road in defender.roads)
			{
				foreach (Pair<int, int> xy in xySet)
				{
					int index = -1;
					for (int i = 0; i < road.roadLength; i++)
					{
						if (road.roadPlaces[i].x == xy.x && road.roadPlaces[i].y == xy.y)
						{
							index = i;
							break;
						}
					}
					if (index == -1)
						continue;
					if (type.Equals("AOE"))
					{
						road.aoeDmgList[index] += dmgPerTile;
					}
					else
					{
						road.singleTargetDmgList[index] += dmgPerTile;
					}
				}
			}
		}
	}

	class Road
	{
		public List<RoadController> road = new List<RoadController>();
		public List<Pair<int, int>> roadPlaces = new List<Pair<int, int>>();
		public List<int> aoeDmgList;
		public List<int> singleTargetDmgList;
		public List<float> lightDangerList, heavyDangerList, dangerList;
		public int roadLength;
		Defender defender;

		public Road(RoadController[] roadArr, Defender defender)
		{
			this.defender = defender;
			roadLength = 0;
			foreach (RoadController thisRoad in roadArr)
			{
				road.Add(thisRoad);
				roadPlaces.Add(new Pair<int, int>(thisRoad.x, thisRoad.y));
				roadLength++;
			}
			aoeDmgList = new List<int>();
			singleTargetDmgList = new List<int>();
			lightDangerList = new List<float>();
			heavyDangerList = new List<float>();
			dangerList = new List<float>();
			print(roadLength);
			for (int i = 0; i < roadLength; i++)
			{
				aoeDmgList.Add(0);
				singleTargetDmgList.Add(0);
				lightDangerList.Add(0);
				heavyDangerList.Add(0);
				dangerList.Add(0);
			}
		}

		public void refreshDangerList()
		{
			refreshLightDangerList();
			refreshHeavyDangerList();
			for (int i = 0; i < roadLength; i++)
			{
				dangerList[i] = lightDangerList[i] + heavyDangerList[i];
			}
		}

		public void refreshLightDangerList()
		{
			int lightLvl = GameManager.gm.lightUnitLevel;
			int lightHp = defender.calculateLightHp(lightLvl);
			int totalAOEDmg = sumOfAOEDmgFromIndex(0);
			float baseDanger = 100f * ((lightHp * 3 - totalAOEDmg > 0) ? (lightHp * 3 - totalAOEDmg) / 3f / lightHp : 0f);
			for(int i = 0; i < roadLength; i++)
			{
				float thisDanger = baseDanger;
				if (i >= farthestLightEnemyIndex())
					thisDanger *= 2f;
				thisDanger *= (0.5f + 0.5f * (roadLength - i) / roadLength);
				lightDangerList[i] = thisDanger;
			}
		}

		public void refreshHeavyDangerList()
		{
			int heavyHp = defender.calculateHeavyHp(GameManager.gm.heavyUnitLevel);
			int stDmgTillHere = 0, unitsTillHere = 0, stDmgFromHere;
			int totalStDmg = sumOfSTDmgFromIndex(0);
			int farthestUnitIndex = farthestHeavyEnemyIndex();
			Pair<int, int> mostTile = mostHeaviesInTile();
			int mostUnitsIndex = mostTile.x, mostUnitsCount = mostTile.y;
			for(int i = 0; i < roadLength; i++)
			{
				stDmgTillHere += singleTargetDmgList[i];
				unitsTillHere += road[i].heavyUnitsCount;
				stDmgFromHere = totalStDmg - stDmgTillHere;
				float baseDanger = 100f * ((2 * unitsTillHere * heavyHp > stDmgTillHere / 2 + stDmgFromHere) ? 
					(2 * unitsTillHere * heavyHp - (stDmgTillHere / 2 + stDmgFromHere)) / 2f / unitsTillHere / heavyHp : 0f);
				float mult = 0.5f;
				if (unitsTillHere > 0)
					baseDanger *= mult + Mathf.Log(unitsTillHere);
				if (i > mostUnitsIndex)
					baseDanger *= 1.5f;
				if (i > farthestHeavyEnemyIndex())
					baseDanger *= 1.5f;
				heavyDangerList[i] = baseDanger;
			}
		}

		public int farthestEnemyIndex()
		{
			int index = roadLength - 1;
			bool found = false;
			for (; index >= 0; index--)
			{
				if (road[index].heavyUnitsCount > 0 || road[index].lightUnitsCount > 0)
				{
					found = true;
					break;
				}
			}
			if (found)
				return index;
			return -1;
		}

		public int farthestHeavyEnemyIndex()
		{
			int index = roadLength - 1;
			bool found = false;
			for (; index >= 0; index--)
			{
				if (road[index].heavyUnitsCount > 0)
				{
					found = true;
					break;
				}
			}
			if (found)
				return index;
			return -1;
		}

		public int farthestLightEnemyIndex()
		{
			int index = roadLength - 1;
			bool found = false;
			for (; index >= 0; index--)
			{
				if (road[index].lightUnitsCount > 0)
				{
					found = true;
					break;
				}
			}
			if (found)
				return index;
			return -1;
		}

		public Pair<int, int> mostLightsInTile()
		{
			Pair<int, int> answer;
			int maxCount = 0;
			int index = -1;
			for(int i = 0; i < roadLength; i++)
			{
				if(road[i].lightUnitsCount >= maxCount)
				{
					maxCount = road[i].lightUnitsCount;
					index = i;
				}
			}
			answer = new Pair<int, int>(index, maxCount);
			return answer;
		}

		public Pair<int, int> mostHeaviesInTile()
		{
			Pair<int, int> answer;
			int maxCount = 0;
			int index = -1;
			for (int i = 0; i < roadLength; i++)
			{
				if (road[i].heavyUnitsCount >= maxCount)
				{
					maxCount = road[i].heavyUnitsCount;
					index = i;
				}
			}
			answer = new Pair<int, int>(index, maxCount);
			return answer;
		}

		public int sumOfAOEDmgFromIndex(int index)
		{
			int ans = 0;
			for (int i = index; i < roadLength; i++)
				ans += aoeDmgList[i];
			return ans;
		}

		public int sumOfSTDmgFromIndex(int index)
		{
			int ans = 0;
			for (int i = index; i < roadLength; i++)
				ans += singleTargetDmgList[i];
			return ans;
		}
	}
}

