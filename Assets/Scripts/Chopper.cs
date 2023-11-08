using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chopper : MonoBehaviour
{
    private PlayerAnimatorController _playerAnimatorController;
    [SerializeField] private Button _chopButton;
    private float _reloadSpeed = 1f;
    private Felling _felling;
    private bool _isNearTree = false;

    private void Start()
    {
        _playerAnimatorController = GetComponent<PlayerAnimatorController>();
    }

    public void Chop()
    {
        _playerAnimatorController.Chop();
        StartReload();

        if (_isNearTree)
        {
            _felling.Chop();
        }
    }

    private void StartReload()
    {
        _chopButton.enabled = false;
        Invoke("FinishReload", _reloadSpeed);
    }

    private void FinishReload()
    {
        _chopButton.enabled = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Tree")
        {
            _felling = other.GetComponent<Felling>();
            _isNearTree = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Tree")
        {
            _isNearTree = false;
        }
    }
}
