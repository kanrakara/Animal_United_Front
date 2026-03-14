using UnityEngine;

public class BossSlash : MonoBehaviour
{
    public float deleteTime = 0.5f;

    void Start()
    {
        Destroy(gameObject, deleteTime);
    }

}
