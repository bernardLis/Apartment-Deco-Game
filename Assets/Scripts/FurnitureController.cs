using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Shapes;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace ApartmentDeco
{
    public class FurnitureController : MonoBehaviour
    {
        [SerializeField] AudioClip _pickUpSound;
        [SerializeField] AudioClip _dropSound;
        [SerializeField] AudioClip _destroySound;

        public Furniture Furniture;

        Camera _camera;

        PanelRenderer _panelRenderer;
        VisualElement _main;
        Label _nameLabel;

        AudioSource _audioSource;

        Rectangle _rectangle;
        Disc _disc;
        Rectangle _leg;

        BoxCollider2D _boxCollider;
        BoxCollider2D _boxCollider1;

        Vector3 _mousePositionOffset;
        bool _wasDragged;
        IEnumerator _dragSetterCoroutine;

        public event Action OnDestroy;

        public void Initialize(Furniture furniture)
        {
            _camera = Camera.main;

            Furniture = furniture;

            gameObject.name = furniture.Name;

            _audioSource = GetComponent<AudioSource>();
            ParseShape(furniture);

            transform.DOMove(furniture.Position, 0.5f).SetEase(Ease.OutBack);
            Rotate(furniture.Rotation.z);

            if (_main != null) SetLabel();
        }

        /* DRAG & DROP */
        Vector3 GetMousePosition()
        {
            return _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }

        public void OnMouseDown()
        {
            _wasDragged = false;
            if (_dragSetterCoroutine != null) StopCoroutine(_dragSetterCoroutine);
            _dragSetterCoroutine = DragSetterCoroutine();
            StartCoroutine(_dragSetterCoroutine);

            _audioSource.pitch = Random.Range(0.8f, 1.2f);
            _audioSource.PlayOneShot(_pickUpSound);
            transform.DOScale(1.25f, 0.25f);
            transform.DOScale(1f, 0.25f).SetDelay(0.25f);

            _mousePositionOffset = transform.position - GetMousePosition();
        }

        public void OnMouseUp()
        {
            if (_dragSetterCoroutine != null) StopCoroutine(_dragSetterCoroutine);

            _audioSource.pitch = Random.Range(0.8f, 1.2f);
            _audioSource.PlayOneShot(_dropSound);

            Furniture.SetPosition(transform.position);

            if (_wasDragged) return;
            FurnitureCardManager.Instance.DisplayCard(this);
        }

        public void OnMouseDrag()
        {
            transform.position = GetMousePosition() + _mousePositionOffset;
        }

        IEnumerator DragSetterCoroutine()
        {
            yield return new WaitForSeconds(0.25f);
            _wasDragged = true;
        }

        /* Label */
        void OnEnable()
        {
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
            _nameLabel = _main.Q<Label>("text");

            if (Furniture == null) return;
            //    if (Furniture.Name.Length == 0) return;
            SetLabel();
        }

        void SetLabel()
        {
            if (_nameLabel != null)
                _nameLabel.text = Furniture.Name;
        }

        void ParseShape(Furniture furniture)
        {
            SetPanelSize(furniture);

            if (furniture.Shape == Shape.Rectangle || furniture.Shape == Shape.L)
            {
                _rectangle = gameObject.AddComponent<Rectangle>();
                DOTween.To(x => _rectangle.Width = x, 0, furniture.Width, 0.25f).SetEase(Ease.OutBack);
                DOTween.To(x => _rectangle.Height = x, 0, furniture.Height, 0.25f).SetEase(Ease.OutBack);
                _rectangle.Color = furniture.Color;
                _boxCollider = gameObject.AddComponent<BoxCollider2D>();
                _boxCollider.size = new Vector2(furniture.Width, furniture.Height);
            }


            if (furniture.Shape == Shape.Circle)
            {
                _disc = gameObject.AddComponent<Disc>();
                DOTween.To(x => _disc.Radius = x, 0, furniture.Width * 0.5f, 0.25f).SetEase(Ease.OutBack);
                _disc.Color = furniture.Color;
                _boxCollider = gameObject.AddComponent<BoxCollider2D>();
                _boxCollider.size = new Vector2(furniture.Width, furniture.Width);
            }

            if (furniture.Shape == Shape.L)
            {
                // draw l shape with 2 rectangles
                GameObject legGameObject = new GameObject();
                legGameObject.transform.SetParent(transform);
                legGameObject.transform.localPosition =
                    new Vector3(-Furniture.LegWidth / 2, Furniture.LegWidth - furniture.Width, 0);
                legGameObject.transform.localRotation = Quaternion.Euler(0, 0, 90);

                _leg = legGameObject.AddComponent<Rectangle>();
                _leg.SortingOrder = -1;

                DOTween.To(x => _leg.Width = x, 0, furniture.LegWidth, 0.25f).SetEase(Ease.OutBack);
                DOTween.To(x => _leg.Height = x, 0, furniture.LegHeight, 0.25f).SetEase(Ease.OutBack);
                _leg.Color = furniture.Color;

                _boxCollider1 = gameObject.AddComponent<BoxCollider2D>();
                _boxCollider1.size = new Vector2(furniture.LegWidth, furniture.LegHeight);
            }
        }

        void SetPanelSize(Furniture furniture)
        {
            int width = Mathf.Clamp((int)furniture.Width * 100, 100, (int)furniture.Width * 100);
            int height = Mathf.Clamp((int)furniture.Height * 100, 100, (int)furniture.Height * 100);

            _panelRenderer.worldSpaceSize = new Vector2(width, height);
        }

        public void SetName(string newName)
        {
            Furniture.SetName(newName);
            SetLabel();
        }

        public void SetColor(Color color)
        {
            if (_rectangle != null) _rectangle.Color = color;
            if (_disc != null) _disc.Color = color;
            if (_leg != null) _leg.Color = color;
            Furniture.SetColor(color);
        }

        public void SetWidth(float evtNewValue)
        {
            DOTween.Kill("size", true);

            if (_rectangle != null)
                DOTween.To(x => _rectangle.Width = x, _rectangle.Width, evtNewValue, 0.25f).SetEase(Ease.OutBack)
                    .SetId("size");
            if (_disc != null)
                DOTween.To(x => _disc.Radius = x, _disc.Radius, evtNewValue * 0.5f, 0.25f).SetEase(Ease.OutBack)
                    .SetId("size");
            if (_boxCollider != null) _boxCollider.size = new(evtNewValue, _boxCollider.size.y);
            Furniture.SetWidth(evtNewValue);
        }

        public void SetHeight(float evtNewValue)
        {
            DOTween.Kill("size", true);

            if (_rectangle != null)
                DOTween.To(x => _rectangle.Height = x, _rectangle.Height, evtNewValue, 0.25f).SetEase(Ease.OutBack)
                    .SetId("size");

            if (_boxCollider != null) _boxCollider.size = new(_boxCollider.size.x, evtNewValue);
            Furniture.SetHeight(evtNewValue);
        }

        public void SetLegWidth(float evtNewValue)
        {
            DOTween.Kill("size", true);

            if (_leg != null)
                DOTween.To(x => _leg.Width = x, _leg.Width, evtNewValue, 0.25f).SetEase(Ease.OutBack).SetId("size");
            if (_boxCollider1 != null) _boxCollider1.size = new(evtNewValue, _boxCollider1.size.y);
            Furniture.SetLegWidth(evtNewValue);
        }

        public void SetLegHeight(float evtNewValue)
        {
            DOTween.Kill("size", true);

            if (_leg != null)
                DOTween.To(x => _leg.Height = x, _leg.Height, evtNewValue, 0.25f).SetEase(Ease.OutBack).SetId("size");

            if (_boxCollider1 != null) _boxCollider1.size = new(_boxCollider1.size.x, evtNewValue);
            Furniture.SetLegHeight(evtNewValue);
        }

        public void Rotate(float evtNewValue)
        {
            DOTween.Kill("rotate", true);
            transform.DORotate(new(0, 0, evtNewValue), 0.25f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutBack)
                .SetId("rotate")
                .OnComplete(() =>
                {
                    _panelRenderer.transform.DORotate(new Vector3(0, 0, -evtNewValue), 0.25f,
                        RotateMode.WorldAxisAdd);
                    Furniture.SetRotation(transform.rotation.eulerAngles);
                });
        }


        public void ToggleLabel()
        {
            if (_nameLabel != null)
                _nameLabel.style.display = _nameLabel.style.display == DisplayStyle.None
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }


        public void DestroySelf()
        {
            OnDestroy?.Invoke();

            FurnitureCardManager.Instance.HideCard();
            _audioSource.pitch = Random.Range(0.8f, 1.2f);
            _audioSource.PlayOneShot(_destroySound);

            transform.DOShakeScale(0.25f).OnComplete(() =>
            {
                transform.DOScale(0, 0.25f).OnComplete(() => Destroy(gameObject));
            });
        }
    }
}