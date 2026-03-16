using UnityEngine;

public class shotController : MonoBehaviour
{
    public float deleteTime = 5.0f;　//削除時間


    void Start()
    {
        Destroy(gameObject, deleteTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

}

