using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 5;
    [SerializeField] private FixedJoystick _fixedJoystick;
    private PlayerAnimatorController _playerAnimatorController;
    private Rigidbody _rigidbody;
    private Vector3 _direction = Vector3.zero;
    private bool _isAlreadyIdleAnim = false;
    private bool _isAlreadyRunAnim = false;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerAnimatorController = GetComponent<PlayerAnimatorController>();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    private void Rotate()
    {

        if (_direction == Vector3.zero)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(_direction);
    }

    private void Move()
    {
        _direction = Vector3.forward * _fixedJoystick.Vertical + Vector3.right * _fixedJoystick.Horizontal;

        if ( ! IsIdle())
        {
            return;
        }

        _rigidbody.velocity = _direction * _speed;
        _isAlreadyIdleAnim = false;
    }

    private bool IsIdle()
    {
        if (_direction == Vector3.zero)
        {
            if (_isAlreadyIdleAnim)
            {
                return false;
            }
            else
            {
                _playerAnimatorController.RunAndIdle(_direction);
                _isAlreadyRunAnim = false;
                _isAlreadyIdleAnim = true;
            }

            return false;
        }
        else
        {
            if (_isAlreadyRunAnim)
            {
                return true;
            }
            else
            {
                _playerAnimatorController.RunAndIdle(_direction);
                _isAlreadyRunAnim = true;
            }

            return true;
        }
    }
}
