using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace sloth
{
  class Program
  {
    static void Main(string[] args)
    {
      const string __version = "0.0.1";

      Console.WriteLine($"Sloth {__version}");
      Console.WriteLine("by Oliver Kötter, oliver.koetter@interrogare.de");
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
      processScriptFile(scriptFileContent);
    }
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

    private static string[] SplitByCommaExceptInStrings(string input)
    {
      List<string> result = new List<string>();
      bool withinString = false;
      int startIndex = 0;
      for(int i = startIndex; i < input.Length; i++) {
        var c = input[i];
        if(c=='x')
          System.Console.WriteLine("x");
        if (c == '"') withinString = !withinString;
        if ((c == ',' && !withinString) || i >= input.Length - 1) {
          if( (c == ',' && !withinString)) result.Add(input.Substring(startIndex, i - startIndex).Trim());
          else result.Add(input.Substring(startIndex, i - (startIndex - 1)).Trim());
          startIndex = i + 1;
        }
      }

      return result.ToArray();
    }

    private static void processScriptFile(string scriptFileContent)
    {
      // remove comments /* ... */
      scriptFileContent = Regex.Replace(scriptFileContent, @"\/\*.+?\*\/", "", RegexOptions.Singleline);
      // remove comments //
      scriptFileContent = Regex.Replace(scriptFileContent, @"\/\/.+?$", "", RegexOptions.Multiline);
      // remove line breaks
      scriptFileContent = Regex.Replace(scriptFileContent, @"[\r\n]", "");

      SloScript script = new SloScript();
      string[] lines = scriptFileContent.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      foreach (string line in lines)
      {
        var parts = Regex.Matches(line, @"([a-z]+)\s*\(\s*(.+)\s*\)", RegexOptions.IgnoreCase);
        var command = parts[0].Groups[1].ToString();
        var parameterString = parts[0].Groups[2].ToString();
        var parameters = SplitByCommaExceptInStrings(parameterString);

        //SloCommand command = new SloCommand();
      }
    }
  }
}
