using System;
using HarmonyLib;


namespace BalanceOfBumbo
{
    public static class Enemies
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Enemies));

            Console.WriteLine("[Balance of Bum-Bo] Enemy changes loaded");
        }

        // Meat Golem - Take away a move 
        [HarmonyPostfix, HarmonyPatch(typeof(MeatGolemEnemy), "Init")]
        public static void Init_Postfix(MeatGolemEnemy __instance)
        {
            __instance.turns = 1;
            Console.WriteLine("[Balance of Bum-bo] - Meat Golum now has 1 turn");
        }

        // Daddy Tato - Take away a move 
        [HarmonyPostfix, HarmonyPatch(typeof(TaderEnemy), "Init")]
        public static void Init_Postfix(TaderEnemy __instance)
        {
            __instance.turns = 1;
            Console.WriteLine("[Balance of Bum-bo] - Daddy Tato now has 1 turn");
        }
    }
}