using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Data;
using ExcelDataReader;

namespace sloth
{
  public enum SloTypes { String, Array, Unknown }

  public class SloScript: List<SloCommand> { }

  public class SloCommand
  {
    public int LineNo { get; }
    public string Command { get; }
    public string[] Arguments { get; }

    public SloCommand(int lineNo, string command, string[] args)
    {
      (this.LineNo, this.Command, this.Arguments) = (lineNo, command, args);
    }
  }
  
  public static class SloCommands
  {
    /// <summary>
    /// checks a script for correct syntax
    /// </summary>
    /// <param name="script">command</param>
    /// <returns>list of error messages</returns>
    static List<string> CheckSyntax(SloScript script) {
      List<string> errors = new List<string>();

      foreach (SloCommand command in script)
      {
        // build argument types
        List<SloTypes> types = new List<SloTypes>();
        foreach(string arg in command.Arguments)
        {
          int quoteCount = arg.Count(c => c == '"');
          int squareBracketOpenCount = arg.Count(c => c == '[');
          int squareBracketCloseCount = arg.Count(c => c == ']');

          if(Regex.IsMatch(arg, @"^\s*""") && Regex.IsMatch(arg, @"\s""*$") && quoteCount == 2) types.Add(SloTypes.String);
          else if(Regex.IsMatch(arg, @"^\s*\[") && Regex.IsMatch(arg, @"\]\s*$") && squareBracketOpenCount == 1 && squareBracketCloseCount == 1) types.Add(SloTypes.Array);
          else types.Add(SloTypes.Unknown);
        }

        // check commands          
        switch(command.Command.ToLower()) {
          // load(filename: string, type: "xlsx" | "\t" | ";" | ",")
          case "load":
            if(command.Arguments.Length != 2) errors.Add($"Two arguments expected for Load() in line {command.LineNo}");
            if(types[0] != SloTypes.String) errors.Add($"First argument of Load() must be of type string in line {command.LineNo}");
            if(types[1] != SloTypes.String) errors.Add($"Second argument of Load() must be of type string in line {command.LineNo}");
            if(types[0] == SloTypes.String && !File.Exists(command.Arguments[0])) errors.Add($"File not found in line {command.LineNo}");
            if(!new[] { "xlsx", "\t", ";", "," }.Contains(command.Arguments[1].ToLower())) errors.Add($"Invalid file type '{command.Arguments[1]}' in line {command.LineNo}");
            break;

          // save(filename: string, type: "xlsx" | "\t" | ";" | ",")
          case "save":
            if(command.Arguments.Length != 2) errors.Add($"Two arguments expected for Save() in line {command.LineNo}");
            if(types[0] != SloTypes.String) errors.Add($"First argument of Save() must be of type string in line {command.LineNo}");
            if(types[1] != SloTypes.String) errors.Add($"Second argument of Save() must be of type string in line {command.LineNo}");
            if(types[0] == SloTypes.String && !File.Exists(command.Arguments[0])) errors.Add($"File not found in line {command.LineNo}");
            if(!new[] { "xlsx", "\t", ";", "," }.Contains(command.Arguments[1].ToLower())) errors.Add($"Invalid file type '{command.Arguments[1]}' in line {command.LineNo}");
            break;

          default:
            errors.Add($"Unknown command in line {command.LineNo}");
            break;
        }
      }

      return errors;
    }
  
    static DataTable Load(string filename, string type)
    {
      if (type.ToLower() == "xlsx") {
        using (var inputStream = File.Open(filename, FileMode.Open, FileAccess.Read))
        {
          try
          {
            var excelReader = ExcelReaderFactory.CreateOpenXmlReader(inputStream);
            var result = excelReader.AsDataSet();
            return result.Tables[0];
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Could not read file {filename}: {ex.Message}");
          }
        }
      }
      
      return new DataTable();
    }
  }
}
