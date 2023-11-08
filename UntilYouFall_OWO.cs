using System;
using MelonLoader;
using HarmonyLib;
using System.Threading;

using Il2CppSG.Claymore.Movement.Dash;
using Il2CppSG.Claymore.Combat.Blocking;
using Il2CppSG.Claymore.Interaction;
using Il2CppSG.Claymore.Armaments;
using Il2CppSG.Claymore.Armaments.Abilities;
using Il2CppSG.Claymore.Player;
using Il2CppSG.Claymore.Combat.EnemyAttacks;
using Il2CppSG.Claymore.Entities;
using Il2CppSG.Claymore.Armory;
using MyOWOVest;
using static MelonLoader.MelonLogger;

[assembly: MelonInfo(typeof(UntilYouFall_OWO.UntilYouFall_OWO), "UntilYouFall_OWO", "3.0.1", "Florian Fahrenberger")]
[assembly: MelonGame("Schell Games", "UntilYouFall")]

namespace UntilYouFall_OWO
{
    public class UntilYouFall_OWO : MelonMod
    {
        public static TactsuitVR tactsuitVr = null!;
        private static bool BulwarkActive = false;

        public override void OnUpdate()
        {
        }


        public override void OnInitializeMelon()
        {
            tactsuitVr = new TactsuitVR();
        }


        [HarmonyPatch(typeof(PlayerDash), "OnDashForward")]
        public class DashForward
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerDash __instance)
            {
                //tactsuitVr.LOG("OnDashForward.");
                try
                {
                    //TactsuitVR.FeedbackType feedback = TactsuitVR.FeedbackType.DashForward;
                    //TactsuitUntilYouFall.tactsuitVr.SimpleFeedback(feedback);
                    tactsuitVr.PlayBackFeedback("DashForward");
                }
                catch(Exception e) { tactsuitVr.LOG("Feedback failed. " + e.ToString()); }
            }
        }

        [HarmonyPatch(typeof(Armament), "InitSummon")]
        public class InitSummon
        {
            [HarmonyPostfix]
            public static void Postfix(Armament __instance, PlayerHand summoningHand)
            {
                if (summoningHand.name == "PlayerHandLeft")
                {
                    // tactsuitVr.LOG("MeleeWeapon.Summon Left");
                    tactsuitVr.PlayBackFeedback("SummonWeapon_L");
                }
                else
                {
                    // tactsuitVr.LOG("MeleeWeapon.Summon Right");
                    tactsuitVr.PlayBackFeedback("SummonWeapon_R");
                }
            }
        }


        [HarmonyPatch(typeof(MeleeWeapon), "GetForceRating", new Type[] { typeof(Il2CppSG.Claymore.HitSystem.HitData.HitQualityType) })]
        public class HitForce
        {
            [HarmonyPostfix]
            public static void Postfix(MeleeWeapon __instance, float __result)
            {

                if (__result > 1.0)
                {
                    float intensity = __result / 10.0f + 0.02f;
                    if (__instance.armament.BoundHand == __instance.HoldingPlayer.LeftHand)
                    {
                        tactsuitVr.PlayBackFeedback("Block_L", intensity);
                    }
                        else
                    {
                        tactsuitVr.PlayBackFeedback("Block_R", intensity);
                    }

                }
            }
        }

        [HarmonyPatch(typeof(ArmamentAbilityUser), "OnSuperActivated")]
        public class SuperActivated
        {
            [HarmonyPostfix]
            public static void Postfix(ArmamentAbilityUser __instance)
            {
                // tactsuitVr.LOG("ArmamentAbilityUser");
                if (__instance.HoldingPlayer.LeftHand == __instance.armament.BoundHand)
                {
                    tactsuitVr.PlayBackFeedback("ActivateSuper_L");
                } else
                {
                    tactsuitVr.PlayBackFeedback("ActivateSuper_R");
                }
            }
        }

        [HarmonyPatch(typeof(PlayerDefense), "OnAttackHit")]
        public class OnAttackHit
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerDefense __instance, BlockTimingData blockTiming)
            {
                // tactsuitVr.LOG("PlayerDefense.OnAttackHit");
                if (__instance.health.IsOnDeathsDoor)
                {
                    tactsuitVr.PlayBackFeedback("ThreeHeartBeats");
                    return;
                }
                if (BulwarkActive)
                {
                    // tactsuitVr.LOG("Bulwark active");
                    tactsuitVr.PlayBackFeedback("Bulwark");
                }
                else if (blockTiming.IsDodgePremonition)
                {
                    tactsuitVr.PlayBackFeedback("HitByHammer");
                }
                else
                {
                    tactsuitVr.PlayBackFeedback("SlashDefault");
                }
            }
        }

        [HarmonyPatch(typeof(PlayerDefense), "OnAttackBlocked")]
        public class OnAttackBlocked
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerDefense __instance, AttackBlocker blocker)
            {
                // tactsuitVr.LOG("PlayerDefense.OnAttackBlocked");
                if (blocker == blocker.HoldingPlayer.leftBlocker)
                {
                    tactsuitVr.PlayBackFeedback("Block_L");
                }
                else
                {
                    tactsuitVr.PlayBackFeedback("Block_R");
                }

            }
        }

        [HarmonyPatch(typeof(PlayerDefense), "KnockbackPlayer")]
        public class Knockback
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                // tactsuitVr.LOG("PlayerDefense.KnockBackPlayer");
                tactsuitVr.PlayBackFeedback("KnockBack");
            }
        }

        [HarmonyPatch(typeof(CrushInteractable), "FinishCrush")]
        public class CrushComplete
        {
            [HarmonyPostfix]
            public static void Postfix(CrushInteractable __instance)
            {
                if (__instance.crushingHand.name == "PlayerHandLeft")
                {
                    //tactsuitVr.LOG("CrushInteractable.OnCrushStart Left");
                    tactsuitVr.PlayBackFeedback("CrystalCrushed_L");
                }
                else
                {
                    //tactsuitVr.LOG("CrushInteractable.OnCrushStart Right");
                    tactsuitVr.PlayBackFeedback("CrystalCrushed_R");
                }
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "Restore")]
        public class RestoreHealth
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                // tactsuitVr.LOG("PlayerHealth.Restore");
                tactsuitVr.PlayBackFeedback("Healing");
            }
        }

        [HarmonyPatch(typeof(WeaponRackSlot), "OnEnable")]
        public class WeaponRack
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlayBackFeedback("Start");
            }
        }

        [HarmonyPatch(typeof(BulwarkAbility), "OnActivationSuccess")]
        public class ActivateBulwark
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                // tactsuitVr.LOG("Activate Bulwark");
                BulwarkActive = true;
            }
        }

    }
}
