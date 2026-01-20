using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class OccluderFader : MonoBehaviour
{
    #region SETTINGS
    [Header("Fading Ayarlarý")]
    [Tooltip("Tamamen saydam olduðundaki alfa deðeri")]
    [SerializeField] private float fadedAlpha = 0.2f;
    [Tooltip("Ne kadar hýzlý saydamlaþacaðý")]
    [SerializeField] private float fadeSpeed = 10f; // Biraz hýzlandýrdým

    [SerializeField] private string colorPropertyName = "_BaseColor";
    #endregion

    #region STATE
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private Color _baseColor;
    private Coroutine _fadeRoutine;
    private int _colorID;
    private bool _isInitialized = false;
    private float _currentAlpha = 1f;
    #endregion

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        // Oyun baþlar baþlamaz objeyi tamamen opak (1.0) hale getir.
        // Bu sayede editörde saydam býraksan bile oyunda düzelir.
        FadeIn(true);
    }

    private void Initialize()
    {
        if (_isInitialized) return;

        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
        _colorID = Shader.PropertyToID(colorPropertyName);

        if (_renderer != null)
        {
            // DÝKKAT: .material yerine .sharedMaterial kullanýyoruz.
            // Bu sayede konsoldaki "Instantiating material" hatasýný önleriz.
            if (_renderer.sharedMaterial.HasProperty(_colorID))
            {
                _baseColor = _renderer.sharedMaterial.GetColor(_colorID);
            }
            else
            {
                // URP varsayýlan _BaseColor yoksa _Color dene
                int altID = Shader.PropertyToID("_Color");
                if (_renderer.sharedMaterial.HasProperty(altID))
                {
                    _colorID = altID;
                    _baseColor = _renderer.sharedMaterial.GetColor(_colorID);
                }
                else
                {
                    _baseColor = Color.white;
                }
            }

            // KRÝTÝK NOKTA:
            // Materyalin orijinal rengini al ama Alphasýný 1 (Tam Opak) olarak zorla.
            // Böylece materyal "bozuk" veya "silik" görünmez.
            _baseColor.a = 1f;
        }

        _isInitialized = true;
    }

    public void FadeOut()
    {
        if (!_isInitialized) Initialize();
        if (_renderer == null) return;

        // Zaten hedeflediðimiz yerdeysek tekrar coroutine baþlatma
        if (Mathf.Abs(_currentAlpha - fadedAlpha) < 0.01f) return;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeTo(fadedAlpha));
    }

    public void FadeIn(bool instant = false)
    {
        if (!_isInitialized) Initialize();
        if (_renderer == null) return;

        // Zaten opaksak tekrar baþlatma
        if (!instant && Mathf.Abs(_currentAlpha - 1f) < 0.01f) return;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);

        if (instant)
        {
            SetAlpha(1f);
        }
        else
        {
            _fadeRoutine = StartCoroutine(FadeTo(1f));
        }
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        while (Mathf.Abs(_currentAlpha - targetAlpha) > 0.01f)
        {
            // Mathf.MoveTowards, Lerp'ten daha stabil bir bitiþ saðlar
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
            SetAlpha(_currentAlpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        _currentAlpha = alpha;

        // PropertyBlock kullanarak sadece bu objenin rengini deðiþtiriyoruz
        // Orijinal materyal bozulmuyor.
        _renderer.GetPropertyBlock(_propBlock);

        Color targetColor = _baseColor;
        targetColor.a = alpha;

        _propBlock.SetColor(_colorID, targetColor);
        _renderer.SetPropertyBlock(_propBlock);
    }
}