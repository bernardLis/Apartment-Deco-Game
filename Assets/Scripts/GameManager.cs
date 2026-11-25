using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace ApartmentDeco
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] FurnitureController _furniturePrefab;
        [SerializeField] AudioClip _furnitureCreateSound;

        List<FurnitureController> _furnitureControllers = new();

        AudioSource _audioSource;

        PanelRenderer _panelRenderer;
        VisualElement _main;
        Label _infoLabel;
        Button _muteButton;

        Queue<string> _infoQueue = new();

        public void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            if (!File.Exists(Application.persistentDataPath + "/save.txt"))
            {
                TextAsset ta = (TextAsset)Resources.Load("DefaultSave");
                if (ta == null) return;
                Load(ta.text);
            }
            else
            {
                Load(File.ReadAllText(Application.persistentDataPath + "/save.txt"));
            }

            StartCoroutine(SaveCoroutine());
            StartCoroutine(DisplayInfoCoroutine());
        }


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
            _infoLabel = _main.Q<Label>("infoLabel");

            Button labelsButton = _main.Q<Button>("labelsButton");
            labelsButton.clickable.clicked += ToggleLabels;

            Button controlsButton = _main.Q<Button>("controlsButton");
            controlsButton.clickable.clicked += ShowControlsWindow;

            Button saveButton = _main.Q<Button>("saveButton");
            saveButton.clickable.clicked += Save;

            _muteButton = _main.Q<Button>("muteButton");
            _muteButton.clickable.clicked += ToggleAudio;

            Button displayString = _main.Q<Button>("copyButton");
            displayString.clickable.clicked += ShowSaveString;

            Button loadButton = _main.Q<Button>("loadButton");
            loadButton.clickable.clicked += LoadFromString;
        }

        void ToggleLabels()
        {
            foreach (FurnitureController fc in _furnitureControllers)
            {
                fc.ToggleLabel();
            }
        }

        VisualElement _controlsWindow;

        void ShowControlsWindow()
        {
            if (_controlsWindow != null)
            {
                _controlsWindow.RemoveFromHierarchy();
                _controlsWindow = null;
                return;
            }

            _controlsWindow = new();
            _controlsWindow.style.position = Position.Absolute;
            _controlsWindow.style.alignSelf = Align.Center;
            _controlsWindow.style.top = Length.Percent(25);
            _controlsWindow.style.width = 500;
            _controlsWindow.style.height = 500;
            _controlsWindow.style.backgroundColor = new Color(0.498f, 0.4392f, 0.5411765f);

            Label label = new("Inputs");
            label.style.fontSize = 48;
            _controlsWindow.Add(label);

            Label arrows = new("Arrows Keys to Move");
            arrows.style.fontSize = 36;
            _controlsWindow.Add(arrows);

            Label zoomIn = new("Z to zoom in");
            zoomIn.style.fontSize = 36;
            _controlsWindow.Add(zoomIn);

            Label zoomOut = new("X to zoom in");
            zoomOut.style.fontSize = 36;
            _controlsWindow.Add(zoomOut);

            Button closeButton = new();
            closeButton.text = "Close";
            _controlsWindow.Add(closeButton);
            closeButton.clickable.clicked += () =>
            {
                _controlsWindow.RemoveFromHierarchy();
                _controlsWindow = null;
            };

            _main.Add(_controlsWindow);
        }


        VisualElement _saveStringWindow;

        void ShowSaveString()
        {
            if (_saveStringWindow != null)
            {
                _saveStringWindow.RemoveFromHierarchy();
                _saveStringWindow = null;
                return;
            }

            _saveStringWindow = new();
            _saveStringWindow.style.position = Position.Absolute;
            _saveStringWindow.style.alignSelf = Align.Center;
            _saveStringWindow.style.top = Length.Percent(25);
            _saveStringWindow.style.width = 500;
            _saveStringWindow.style.height = 500;
            _saveStringWindow.style.backgroundColor = new Color(0.498f, 0.4392f, 0.5411765f);

            Label label = new("Save String");
            label.style.fontSize = 36;
            _saveStringWindow.Add(label);

            TextField textField = new();
            textField.value = GetSaveString();
            textField.multiline = true;
            textField.style.fontSize = 24;
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.style.height = 300;
            textField.style.width = 400;
            _saveStringWindow.Add(textField);

            Button closeButton = new();
            closeButton.text = "Close";
            _saveStringWindow.Add(closeButton);
            closeButton.clickable.clicked += () =>
            {
                _saveStringWindow.RemoveFromHierarchy();
                _saveStringWindow = null;
            };

            _main.Add(_saveStringWindow);

            EnqueueInfo("Showing Save String!");
        }

        VisualElement _loadStringWindow;

        void LoadFromString()
        {
            if (_loadStringWindow != null)
            {
                _loadStringWindow.RemoveFromHierarchy();
                _loadStringWindow = null;
                return;
            }

            _loadStringWindow = new();
            _loadStringWindow.style.position = Position.Absolute;
            _loadStringWindow.style.alignSelf = Align.Center;
            _loadStringWindow.style.top = Length.Percent(25);
            _loadStringWindow.style.width = 500;
            _loadStringWindow.style.height = 500;
            _loadStringWindow.style.backgroundColor = new Color(0.498f, 0.4392f, 0.5411765f);

            Label label = new("Paste Save String & Click Load");
            label.style.fontSize = 36;
            _loadStringWindow.Add(label);

            TextField textField = new();
            textField.multiline = true;
            textField.style.fontSize = 24;
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.style.height = 300;
            textField.style.width = 400;
            _loadStringWindow.Add(textField);

            Button loadButton = new();
            loadButton.text = "Load";
            _loadStringWindow.Add(loadButton);
            loadButton.clickable.clicked += () =>
            {
                Load(textField.value);
                _loadStringWindow.RemoveFromHierarchy();
                EnqueueInfo("Loading from string!");
            };

            Button closeButton = new();
            closeButton.text = "Close";
            _loadStringWindow.Add(closeButton);
            closeButton.clickable.clicked += () =>
            {
                _loadStringWindow.RemoveFromHierarchy();
                _loadStringWindow = null;
            };

            _main.Add(_loadStringWindow);
        }


        void ToggleAudio()
        {
            if (AudioListener.volume == 0)
            {
                _muteButton.RemoveFromClassList("common__unmute-icon");
                _muteButton.AddToClassList("common__mute-icon");
                AudioListener.volume = 1;
                EnqueueInfo("Audio Unmuted!");
            }
            else
            {
                _muteButton.RemoveFromClassList("common__mute-icon");
                _muteButton.AddToClassList("common__unmute-icon");
                AudioListener.volume = 0;
                EnqueueInfo("Audio Muted!");
            }
        }

        public void EnqueueInfo(string text)
        {
            _infoQueue.Enqueue(text);
        }

        IEnumerator DisplayInfoCoroutine()
        {
            while (true)
            {
                if (this == null) yield break;
                if (_infoQueue.Count == 0)
                {
                    yield return new WaitForSeconds(0.2f);
                    continue;
                }

                _infoLabel.text = _infoQueue.Dequeue();
                _infoLabel.style.opacity = 1;
                yield return new WaitForSeconds(1.5f);
                _infoLabel.style.opacity = 0;
                yield return new WaitForSeconds(0.25f);
            }
        }

        IEnumerator SaveCoroutine()
        {
            while (true)
            {
                if (this == null) yield break;
                yield return new WaitForSeconds(60);
                Save();
            }
        }

        public void CreateFurniture(Furniture furniture, bool isNew = true)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.PlayOneShot(_furnitureCreateSound);

            FurnitureController fc = Instantiate(_furniturePrefab, new Vector3(-10, 5, 0), Quaternion.identity);
            fc.Initialize(furniture);
            fc.OnDestroy += () => _furnitureControllers.Remove(fc);

            _furnitureControllers.Add(fc);

            if (!isNew) return;
            fc.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
            Save();
        }

        [Button]
        void Save()
        {
            File.WriteAllText(Application.persistentDataPath + "/save.txt", GetSaveString());
            EnqueueInfo("Saved!");
        }

        string GetSaveString()
        {
            SaveObject saveObject = new();
            saveObject.FurnitureData = new List<FurnitureData>();
            foreach (FurnitureController fc in _furnitureControllers)
                saveObject.FurnitureData.Add(fc.Furniture.SerializeSelf());

            return JsonUtility.ToJson(saveObject);
        }

        void Load(string json)
        {
            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(json);

            StartCoroutine(LoadCoroutine(saveObject));
        }

        IEnumerator LoadCoroutine(SaveObject saveObject)
        {
            if (_furnitureControllers.Count > 0)
            {
                List<FurnitureController> furnitureControllers = new(_furnitureControllers);
                foreach (FurnitureController fc in furnitureControllers)
                    fc.DestroySelf();
                yield return new WaitForSeconds(1f);
                _furnitureControllers.Clear();
            }

            foreach (FurnitureData fd in saveObject.FurnitureData)
            {
                Furniture furniture = ScriptableObject.CreateInstance<Furniture>();
                furniture.LoadFromData(fd);
                CreateFurniture(furniture, false);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    class SaveObject
    {
        public int SaveVersion = 1;
        public List<FurnitureData> FurnitureData = new();
    }
}