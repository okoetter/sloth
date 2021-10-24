using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace sloth
{
  class Program
  {
    /// <summary>
    /// main entry point of app
    /// </summary>
    /// <param name="args">arguments for sloth</param>
    static void Main(string[] args)
    {
      const string __version = "0.0.1";

      Console.WriteLine($"Sloth {__version}");
      Console.WriteLine("by Oliver Kötter, oliver@koetter.cc");
      Console.WriteLine();
      var scriptFilename = "";
      if (args.Length == 0) {
        Console.WriteLine($"Syntax:{Environment.NewLine}sloth scriptfile.slo [arg1] [arg2] [...] [argn]{Environment.NewLine}");
        Environment.Exit(0);
      }
      else {
        if (File.Exists(args[0])) scriptFilename = args[0];
        else {
          Console.WriteLine($"Script file not found: {scriptFilename}");
          Environment.Exit(1);
        }
        Console.WriteLine($"Script file: {scriptFilename}");
      }

      var scriptFileContent = readScriptFileContent(scriptFilename);
      (SloScript script, List<string> errors) = processScriptFile(scriptFileContent);
      if (errors.Count > 0)
      {
        errors.ForEach(e => Console.WriteLine(e));
        Environment.Exit(1);
      }
    }

    /// <summary>
    /// reads the whole script file into one giant string
    /// </summary>
    /// <param name="scriptFilename">full path to script file to read</param>
    /// <returns>string containing the script file</returns>
    private static string readScriptFileContent(string scriptFilename)
    {
      var scriptContent = "";
      try
      {
        using(StreamReader sr = new StreamReader(scriptFilename, true)) {
          scriptContent = sr.ReadToEnd();
        }
      }
      catch(Exception ex)
      {
        Console.WriteLine($"Could not read script file {scriptFilename}:{Environment.NewLine}{ex.Message}");
        Environment.Exit(1);
      }

      return scriptContent;
    }

    /// <summary>
    /// splits comma separated argument list into array while ignoring commas in strings or arrays
    /// </summary>
    /// <param name="input">comma separated argument list</param>
    /// <returns>array of arguments</returns>
    private static string[] splitArguments(string input)
    {
      List<string> result = new List<string>();
      bool withinString = false;
      bool withinArray = false;
      int startIndex = 0;
      for(int i = startIndex; i < input.Length; i++) {
        var c = input[i];
        if (c == '"') withinString = !withinString;
        else if (c == '[' && !withinString) withinArray = true;
        else if (c == ']' && !withinString) withinArray = false;

        if ((c == ',' && !withinString && !withinArray) || i >= input.Length - 1) {
          if( (c == ',' && !withinString && !withinArray)) result.Add(input.Substring(startIndex, i - startIndex).Trim());
          else result.Add(input.Substring(startIndex, i - (startIndex - 1)).Trim());
          startIndex = i + 1;
        }
      }

      return result.ToArray();
    }

    /// <summary>
    /// processes slo script string and build SloScript object
    /// </summary>
    /// <param name="scriptFileContent">slo script string</param>
    /// <returns>Tuple with Script and errors list containing any syntax errors</returns>
    private static (SloScript, List<string> errors) processScriptFile(string scriptFileContent)
    {
      SloScript script = new SloScript();
      List<string> errors = new List<string>();
      string[] scriptLines = scriptFileContent.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      bool isInMultiLineComment = false;
      for (int lineNo = 0; lineNo < scriptLines.Length; lineNo++)
      {
        string scriptLine = scriptLines[lineNo];
        
        // remove comments /* ... */
        scriptLine = Regex.Replace(scriptLine, @"\/\*.+?\*\/", string.Empty);
        // remove comments //
        scriptLine = Regex.Replace(scriptLine, @"\/\/.+?$", string.Empty);
        // check if multi line comment starts
        if (scriptLine.Contains("/*"))
        {
          isInMultiLineComment = true;
          scriptLine = Regex.Replace(scriptLine, @"\/\*.*$", string.Empty);
        }
        // check if multi line comment ends
        else if (scriptLine.Contains("*/"))
        {
          isInMultiLineComment = false;
          scriptLine = Regex.Replace(scriptLine, @"^.*?\*\/", string.Empty);
        }
        // skip whole line if within multi line comment 
        else if (isInMultiLineComment) continue;

        // skip whole line if line is empty
        if (scriptLine.Trim().Length == 0) continue;

        // process line
        string[] commands = scriptLine.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (string command in commands)
        {
          var parts = Regex.Matches(command, @"(?<function>[a-z]+)\s*\(\s*(?<arguments>.*)\s*\)", RegexOptions.IgnoreCase);
          if (parts.Count == 0 || parts[0].Groups.Count != 3) {
            errors.Add($"Syntax error in line {lineNo + 1}: '{command}'");
            continue;
          }
          var function = parts[0].Groups["function"].ToString();
          var parameterString = parts[0].Groups["arguments"].ToString();
          var parameters = splitArguments(parameterString);
          var sloCommand = new SloCommand(lineNo + 1, function, parameters);
          script.Add(sloCommand);
        }
      }

      return (script, errors);
    }  
  }
}
