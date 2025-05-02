using Godot;

public partial class MapVision : Camera2D
{
    /// <summary>
    /// 相机移动速度
    /// </summary>
    [Export]
    public float MoveSpeed { get; set; } = 500f;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            // 鼠标滚轮缩放
            if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp)
            {
                Zoom *= 0.95f;
            }
            else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
            {
                Zoom *= 1.05f;
            }
            Zoom = new Vector2(Mathf.Clamp(Zoom.X, 0.1f, 10f), Mathf.Clamp(Zoom.Y, 0.1f, 10f));
        }
    }

    // 处理物理更新，用于移动相机
    public override void _PhysicsProcess(double delta)
    {
        // 获取键盘输入方向
        Vector2 inputDir = Vector2.Zero;

        // 检测方向键和 WASD 键的输入
        if (Input.IsActionPressed("ui_right"))
            inputDir.X += 1;
        if (Input.IsActionPressed("ui_left"))
            inputDir.X -= 1;
        if (Input.IsActionPressed("ui_down"))
            inputDir.Y += 1;
        if (Input.IsActionPressed("ui_up"))
            inputDir.Y -= 1;

        // 归一化方向向量，确保斜向移动速度不会更快
        if (inputDir.LengthSquared() > 0)
            inputDir = inputDir.Normalized();

        // 考虑缩放级别调整移动速度（缩放越大，移动越慢）
        float adjustedSpeed = MoveSpeed / Zoom.X;

        // 应用移动
        GlobalPosition += inputDir * adjustedSpeed * (float)delta;
        MapUtils.MapLoader.TargetPosition = GlobalPosition;
    }

    /// <summary>
    /// 获取当前相机的视野范围
    /// </summary>
    public Rect2 GetCameraRect()
    {
        Vector2 topLeft = Position - GetViewportRect().Size / 2 / Zoom;
        Vector2 bottomRight = Position + GetViewportRect().Size / 2 / Zoom;
        return new Rect2(topLeft, bottomRight - topLeft);
    }
}
