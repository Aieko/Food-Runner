using UnityEngine;

public class MoveTowards : MonoBehaviour
{

    private float deadZone = -10;

    [SerializeField] private bool randomRotateOnEnable = false;

    [SerializeField] private GameObject meshObject;
    

    private int direction;

    private void OnEnable()
    {
        direction = transform.rotation.eulerAngles.y == 180? 1 : - 1;

        if (randomRotateOnEnable)
        {
            if (meshObject == null) Debug.LogError("Mesh is null");
            else meshObject.transform.RotateAround(transform.position, Vector3.up, Random.Range(0, 361));
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.gameOver == false)
        {
            var move = new Vector3(direction * Time.deltaTime * GameManager.Instance.OMS.speed, 0, 0);
            transform.Translate(move);
        }

        if (transform.position.x < deadZone)
        {
            gameObject.SetActive(false);
        }
    }
}
