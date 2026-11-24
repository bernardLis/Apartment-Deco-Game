using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace ApartmentDeco
{
    public class FurnitureCardManager : Singleton<FurnitureCardManager>
    {
        AudioSource _audioSource;

        PanelRenderer _panelRenderer;
        VisualElement _main;

        IEnumerator _hideUICoroutine;

        void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();

            _panelRenderer = GetComponentInChildren<PanelRenderer>();
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
        }

        public void DisplayCard(FurnitureController furnitureController)
        {
            if (_hideUICoroutine != null) StopCoroutine(_hideUICoroutine);

            _main.style.display = DisplayStyle.Flex;
            _main.style.opacity = 1;

            _main.Clear();
            _main.Add(new FurnitureCardElement(furnitureController));
            Button closeButton = new();
            closeButton.text = "Close";
            _main.Add(closeButton);
            closeButton.clickable.clicked += HideCard;
        }


        public void HideCard()
        {
            _main.style.opacity = 0;
            _main.Clear();

            if (_hideUICoroutine != null) StopCoroutine(_hideUICoroutine);
            _hideUICoroutine = HideUICoroutine();
            StartCoroutine(_hideUICoroutine);
        }

        IEnumerator HideUICoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            _main.style.display = DisplayStyle.None;
        }
    }
}