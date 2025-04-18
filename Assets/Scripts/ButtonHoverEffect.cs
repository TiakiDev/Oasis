using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector3 normalScale;
    private Vector3 targetScale;
    [SerializeField] private float animationTime = 0.3f; // Czas animacji
    [SerializeField] private float scaleFactor = 1.1f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        normalScale = rectTransform.localScale;
        targetScale = normalScale * scaleFactor; 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleEffect(rectTransform.localScale, targetScale, animationTime));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleEffect(rectTransform.localScale, normalScale, animationTime));
    }

    private System.Collections.IEnumerator ScaleEffect(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.localScale = endScale;
    }
}