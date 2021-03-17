using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;
using BepInEx.Logging;
using DG.Tweening;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using PathologicalGames;


namespace BalanceOfBumbo
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Balance : BaseUnityPlugin
    {
        public const string pluginGuid = "bumbo.plugins.balanceofbumbo";
        public const string pluginName = "Balance of Bumbo";
        public const string pluginVersion = "1.0.0.0";

        private void Awake()
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

            Harmony.CreateAndPatchAll(typeof(Balance));
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

        // TAINTED PEEP
        [HarmonyPostfix, HarmonyPatch(typeof(PeepsBoss), "Init")]
        public static void Init_Postfix(PeepsBoss __instance)
        {
            __instance.turns = 1;
        }


        /* ENEMIES */

        // Meat Golem - Take away a move
        [HarmonyPostfix, HarmonyPatch(typeof(MeatGolemEnemy), "Init")]
        public static void Init_Postfix(MeatGolemEnemy __instance)
        {
            __instance.turns = 1;
            Console.WriteLine("[Balance of Bum-bo] - Meat Golum now has 1 turn");
        }


        /* SPELLS */

        // D10 - Reworked the floor specific enemies
        [HarmonyReversePatch, HarmonyPatch(typeof(UseSpell), "CastSpell")] // Reverse patch allows access to `base.`  in CastSpell()
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool BaseCastSpellStub(UseSpell instance)
        {
            return default(bool);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(D10Spell), "CastSpell")] // Rewriting the method
        public static bool Prefix(ref D10Spell __instance)
        {
            // use the stub as base.CastSpell()
            if (!BaseCastSpellStub(__instance))
            {
                return false;
            }

            __instance.charge = 0;
            __instance.app.model.spellModel.currentSpell = null;
            __instance.app.model.spellModel.spellQueued = false;
            List<EnemyName> list = new List<EnemyName>();
            List<EnemyName> list2 = new List<EnemyName>();

            int cFloor = __instance.app.model.characterSheet.currentFloor;

            if (cFloor == 1)
            {
                list = new List<EnemyName>
            {
                EnemyName.Dip,
                EnemyName.Tado,     // Tado Kid
                EnemyName.Arsemouth // Tall Boy
            };
                list2 = new List<EnemyName>
            {
                EnemyName.Fly,
                EnemyName.Pooter,
                EnemyName.BoomFly
            };
            }

            if (cFloor == 2)
            {
                list = new List<EnemyName>
            {
                EnemyName.Larry,
                EnemyName.Imposter,
                EnemyName.Greedling,
                EnemyName.Arsemouth // Tall Boy
            };
                list2 = new List<EnemyName>
            {
                EnemyName.Fly,
                EnemyName.Pooter,
                EnemyName.BoomFly,
                EnemyName.Longit
            };
            }

            if (cFloor == 3)
            {
                list = new List<EnemyName>
            {
                EnemyName.Hopper,
                EnemyName.Imposter,
                EnemyName.Burfer,
                EnemyName.RedBlobby,
                EnemyName.Host
            };
                list2 = new List<EnemyName>
            {
                EnemyName.Longit,
                EnemyName.FloatingCultist, // Floater
                EnemyName.Spookie
            };
            }

            if (cFloor == 4)
            {
                list = new List<EnemyName>
            {
                EnemyName.WalkingCultist, // Cultist
                EnemyName.BlackBlobby,
                EnemyName.Burfer,
                EnemyName.RedBlobby
            };
                list2 = new List<EnemyName>
            {
                EnemyName.RedCultist,   // Red Floater
                EnemyName.FloatingCultist, // Floater
                EnemyName.Poofer,
                EnemyName.Hanger // Keeper
            };
            }

            for (int i = __instance.app.model.enemies.Count - 1; i >= 0; i--)
            {
                if (!__instance.app.model.enemies[i].boss && !__instance.app.model.enemies[i].immuneToConversion && __instance.app.model.enemies[i].enemyName != EnemyName.Shit && __instance.app.model.enemies[i].enemyName != EnemyName.Stone)
                {
                    Enemy.EnemyType enemyType = __instance.app.model.enemies[i].enemyType;
                    short x = (short)__instance.app.model.enemies[i].position.x;
                    short y = (short)__instance.app.model.enemies[i].position.y;
                    __instance.app.model.enemies[i].RemoveEnemy();
                    EnemyName enemyName;

                    if (enemyType == Enemy.EnemyType.Ground)
                    {
                        enemyName = list[UnityEngine.Random.Range(0, list.Count)];
                    }
                    else
                    {
                        enemyName = list2[UnityEngine.Random.Range(0, list2.Count)];
                    }

                    __instance.app.model.enemies.Add(PoolManager.Pools["Enemies"].Spawn(enemyName.ToString(), new Vector3(0f, 0f, 0f), Quaternion.Euler(new Vector3(0f, 180f, 0f))).GetComponent<Enemy>());
                    __instance.app.model.enemies[__instance.app.model.enemies.Count - 1].GetComponent<Enemy>().setPosition((int)x, (int)y, 3, 3, false);
                    __instance.app.model.enemies[__instance.app.model.enemies.Count - 1].ChangeOwnership((int)x, (int)y, __instance.app.model.enemies[__instance.app.model.enemies.Count - 1].gameObject);
                    __instance.app.model.enemies[__instance.app.model.enemies.Count - 1].Init();

                    if (__instance.app.model.enemyModel.enemies[enemyName].GetComponent<Enemy>().health != 0f)
                    {
                        __instance.app.model.enemies[__instance.app.model.enemies.Count - 1].GetComponent<Enemy>().health = __instance.app.model.enemyModel.enemies[enemyName].GetComponent<Enemy>().health;
                    }
                    __instance.app.model.enemies[__instance.app.model.enemies.Count - 1].enemyName = enemyName;
                }
                __instance.app.controller.eventsController.SetEvent(new IdleEvent());
            }

            return true;
        }

        // Dog Tooth - Updated description from "Attack that Heals You on Kill"
        [HarmonyPostfix, HarmonyPatch(typeof(DogToothSpell), MethodType.Constructor)]
        public static void DogToothSpell_Ctor_Postfix(DogToothSpell __instance)
        {
            __instance.Name = "Attack that Heals You on Hit";
        }

        // Mega Bean - Updated description from "Knocks back Front Row"
        [HarmonyPostfix, HarmonyPatch(typeof(MegaBeanSpell), MethodType.Constructor)]
        public static void MegaBeanSpell_Ctor_Postfix(MegaBeanSpell __instance)
        {
            __instance.Name = "Knocks back Front Row + Poisons";
        }

        // Barbed Wire - Updated description from "Melee Attackers Take Damage"
        [HarmonyPostfix, HarmonyPatch(typeof(BarbedWireSpell), MethodType.Constructor)]
        public static void BarbedWireSpell_Ctor_Postfix(BarbedWireSpell __instance)
        {
            __instance.Name = "All Attackers Take Damage";
        }

        // Euthanasia - Updated description from "Hurts an Enemy that Hits You"
        [HarmonyPostfix, HarmonyPatch(typeof(EuthanasiaSpell), MethodType.Constructor)]
        public static void EuthanasiaSpell_Ctor_Postfix(EuthanasiaSpell __instance)
        {
            __instance.Name = "Hurts an attacking Enemy";
        }

        // Bumbo Shake - Updated description from "Rerolls the Puzzle Board"
        [HarmonyPostfix, HarmonyPatch(typeof(BumboShakeSpell), MethodType.Constructor)]
        public static void BumboShakeSpell_Ctor_Postfix(BumboShakeSpell __instance)
        {
            __instance.Name = "Reshuffles the Puzzle Board";
        }

        // Mushroom - Updated description from "+1 Damage and Life Gain"
        [HarmonyPostfix, HarmonyPatch(typeof(MushroomSpell), MethodType.Constructor)]
        public static void MushroomSpell_Ctor_Postfix(MushroomSpell __instance)
        {
            __instance.Name = "+1 Damage and Heals";
        }

        // Yum Heart - Updated description from "Gain 1 Heart"
        [HarmonyPostfix, HarmonyPatch(typeof(YumHeartSpell), MethodType.Constructor)]
        public static void YumHeartSpell_Ctor_Postfix(YumHeartSpell __instance)
        {
            __instance.Name = "Heals 1 Heart";
        }


        /* TRINKETS */

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