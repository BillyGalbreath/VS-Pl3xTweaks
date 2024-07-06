using System.Diagnostics.CodeAnalysis;

namespace pl3xtweaks.configuration;

[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
public class Config {
    public string ServerName = "[EN] Pl3x | {0} {1} of Year {2}";

    public string ServerDescription = """
                                      <strong>Welcome to Pl3x</strong>

                                      Ran on the best server hardware and network money can buy hosted at the 'center of the internet' (Ashburn, Virginia) for low latency to both US and EU for a lag-free experience.

                                      Reduced hunger and food spoilage, custom mods, popular mods, pvp disabled, hour long days and 24 day months, unlimited claim blocks, laid-back rule-free (almost) atmosphere.

                                      Join us on <strong>Discord</strong> at: <a href='https://discord.gg/JXra7N4'>https://discord.gg/JXra7N4</a>
                                      """;

    public Tips Tips = new();
}
