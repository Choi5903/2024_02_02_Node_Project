using System;
using System.Collections.Generic;

[Serializable]
public class PlayerModel
{
    public string playerName;
    public int metal;
    public int crystal;
    public int deuterium;
    public List<PlanetModel> Planets;
    public PlayerModel(string name)
    {
        this.playerName = name;
        this.metal = 500;
        this.crystal = 300;
        this.deuterium = 100;
    }

    public void CollectResources()
    {
        metal += 10;
        crystal += 5;
        deuterium += 2;
    }
}

public class PlanetModel
{
    public int id;
    public string name;
    public int metal;
    public int crystal;
    public int deuterium;

    public PlanetModel(int id, string name)
    {
        this.id = id;
        this.name = name;
        this.metal = 500;
        this.crystal = 300;
        this.deuterium = 100;
    }
}
