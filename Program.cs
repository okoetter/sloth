using System;
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

    private static void processScriptFile(string scriptFileContent)
    {
      // remove comments /* ... */
      scriptFileContent = Regex.Replace(scriptFileContent, @"\/\*.+?\*\/", "", RegexOptions.Singleline);
      // remove comments //
      scriptFileContent = Regex.Replace(scriptFileContent, @"\/\/.+?$", "", RegexOptions.Multiline);
      // remove line breaks
      scriptFileContent = Regex.Replace(scriptFileContent, @"[\r\n]", "");

      string[] lines = scriptFileContent.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      foreach (string line in lines)
      {

      }
    }
  }
}
