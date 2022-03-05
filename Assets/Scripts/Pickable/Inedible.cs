
public class Inedible : Pickable
{
    public override void Effect()
    {
        GameManager.Instance.PlayExplosionSound();
        GameManager.Instance.GameOver();
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
