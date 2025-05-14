using UnityEngine;

namespace Shitakami.PracticalBoidsSample.Samples
{
    // MEMO: Copilotによる自動生成
    public class Player : MonoBehaviour
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private float _speed = 5.0f;
        [SerializeField] private float _mouseSensitivity = 100f;
        
        private void Start()
        {
            if (_characterController == null)
                _characterController = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            // 入力を取得
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            // プレイヤーの回転に合わせた移動方向を計算
            var movement = transform.right * horizontal + transform.forward * vertical;

            // CharacterController を使って移動
            _characterController.Move(movement.normalized * _speed * Time.deltaTime);

            // マウス入力でY軸回転
            var mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
            transform.Rotate(0f, mouseX, 0f);
        }
    }
}
