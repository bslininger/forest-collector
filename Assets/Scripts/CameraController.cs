using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Vector3 _cameraOffset;

    private void LateUpdate()
    {
        transform.position = _playerTransform.TransformPoint(_cameraOffset);
        transform.LookAt(_playerTransform);
    }
}
