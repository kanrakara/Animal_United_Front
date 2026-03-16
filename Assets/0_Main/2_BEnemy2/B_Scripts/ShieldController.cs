using UnityEngine;

public class ShieldController : MonoBehaviour
{


 private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Destroy(other.gameObject);
        }
    }


}
