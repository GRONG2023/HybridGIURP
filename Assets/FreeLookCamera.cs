using UnityEngine;

public class FreeLookCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("相机移动速度")]
    public float movementSpeed = 5.0f;

    [Header("Look Settings")]
    [Tooltip("鼠标灵敏度")]
    public float lookSensitivity = 2.0f;
    [Tooltip("垂直视角限制 (俯仰角)")]
    public float lookXLimit = 90.0f; // 限制俯仰角在 -90 到 90 度之间

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // 锁定鼠标光标到屏幕中心并隐藏
        // 这样鼠标就不会移出游戏窗口，方便自由视角操作
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化旋转值，使其与相机当前的欧拉角匹配
        // 这样相机就不会在游戏开始时突然跳动
        Vector3 currentEuler = transform.localEulerAngles;
        rotationY = currentEuler.y;
        // 处理Unity在Inspector中显示大于180度的角度时自动转换为负值的情况
        if (currentEuler.x > 180)
        {
            rotationX = currentEuler.x - 360;
        }
        else
        {
            rotationX = currentEuler.x;
        }
    }

    void Update()
    {
        // --- 鼠标视角旋转 ---
        HandleMouseLook();

        // --- 键盘相机移动 ---
        HandleKeyboardMovement();

        // --- 鼠标解锁 (可选) ---
        // 按下Escape键可以解锁鼠标，方便操作UI或退出游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void HandleMouseLook()
    {
        // 获取鼠标X轴和Y轴的输入
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        // 累计Y轴旋转 (左右看，即Yaw)
        rotationY += mouseX;

        // 累计X轴旋转 (上下看，即Pitch)
        // 注意：这里是减去mouseY，因为鼠标向上移动通常表示向上看 (Pitch增加)，
        // 而Y轴的输入值在鼠标向上移动时是正的，需要反转
        rotationX -= mouseY;

        // 限制X轴旋转 (俯仰角) 在设定的范围内，防止相机翻转
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // 应用旋转到相机
        // Quaternion.Euler() 函数接受欧拉角 (X, Y, Z) 并返回一个四元数旋转
        // 这里Z轴旋转始终为0，因为我们不需要滚转 (Roll)
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    void HandleKeyboardMovement()
    {
        // 获取键盘W/S (垂直) 和 A/D (水平) 的输入
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D 键
        float verticalInput = Input.GetAxis("Vertical");   // W/S 键

        // 根据相机自身的朝向计算移动方向
        // transform.forward 是相机当前朝向的前方向量
        // transform.right 是相机当前朝向的右方向量
        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        // 处理Q (向下) 和 E (向上) 键的输入
        if (Input.GetKey(KeyCode.Q))
        {
            // 向下移动，使用相机自身的下方向量
            moveDirection -= transform.up;
        }
        if (Input.GetKey(KeyCode.E))
        {
            // 向上移动，使用相机自身的上方向量
            moveDirection += transform.up;
        }

        // 将移动方向向量乘以速度和Time.deltaTime，以实现平滑且帧率无关的移动
        // transform.position += ... 直接修改相机的位置
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
    }
}