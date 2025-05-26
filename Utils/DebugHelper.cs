// Scripts/Utils/DebugHelper.cs
using Godot;
using System;
using System.Diagnostics;
using System.Threading;

namespace GodotHelper
{

    public static class DebugHelper
    {
        /// <summary>
        /// 在 _Ready() 一開始呼叫，在指定毫秒內等待外部 debugger attach，attach 後自動 Break。
        /// 如果超過時間則放行繼續執行。
        /// </summary>
        /// <param name="timeoutMs">最大等待時間（毫秒）。預設 10000ms。</param>
        /// 
        [Conditional("DEBUG")]
        public static void WaitForDebugger(int timeoutMs = 10000)
        {
            GD.Print($"[DebugHelper] Waiting for debugger attach (timeout {timeoutMs}ms)...");
            var sw = Stopwatch.StartNew();
            while (!Debugger.IsAttached && sw.ElapsedMilliseconds < timeoutMs)
            {
                Thread.Sleep(50);
            }

            if (Debugger.IsAttached)
            {
                GD.Print("[DebugHelper] Debugger attached—breaking now.");
                Debugger.Break();
            }
            else
            {
                GD.Print("[DebugHelper] No debugger attached within timeout, continuing.");
            }
        }
    }
}
