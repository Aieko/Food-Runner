using UnityEngine;

public class MoveBomb : MonoBehaviour
{
    private float leftDead = -10;

    [SerializeField] private float exMove = 5f;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.gameOver == false)
        {
            transform.Translate(Vector3.left * Time.deltaTime * (GameManager.Instance.OMS.speed + exMove));

        }
        if (transform.position.x < leftDead)
        {
            Destroy(gameObject);
        }
    }
}
