using HarmonyLib;
using System;


namespace BalanceOfBumbo
{
    public static class Trinkets
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Trinkets));

            Console.WriteLine("[Balance of Bum-Bo] Trinket changes loaded");
        }

        // Santa Sangre - Updated description from "May Gain Life from Kills"
        [HarmonyPostfix, HarmonyPatch(typeof(SantaSangreTrinket), MethodType.Constructor)]
        public static void SantaSangreTrinket_Ctor_Postfix(SantaSangreTrinket __instance)
        {
            __instance.Name = "May Gain Soul Life from Kills";
        }

        // Stray Barb - Updated description from "Enemies May Take Damage"
        [HarmonyPostfix, HarmonyPatch(typeof(StrayBarbTrinket), MethodType.Constructor)]
        public static void StrayBarbTrinket_Ctor_Postfix(StrayBarbTrinket __instance)
        {
            __instance.Name = "Chance For A Counter Attack";
        }

        // One Up - Updated description from "1up!"
        [HarmonyPostfix, HarmonyPatch(typeof(OneUpTrinket), MethodType.Constructor)]
        public static void OneUpTrinket_Ctor_Postfix(OneUpTrinket __instance)
        {
            __instance.Name = "1 Extra Life!";
        }

        // Pink Bow - Updated description from "Gain Soul Hearts"
        [HarmonyPostfix, HarmonyPatch(typeof(PinkBowTrinket), MethodType.Constructor)]
        public static void PinkBowTrinket(PinkBowTrinket __instance)
        {
            __instance.Name = "+1 Soul Heart End of Floor";
        }
    }
}