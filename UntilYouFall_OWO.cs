using System;
using MelonLoader;
using HarmonyLib;
using System.Threading;

using SG.Claymore.Movement.Dash;
using SG.Claymore.Combat.Blocking;
using SG.Claymore.Interaction;
using SG.Claymore.Armaments;
using SG.Claymore.Armaments.Abilities;
using SG.Claymore.Player;
using SG.Claymore.Combat.EnemyAttacks;
using SG.Claymore.Entities;
using SG.Claymore.Armory;
using MyOWOVest;

namespace UntilYouFall_OWO
{
    public class UntilYouFall_OWO : MelonMod
    {
        public static TactsuitVR tactsuitVr;
        private static String ActiveHand = "PlayerHandRight";
        private static ManualResetEvent mrse = new ManualResetEvent(false);
        private static bool BulwarkActive = false;
        private static bool handsConnected = true;

        private static void HeartBeatFunc()
        {
            while (true)
            {
                mrse.WaitOne();
                tactsuitVr.PlayBackFeedback("HeartBeat");
                Thread.Sleep(1000);
            }
        }

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
                    if (handsConnected) { tactsuitVr.PlayBackFeedback("SummonWeaponHands_L"); }
                }
                else
                {
                    // tactsuitVr.LOG("MeleeWeapon.Summon Right");
                    tactsuitVr.PlayBackFeedback("SummonWeapon_R");
                    if (handsConnected) { tactsuitVr.PlayBackFeedback("SummonWeaponHands_R"); }
                }
            }
        }


        [HarmonyPatch(typeof(MeleeWeapon), "GetForceRating", new Type[] { typeof(SG.Claymore.HitSystem.HitData.HitQualityType) })]
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
                        if (handsConnected) { tactsuitVr.PlayBackFeedback("BlockHands_L", intensity); }
                        if (__instance.armament.isHeldInTwoHands)
                        {
                            tactsuitVr.PlayBackFeedback("Block_R", intensity * 0.5f);
                            if (handsConnected) { tactsuitVr.PlayBackFeedback("BlockHands_R", intensity * 0.5f); }
                        }
                    }
                        else
                    {
                        tactsuitVr.PlayBackFeedback("Block_R", intensity);
                        if (handsConnected) { tactsuitVr.PlayBackFeedback("BlockHands_R", intensity); }
                        if (__instance.armament.isHeldInTwoHands)
                        {
                            tactsuitVr.PlayBackFeedback("Block_L", intensity * 0.5f);
                            if (handsConnected) { tactsuitVr.PlayBackFeedback("BlockHands_L", intensity * 0.5f); }
                        }
                    }
                    // tactsuitVr.LOG("GetForceRating");
                    // tactsuitVr.LOG(__result.ToString());

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
                    tactsuitVr.PlayBackFeedback("CrushCrystal_L");
                    tactsuitVr.PlayBackFeedback("ActivateSuper_L");
                    if (handsConnected) { tactsuitVr.PlayBackFeedback("CrushCrystalHands_L"); }
                } else
                {
                    tactsuitVr.PlayBackFeedback("CrushCrystal_R");
                    tactsuitVr.PlayBackFeedback("ActivateSuper_R");
                    if (handsConnected) { tactsuitVr.PlayBackFeedback("CrushCrystalHands_R"); }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "OnHealthChanged")]
        public class HealthChanged
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerHealth __instance, float hp)
            {
                BulwarkActive = false;
                // tactsuitVr.LOG(hp.ToString());
                if (hp >= 1.0f)
                {
                    mrse.Reset();
                }
                if(__instance.IsDead)
                {
                    mrse.Reset();
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
                    mrse.Set();
                }
                if (__instance.health.IsDead)
                {
                    mrse.Reset();
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
                    tactsuitVr.PlayBackFeedback("BlockVest_L");
                    if (handsConnected) { tactsuitVr.PlayBackFeedback("BlockHands_L"); }
                    if (blocker.armament.isHeldInTwoHands)
                    {
                        tactsuitVr.PlayBackFeedback("Block_R", 0.5f);
                        tactsuitVr.PlayBackFeedback("BlockVest_R", 0.5f);
                        if (handsConnected) { tactsuitVr.PlayBackFeedback("BlockHands_R", 0.5f); }
                    }
                }
                else
                {
                    tactsuitVr.PlayBackFeedback("Block_R");
                    tactsuitVr.PlayBackFeedback("BlockVest_R");
                    if (handsConnected) { tactsuitVr.PlayBackFeedback("BlockHands_R"); }
                    if (blocker.armament.isHeldInTwoHands)
                    {
                        tactsuitVr.PlayBackFeedback("Block_L", 0.5f);
                        tactsuitVr.PlayBackFeedback("BlockVest_L", 0.5f);
                        if (handsConnected) { tactsuitVr.PlayBackFeedback("BlockHands_L", 0.5f); }
                    }
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

        [HarmonyPatch(typeof(CrushInteractable), "OnCrushStart")]
        public class CrushStart
        {
            [HarmonyPostfix]
            public static void Postfix(CrushInteractable __instance)
            {
                if (__instance.crushingHand.name == "PlayerHandLeft")
                {
                    //tactsuitVr.LOG("CrushInteractable.OnCrushStart Left");
                    tactsuitVr.PlayBackFeedback("CrushCrystal_L");
                    if (handsConnected) { tactsuitVr.PlayBackFeedback("CrushCrystalHands_L"); }
                    ActiveHand = "PlayerHandLeft";
                } else
                {
                    //tactsuitVr.LOG("CrushInteractable.OnCrushStart Right");
                    tactsuitVr.PlayBackFeedback("CrushCrystal_R");
                    if (handsConnected) { tactsuitVr.PlayBackFeedback("CrushCrystalHands_R"); }
                    ActiveHand = "PlayerHandRight";
                }
            }
        }

        [HarmonyPatch(typeof(CrushInteractable), "OnCancelCrush")]
        public class CrushCancel
        {
            [HarmonyPostfix]
            public static void Postfix(CrushInteractable __instance)
            {
                //tactsuitVr.LOG("CrushInteractable.OnCancelCrush");
                if (__instance.crushingHand.name == "PlayerHandLeft")
                {
                    //tactsuitVr.StopHapticFeedback(TactsuitVR.FeedbackType.CrushCrystal_L);
                    //tactsuitVr.StopHapticFeedback(TactsuitVR.FeedbackType.CrushCrystalHands_L);
                }
                else
                {
                    //tactsuitVr.StopHapticFeedback(TactsuitVR.FeedbackType.CrushCrystal_R);
                    //tactsuitVr.StopHapticFeedback(TactsuitVR.FeedbackType.CrushCrystalHands_R);
                }
            }
        }

        [HarmonyPatch(typeof(CrushInteractable), "FinishCrush")]
        public class CrushComplete
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (ActiveHand == "PlayerHandLeft")
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

        [HarmonyPatch(typeof(GroundAttackPlayable), "Raise")]
        public class GroundAttack
        {
            [HarmonyPostfix]
            public static void Postfix(GroundAttackPlayable __instance)
            {
                tactsuitVr.LOG("GroundAttackPlayable.Raise");
                tactsuitVr.PlayBackFeedback("GroundAttack");
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "Restore")]
        public class RestoreHealth
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                // tactsuitVr.LOG("PlayerHealth.Restore");
                tactsuitVr.PlayBackFeedback("Heal");
            }
        }

        [HarmonyPatch(typeof(WeaponRackSlot), "OnEnable")]
        public class WeaponRack
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlayBackFeedback("RiseAgain");
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
