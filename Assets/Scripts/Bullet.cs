using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<SphereCollider>().enabled = false;
    }
}
