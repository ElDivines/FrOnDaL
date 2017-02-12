using System;
using SharpDX;
using EloBuddy;
using System.Linq;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;
using Color = System.Drawing.Color;

namespace FrOnDaL_Twitch
{
    internal static class Program
    {
        private static AIHeroClient Twitch => Player.Instance;
        private static readonly string[] StealMonster = { "SRU_Red", "SRU_Blue", "SRU_Baron", "SRU_RiftHerald", "SRU_Dragon_Fire", "SRU_Dragon_Water", "SRU_Dragon_Earth", "SRU_Dragon_Air", "SRU_Dragon_Elder" };
        private static Spellbook _lvl;
      //private static Item _ghostBlade;
        private static Spell.Active _q, _e/*, _r*/;
        private static Spell.Skillshot _w;
        private static float _dikey, _yatay;
        private static float genislik = 104;
        private static float yukseklik = 9.82f;
        private static Menu _main, _combo, _laneclear, _jungleclear, _drawings, _misc;
        private static void Main(){Loading.OnLoadingComplete += OnLoadingComplete;}
        private static bool Passive(this Obj_AI_Base obj) { return obj.HasBuff("TwitchDeadlyVenom"); }

        private static int StacksPassive(Obj_AI_Base obj)
        {
            var stacks = 0;
            if (obj.IsInRange(Twitch, _e.Range))
            {
                var passiveHit = ObjectManager.Get<Obj_GeneralParticleEmitter>().FirstOrDefault(e => e.Name.Contains("twitch_poison_counter"));
                if (passiveHit == null) stacks = 0;
                if (passiveHit == null) return stacks;
                if (passiveHit.Name != "twitch_poison_counter_01.troy")
                {
                    if (passiveHit.Name != "twitch_poison_counter_02.troy")
                    {
                        if (passiveHit.Name != "twitch_poison_counter_03.troy")
                        {
                            if (passiveHit.Name != "twitch_poison_counter_04.troy")
                            {
                                if (passiveHit.Name != "twitch_poison_counter_05.troy")
                                {
                                    if (passiveHit.Name == "twitch_poison_counter_06.troy")
                                    {
                                        stacks = 6;
                                    }
                                }
                                else
                                {
                                    stacks = 5;
                                }
                            }
                            else
                            {
                                stacks = 4;
                            }
                        }
                        else
                        {
                            stacks = 3;
                        }
                    }
                    else
                    {
                        stacks = 2;
                    }
                }
                else
                {
                    stacks = 1;
                }
            }
            else
            {
                stacks = 0;
            }
            return stacks;
        }
        private static float ESpellDamage(Obj_AI_Base obj)
        {
            var temel = Player.GetSpell(SpellSlot.E).Level - 1;
            float temelDamage = 0;
            if (!_e.IsReady()) return Twitch.CalculateDamageOnUnit(obj, DamageType.Physical, temelDamage);
            var temelDeger = new float[] { 20, 35, 50, 65, 80 }[temel];
            var temelHit = new float[] { 15, 20, 25, 30, 35 }[temel];
            var ekstraHasarD = 0.25f * Twitch.FlatPhysicalDamageMod;
            var ekstraHasarP = 0.20f * Twitch.FlatMagicDamageMod;
            temelDamage = temelDeger + ((temelHit + ekstraHasarD + ekstraHasarP) * StacksPassive(obj));
            return Twitch.CalculateDamageOnUnit(obj, DamageType.Physical, temelDamage);
        }
        private static void HasarGostergesi(EventArgs args)
        {
            foreach (var toHeroes in EntityManager.Heroes.Enemies.Where(x => x.VisibleOnScreen && x.Passive()))
            {
                if (toHeroes.Hero != Champion.Annie)
                {
                    if (toHeroes.Hero != Champion.Jhin)
                    {
                        if (toHeroes.Hero == Champion.Darius)
                        {
                            _dikey = 9.8f;
                            _yatay = -2;
                        }
                        else
                        {
                            _dikey = 9.8f;
                            _yatay = 2;
                        }
                    }
                    else
                    {
                        _dikey = -4.8f;
                        _yatay = -9;
                    }
                }
                else
                {
                    _dikey = -1.8f;
                    _yatay = -9;
                }
                var toHeroesD = ESpellDamage(toHeroes);

                if (toHeroesD < 1) return;

                if (!_drawings["EKillStealD"].Cast<CheckBox>().CurrentValue) continue;
                var hasarX = (toHeroes.TotalShieldHealth() - toHeroesD > 0 ? toHeroes.TotalShieldHealth() - toHeroesD : 0) / toHeroes.TotalShieldMaxHealth();
                var hasarY = toHeroes.TotalShieldHealth() / toHeroes.TotalShieldMaxHealth();
                var go = new Vector2((int)(toHeroes.HPBarPosition.X + _yatay + hasarX * genislik), (int)toHeroes.HPBarPosition.Y + _dikey);
                var finish = new Vector2((int)(toHeroes.HPBarPosition.X + _yatay + hasarY * genislik) + 1, (int)toHeroes.HPBarPosition.Y + _dikey);
                Drawing.DrawLine(go, finish, yukseklik, Color.FromArgb(180, Color.Green));
            }
        }

        private static void OnLoadingComplete(EventArgs args) {
            if (Player.Instance.Hero != Champion.Twitch) return;
            Drawing.OnEndScene += HasarGostergesi;
            _lvl = Twitch.Spellbook;
          //_ghostBlade = new Item(ItemId.Youmuus_Ghostblade);          
            Obj_AI_Base.OnLevelUp += OnLevelUpR;       
            Game.OnTick += TwitchActive;
            Drawing.OnDraw += SpellDraw;
            Chat.Print("<font color='#00FFCC'><b>[FrOnDaL]</b></font> Twitch Successfully loaded.");            
            Spellbook.OnCastSpell += auto_ghostBlade_baseQ;
            _q = new Spell.Active(SpellSlot.Q);
            _w = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 250, 1400, 280) { AllowedCollisionCount = -1, MinimumHitChance = HitChance.Medium };
            _e = new Spell.Active(SpellSlot.E, 1200);
          //_r = new Spell.Active(SpellSlot.R);
            _main = MainMenu.AddMenu("FrOnDaL Twitch", "index");
            _main.AddGroupLabel("Welcome FrOnDaL Twitch");
            _main.AddSeparator(5);
            _main.AddLabel("For faults please visit the 'elobuddy' forum and let me know.");
            _main.AddSeparator(10);
            _main.AddLabel("My good deeds -> FrOnDaL");
            _combo = _main.AddSubMenu("Combo");
            _combo.AddGroupLabel("Combo mode settings for Twitch");
            _combo.AddLabel("Use Combo W (On/Off)" + "                                 " + "Auto E Kill Steal");
            _combo.Add("w", new CheckBox("Use W"));
            _combo.Add("autoE", new CheckBox("Auto E"));
            _laneclear = _main.AddSubMenu("Laneclear");
            _laneclear.AddGroupLabel("LaneClear mode settings for Twitch");
            _laneclear.Add("LmanaP", new Slider("LaneClear Mana Control Min mana percentage ({0}%) to use W and E", 50, 1));
            _laneclear.AddSeparator(14);
            _laneclear.Add("w", new CheckBox("Venom Cask (W) settings", false));
            _laneclear.Add("UnitsWhit", new Slider("Hit {0} Units Enemy and Minions", 4, 1, 8));
            _laneclear.AddSeparator(14);
            _laneclear.Add("e", new CheckBox("Use E", false));
            _laneclear.Add("MinionsEhit", new Slider("Min minions hit {0} to use E", 3, 1, 8));
            _laneclear.Add("MinionsEstacks", new Slider("{0} stacks aa", 3, 1, 4));
            _jungleclear = _main.AddSubMenu("Jungleclear");
            _jungleclear.AddGroupLabel("JungleClear mode settings for Twitch");         
            _jungleclear.Add("JmanaP", new Slider("Jungle Clear Mana Control Min mana percentage ({0}%) to use W and E", 30, 1));
            _jungleclear.AddLabel("Venom Cask (W) settings :"+"                               "+ "Contaminate (E) settings :");
            _jungleclear.Add("w", new CheckBox("Use W"));
            _jungleclear.Add("e", new CheckBox("Use E"));
            _jungleclear.AddSeparator(8);
            _jungleclear.AddLabel("Uses E only on big monsters and buffs");
            _jungleclear.Add("StealRB", new CheckBox("Steal Red & Blue"));
            _jungleclear.Add("StealBD", new CheckBox("Steal Baron & Dragons"));
            _drawings = _main.AddSubMenu("Drawings");
            _drawings.AddLabel("Use Drawings W-E-R (On/Off)");
            _drawings.Add("w", new CheckBox("Draw W and R", false));
            _drawings.Add("e", new CheckBox("Draw E", false));
            _drawings.AddSeparator(8);
            _drawings.AddLabel("Use Draw E Damage (On/Off)");
            _drawings.Add("EKillStealD", new CheckBox("Draw Damage Indicator [E Damage]"));
            _misc = _main.AddSubMenu("Misc");
            _misc.AddLabel("Auto base use Q (On/Off)");        
            _misc.Add("autob", new CheckBox("Auto Base Q (On/Off)"));
           /* _misc.AddSeparator(8);
            _misc.AddLabel("Auto Youmuu Ghost Blade and Blade of the Ruined King");
            _misc.Add("auto.ghostBlade.botrk", new CheckBox("Use Youmuu and Botrk (On/Off)"));*/
        }
        public static void OnLevelUpR(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {if (Twitch.Level > 4){_lvl.LevelSpell(SpellSlot.R);}}
        private static void auto_ghostBlade_baseQ(Spellbook sender, SpellbookCastSpellEventArgs eventArgs)
        {
            if (eventArgs.Slot != SpellSlot.Recall || !_q.IsReady() || !_misc["autob"].Cast<CheckBox>().CurrentValue)
                return;
            _q.Cast();
            Core.DelayAction(() => ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall), _q.CastDelay + 300);
            eventArgs.Process = false;
            /*if (eventArgs.Slot != SpellSlot.R || _misc["auto.ghostBlade.botrk"].Cast<CheckBox>().CurrentValue != true)
                return;
            if (_r.IsReady() && _ghostBlade.IsOwned() && _ghostBlade.IsReady())
            {
                _ghostBlade.Cast();
            }*/
        }
        private static void TwitchActive(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                if (_combo["w"].Cast<CheckBox>().CurrentValue)
                {
                    var hedef = TargetSelector.GetTarget(_w.Range, DamageType.True);
                    if (hedef != null && !hedef.IsInvulnerable)
                    {
                        var tahmin = _w.GetPrediction(hedef);
                        if (tahmin != null && tahmin.HitChance > HitChance.Medium)
                        {
                            if (_w.IsReady())
                            {
                                _w.Cast(hedef);
                            }
                        }
                    }
                }
             }          
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                if (Twitch.ManaPercent >= _laneclear["LmanaP"].Cast<Slider>().CurrentValue)
                    {LanClear();}               
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {                
                if (Twitch.ManaPercent >= _jungleclear["JmanaP"].Cast<Slider>().CurrentValue)
                    {JunClear(); }                                
            }
            if (!_combo["autoE"].Cast<CheckBox>().CurrentValue) return;
            var rival = EntityManager.Heroes.Enemies.Where(x => x.IsValid && !x.IsDead && !x.IsInvulnerable && x.Passive() && Prediction.Health.GetPrediction(x, 275) < ESpellDamage(x)).FirstOrDefault(d => d.IsInRange(Twitch, _e.Range));
            if (rival != null && _e.IsReady())
            { _e.Cast();}
        }
        private static void SpellDraw(EventArgs args)
        {
            if (_drawings["w"].Cast<CheckBox>().CurrentValue){ _w.DrawRange(Color.FromArgb(130, Color.Green));}
            if (_drawings["e"].Cast<CheckBox>().CurrentValue){ _e.DrawRange(Color.FromArgb(130, Color.Green));}
        }
        private static void LanClear()
        {
            if (_laneclear["w"].Cast<CheckBox>().CurrentValue)
            {
                var farm = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Twitch.ServerPosition).Where(x => x.IsInRange(Twitch, _w.Range));
                if (true)
                {
                    var keyhit = _w.GetBestCircularCastPosition(farm);
                    if (keyhit.HitNumber >= _laneclear["UnitsWhit"].Cast<Slider>().CurrentValue && _e.IsReady())
                    { _w.Cast(keyhit.CastPosition);}
                }
            }

            if (!_laneclear["e"].Cast<CheckBox>().CurrentValue) return;
            {
                var farm = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Twitch.ServerPosition).Where(x => x.IsInRange(Twitch, _e.Range) && x.Passive());
                if (true)
                {
                    var objAiMinions = farm as Obj_AI_Minion[] ?? farm.ToArray();
                    if (objAiMinions.Count(c => Prediction.Health.GetPrediction(c, 275) < ESpellDamage(c) && StacksPassive(c) >= _laneclear["MinionsEstacks"].Cast<Slider>().CurrentValue) >= _laneclear["MinionsEhit"].Cast<Slider>().CurrentValue)
                    {if (_e.IsReady()) _e.Cast();}
                    if (objAiMinions.Count(c => StacksPassive(c) >= _laneclear["MinionsEstacks"].Cast<Slider>().CurrentValue) < _laneclear["MinionsEhit"].Cast<Slider>().CurrentValue) return;
                    if (_e.IsReady()) _e.Cast();
                }
            }
        }
        private static void JunClear()
        {
            if (_jungleclear["w"].Cast<CheckBox>().CurrentValue)
            {
                var farmjung = EntityManager.MinionsAndMonsters.GetJungleMonsters(Twitch.ServerPosition).FirstOrDefault(x => x.IsInRange(Twitch, 550));
                if (farmjung != null && _w.IsReady())
                { _w.Cast(farmjung.ServerPosition); }
            }

            if (!_jungleclear["e"].Cast<CheckBox>().CurrentValue) return;
            {
                var farmjung = EntityManager.MinionsAndMonsters.GetJungleMonsters(Twitch.ServerPosition).FirstOrDefault(x => x.IsInRange(Twitch, _e.Range) && x.Passive());
                if (farmjung == null || !_e.IsReady()) return;
                if (_jungleclear["StealRB"].Cast<CheckBox>().CurrentValue)
                {
                    if (StealMonster.Contains(farmjung.BaseSkinName))
                    {
                        var healtPred = Prediction.Health.GetPrediction(farmjung, 275);
                        if (ESpellDamage(farmjung) >= healtPred)
                        { _e.Cast(); }
                    }
                }
                if (!_jungleclear["StealBD"].Cast<CheckBox>().CurrentValue) return;
                {
                    if (!StealMonster.Contains(farmjung.BaseSkinName)) return;
                    var healtPred = Prediction.Health.GetPrediction(farmjung, 275);
                    if (ESpellDamage(farmjung) >= healtPred)
                    { _e.Cast(); }
                }
            }
        }
    }
}