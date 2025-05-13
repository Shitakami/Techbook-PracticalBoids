using UnityEngine;

namespace Part5
{
    public class PlayerRotation : MonoBehaviour
    {
        public float mouseSensitivity = 100f;

        private float xRotation = 0f;
        private float yRotation = 0f;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            // マウスの移動を取得
            var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // 垂直方向（X軸）の回転を制御
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 45f);

            // 水平方向（Y軸）の回転を制御
            yRotation += mouseX;

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}