using UnityEngine;
using UnityEngine.UIElements;

public class LabelTextSetter : MonoBehaviour
{
    [SerializeField] string _text;
    [SerializeField] int _fontSize = 36;

    PanelRenderer _panelRenderer;
    VisualElement _main;

    void OnEnable()
    {
        _panelRenderer = GetComponent<PanelRenderer>();
        _panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    void OnDisable()
    {
        _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        _main = root.Q<VisualElement>("main");
        _main.Clear();

        if (_text.Length == 0) return;

        Label l = new(_text);
        l.style.fontSize = _fontSize;
        _main.Add(l);
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Application.IsPlaying(this)) return;
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnGizmosUI);
    }

    void OnGizmosUI(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {
        Label l = rootElement.Q<Label>("text");
        l.text = _text;
        l.style.fontSize = _fontSize;
    }

#endif
}