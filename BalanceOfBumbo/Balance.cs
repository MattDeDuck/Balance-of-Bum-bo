using BepInEx;
using HarmonyLib;


namespace BalanceOfBumbo
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Balance : BaseUnityPlugin
    {
        public const string pluginGuid = "bumbo.plugins.balanceofbumbo";
        public const string pluginName = "Balance of Bumbo";
        public const string pluginVersion = "1.2.0.0";

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Balance));
                        
            Bosses.Awake();     // Boss changes            
            Enemies.Awake();    // Enemy changes            
            Spells.Awake();     // Spell changes            
            Trinkets.Awake();   // Trinket changes
        }
    }
}