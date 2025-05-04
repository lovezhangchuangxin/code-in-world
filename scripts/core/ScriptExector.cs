using Godot;

public partial class ScriptExector : Node
{
    /// <summary>
    /// 脚本路径
    /// </summary>
    [Export]
    public string scriptPath;

    public ScriptEngine ScriptEngine { get; set; }

    /// <summary>
    /// 上次执行的时间
    /// </summary>
    private double lastTime = 0;

    public override void _Ready()
    {
        if (string.IsNullOrEmpty(scriptPath))
        {
            GD.PrintErr("脚本路径不能为空");
            return;
        }

        ScriptEngine = new ScriptEngine(scriptPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        // 如果脚本引擎为空，直接返回
        if (ScriptEngine == null)
        {
            return;
        }

        if (Time.GetTicksMsec() - lastTime < Game.TickTime * 1000)
        {
            return;
        }
        lastTime = Time.GetTicksMsec();
        Game.Tick++;
        ScriptEngine.Run();
    }
}
