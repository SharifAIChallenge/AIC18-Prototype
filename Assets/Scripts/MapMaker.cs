using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class MapMaker : MonoBehaviour {
    public GameObject GrassPrefab;
    public GameObject RoadPrefab;
    public float tileSize = 2;
    public static int numberOfRows = 20;
    public string[][] map;
    public int numOfRoads;
    public int[] roadLength;
    [HideInInspector]
    public GameObject[][] tiles = new GameObject[numberOfRows][];
    [HideInInspector]
    public int numOfCols;
    float tileLength;
    GameObject currentRowObject;
	public string[] RoadCordinates;
	// Use this for initialization
	void Start () {
        numOfRoads = GameManager.gm.roadNumber;
        tileLength = GrassPrefab.GetComponent<BoxCollider>().size.x;
		map = new string[GameManager.gm.row] [];
        //print(tileLength);
        int roadNumber;
        int tileNumber;
		string[] currentRoad;
		int x;
		int y;
		for (int i = 0; i < GameManager.gm.row; i++) {
			map [i] = new string[GameManager.gm.col];
			for (int j = 0; j < GameManager.gm.col; j++) {
				map [i] [j] = "g";
				GameManager.gm.vision [i] [j] = "g";
			}
		}
		for (int i = 0; i < numOfRoads; i++) {
			currentRoad = RoadCordinates [i].Split (',');
			for (int j = 0; j < roadLength [i]; j++) {
				string[] s = currentRoad [j].Split (':');
				Int32.TryParse(s[0], out x);
				Int32.TryParse(s[1], out y);
				map [y] [x] = "r";
				GameManager.gm.vision [y] [x] = "r";
			}
		}
        for (int i = 0; i < numberOfRows; i++)
        {
            currentRowObject = new GameObject("row " + i);
            currentRowObject.transform.parent = this.transform;
            string[] row = map[i];
            numOfCols = row.GetLength(0);
            tiles[i] = new GameObject[numOfCols];
            for (int j = 0; j < numOfCols; j++)
            {
                if (row[j].Equals("g"))
                    InstantiateTile(GrassPrefab, i, j);
				else if(row[j].Equals("r"))
				{
					InstantiateTile(RoadPrefab, i, j);
				}
            }
        }
		for (int i = 0; i < numOfRoads; i++) {
			currentRoad = RoadCordinates [i].Split (',');
			for (int j = 0; j < roadLength [i]; j++) {
				string[] s = currentRoad [j].Split (':');
				Int32.TryParse (s [0], out x);
				Int32.TryParse (s [1], out y);
				GameManager.gm.Road [i] [j] = tiles [y] [x].GetComponent<RoadController>();
				GameManager.gm.Road [i] [j].roadIndex [i] = j;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void InstantiateTile(GameObject prefab, int i, int j)
    {
        tiles[i][j] = (GameObject)Instantiate(prefab, new Vector3
                        (this.transform.position.x + tileSize * (tileLength * (j - numOfCols / 2f)),
                        this.transform.position.y +  tileSize * (tileLength * ((numberOfRows / 2f) - i)), 0f)
                        , Quaternion.Euler(new Vector3()));
        if (prefab.tag == "Road")
        {
            tiles[i][j].GetComponent<RoadController>().y = i;
            tiles[i][j].GetComponent<RoadController>().x = j;
        }else
        {

            tiles[i][j].GetComponent<TowerPlaceManager>().y = i;
            tiles[i][j].GetComponent<TowerPlaceManager>().x = j;
        }
        //print("tile" + i + " " + " " + j + ": " + (this.transform.position.y + (tileLength * (numberOfRows / 2f) - i)));
        tiles[i][j].transform.parent = currentRowObject.transform;
    }
}
