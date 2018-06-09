using UnityEngine;
using System.Collections;
using UnityEditor;

public class TowerPlaceManager : MonoBehaviour {
	public GameObject tower = null;
    public bool hasTower = false;
	public bool canPlaceTower = true;
    public int x;
    public int y;
	public int rta = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        if (hasTower)
        {
            EditorUtility.DisplayDialog(tower.name, tower.GetComponent<Tower>().getInfo(), "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Empty Room", "No Towers Here!", "OK");
        }
    }

    public void setTower(GameObject tower)
    {
        this.tower = tower;
        if (!hasTower && tower != null)
            hasTower = true;
    }

    public GameObject getTower()
    {
        return this.tower;
    }

    public void removeTower()
    {
        this.tower = null;
        this.hasTower = false;
    }
}
