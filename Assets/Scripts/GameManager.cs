using UnityEngine;

namespace ApartmentDeco
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] Furniture[] _startFurniture;
        [SerializeField] FurnitureController _furniturePrefab;
        [SerializeField] AudioClip _furnitureCreateSound;

        AudioSource _audioSource;

        public void Start()
        {
            _audioSource = GetComponent<AudioSource>();

            foreach (Furniture furniture in _startFurniture)
            {
                CreateFurniture(furniture);
            }
        }

        public void CreateFurniture(Furniture furniture)
        {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.PlayOneShot(_furnitureCreateSound);

            FurnitureController fc = Instantiate(_furniturePrefab);
            fc.Initialize(furniture);
        }
    }
}