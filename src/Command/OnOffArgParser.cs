using Vintagestory.API.Common;

namespace Pl3xTweaks.Command;

public class OnOffArgParser : BoolArgParser {
    public OnOffArgParser(string name) : base(name, "on", true) { }

    public override string GetSyntaxExplanation(string indent) {
        return indent + GetSyntax() + " is a boolean, including 1 or 0, yes or no, true or false, and on or off";
    }
}
