using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> _listTreesPrefabs;
    private float _spawnTime = 15;

    private void Start()
    {
        Invoke("SpawnTree", _spawnTime);
    }

    private void SpawnTree()
    {
        SpawnRandomTree();
    }

    private void SpawnRandomTree()
    {
        int randomTreeNumber = Random.Range(0, _listTreesPrefabs.Count);
        Instantiate(_listTreesPrefabs[randomTreeNumber], transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
