using UnityEditor.Analytics;
using UnityEngine;

public class CueDancingGuy : MonoBehaviour
{
    private GameObject cameraPosition;
    void Start()
    {
        GameObject.FindGameObjectWithTag("MouseRotation").GetComponent<MouseRotationController>().enabled = false;
        GameObject.FindGameObjectWithTag("CameraGun").SetActive(false);
        GameObject.FindGameObjectWithTag("Player").SetActive(false);
        GetComponent<Animator>().SetInteger("RandomDance", UnityEngine.Random.Range(-1, 2));
        cameraPosition = GameObject.FindGameObjectWithTag("CameraPosition");
        cameraPosition.transform.SetParent(null);
        cameraPosition.transform.position = transform.localPosition + Camera.main.transform.forward * 5f;
        cameraPosition.transform.LookAt(transform.position);
        cameraPosition.transform.Rotate(0f, 180f, 0f, Space.Self);
    }
}
