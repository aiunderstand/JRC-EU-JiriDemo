using System.Collections.Generic;

public class Family
{
    public int X;
    public int Y;
    public int Buurtcode;
    public string Postcode;
    public string FamId;
    public string Family_with;
    public float Income;
    public List<string> People;
    public int Persons;
    public string Young_fami;
    public List<Hub> Hubs;

    public Family(int x, int y, int buurtcode, string postcode, string famid, string family_with, float income, List<string> people, int persons, string young_fami, List<Hub> hubs)
    {
        X = x;
        Y = y;
        Buurtcode = buurtcode;
        Postcode = postcode;
        FamId = famid;
        Family_with = family_with;
        Income = income;
        People = people;
        Persons = persons;
        Young_fami = young_fami;
        Hubs = hubs;
    }
}

public class Hub
{

    public string HubName;
    public float HubDist;

    public Hub(string hubname, float hubdist)
    {
        HubName = hubname;
        HubDist = hubdist;
    }
}
