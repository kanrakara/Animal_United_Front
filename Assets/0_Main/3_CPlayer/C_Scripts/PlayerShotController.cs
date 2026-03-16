using UnityEngine;

public class PlayerShotController : MonoBehaviour
{
    [Header("削除時間")]
    public float deleteTime = 5.0f;

    void Start()
    {
        Destroy(gameObject, deleteTime);
    }

    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject, 0.2f);
    }
}
