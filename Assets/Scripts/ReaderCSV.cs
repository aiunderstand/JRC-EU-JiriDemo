using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ReaderCSV
{
    public static void ReadCSV(string path, List<Family> families)
    {
        if (!File.Exists(path))
        {
            return;
        }

        int limit = 1000;
        var lineCount = 0;
        string line;
        string[] fields = null;
     
        using (StreamReader reader = new StreamReader(path))
        {
            //check if family exist otherwise get family and update
            //shortcut: assume hub data is identical (eg family1 is on line 1 in file 1 and file 2) so we can index fast
            bool createNewFamily;
            if (families.Count == 0)
                createNewFamily = true;
            else
                createNewFamily = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (lineCount >0 && lineCount < limit) //skip header
                {
                    var attr = line.Split(',');

                    if (attr.Length > 0) // skip empty lines
                    {
                        if (createNewFamily)
                        {
                            int x = (int) float.Parse(attr[0], System.Globalization.CultureInfo.InvariantCulture);
                            int y = (int) float.Parse(attr[1], System.Globalization.CultureInfo.InvariantCulture);
                            int buurtcode = int.Parse(attr[2]);
                            string postcode = attr[3];
                            string famid = attr[4];
                            string family_with = attr[5];
                            float income = float.Parse(attr[6], System.Globalization.CultureInfo.InvariantCulture);

                            int persons = (int)float.Parse(attr[attr.Length - 4], System.Globalization.CultureInfo.InvariantCulture);
                            string young_fami = attr[attr.Length - 3];

                            List<string> people = new List<string>();
                            for (int i = 7; i < 7 + persons; i++)
                            {
                                people.Add(attr[i]);
                            }

                            Hub hub = new Hub(attr[attr.Length - 2], float.Parse(attr[attr.Length - 1], System.Globalization.CultureInfo.InvariantCulture));
                            List<Hub> hubs = new List<Hub>() { hub };

                            Family family = new Family(x, y, buurtcode, postcode, famid, family_with, income, people, persons, young_fami, hubs);
                            families.Add(family);
                        }
                        else //update family
                        {
                            Hub hub = new Hub(attr[attr.Length - 2], float.Parse(attr[attr.Length - 1], System.Globalization.CultureInfo.InvariantCulture));
                            families[lineCount - 1].Hubs.Add(hub);
                        }
                    }
                }

                lineCount++;
            }

            Debug.Log("Finished reading: " + path);
        }
    }

}
