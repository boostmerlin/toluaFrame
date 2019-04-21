using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Debugger = LuaInterface.Debugger;

public class LaunchAssetServer : ScriptableSingleton<LaunchAssetServer>
{
    const int port = 8081;
    const string kLocalAssetServerMenu = EditorHelper.Prefix_FrameToolkit + "Local Asset Server";
    int m_serverPID = 0;

    //[MenuItem(kLocalAssetServerMenu)]
    public static void ToggleLocalServer()
    {
        if (EditorHelper.RunCmd("where", "python").code == 1)
        {
            EditorUtility.DisplayDialog(":(", "no python found in PATH", "OK");
            return;
        }

        bool isRunning = IsRunning();
        if (!isRunning)
        {
            Run();
        }
        else
        {
            KillRunningServer();
        }
    }

    [MenuItem(kLocalAssetServerMenu, true)]
    public static bool ToggleLocalServerChecked()
    {
        bool isRunnning = IsRunning();
        Menu.SetChecked(kLocalAssetServerMenu, isRunnning);
        return true;
    }

    static bool IsRunning()
    {
        if (instance.m_serverPID == 0)
            return false;

        var process = Process.GetProcessById(instance.m_serverPID);
        if (process == null)
            return false;

        return !process.HasExited;
    }

    static void KillRunningServer()
    {
        try
        {
            if (instance.m_serverPID == 0)
                return;

            var lastProcess = Process.GetProcessById(instance.m_serverPID);
            lastProcess.Kill();
            instance.m_serverPID = 0;
        }
        catch
        {
        }
    }

    void OnDisable()
    {
        KillRunningServer();
    }

    static void Run()
    {
        string serverroot = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
        KillRunningServer();
        string model = "SimpleHTTPServer";
        ProcessStartInfo startInfo = new ProcessStartInfo("python", string.Format("-m {0} {1}", model, port));
        startInfo.WorkingDirectory = serverroot;
        startInfo.UseShellExecute = false;

        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        Process launchProcess = Process.Start(startInfo);

        if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
        {
            Debugger.LogError("Unable Start AssetServer process");
        }
        else
        {
            instance.m_serverPID = launchProcess.Id;
            Debugger.Log("Local AssetServer Listen: {0}, Root Dir: {1}", port, serverroot);
        }
    }
}
