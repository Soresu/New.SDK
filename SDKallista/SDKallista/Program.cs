using System;
using System.Linq;
using System.Reflection;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Math.Prediction;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.Utils;
using SharpDX;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace SDKallista
{
    internal class Program
    {
        public static Menu config;
        //public static AutoLeveler autoLeveler;
        public static Spell Q, W, E, R;
        public static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static Obj_AI_Hero Blitz;

        private static void Main(string[] args)
        {
            Load.OnLoad += OnGameLoad;
        }

        private static void OnGameLoad(object sender, EventArgs e)
        {
            if (Player.ChampionName != "Kalista")
            {
                return;
            }

            InitKallista();
            InitMenu();
            Drawing.OnDraw += Game_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            HpBarDamageIndicator.DamageToUnit = GetEdamage;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    Combo();
                    break;
                case OrbwalkerMode.Hybrid:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    break;
                case OrbwalkerMode.LastHit:
                    Lasthit();
                    break;
                default:
                    break;
            }
            JungleSteal();
        }

        private static void JungleSteal()
        {
            if (!E.IsReady())
            {
                return;
            }
            var legendaryMob =
                GameObjects.JungleLegendary.FirstOrDefault(
                    l => l.Distance(Player) < E.Range && l.Health < GetEdamage(l));
            if (legendaryMob != null)
            {
                E.Cast();
                //Console.WriteLine("Damage={0}, Target health={1}", GetEdamage(legendaryMob), legendaryMob.Health);
            }
        }

        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(2000, DamageType.Physical);
            if (target == null)
            {
                return;
            }
            if (config["Combo"]["useQc"].GetValue<MenuBool>().Value && Q.IsReady() && Q.CanCast(target))
            {
                Q.CastIfHitchanceMinimum(target, HitChance.High);
            }

            if (E.IsReady())
            {
                var buff = target.GetBuff("KalistaExpungeMarker");
                var killableEnemies =
                    GameObjects.EnemyHeroes.FirstOrDefault(
                        e => e.Distance(Player) < E.Range && GetEdamage(e) > e.Health && e.IsValidTarget());
                if (config["Combo"]["useEkill"].GetValue<MenuBool>().Value &&
                    ((buff != null && E.CanCast(target) && GetEdamage(target) > target.Health) ||
                     killableEnemies != null))
                {
                    E.Cast();
                    //Console.WriteLine("Damage={0}, Target health={1}", GetEdamage(target), target.Health);
                }
                if (buff != null && config["Combo"]["useEbeforeLeave"].GetValue<MenuBool>().Value &&
                    Player.Distance(target) > E.Range - 100 &&
                    buff.Count >= config["Combo"]["usEcStack"].GetValue<MenuSlider>().Value)
                {
                    E.Cast();
                }
            }

            if (config["Combo"]["useRc"].GetValue<MenuBool>().Value && R.IsReady())
            {
                var ally =
                    GameObjects.AllyHeroes.FirstOrDefault(
                        a =>
                            a.HasBuff("kalistacoopstrikeally") &&
                            a.HealthPercent < config["Combo"]["useRhp"].GetValue<MenuSlider>().Value &&
                            CountChampsAtrange(a.Position, 700f) > 0);
                if (ally != null)
                {
                    R.Cast();
                }
            }

            if (Blitz != null && config["Combo"]["BlitzKalista"]["useRbk"].GetValue<MenuBool>().Value && R.IsReady() &&
                Blitz.HasBuff("kalistacoopstrikeally") &&
                Blitz.Distance(Player) > config["Combo"]["BlitzKalista"]["useRbkMinb"].GetValue<MenuSlider>().Value)
            {
                var blitzTarg =
                    GameObjects.EnemyHeroes.FirstOrDefault(
                        h =>
                            config["Combo"]["BlitzKalista"]["useRbke" + h.ChampionName].GetValue<MenuBool>().Value &&
                            h.HasBuff("rocketgrab2") &&
                            target.Distance(Player) >
                            config["Combo"]["BlitzKalista"]["useRbkMint"].GetValue<MenuSlider>().Value);
                if (blitzTarg != null)
                {
                    R.Cast();
                }
            }
            if (config["Misc"]["QSS"]["QSSEnabled"].GetValue<MenuBool>().Value)
            {
                Cleanse.UseCleanse(config);
            }
        }

        private static void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(2000, DamageType.Physical);
            if (Player.ManaPercent < config["Harass"]["minmanaH"].GetValue<MenuSlider>().Value || target == null)
            {
                return;
            }
            if (config["Harass"]["useQh"].GetValue<MenuBool>().Value && Q.IsReady() && Q.CanCast(target))
            {
                Q.CastIfHitchanceMinimum(target, HitChance.High);
            }

            if (config["Harass"]["useEh"].GetValue<MenuBool>().Value && E.IsReady() && E.CanCast(target))
            {
                var buff = target.GetBuff("KalistaExpungeMarker");
                if (buff != null && buff.Count >= config["Harass"]["useEhStack"].GetValue<MenuSlider>().Value)
                {
                    var minion =
                        GameObjects.EnemyMinions.FirstOrDefault(m => GetEdamage(m) > m.Health && m.IsValidTarget());
                    if (minion != null)
                    {
                        E.Cast();
                    }
                }
            }
        }

        private static void Clear()
        {
            if (Player.ManaPercent < config["Clear"]["minmanaLC"].GetValue<MenuSlider>().Value)
            {
                return;
            }
            if (config["Clear"]["useElc"].GetValue<MenuBool>().Value && E.IsReady())
            {
                var minions = GameObjects.EnemyMinions.Where(m => GetEdamage(m) > m.Health && m.IsValidTarget());
                if (minions.Any() && minions.Count() >= config["Clear"]["useElcStack"].GetValue<MenuSlider>().Value)
                {
                    var minion = GameObjects.EnemyMinions.FirstOrDefault(m => GetEdamage(m) > m.Health);
                    if (minion != null)
                    {
                        E.Cast();
                    }
                }
            }
        }

        private static void Lasthit()
        {
            if (Player.ManaPercent < config["Lasthit"]["minmanaHH"].GetValue<MenuSlider>().Value)
            {
                return;
            }
            if (config["Lasthit"]["useELH"].GetValue<MenuBool>().Value && E.IsReady())
            {
                var minions =
                    GameObjects.EnemyMinions.Where(
                        m =>
                            GetEdamage(m) > m.Health && m.IsValidTarget() &&
                            Health.GetPrediction(m, (int) (m.Distance(Player) / Player.GetProjectileSpeed())) < 0);
                if (minions.Any())
                {
                    E.Cast();
                }
            }
        }

        private static void Game_OnDraw(EventArgs args)
        {
            DrawCircle(config["Drawings"]["drawQ"].GetValue<MenuBool>().Value, Q.Range, Color.DarkCyan);
            DrawCircle(config["Drawings"]["drawE"].GetValue<MenuBool>().Value, E.Range, Color.DarkCyan);
            DrawCircle(config["Drawings"]["drawR"].GetValue<MenuBool>().Value, R.Width, Color.DarkCyan);
            HpBarDamageIndicator.Enabled = config["Drawings"]["drawcombo"].GetValue<MenuBool>().Value;
            if (Blitz == null || !Blitz.HasBuff("kalistacoopstrikeally"))
            {
                return;
            }
            DrawCircle(
                config["Drawings"]["drawRb"].GetValue<MenuBool>().Value,
                config["Combo"]["BlitzKalista"]["useRbkMinb"].GetValue<MenuSlider>().Value, Color.Yellow);
            DrawCircle(
                config["Drawings"]["drawRt"].GetValue<MenuBool>().Value,
                config["Combo"]["BlitzKalista"]["useRbkMint"].GetValue<MenuSlider>().Value, Color.Red);
        }


        public static void DrawCircle(bool enabled, float spellRange, Color color)
        {
            if (enabled)
            {
                Drawing.DrawCircle(Player.Position, spellRange, Color.FromArgb(color.R, color.G, color.B, color.A));
            }
        }

        private static void InitMenu()
        {
            Bootstrap.Init(null);
            config = new Menu("SDKalista ", "SDKalista", true);
            // Blitz+Kallista Settings
            Menu menuBk = new Menu("BlitzKalista", "Blitz+Kalista");
            menuBk.Add(new MenuBool("useRbk", "Use R", true));
            menuBk.Add(new MenuSlider("useRbkMinb", "   Blitz min range", 500, 0, 1400));
            menuBk.Add(new MenuSlider("useRbkMint", "   Target Min range", 1200, 0, 2300));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy))
            {
                menuBk.Add(new MenuBool("useRbke" + enemy.ChampionName, enemy.ChampionName, true));
            }
            // Combo Settings
            Menu menuC = new Menu("Combo", "Combo");
            menuC.Add(new MenuBool("useQc", "Use Q", true));
            menuC.Add(new MenuBool("useEkill", "Use E for kill", true));
            menuC.Add(new MenuBool("useEbeforeLeave", "Use Before leave", true));
            menuC.Add(new MenuSlider("usEcStack", "   Min stack", 4, 1, 15));
            menuC.Add(new MenuBool("useRc", "Use R", true));
            menuC.Add(new MenuSlider("useRhp", "   Under health", 15, 0, 100));
            Blitz = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(a => a.IsAlly && a.ChampionName == "Blitzcrank");
            if (Blitz != null)
            {
                menuC.Add(menuBk);
            }
            //menuC.Add(new MenuBool("useIgnite", "Use Ignite", true));
            config.Add(menuC);
            // Harass Settings
            Menu menuH = new Menu("Harass", "Harass");
            menuH.Add(new MenuBool("useQh", "Use Q", true));
            menuH.Add(new MenuBool("useEh", "Use E", true));
            menuH.Add(new MenuSlider("useEhStack", "   Min stack", 2, 1, 10));
            menuH.Add(new MenuSlider("minmanaH", "Keep X% mana", 0, 0, 100));
            config.Add(menuH);
            // LaneClear Settings
            Menu menuLC = new Menu("Clear", "Clear");
            menuLC.Add(new MenuBool("useElc", "Use E", true));
            menuLC.Add(new MenuSlider("useElcStack", "   Min minion", 2, 1, 10));
            menuLC.Add(new MenuSlider("minmanaLC", "Keep X% mana", 0, 0, 100));
            config.Add(menuLC);
            // Lasthit Settings
            Menu menuLH = new Menu("Lasthit", "Lasthit");
            menuLH.Add(new MenuBool("useELH", "Use E", true));
            menuLH.Add(new MenuSlider("minmanaLH", "Keep X% mana", 0, 0, 100));
            config.Add(menuLH);
            // Draw settings
            Menu menuD = new Menu("Drawings", "Drawings");
            menuD.Add(new MenuBool("drawQ", "Q range", false));
            menuD.Add(new MenuBool("drawE", "E range", false));
            menuD.Add(new MenuBool("drawR", "R range", false));
            menuD.Add(new MenuBool("drawRb", "R Blitz min", false));
            menuD.Add(new MenuBool("drawRt", "R target min", false));
            menuD.Add(new MenuBool("drawcombo", "Draw E damage", true));
            config.Add(menuD);

            Menu menuQ = new Menu("QSS", "QSS in Combo");
            menuQ.Add(new MenuBool("slow", "Slow", false));
            menuQ.Add(new MenuBool("blind", "Blind", false));
            menuQ.Add(new MenuBool("silence", "Silence", false));
            menuQ.Add(new MenuBool("snare", "Snare", false));
            menuQ.Add(new MenuBool("stun", "Stun", false));
            menuQ.Add(new MenuBool("charm", "Charm", true));
            menuQ.Add(new MenuBool("taunt", "Taunt", true));
            menuQ.Add(new MenuBool("fear", "Fear", true));
            menuQ.Add(new MenuBool("suppression", "Suppression", true));
            menuQ.Add(new MenuBool("polymorph", "Polymorph", true));
            menuQ.Add(new MenuBool("damager", "Vlad/Zed ult", true));
            menuQ.Add(new MenuSlider("QSSdelay", "Delay in ms", 600, 0, 1500));
            menuQ.Add(new MenuBool("QSSEnabled", "Enabled", true));


            Menu menuM = new Menu("Misc", "Misc");
            menuM.Add(new MenuSlider("DmgRed", "E damage red", 10, 0, 200));
            menuM.Add(menuQ);
            config.Add(menuM);
            config.Add(
                new MenuSeparator(
                    "SDKalista",
                    "by Soresu v" + Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(",", ".")));
            config.Attach();
        }

        private static void InitKallista()
        {
            Q = new Spell(SpellSlot.Q, 1150);
            Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 5200);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 1200);
            R.SetSkillshot(0.50f, 1500f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private static float GetQdamage(Obj_AI_Base target)
        {
            var dmg = new double[] { 10, 70, 130, 190, 250 }[Q.Level - 1] + Player.BaseAttackDamage +
                      Player.FlatPhysicalDamageMod;
            return (float) Player.CalculateDamage(target, DamageType.Physical, dmg);
        }

        private static float GetWdamage(Obj_AI_Base target)
        {
            var dmg = (new double[] { 12, 14, 16, 18, 20 }[W.Level - 1] / 100 * target.MaxHealth);
            return (float) Player.CalculateDamage(target, DamageType.Magical, dmg);
        }

        // Made somewhen with Justy
        private static float GetEdamage(Obj_AI_Base target)
        {
            var buff = target.GetBuff("KalistaExpungeMarker");
            if (buff != null)
            {
                var dmg =
                    (float)
                        ((new double[] { 20, 30, 40, 50, 60 }[E.Level - 1] +
                          0.6 * (Player.BaseAttackDamage + Player.FlatPhysicalDamageMod)) +
                         ((buff.Count - 1) *
                          (new double[] { 5, 9, 14, 20, 27 }[E.Level - 1] +
                           new double[] { .15, .18, .21, .24, .27 }[E.Level - 1] *
                           (Player.BaseAttackDamage + Player.FlatPhysicalDamageMod))));
                if (target.Name.Contains("SRU_Dragon"))
                {
                    var dsBuff = Player.GetBuff("s5test_dragonslayerbuff");
                    if (dsBuff != null)
                    {
                        dmg = dmg * (1 - 0.07f * dsBuff.Count);
                    }
                }
                return (float) Player.CalculateDamage(target, DamageType.Physical, dmg) -
                       config["Misc"]["DmgRed"].GetValue<MenuSlider>().Value;
            }
            return 0;
        }

        public static int CountChampsAtrange(Vector3 l, float p)
        {
            return ObjectManager.Get<Obj_AI_Hero>().Count(i => !i.IsDead && i.IsEnemy && i.Distance(l) < p);
        }
    }
}