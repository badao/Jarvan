using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Jarvan
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static Menu Menu;

        private static int Rs , forceQ;

        private static float Rcount, Qcount;

        private static Vector3 QE , Rpos;
     
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "JarvanIV")
                return;

            Q = new Spell(SpellSlot.Q, 870);
            W = new Spell(SpellSlot.W,300);
            E = new Spell(SpellSlot.E,830);
            R = new Spell(SpellSlot.R,650);
            Q.SetSkillshot(250, 70, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(150, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(250, 70, 1450, false, SkillshotType.SkillshotLine);


            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu); 
            Menu.AddSubMenu(orbwalkerMenu);
            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            Menu spellMenu = Menu.AddSubMenu(new Menu("Spells", "Spells"));
            spellMenu.AddItem(new MenuItem("Use Q Harass", "Use Q Harass").SetValue(true));
            //spellMenu.AddItem(new MenuItem("Use W Harass", "Use W Harass").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use W Combo", "Use W Combo").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use R Combo", "Use R Combo").SetValue(true));
            spellMenu.AddItem(new MenuItem("force focus selected", "force focus selected").SetValue(false));
            spellMenu.AddItem(new MenuItem("if selected in :", "if selected in :").SetValue(new Slider(1000, 1000, 1500)));
            spellMenu.AddItem(new MenuItem("Cage Selected Hero", "Cage Selected Hero").SetValue(new KeyBind(71,KeyBindType.Press)));
            spellMenu.AddItem(new MenuItem("Escape", "Escape").SetValue(new KeyBind(90, KeyBindType.Press)));
            //spellMenu.AddItem(new MenuItem("Use E", "Use E")).SetValue(false);
            //foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            //{
            //    spellMenu.AddItem(new MenuItem("use R" + hero.SkinName, "use R" + hero.SkinName)).SetValue(true);
            //}

            //spellMenu.AddItem(new MenuItem("useR", "Use R to Farm").SetValue(true));
            //spellMenu.AddItem(new MenuItem("LaughButton", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            //spellMenu.AddItem(new MenuItem("ConsumeHealth", "Consume below HP").SetValue(new Slider(40, 1, 100)));

            Menu.AddToMainMenu();

            //Drawing.OnDraw += Drawing_OnDraw;

            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnCast;

            Game.PrintChat("Welcome to JarvanIV World");
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            castR2();
            Rstate();
            castFQ();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Menu.Item("Use W Combo").GetValue<bool>())
                {
                    useW();
                }
                if (Menu.Item("Use R Combo").GetValue<bool>())
                {
                    useR();
                }
                useE();
                useQ();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Menu.Item("Use Q Harass").GetValue<bool>())
                {
                    useQH();
                }
            }
            if (Menu.Item("Cage Selected Hero").GetValue<KeyBind>().Active)
            {
                Cage();
            }
            if (Menu.Item("Escape").GetValue<KeyBind>().Active)
            {
                Esc();
            }


        }


        public static bool Selected()
        {
            if (!Menu.Item("force focus selected").GetValue<bool>())
            {
                return false;
            }
            else
            {
                var target = TargetSelector.GetSelectedTarget();
                float a = Menu.Item("if selected in :").GetValue<Slider>().Value;
                if (target == null || target.IsDead || target.IsZombie)
                {
                    return false;
                }
                else
                {
                    if (Player.Distance(target.Position) > a)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        public static void useQH()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget())
                {
                    castQH(target);
                }
            }
            else
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (target != null && target.IsValidTarget())
                {
                    castQH(target);
                }
            }
        }
        public static void useQ ()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget())
                {
                    castQ(target);
                }
            }
            else
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (target != null && target.IsValidTarget())
                {
                    castQ(target);
                }
            }
        }
        public static void useE ()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget())
                {
                    castE(target);
                }
            }
            else
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (target != null && target.IsValidTarget())
                {
                    castE(target);
                }
            }
        }
        public static void useW()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget(W.Range))
                {
                    castW(target);
                }
            }
            else
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (target != null && target.IsValidTarget(W.Range))
                {
                    castW(target);
                }
            }
        }
        public static void useR ()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget())
                {
                    castR(target);
                }
            }
            else
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (target != null && target.IsValidTarget())
                {
                    castR(target);
                }
            }
        }

        public static void castQH(Obj_AI_Base target)
        {
            var t = Prediction.GetPrediction(target, 625).CastPosition;
            float x = target.MoveSpeed;
            float y = x * 250 / 1000;
            var pos = target.Position;
            if (target.Distance(t) <= y)
            {
                pos = t;
            }
            if (target.Distance(t) > y)
            {
                pos = target.Position.Extend(t, y);
            }
            float z = Player.Distance(pos);
            var i = Player.Position.Extend(pos, z);
            if ( Player.Distance(i) <= Q.Range)
            {
                Q.Cast(i);
            }
        }
        public static void castW(Obj_AI_Base target)
        {
            if (W.IsReady() && target.IsValidTarget(W.Range) && Player.Mana >= (Q.Instance.ManaCost + E.Instance.ManaCost + W.Instance.ManaCost ))
            {
                W.Cast(target);
            }
        }
        public static double GetRdmg(Obj_AI_Base target)
        {
            if (R.Level >= 1)
            {
                var dmg = new double[] { 200, 325, 450 }[R.Level -1] + 1.5 * Player.FlatPhysicalDamageMod;
                return Damage.CalcDamage(Player,target, Damage.DamageType.Physical, dmg);
            }
            else
            {
                return 0;
            }
        }
        public static void castR( Obj_AI_Base target)
        {
            if (R.IsReady() && target.IsValidTarget(R.Range) && Rs == 1)
            {
                if( GetRdmg(target) > target.Health)
                {
                    R.Cast(target);
                }
            }
        }
        public static void castR2 ()
        {
            if (Rs != 2)
                return;
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Obj_AI_Base target;
                if (Selected())
                {
                    target = TargetSelector.GetSelectedTarget();     
                }
                else
                {
                    target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                }
                if( Player.Distance(target.Position) > 350)
                {
                    R.Cast();
                }
            }
        }
        public static void castQ (Obj_AI_Base target)
        {
            if ((forceQ == 0 && !E.IsReady()) || Player.Mana < (Q.Instance.ManaCost + E.Instance.ManaCost))
            {
                var t = Prediction.GetPrediction(target, 625).CastPosition;
                float x = target.MoveSpeed;
                float y = x * 250 / 1000;
                var pos = target.Position;
                if (target.Distance(t) <= y)
                {
                    pos = t;
                }
                if (target.Distance(t) > y)
                {
                    pos = target.Position.Extend(t, y);
                }
                float z = Player.Distance(pos);
                var i = Player.Position.Extend(pos, z);
                if (Player.Mana >= (Q.Instance.ManaCost + E.Instance.ManaCost) && Player.Distance(i) <= Q.Range)
                {
                    Q.Cast(i);
                }
            }
        }

        public static void castFQ()
        {
            if (!Q.IsReady())
            {
                forceQ = 0;
            }
            if (forceQ == 1)
            {
                if (Environment.TickCount - Qcount >= 500)
                {
                    forceQ = 0;
                }
                else
                {
                    var x = Player.Position.Extend(QE, Q.Range);
                    Q.Cast(x);
                }
            }
        }

        public static void castE (Obj_AI_Base target)
        {

                var t = Prediction.GetPrediction(target, 625).CastPosition;
                float x = target.MoveSpeed;
                float y = x * 250 / 1000;
                var pos = target.Position;
                if (target.Distance(t) <= 100)
                {
                    pos = t;
                }
            if (target.Distance(t) > 100)
            {
                float m = 250;
                var n = target.Position.Extend(t, y);
                float l = Player.Distance(n) * 1000 / 1450;
                float k = x *500 / 1000;
                pos = target.Position.Extend(t, k);
            }
                float z = Player.Distance(pos);
                QE = Player.Position.Extend(pos, z + 75);
                if(Player.Mana >= (Q.Instance.ManaCost + E.Instance.ManaCost) && Player.Distance(QE) <= E.Range && Q.IsReady() && !QE.IsWall())
                {
                    E.Cast(QE);
                }
                else if (Player.Mana >= (Q.Instance.ManaCost + E.Instance.ManaCost) && Q.IsReady() && !QE.IsWall())
                {
                    if (Player.Distance(QE) > E.Range)
                    {
                        int j = 74;

                        for (int o = 1; o < j; o++)
                        {
                            var Ec = Player.Position.Extend(pos, z + o);
                            if (Player.Distance(Ec) <= E.Range && E.IsReady())
                            {
                                E.Cast(Ec);
                            }
                        }
                    }
                }
            
        }

        public static void Cage()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var target = TargetSelector.GetSelectedTarget();
            if (target == null)
                return;
            if (target.IsZombie)
                return;
            if (target.IsDead)
                return;
            if (!R.IsReady())
                return;
            if (Rs != 1)
                return;
            if (Player.Distance(target.Position) > (Q.Range + R.Range))
                return;

            if (Player.Distance(target.Position) <= R.Range)
            {
                R.Cast(target);
            }
            else
            {
                if (Player.Mana >= (Q.Instance.ManaCost + E.Instance.ManaCost + R.Instance.ManaCost))
                {
                    var x = Player.Position.Extend(target.Position, E.Range);
                    if (!x.IsWall())
                    {
                        E.Cast(x);
                    }
                }
            }
        }

        public static void Esc()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (Player.Mana >= (Q.Instance.ManaCost + E.Instance.ManaCost))
            {
                var x = Player.Position.Extend(Game.CursorPos, E.Range);
                if (!x.IsWall() && Q.IsReady())
                {
                    E.Cast(x);
                }
            }
        }

        public static void Rstate ()
        {
            if (Environment.TickCount - Rcount <= 3500)
            {
                Rs = 2;
            }
            else
                Rs = 1;
        }

        public static void OnCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
            {
                return;
            }
            if (spell.Name == "JarvanIVCataclysm")
            {
                Rcount = Environment.TickCount;
                if (Rs == 2)
                {
                    Rpos = args.End;
                }
            }
            if (spell.Name == "JarvanIVDemacianStandard" && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var pos = args.End;
                QE = pos;
                forceQ = 1;
                Qcount = Environment.TickCount;
            }
            if (spell.Name == "JarvanIVDemacianStandard" && Menu.Item("Cage Selected Hero").GetValue<KeyBind>().Active)
            {
                var pos = args.End;
                QE = pos;
                forceQ = 1;
                Qcount = Environment.TickCount;
            }
            if (spell.Name == "JarvanIVDemacianStandard" && Menu.Item("Escape").GetValue<KeyBind>().Active)
            {
                var pos = args.End;
                QE = pos;
                forceQ = 1;
                Qcount = Environment.TickCount;
            }
        }
    }
}
