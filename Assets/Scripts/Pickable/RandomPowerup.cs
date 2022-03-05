using UnityEngine;

public class RandomPowerup : Pickable
{
    public override void Effect()
    {
        var num = Random.Range(0, 2);
        switch (num)
        {
            case 0:
                GameManager.Instance.DoubleScorePowerupActivate();
                break;
            case 1:
                GameManager.Instance.ImmortalityPowerupActivate();
                break;
            default:
                break;
        }
       
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
