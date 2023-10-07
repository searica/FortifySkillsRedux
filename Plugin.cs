using BepInEx;
using HarmonyLib;
using System.Reflection;


namespace FortifySkillsRedux
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginName = "FortifySkillsRedux";
        internal const string Author = "Searica";
        public const string PluginGuid = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "1.0.0";

        Harmony _harmony;
        public void Awake()
        {
            Log.Init(Logger);
            PluginConfig.Init(Config);
            PluginConfig.SetUpConfig();
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
        }

        public void OnDestroy()
        {
            PluginConfig.Save();
            _harmony?.UnpatchSelf();
        }
    }
}
