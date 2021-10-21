using System;
using System.Collections.Generic;

namespace sloth
{
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
}
