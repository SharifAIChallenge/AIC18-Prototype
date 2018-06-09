using UnityEngine;
using System.Collections.Generic;
using System;
public class Attacker : MonoBehaviour{

	List<Unit> aliveUnits = new List<Unit>();
	int l2h_coef = 5;
	List<List<string> > to_create_type = new List<List<string> >();
	List<List<int> > to_create_path = new List<List<int> >();
	int lightLevel = 0;
	int heavyLevel = 0; 
	double spend_base = 0.3;
	double spend_increase = 0.01;
	double spend_current;
	double heavy_prob = 0.6;
	List<string> create_temp = new List<string>();
	int archer_base_cost;
	int archer_per_cell_cost;
	int canon_base_cost;
	int canon_per_cell_cost;
	int max_planning;



	public void removeUnit(Unit unit)
	{
		aliveUnits.Remove(unit);
	}

	public void addUnit(Unit unit)
	{
		aliveUnits.Add(unit);
	}
	void Awake(){
		spend_current = spend_base;
	}
	public void whatToDo()
	{
		List<int> archer_x = get_tow_x('a');
		List<int> archer_y = get_tow_y('a');
		List<int> canon_x = get_tow_x('c');
		List<int> canon_y = get_tow_y('c');


		bool[][][] pathTable = pathPresent();
		List<int> archer_path_count = towerPathCount(pathTable, 'a');
		List<int> canon_path_count = towerPathCount(pathTable, 'c');

		double[] path_archer_cost = pathTowerCount(pathTable, 'a', archer_path_count);
		double[] path_canon_cost = pathTowerCount(pathTable, 'c', canon_path_count);


		System.Random random = new System.Random();

		double sum = 0;
		for(int i=0; i<path_archer_cost.Length; i++)
			sum += 1.0/path_archer_cost[i];
		for(int i=0; i<path_canon_cost.Length; i++)
			sum += 1.0/path_canon_cost[i];

		int tmpGold = GameManager.gm.attackerGold;
		while(tmpGold > 0)				
		{
			double choose = random.NextDouble() * sum;
			double curr = 0;
			int archer_pick = -1;
			int canon_pick = -1;
			for(int i = 0; i<path_archer_cost.Length; i++)
			{
				curr += 1.0/path_archer_cost[i];
				if(curr >= choose)
				{
					archer_pick = i;
					continue;
				}
			}
			if(archer_pick >= 0)
				for(int i = 0; i<path_canon_cost.Length; i++)
				{
					curr += 1.0/path_canon_cost[i];
					if(curr >= choose)
					{
						canon_pick = i;
						continue;
					}
				}

			int step = random.Next(1, max_planning+1);

			while(to_create_type.Count < step+1)
			{
				to_create_type.Add(new List<String>());
				to_create_path.Add(new List<int>());
			}

			if(archer_pick >= 0)
			{
				to_create_type[step].Add("HeavyUnit");
				to_create_path[step].Add(archer_pick);
				tmpGold -= HeavyUnitDetail.hud.cost (heavyLevel);
			}
			else
			{
				to_create_type[step].Add("LightUnit");
				to_create_path[step].Add(canon_pick);
				tmpGold -= LightUnitDetail.lud.cost (lightLevel);
			}
		}

	}

	List<int> get_tow_x(char type){
		List<int> result = new List<int>();
		for(int i=0; i<GameManager.gm.col; i++)
			for(int j=0; j<GameManager.gm.row; j++)
				if(GameManager.gm.vision[j][i].ToCharArray()[0] == type)
					result.Add(i);
		return result;
	}
			



	List<int> get_tow_y(char type){
		List<int> result = new List<int>();
		for(int i=0; i<GameManager.gm.col; i++)
			for(int j=0; j<GameManager.gm.row; j++)
				if(GameManager.gm.vision[j][i].ToCharArray()[0] == type)
					result.Add(j);
		return result;	

	} 


	bool[][][] pathPresent(){
		bool[][][] result = new bool[GameManager.gm.row][][];
		for (int i = 0; i < GameManager.gm.row; i++) {
			result[i] = new bool[GameManager.gm.col][];
			for (int j = 0; j < GameManager.gm.col; j++) {
				result[i][j] = new bool[GameManager.gm.roadNumber];
			}
		}
		for(int i=0; i<GameManager.gm.roadNumber; i++)
			for(int j=0; j<GameManager.gm.roadLength[i]; j++)
				result[GameManager.gm.Road[i][j].y][GameManager.gm.Road[i][j].x][i] = true;
		return result;	
	}

		



	List<int> towerPathCount(bool[][][] pathTable, char type){
		List<int> result = new List<int>();
		for(int i=0; i<GameManager.gm.col; i++)
			for(int j=0; j<GameManager.gm.row; j++)
				if(GameManager.gm.vision[j][i].ToCharArray()[0] == type)
				{
					int range;
					if(type == 'a')
						range = ArcherTowerDetail.atd.range;
					else
						range = CannonTowerDetail.ctd.range;

					bool[] pathTemp = new bool[GameManager.gm.roadNumber];
					for(int k= -range; k<=range; k++)
						for(int w= -range+Mathf.Abs(k); w<= range-Mathf.Abs(k); w++)
							if(i+k < 0 || i+k >= GameManager.gm.col || j+w < 0 || j+w >= GameManager.gm.row)
								continue;
							else
								for(int p=0; p<GameManager.gm.roadNumber; p++)
									if(pathTable[j][i][p])
										pathTemp[p] = true;					
					int temp = 0;

					for(int p=0; p<GameManager.gm.roadNumber; p++)
						if(pathTemp[p])
							temp ++;
					result.Add(temp);	
				}
		return result;	

	}





	double[] pathTowerCount(bool[][][] pathTable, char type, List<int> path_count){
		double[] result = new double[GameManager.gm.roadNumber];
		int count = 0;
		for(int i=0; i<GameManager.gm.col; i++)
			for(int j=0; j<GameManager.gm.row; j++)
				if(GameManager.gm.vision[j][i].ToCharArray()[0] == type)
				{
					int range;
					if(type == 'a')
						range = ArcherTowerDetail.atd.range;
					else
						range = CannonTowerDetail.ctd.range;

					int[] pathTemp = new int[GameManager.gm.roadNumber];
					for(int k= -range; k<=range; k++)
						for(int w= -range+Mathf.Abs(k); w<= range-Mathf.Abs(k); w++)
							if(i+k < 0 || i+k >= GameManager.gm.col || j+w < 0 || j+w >= GameManager.gm.row)
								continue;
							else
								for(int p=0; p<GameManager.gm.roadNumber; p++)
									if(pathTable[j][i][p])
										pathTemp[p] += 1;					
					for(int p=0; p<GameManager.gm.roadNumber; p++)
						if(pathTemp[p] > 0)
						if(type == 'a')
						{
							result[p] += (double) archer_base_cost / path_count[count];
							result[p] += (double) archer_per_cell_cost * pathTemp[p] / path_count[count];
						}
						else
						{
							result[p] += (double) canon_base_cost / path_count[count];
							result[p] += (double) canon_per_cell_cost * pathTemp[p] / path_count[count];
						}	

					count ++;
				}
		return result;				

	}


	public void updateGame(){
		List<string> temp1;
		List<int> temp2;
		if (to_create_type.Count > 0) {
			temp1 = to_create_type [0];
			temp2 = to_create_path [0];
			to_create_path.RemoveAt (0);
			to_create_type.RemoveAt (0);
			for (int i = 0; i < temp1.Count; i++)
				GameManager.gm.createUnit (temp1 [i], temp2 [i]);
		}
	}

}
