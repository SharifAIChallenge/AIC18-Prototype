using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoadController : MonoBehaviour {

    public TextMesh lightText;
    public TextMesh heavyText;
    public List<GameObject> lightUnits = new List<GameObject>();
    public List<GameObject> heavyUnits = new List<GameObject>();
    public int x;
    public int y;
    public int lightUnitsCount = 0, heavyUnitsCount = 0;
	public int[] roadIndex;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void OnMouseDown()
    {
        if(lightUnits.Count == 0 && heavyUnits.Count == 0)
        {
            UnityEditor.EditorUtility.DisplayDialog("Road No.", "No Units in This Block", "OK");
        }
        else
        {
            string message = "Light Units: \n";
            foreach (GameObject unit in lightUnits)
            {
                message += unit.GetComponent<Unit>().getInfo() + "\n";
            }
            message += "Heavy Units: \n";
            foreach (GameObject unit in heavyUnits)
            {
                message += unit.GetComponent<Unit>().getInfo() + "\n";
            }
            UnityEditor.EditorUtility.DisplayDialog("Road No.", message, "OK");
        }
    }

    public void addUnit(GameObject unit)
    {
        if (unit.tag.Equals("LightUnit"))
        {
            if (!lightUnits.Contains(unit))
            {
                lightUnitsCount++;
                lightUnits.Add(unit);
                print("Added " + this.gameObject + " - " + unit + " Current Light Count:" + lightUnitsCount);
                unit.transform.position = this.transform.position + new Vector3(-0.4f + 0.08f * (lightUnits.Count - 1), 0.3f, 0f);
            }
        }
        else
        {
            if (!heavyUnits.Contains(unit))
            {
                heavyUnitsCount++;
                heavyUnits.Add(unit);
                print("Added " + this.gameObject + " - "+ unit + " Current Heavy Count:" + heavyUnitsCount);
                unit.transform.position = this.transform.position + new Vector3(-0.35f + 0.1f * (heavyUnits.Count - 1), -0.25f, 0f);
            }
        }
    }

    public bool removeUnit(GameObject unit)
    {
        bool output;
        if (unit.tag.Equals("LightUnit"))
        {
            output = lightUnits.Remove(unit);
        }
        else
        {
            output = heavyUnits.Remove(unit);
        }
        if (output)
        {
            if (unit.tag.Equals("LightUnit"))
                lightUnitsCount--;
            else
                heavyUnitsCount--;
            print("Removed!");
            refreshPlaces();
        }
        return output;
    }

    public List<GameObject> getUnits()
    {
        List<GameObject> output = new List<GameObject>(heavyUnits);
        output.AddRange(lightUnits);
        return output;
    }

    private void refreshPlaces()
    {
        float lightHpMed = 0f, HeavyHpMed = 0f;
        for (int i = 0; i < lightUnitsCount; i++)
        {
            lightUnits[i].transform.position = this.transform.position + new Vector3(-0.5f + 0.1f * i, 0.3f, 0f);
            lightHpMed += lightUnits[i].GetComponent<Unit>().getHealth();
            print(lightHpMed);
        }
        for (int i = 0; i < heavyUnitsCount; i++)
        {
			if (heavyUnits [i] == null)
				break;
            heavyUnits[i].transform.position = this.transform.position + new Vector3(-0.5f + 0.1f * i, -0.3f, 0f);
            HeavyHpMed += heavyUnits[i].GetComponent<Unit>().getHealth();
            print(HeavyHpMed);
        }
        lightHpMed = lightHpMed / lightUnitsCount;
        HeavyHpMed = HeavyHpMed / heavyUnitsCount;
        if (lightUnitsCount != 0)
            lightText.text = lightHpMed.ToString();
        else
            lightText.text = "0";
        if (heavyUnitsCount != 0)
            heavyText.text = HeavyHpMed.ToString();
        else
            heavyText.text = "0";
    }
}
