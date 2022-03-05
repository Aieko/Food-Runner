using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    private float leftDead=-20;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.gameOver == false)
        {
            var move = new Vector3(-1 * Time.deltaTime * GameManager.Instance.OMS.speed, 0, 0);
            transform.Translate(move);

        }
        if(transform.position.x < leftDead)
        {
            Destroy(gameObject);
        }
    }
}
