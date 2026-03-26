using UnityEngine;

[System.Serializable]
public class WeightedOption
{
    public string name;
    public float weight;
}

[System.Serializable]
public class DrillStep
{
    public string action;

    public float delay;
    public float delayMin;
    public float delayMax;

    public bool random;
    public bool waitForTrigger;
    public WeightedOption[] weights;
}

[System.Serializable]
public class DrillSequence
{
    public string type;
    public DrillStep[] sequence;
}
