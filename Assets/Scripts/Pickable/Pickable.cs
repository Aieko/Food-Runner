using UnityEngine;

abstract public class Pickable : MonoBehaviour
{
    [SerializeField] private float rotSpeed;

    virtual protected void Awake()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.x, Random.Range(0, 360), transform.rotation.z);
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime);
    }

    abstract public void Effect();

}
