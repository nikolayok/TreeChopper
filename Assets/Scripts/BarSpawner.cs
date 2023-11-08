using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _barPrefab;

    public void SpawnBar()
    {
        Vector3 position = transform.position;
        position.y += 1;
        Instantiate(_barPrefab, position, Quaternion.identity);
    }
}
