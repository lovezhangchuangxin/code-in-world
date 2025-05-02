using System;
using System.Diagnostics;
using Godot;
using Jint;
using Jint.Native;

/// <summary>
/// 游戏脚本引擎，用于执行玩家提供的脚本
/// </summary>
public class ScriptEngine
{
    /// <summary>
    /// 脚本引擎实例
    /// </summary>
    private Jint.Engine _engine;

    /// <summary>
    /// 脚本路径
    /// </summary>
    private string _scriptPath;

    /// <summary>
    /// 脚本入口文件名称
    /// </summary>
    private string _scriptName;

    /// <summary>
    /// 脚本运行函数，每 t 调用
    /// </summary>
    private JsValue _runFunction;

    /// <summary>
    /// 每次执行的语句条数限制
    /// </summary>
    public int MaxStatements { get; set; } = 100000;

    /// <summary>
    /// 每次执行的超时时间
    /// </summary>
    public int TimeoutMs { get; set; } = 50;

    /// <summary>
    /// 每次执行的递归调用深度限制
    /// </summary>
    public int LimitRecursion { get; set; } = 10000;

    public ScriptEngine(string scriptPath, string scriptName = "main.js")
    {
        _scriptPath = scriptPath;
        _scriptName = scriptName;
        _engine = new Jint.Engine(cfg =>
        {
            cfg.LimitMemory(20_000_000);
            cfg.LimitRecursion(LimitRecursion); // 设置递归调用深度限制
            cfg.EnableModules(_scriptPath); // 启用模块系统并设置根目录

            ExecutionConstraint constraint = new ExecutionConstraint(TimeoutMs, MaxStatements);
            cfg.Constraint(constraint);
        });

        SetupJavaScriptAPI();

        // 导入入口模块
        string modulePath = $"./{_scriptName}";
        var moduleNamespace = _engine.Modules.Import(modulePath);
        // 获取导出的run函数
        if (moduleNamespace.HasProperty("run"))
        {
            _runFunction = moduleNamespace.Get("run").AsFunctionInstance();
        }
        else
        {
            GD.PrintErr("main.js没有导出run函数");
        }
    }

    /// <summary>
    /// 设置 Javascript Api
    /// </summary>
    private void SetupJavaScriptAPI()
    {
        // 添加控制台日志功能
        _engine.SetValue("console", new
        {
            log = new Action<object>(message => GD.Print($"{message}")),
            warn = new Action<object>(message => GD.Print($"[WARNING] {message}")),
            error = new Action<object>(message => GD.PrintErr($"[ERROR] {message}"))
        });

        Game.SetEngineApi(_engine);
    }

    /// <summary>
    /// 运行 run 函数
    /// </summary>
    public bool Run()
    {
        if (_runFunction == null)
        {
            return false;
        }

        try
        {
            var constraint = _engine.Constraints.Find<ExecutionConstraint>();
            constraint.Reset();
            // 执行脚本
            _engine.Invoke(_runFunction);
            return true;
        }
        catch (OperationCanceledException ocEx)
        {
            GD.PrintErr(ocEx.Message);
        }
        catch (Jint.Runtime.JavaScriptException jsEx)
        {
            GD.PrintErr($"JavaScript运行时错误: {jsEx.Message}");
            GD.PrintErr($"调用栈: {jsEx.JavaScriptStackTrace}");
        }
        return false;
    }

    /// <summary>
    /// 自定义执行约束类
    /// </summary>
    public class ExecutionConstraint : Constraint
    {
        private readonly int _maxExecutionTimeMs;
        private readonly int _maxStatements;
        private int _statementCount = 0;
        private Stopwatch _stopwatch;

        public ExecutionConstraint(int maxExecutionTimeMs, int maxStatements)
        {
            _maxExecutionTimeMs = maxExecutionTimeMs;
            _maxStatements = maxStatements;
            _stopwatch = new Stopwatch();
        }

        public override void Reset()
        {
            _stopwatch.Restart();
            _statementCount = 0;
        }

        public override void Check()
        {
            if (_stopwatch.ElapsedMilliseconds > _maxExecutionTimeMs)
            {
                throw new OperationCanceledException($"JavaScript执行时间超过了{_maxExecutionTimeMs}毫秒。");
            }
            if (_statementCount > _maxStatements)
            {
                throw new OperationCanceledException($"JavaScript执行语句超过了{_maxStatements}条。");
            }
            _statementCount++;
        }
    }
}