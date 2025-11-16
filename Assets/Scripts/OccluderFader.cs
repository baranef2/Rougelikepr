using UnityEngine;
using System.Collections;

/// <summary>
/// Saydamlaþabilen bir objenin üzerine eklenir. 
/// Materyalinin saydamlýðýný hýzlýca deðiþtirmeyi saðlar.
/// DÝKKAT: Bu objenin materyali "Transparent" veya "Fade" modunda olmalýdýr!
/// </summary>
[RequireComponent(typeof(Renderer))]
public class OccluderFader : MonoBehaviour
{
    #region SETTINGS
    [Tooltip("Tamamen saydam olduðundaki alfa deðeri")]
    [SerializeField] private float fadedAlpha = 0.2f;
    [Tooltip("Ne kadar hýzlý saydamlaþacaðý")]
    [SerializeField] private float fadeSpeed = 8f;
    #endregion

    #region STATE
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private Color _originalColor;
    private Coroutine _fadeRoutine;

    // Shader'daki renk parametresinin ID'sini önbelleðe alýyoruz (performans için)
    // --- DOÐRU SATIR ---
    private static readonly int _colorPropertyID = Shader.PropertyToID("_BaseColor");
    #endregion

    #region UNITY_METHODS
    private void Awake()
    {
        // Gerekli component'leri ve varsayýlan deðerleri al
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        // Varsayýlan rengi kaydet (materyalin kendi rengi)
        // Eðer materyalin "_Color" adýnda bir property'si yoksa bu satýr hata verebilir.
        // Genellikle Standard, URP Lit, HDRP Lit shader'larda bu isim "_Color" veya "_BaseColor" olur.
        // Biz "_Color" varsayýyoruz.
        if (_renderer.material.HasProperty(_colorPropertyID))
        {
            _originalColor = _renderer.material.color;
        }
        else
        {
            // Alternatif olarak _BaseColor'ý deneyebilir veya manuel bir renk atayabilirsiniz
            _originalColor = Color.white;
            Debug.LogWarning($"Materyalde '_Color' property'si bulunamadý: {name}. Varsayýlan olarak 'White' kullanýlýyor.");
        }

        // Baþlangýçta opak yap
        SetAlpha(_originalColor.a);
    }
    #endregion

    #region PUBLIC_METHODS
    /// <summary>
    /// Objeyi hedef alfa deðerine doðru saydamlaþtýrýr.
    /// </summary>
    public void FadeOut()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeTo(fadedAlpha));
    }

    /// <summary>
    /// Objeyi orijinal opaklýðýna geri döndürür.
    /// </summary>
    public void FadeIn()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeTo(_originalColor.a));
    }
    #endregion

    #region PRIVATE_METHODS
    private IEnumerator FadeTo(float targetAlpha)
    {
        // PropertyBlock'u renderer'dan oku
        _renderer.GetPropertyBlock(_propBlock);

        // Mevcut rengi al veya varsayýlana dön
        Color currentColor = _renderer.material.HasProperty(_colorPropertyID)
            ? _propBlock.GetColor(_colorPropertyID)
            : _originalColor;

        // Renk zaten hedefe yakýnsa rutini çalýþtýrma
        if (Mathf.Abs(currentColor.a - targetAlpha) < 0.01f)
            yield break;

        // Hedef alfaya doðru yumuþak geçiþ (Lerp)
        while (Mathf.Abs(currentColor.a - targetAlpha) > 0.01f)
        {
            float newAlpha = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * fadeSpeed);
            currentColor.a = newAlpha;

            SetAlpha(newAlpha, currentColor);

            yield return null; // Bir sonraki kareye kadar bekle
        }

        // Tam olarak hedef alfayý ayarla
        SetAlpha(targetAlpha, currentColor);
    }

    /// <summary>
    /// Performanslý þekilde materyalin sadece alfa deðerini ayarlar.
    /// </summary>
    private void SetAlpha(float alpha, Color? baseColor = null)
    {
        Color colorToSet = baseColor ?? _originalColor;
        colorToSet.a = alpha;

        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(_colorPropertyID, colorToSet);
        _renderer.SetPropertyBlock(_propBlock);
    }
    #endregion
}