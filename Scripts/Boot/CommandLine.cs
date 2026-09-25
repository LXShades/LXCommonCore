#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Unity.Multiplayer.PlayMode;
using System.Collections.Generic;
using LX.Common.Core;
using System;

public static class CommandLine
{
    private static string[] commands
    {
        get
        {
            // Lazy init because some requesters run _super_ early
            if (_commands == null)
                ReceiveCommandsFromEditorOrSystem();
            return _commands;
        }
    }
    private static string[] _commands = null;

#if UNITY_EDITOR
    public static string editorCommands
    {
        get => EditorPrefs.GetString("_editorCommandLine", "");
        set => EditorPrefs.SetString("_editorCommandLine", value);
    }
#endif

    /// <summary>
    /// Can be used to insert extra commands from editor tools
    /// </summary>
    public static Action<List<string>> onPostProcessEditorCommands;

    private static void ReceiveCommandsFromEditorOrSystem()
    {
#if UNITY_EDITOR
        // Double-quotes should allow spaces
        string[] splitByQuotes = editorCommands.Split(new char[] { '"' }, System.StringSplitOptions.RemoveEmptyEntries);
        using var joinedAsList = DisposableList<string>.Create();

        for (int i = 0; i < splitByQuotes.Length; i++)
        {
            if ((i & 1) == 1)
                joinedAsList.list.Add(splitByQuotes[i]);
            else
                joinedAsList.list.AddRange(splitByQuotes[i].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));
        }

        onPostProcessEditorCommands?.Invoke(joinedAsList.list);

        _commands = joinedAsList.list.ToArray();
#else
        _commands = System.Environment.GetCommandLineArgs();
#endif

        UnityEngine.Debug.Log($"[CommandLine] Startup command line: {string.Join(" ", commands)}");
    }

    public static bool HasCommand(string commandName)
    {
        for (int i = 0; i < commands.Length; i++)
        {
            if (commands[i].Equals(commandName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool GetCommand(string commandName, int numParams, out string[] paramsOut)
    {
        paramsOut = null;

        commandName = commandName.ToLower();

        for (int i = 0; i < commands.Length - numParams; i++)
        {
            if (commands[i].ToLower() == commandName)
            {
                paramsOut = new string[numParams];
                System.Array.Copy(commands, i + 1, paramsOut, 0, numParams);
                return true;
            }
        }

        return false;
    }

    public static string GetAllCommandsAsString()
    {
        return string.Join(" ", commands);
    }
}
