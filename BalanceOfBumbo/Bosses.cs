using System;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;


namespace BalanceOfBumbo
{
    public static class Bosses
    {
        public static void Awake()
        {
            // Pyre - Grab methods for Hurt and MadeCombo
            var pyreHurt = typeof(PyreBoss).GetMethod(nameof(Hurt));
            var pyreMadeCombo = typeof(PyreBoss).GetMethod("MadeCombo");

            // Pyre - Remove spawn wisp from Hurt method
            var wispRemoveHook = new ILHook(pyreHurt, RemoveSpawnWisp);
            wispRemoveHook.Apply();

            // Pyre - add spawn wisp to MadeCombo method
            var wispAddHook = new ILHook(pyreMadeCombo, AddSpawnWisp);
            wispAddHook.Apply();

            var caddyHurt = typeof(CaddyBoss).GetMethod(nameof(Hurt));
            var caddyHurtChange = new ILHook(caddyHurt, ChangeHurtDamage);
            caddyHurtChange.Apply();

            Harmony.CreateAndPatchAll(typeof(Bosses));

            Console.WriteLine("[Balance of Bum-Bo] Boss changes loaded");
        }


        /* BOSSES */

        // PYRE - Making him only spawn Wisps on extinguish
        public static void RemoveSpawnWisp(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<PyreBoss>("SpawnWisp"));

            var label = c.DefineLabel();
            c.Emit(OpCodes.Br, label);
            c.Index += 2;
            c.MarkLabel(label);
        }

        public static void AddSpawnWisp(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Enemy>("Hurt"));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, typeof(PyreBoss).GetMethod("SpawnWisp"));
        }

        // Sangre - Chest only closes with 4 or more damage
        public static void ChangeHurtDamage(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchCall<CaddyBoss>("CloseChest"));

            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldc_R4, 4f);

            var target = c.DefineLabel();

            c.Emit(OpCodes.Blt_Un_S, target);

            c.GotoNext(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(1),
            x => x.MatchLdarg(2),
            x => x.MatchLdarg(3));

            c.MarkLabel(target);

            Console.WriteLine("[Balance of Bum-Bo] Change Sangre");
            Console.WriteLine("[Balance of Bum-Bo] Chest now only closes with >= 4 damage");
        }

        // TAINTED PEEPER
        [HarmonyPostfix, HarmonyPatch(typeof(PeepsBoss), "Init")]
        public static void Init_Postfix(PeepsBoss __instance)
        {
            __instance.turns = 1;
            Console.WriteLine("[Balance of Bum-Bo] Changed Tainted Peeper's move to 1");
        }
    }
}

// Sangre - Chest now only closes with 4 or more damage