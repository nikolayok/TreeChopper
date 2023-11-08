using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarPickUp : MonoBehaviour
{
    [SerializeField] private Text _barsCountText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bar")
        {
            _barsCountText.text = (int.Parse(_barsCountText.text) + 1).ToString();
            Destroy(other.gameObject);
        }
    }
}
