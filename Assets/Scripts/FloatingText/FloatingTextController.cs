using System.Collections.Generic;
using UnityEngine;


public class FloatingTextController : MonoBehaviour
{
    [SerializeField] ObjectPool floatingTextPools;
    private static FloatingTextController instance;
    public static FloatingTextController Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public FloatingTextBaseBehaviour SpawnFloatingText(string text, Vector3 position, Quaternion rotation, float scale)
    {
        GameObject floatingTextObject = floatingTextPools.GetObject();
        floatingTextObject.transform.position = position;
        floatingTextObject.transform.rotation = rotation;
        floatingTextObject.SetActive(true);

        FloatingTextBaseBehaviour floatingTextBehaviour = floatingTextObject.GetComponent<FloatingTextBaseBehaviour>();
        floatingTextBehaviour.Activate(text, scale);

        return floatingTextBehaviour;
    }
}
