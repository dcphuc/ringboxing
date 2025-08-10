using TMPro;
using UnityEngine;
using DG.Tweening;


public class FloatingTextHitBehaviour : FloatingTextBaseBehaviour
{
    [SerializeField] TextMeshProUGUI floatingText;

    [Space]
    [SerializeField] float delay;
    [SerializeField] float disableDelay;
    [SerializeField] float scale;
    [SerializeField] float time;
    [SerializeField] Ease easing;

    [Space]
    [SerializeField] float scaleTime;
    [SerializeField] Ease scaleEasing;

    private Vector3 defaultScale;

    private void Awake()
    {
        defaultScale = transform.localScale;
    }

    public override void Activate(string text, float scale = 1.0f)
    {
        floatingText.text = text;

        int sign = Random.value >= 0.5f ? 1 : -1;

        transform.localScale = defaultScale * scale * this.scale;
        transform.localRotation = Quaternion.Euler(0, -45f, 18 * sign);

        transform.DOLocalRotate(Quaternion.Euler(0, -45f, 0).eulerAngles, time).SetEase(easing).OnComplete(delegate
        {
            gameObject.SetActive(false);
            gameObject.GetComponent<PooledObject>().Pool.ReturnObject(gameObject);
        });
        transform.DOScale(defaultScale, scaleTime).SetEase(scaleEasing);
    }
}
