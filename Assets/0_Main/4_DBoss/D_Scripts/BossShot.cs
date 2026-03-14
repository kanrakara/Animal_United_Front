using UnityEngine;

public class BossShot : MonoBehaviour
{
    public float deleteTime = 5.0f;

    void Start()
    {
        Destroy(gameObject, deleteTime);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    Destroy(gameObject);
    //}

}
