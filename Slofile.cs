using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

namespace sloth
{
  public enum sloTypes { String, Array, Unknown }

  public class SloScript
  {
    public List<SloCommand> commands = new List<SloCommand>();
  }

  public class SloCommand
  {
    public SloCommand(string command, string[] args)
    {
        (this.command, this.args) = (command, args);
    }
    public string command { get; }
    public string[] args { get; }
  }

  public static class SloCommands
  {
    /// <summary>
    /// checks a command and its arguments for correct syntax
    /// </summary>
    /// <param name="command">command</param>
    /// <param name="args">arguments</param>
    static List<string> CheckCommand(string command, string[] args) {
      List<string> errors = new List<string>();

      // build argument types
      List<sloTypes> types = new List<sloTypes>();
      foreach(string arg in args)
      {
        int quoteCount = arg.Count(c => c == '"');
        int squareBracketOpenCount = arg.Count(c => c == '[');
        int squareBracketCloseCount = arg.Count(c => c == ']');

        if(Regex.IsMatch(arg, @"^\s*""") && Regex.IsMatch(arg, @"\s""*$") && quoteCount == 2) types.Add(sloTypes.String);
        else if(Regex.IsMatch(arg, @"^\s*\[") && Regex.IsMatch(arg, @"\]\s*$") && squareBracketOpenCount == 1 && squareBracketCloseCount == 1) types.Add(sloTypes.Array);
        else types.Add(sloTypes.Unknown);
      }

      switch(command.ToLower()) {
        case "load":
          if(args.Length != 2) errors.Add("Two arguments expected for Load() in line ");
          if(types[0] != sloTypes.String) errors.Add("First argument of Load() must be of type string in line ");
          if(types[1] != sloTypes.String) errors.Add("Second argument of Load() must be of type string in line ");
          if(types[0] == sloTypes.String && !File.Exists(args[0])) errors.Add("File not found in line ");
          break;
      }

      return errors;
    }
  }
}
