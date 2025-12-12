using UnityEngine;

public class CueDancingGuy : MonoBehaviour
{
    void Start()
    {
        var camPosObj = GameObject.FindGameObjectWithTag("CameraPosition");
        var mouseRot = GameObject.FindGameObjectWithTag("MouseRotation");
        var camGun = GameObject.FindGameObjectWithTag("CameraGun");
        var player = GameObject.FindGameObjectWithTag("Player");

        camPosObj.transform.SetParent(null);
        mouseRot.GetComponent<MouseRotationController>().enabled = false;
        camGun.SetActive(false);

        GetComponent<Animator>().SetInteger("RandomDance", Random.Range(-1, 2));

        mouseRot.transform.position = transform.position - transform.forward * 5f + Vector3.up * 1.2f;

        camPosObj.transform.position = player.transform.position - player.transform.forward * 5f + Vector3.up * 1.2f;

        Vector3 lookTarget = new Vector3(transform.position.x, mouseRot.transform.position.y, transform.position.z);
        mouseRot.transform.LookAt(lookTarget);

        camPosObj.transform.LookAt(transform.position);

        player.SetActive(false);
    }
}
