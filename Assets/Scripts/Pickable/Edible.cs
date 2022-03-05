using UnityEngine;
public class Edible : Pickable
{
    public int scoreToAdd;
    public Color color;
    public override void Effect()
    {
        GameManager.Instance.PickupEdible(scoreToAdd);  
    }

    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    
}
