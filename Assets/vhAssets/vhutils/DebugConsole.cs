using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour
{
    public const float CharacterWidth = 8;////13.3125f;
    public const string CommandTypingBoxName = "CommandTypingBox";
    public const float TextHeight = 28;
    public const char ParameterStart = ' ';
    public const int MaxLogCapacity = 200;

    public delegate void ConsoleCallback(string commandEntered, DebugConsole console);

    static DebugConsole _DebugConsole;

    public class TextLine
    {
        public Rect m_Position;
        public string m_Text;
        public Color m_TextColor;
        public bool m_bSelected;

        public TextLine(Rect pos, string text, Color color)
        {
            m_Position = pos;
            m_Text = text;
            m_TextColor = color;
            m_bSelected = false;
        }
    }

    // ------> Variables
    public float m_PercentageOfScreenTall = 0.4f;
    Rect m_ScrollViewPosition;
    Rect m_ScrollViewViewRect;
    Vector2 m_ScrollPosition = new Vector2(0, 0);
    Dictionary<string, ConsoleCallback> m_CommandFunctionMap = new Dictionary<string, ConsoleCallback>();

    // TextArea for command string
    string m_CommandString = string.Empty;
    Rect m_CommandStringPosition;

    // true draws the console to the screen
    bool m_bDrawConsole = false;
    bool m_bConsoleLoggingEnabled = true;

    List<TextLine> m_LoggedText = new List<TextLine>(MaxLogCapacity);
    List<string> m_PreviousCommands = new List<string>();
    List<string> m_BrokenUpConsoleText = new List<string>(); // used for storage when breaking up text for the console that is too long
    int m_PreviousCommandIndex = 0;


#region Properties
    public string CommandString
    {
        get { return m_CommandString; }
        set { m_CommandString = value; }
    }

    public bool DrawConsole
    {
        get { return m_bDrawConsole; }
    }

    int MaxCharactersPerLine
    {
        get { return (int)((float)Screen.width / CharacterWidth); }
    }
#endregion

    public static DebugConsole Get()
    {
        if (_DebugConsole == null)
        {
            _DebugConsole = Object.FindObjectOfType(typeof(DebugConsole)) as DebugConsole;
        }

        return _DebugConsole;
    }

    public void Awake()
    {
        InitPositions();

        // allow unity output logs to go to the console as well.
        Application.RegisterLogCallback(HandleUnityLog);
    }

    public void InitPositions()
    {
        float scaledTextHeight = TextHeight / (float)Screen.height;
        m_ScrollViewPosition = new Rect(0, 1.0f - m_PercentageOfScreenTall, 1.0f, m_PercentageOfScreenTall - scaledTextHeight);
        m_ScrollViewViewRect = new Rect(0, 0, 0.95f, (float)MaxLogCapacity * scaledTextHeight);
        m_ScrollPosition.y = m_ScrollViewViewRect.yMax;
        m_CommandStringPosition = new Rect(0, m_ScrollViewPosition.yMax, 1.0f, scaledTextHeight);
    }

    public void HandleInput()
    {
        /*
         * Unity has issues with Input when GUI.TextArea is in focus
         * http://fogbugz.unity3d.com/default.asp?394073_38u3dau5aphm9qlc
         */

        if (Event.current == null || Event.current.type != EventType.KeyDown)
        {
            return;
        }

        //Debug.Log("Event.current.keyCode: " + Event.current.keyCode);

        // special input cases
        if (Event.current.character == '`')
        {
            m_bDrawConsole = !m_bDrawConsole;
            CommandString = string.Empty;
        }

        if (m_bDrawConsole)
        {
            if (Event.current.keyCode == KeyCode.UpArrow)
            {
                if (m_PreviousCommandIndex > 0)
                {
                    CommandString = m_PreviousCommands[--m_PreviousCommandIndex];
                }
            }
            else if (Event.current.keyCode == KeyCode.DownArrow)
            {
                if (m_PreviousCommandIndex < m_PreviousCommands.Count - 1)
                {
                    CommandString = m_PreviousCommands[++m_PreviousCommandIndex];
                }
            }
            else if (Event.current.keyCode == KeyCode.PageUp)
            {
                m_ScrollPosition.y -= TextHeight / (float)Screen.height;
            }
            else if (Event.current.keyCode == KeyCode.PageDown)
            {
                m_ScrollPosition.y += TextHeight / (float)Screen.height;
            }
            else if (Event.current.character == '\n')
            {
                InvokeCommand(m_CommandString);
            }
        }
    }

    public void OnGUI()
    {
        bool currentlyDrawingConsole = m_bDrawConsole;
        HandleInput();

        if (!m_bDrawConsole || currentlyDrawingConsole != m_bDrawConsole)
        {
            CommandString = string.Empty;
            m_PreviousCommandIndex = m_PreviousCommands.Count;
            return;
        }

        GUI.FocusControl(CommandTypingBoxName);
        VHGUI.Box(m_ScrollViewPosition, "");

        m_ScrollPosition = VHGUI.BeginScrollView(m_ScrollViewPosition, m_ScrollPosition, m_ScrollViewViewRect);
        {
            // logged console text
            for (int i = 0; i < m_LoggedText.Count; i++)
            {
                VHGUI.Label(m_LoggedText[i].m_Position, m_LoggedText[i].m_Text, m_LoggedText[i].m_TextColor);
            }
        }
        VHGUI.EndScrollView();

        GUI.SetNextControlName(CommandTypingBoxName);
        CommandString = VHGUI.TextArea(m_CommandStringPosition, m_CommandString, Color.yellow);
    }

    public void AddCommandCallback(string commandString, ConsoleCallback cb)
    {
        commandString.ToLower();
        if (0 == string.Compare(commandString, "help") || 0 == string.Compare(commandString, "?")
            || 0 == string.Compare(commandString, "clear") || 0 == string.Compare(commandString, "cls")
            || 0 == string.Compare(commandString, "q") || 0 == string.Compare(commandString, "quit")
            || 0 == string.Compare(commandString, "exit") || 0 == string.Compare(commandString, "enable_console_logging")
            )
        {
            // reserved keywords
            commandString += " is a reserved keyword by the console.";
            //AddTextToLog(commandString);
            return;
        }

        if (m_CommandFunctionMap.ContainsKey(commandString))
        {
            m_CommandFunctionMap[commandString] = cb;
        }
        else
        {
            m_CommandFunctionMap.Add(commandString, cb);
        }
    }

    public void ToggleConsole()
    {
        m_bDrawConsole = !m_bDrawConsole;
        CommandString = string.Empty;
    }

    void InvokeCommand(string command)
    {
        if (command == null)
        {
            return;
        }
        else
        {
            command = command.Trim();
            command = command.Replace("\n", "");
            VHGUI.TextArea(m_CommandStringPosition, m_CommandString, Color.yellow);
        }

        if (command == string.Empty)
        {
            return;
        }

        // format the string to get rid of extra ending spaces and multiple spaces in a row
        int spaceIndex = 0;
        do
        {
            spaceIndex = command.IndexOf(ParameterStart, spaceIndex);
            if (spaceIndex != -1)
            {
               ++spaceIndex;
               while (spaceIndex < command.Length && command[spaceIndex] == ParameterStart)
               {
                   command = command.Remove(spaceIndex, 1);
               }
            }
        }
        while (spaceIndex != -1);

        if (command[0] == ParameterStart)
        {
            command = command.Remove(0, 1);
        }

        //bool commandExists = false;
        string commandStringWithoutParameters = command;
        string consoleLogString = commandStringWithoutParameters; // used for logging messages to console

        int uiIndex = commandStringWithoutParameters.IndexOf(ParameterStart);
        if (uiIndex != -1)
        {
            // we don't want to check parameters, we just want the command,
            commandStringWithoutParameters = commandStringWithoutParameters.Remove(uiIndex, command.Length - uiIndex);
            commandStringWithoutParameters = commandStringWithoutParameters.ToLower();
        }

        if (m_CommandFunctionMap.ContainsKey(commandStringWithoutParameters))
        {
            // this command string exists, call its function
            m_CommandFunctionMap[commandStringWithoutParameters](command, this);
        }
        else if (0 == string.Compare(command, "clear") || 0 == string.Compare(command, "cls"))
        {
            consoleLogString = string.Empty;
            ClearConsoleLog();
        }
        else if (0 == string.Compare(command, "help") || 0 == string.Compare(command, "?"))
        {
            // show all the commands available to them
            consoleLogString = "Commands Available: ";
            foreach (KeyValuePair<string, ConsoleCallback> kvp in m_CommandFunctionMap)
            {
                consoleLogString += "   " + kvp.Key;
            }
        }
        else if (0 == string.Compare(command, "q") || 0 == string.Compare(command, "quit")
            || 0 == string.Compare(command, "exit"))
        {
            Application.Quit();
        }
        else if (0 == string.Compare(commandStringWithoutParameters, "enable_console_logging"))
        {
            if (!ParseBool(command, ref m_bConsoleLoggingEnabled))
            {
                AddTextToLog(command + " requires parameter '0' or '1'");
            }
        }
        else
        {
            // command doesn't exist
            consoleLogString = commandStringWithoutParameters + " command doesn't exist. Type ? or help for a list of commands.";
        }

        // display what you wrote in the log
        AddTextToLog(consoleLogString);

        // add it so you can find it again with the arrow keys
        if (!m_PreviousCommands.Contains(command))
        {
            //for (int i = 0; i < command.Length; i++)
            //{
            //    if (command[i] == '\n')
            //    {
            //        command = command.Remove(i, 1);
            //    }
            //}
            m_PreviousCommands.Add(command);
        }

        m_PreviousCommandIndex = m_PreviousCommands.Count;

        CommandString = string.Empty;
    }

    void OffsetTextLogPositions(float yOffset)
    {
        for (int i = 0; i < m_LoggedText.Count; i++)
        {
            m_LoggedText[i].m_Position.y += yOffset / (float)Screen.height;
        }
    }

    void ClearConsoleLog()
    {
        m_LoggedText.Clear();
    }

    public void AddTextToLog(string text)
    {
        AddTextToLog(text, Color.white);
    }

    public void AddTextToLog(string text, Color color)
    {
        if (text == null || text == string.Empty || !m_bConsoleLoggingEnabled)
        {
            return;
        }

        m_BrokenUpConsoleText = BreakUpTextLine(text);

        for (int i = 0; i < m_BrokenUpConsoleText.Count; i++)
        {
            if (m_LoggedText.Count == m_LoggedText.Capacity)
            {
                m_LoggedText.RemoveAt(0);
            }

            float offset = TextHeight / (float)Screen.height;

            // move the log upwards
            OffsetTextLogPositions(-TextHeight);

            m_LoggedText.Add(new TextLine(new Rect(m_ScrollViewPosition.x, m_ScrollViewViewRect.yMax - offset,
                   m_ScrollViewPosition.width, offset), m_BrokenUpConsoleText[i], color));
        }
    }

    // seperates 1 long line into multiple shorter lines in order to fit on the screen
    List<string> BreakUpTextLine(string text)
    {
        m_BrokenUpConsoleText.Clear();

        // break up the lines by searching for newlings
        string[] newLineSeperatedLines = text.Split('\n');
        int maxCharactersPerLine = MaxCharactersPerLine;

        for (int i = 0; i < newLineSeperatedLines.Length; i++)
        {
            // go through each newline and make sure it is short enough, if not, break it up into more newlines
            while (newLineSeperatedLines[i].Length > MaxCharactersPerLine)
            {
                m_BrokenUpConsoleText.Add(newLineSeperatedLines[i].Substring(0, maxCharactersPerLine));
                newLineSeperatedLines[i] = newLineSeperatedLines[i].Remove(0, maxCharactersPerLine);
            }

            // add any remaining string
            if (newLineSeperatedLines[i].Length > 0)
            {
                m_BrokenUpConsoleText.Add(newLineSeperatedLines[i]);
            }
        }

        return m_BrokenUpConsoleText;
    }

    void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                AddTextToLog(logString, Color.red);
                break;

            case LogType.Warning:
                AddTextToLog(logString, Color.yellow);
                break;

            default:
                AddTextToLog(logString);
                break;
        }
    }

#region String Parsing Functions
    // these are helper functions to get parameters out of the command string that was entered
    // i.e. in the string enable_lighting 1, 1 is the parameter
    // these functions return true if they succeeded in finding the respective parameter
    public bool ParseBool(string commandEntered, ref bool out_value)
    {
        bool success = false;
        string subString = "";

        success = ParseSingleParameter(commandEntered, ref subString, 0);
        if (success)
        {
            success = bool.TryParse(subString, out out_value);
            if (!success)
            {
                // try one more time, see if they put a number instead of the words true or false
                int holder = 0;
                success = int.TryParse(subString, out holder);
                if (success)
                {
                    out_value = holder == 0 ? false : true;
                }
            }
        }

        return success;
    }

    public bool ParseInt(string commandEntered, ref int out_value)
    {
        bool success = false;
        string subString = "";

        success = ParseSingleParameter(commandEntered, ref subString, 0);
        if (success)
        {
            success = int.TryParse(subString, out out_value);
        }

        return success;
    }

    public bool ParseFloat(string commandEntered, ref float out_value)
    {
        bool success = false;
        string subString = "";

        success = ParseSingleParameter(commandEntered, ref subString, 0);
        if (success)
        {
            success = float.TryParse(subString, out out_value);
        }

        return success;
    }

    public bool ParseVector2(string commandEntered, ref Vector2 out_value)
    {
       bool success = false;
       string subString = "";
       int uiSearchStartIndex = 0, uiParamStartIndex = 0, uiParamEndIndex = 0;
       const int NumTimesToLoop = 2;

       for (int i = 0; i < NumTimesToLoop; i++)
       {
          success = ParseSingleParameter(commandEntered, ref subString, ref uiParamStartIndex,
             ref uiParamEndIndex, uiSearchStartIndex + uiParamEndIndex);
          if (success)
          {
              float val = 0;
              success = float.TryParse(subString, out val);
              out_value[i] = val;
          }

          if (!success)
          {
             break;
          }
        }

        return success;
    }

    public bool ParseVector3(string commandEntered, ref Vector3 out_value)
    {
       bool success = false;
       string subString = "";
       int uiSearchStartIndex = 0, uiParamStartIndex = 0, uiParamEndIndex = 0;
       const int NumTimesToLoop = 3;

       for (int i = 0; i < NumTimesToLoop; i++)
       {
          success = ParseSingleParameter(commandEntered, ref subString, ref uiParamStartIndex,
             ref uiParamEndIndex, uiSearchStartIndex + uiParamEndIndex);
          if (success)
          {
              float val = 0;
              success = float.TryParse(subString, out val);
              out_value[i] = val;
          }

          if (!success)
          {
             break;
          }
       }

       return success;
    }

    public bool ParseVHMSG(string commandEntered, ref string out_opName, ref string out_arg)
    {
       bool success = false;
       //string subString = "";
       int uiStartIndex = 0, uiEndIndex = 0, uiSearchStartIndex = 0;

       // start the the beginning of the string and look for the vhmsg opcode
       success = ParseSingleParameter(commandEntered, ref out_opName, ref uiStartIndex, ref uiEndIndex, uiSearchStartIndex);
       if (success)
       {
          // now try to find the argument if it has one by using the remainder of the string
          // this second check doesn't have to succeed because not all vhmsg's have arguments, some just use opcodes
           if (uiEndIndex < commandEntered.Length - 1 && uiEndIndex != -1)
          {
             out_arg = commandEntered.Substring(uiEndIndex + 1, commandEntered.Length - uiEndIndex - 1);
          }
       }

       return success;
    }

    // these are helper functions for the rest of the Parsing functions.
    public bool ParseSingleParameter(string commandEntered, ref string out_value, int uiSearchStartIndex)
    {
        int uiStartIndex = 0, uiEndIndex = 0;
        return ParseSingleParameter(commandEntered, ref out_value, ref uiStartIndex, ref uiEndIndex, uiSearchStartIndex);
    }

    public bool ParseSingleParameter(string commandEntered, ref string out_value,
        ref int out_uiParamStartIndex, ref int out_uiParamEndIndex, int uiSearchStartIndex)
    {
       bool success = false;

       // find where the parameter begins using the start delimiter
       out_uiParamStartIndex = commandEntered.IndexOf(ParameterStart, uiSearchStartIndex);
       if (out_uiParamStartIndex != -1 && out_uiParamStartIndex != commandEntered.Length - 1)
       {
          // now find where it ends
          out_uiParamEndIndex = commandEntered.IndexOf(ParameterStart, out_uiParamStartIndex + 1);
          if (out_uiParamEndIndex != -1)
          {
              out_value = commandEntered.Substring(out_uiParamStartIndex + 1, out_uiParamEndIndex - out_uiParamStartIndex - 1);
          }
          else
          {
             // there aren't anymore parameters, you've reached the end of the string
             out_value = commandEntered.Substring(out_uiParamStartIndex + 1/*, commandEntered.Length*/);
          }

          success = true;
       }

       return success;
    }
#endregion
}
