using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Felling : MonoBehaviour
{
    private int health = 3;
    //private DestroyMesh _destroyMesh;
    private BarSpawner _barSpawner;
    [SerializeField] private GameObject _treeSpawnerGameObjectPrefab;

    private void Start()
    {
        //_destroyMesh = GetComponent<DestroyMesh>();
        _barSpawner = GetComponent<BarSpawner>();
    }

    public void Chop()
    {
        health -= 1;
        if (health <= 0)
        {
            Invoke("CutDown", 1);
        }
    }

    private void CutDown()
    {
        //_destroyMesh.Destroy();
        _barSpawner.SpawnBar();
        Vector3 position = transform.position;
        position.y += 0.5f;
        Instantiate(_treeSpawnerGameObjectPrefab, position, Quaternion.identity);
        Destroy(gameObject);
    }
}
