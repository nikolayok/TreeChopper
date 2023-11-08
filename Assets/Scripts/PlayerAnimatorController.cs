using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Chop()
    {
        _animator.SetTrigger("Chop");
    }

    public void RunAndIdle(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            _animator.SetTrigger("Idle");
        }
        else
        {
            _animator.SetTrigger("Run");
        }
    }
}
