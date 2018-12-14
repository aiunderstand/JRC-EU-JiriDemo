using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Terrain;
using UnityEngine;

public class Main : MonoBehaviour
{
    List<Family> families;
    [SerializeField]
    Vector3 offset = new Vector3(113200, 0, 477000);
    public GameObject[] markers;
    public float thresholdEdu = 100;
    public float thresholdMedical = 100;
    public Material[] familyStyles;
  
    public void CreateMap() {
        ClearData();

        LoadMap();
        LoadCustomData();
    }

    private void LoadMap()
    {
        var mapengine = FindObjectOfType<MapViewer>();
        mapengine.CreateTiles();
    }

    private void ClearData()
    {
        var folder = GameObject.Find("hubs");
        Destroy(folder);
        folder = GameObject.Find("families");
        Destroy(folder);
    }

    void LoadCustomData()
    {
        families = new List<Family>();

        //read healthhub.csv
        ReaderCSV.ReadCSV(Application.dataPath + "/Data/healthhub.csv", families);

        //read eduhub.csv
        ReaderCSV.ReadCSV(Application.dataPath + "/Data/eduhub.csv", families);

        //Plot families on map
        PlotFamiliesOnMap(families);

        //read hub shapefiles
        ReadShapes(Application.dataPath + "/Data/hubShapes/");

        //Apply style on families
        ApplyStyleOnFamilies();

    }

    public void ApplyStyleOnFamilies()
    {
        foreach (var f in families)
        {
            //both bad
            if ((f.Hubs[0].HubDist > thresholdMedical) && (f.Hubs[1].HubDist > thresholdEdu))
            {
                f.Go.GetComponent<MeshRenderer>().material = familyStyles[2];
            }
            else
            {
                //one bad
                if (f.Hubs[0].HubDist > thresholdMedical || f.Hubs[1].HubDist > thresholdEdu)
                {
                    f.Go.GetComponent<MeshRenderer>().material = familyStyles[1];
                }
                else //both good
                {
                    f.Go.GetComponent<MeshRenderer>().material = familyStyles[0];
                }
            }
        }
    }

    private void ReadShapes(string path)
    {
        string[] shapefiles = Directory.GetFiles(path, "*.shp", SearchOption.TopDirectoryOnly);

        //foreach shape
        var folder = new GameObject("hubs");

        foreach (var shape in shapefiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(shape);
            var folderPath = Path.GetDirectoryName(shape);
            var file = Path.Combine(folderPath, fileName);

            var hubShapes = ShapeImporter.ReadShapes(file);

            foreach (var h in hubShapes)
            {
                var go =Instantiate(markers[1], new Vector3((float) h.Shape.X - offset.x, (float) h.Shape.Z, (float) h.Shape.Y - offset.z), Quaternion.Euler(0, 0, 0));
                go.transform.parent = folder.transform;
                go.AddComponent<ShapeAttributes>().attributes = h.Metadata;
                go.GetComponentInChildren<TMPro.TextMeshPro>().text = h.Metadata["Poi_Name"];

                //hack 
                if (h.Metadata["Poi_Name"].Contains("FONS VITAE LYCEUM RKSG"))
                {
                    go.transform.position = new Vector3(go.transform.position.x, -16f, go.transform.position.z);
                   // go.GetComponentInChildren<TMPro.TextMeshPro>().text = "";
                }
            }
        }

    }

    private void PlotFamiliesOnMap(List<Family> families)
    {
        var folder = new GameObject("families");

        foreach (var f in families)
        {
            var go = Instantiate(markers[0], new Vector3(f.X - offset.x, 0, f.Y -offset.z), Quaternion.Euler(0,0,0));
            go.transform.parent = folder.transform;

            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("X", f.X.ToString());
            metadata.Add("Y", f.Y.ToString());
            metadata.Add("Buurtcode", f.Buurtcode.ToString());
            metadata.Add("Postcode", f.Postcode);
            metadata.Add("FamId", f.FamId);
            metadata.Add("Familiy_with", f.Family_with);
            metadata.Add("Income", f.Income.ToString());

            for (int i = 0; i < f.People.Count; i++)
            {
                metadata.Add("People" +i, f.People[i].ToString());
            }

            metadata.Add("Persons", f.Persons.ToString());
            metadata.Add("Young_fami", f.Young_fami.ToString());

            for (int i = 0; i < f.Hubs.Count; i++)
            {
                metadata.Add("Hubs" + i +"_HubName", f.Hubs[i].HubName);
                metadata.Add("Hubs" + i +"_HubDist", f.Hubs[i].HubDist.ToString());
            }

            go.AddComponent<ShapeAttributes>().attributes = metadata;

            if (f.Persons ==3)
                go.transform.localScale = new Vector3(20,20,20);

            f.Go = go;
        }
    }
}
