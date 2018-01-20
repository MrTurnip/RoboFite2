using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace RoboFite
{
    public class Robot
    {
        public enum Team
        {
            Red, Blue, None
        }
        
        public class Option
        {
            public enum Type
            {
                Ability, Weapon, Ultimate
            }
            
            public enum Targeting
            {
                Enemy, Self
            }

            protected Robot _owner;
            protected string _name, _description;
            protected char _call;
            protected Type _type;
            public Type GetType => _type;
            public Targeting GetTargeting { get; set; } = Targeting.Enemy;


            public virtual bool IsAvailable { get; } = true;

            public string Name => _name;
            public virtual string Description => _description;

            public char Call => _call;

            public string Details => ("(" + Call + ") " + Name + " - " + Description);

            public virtual void Use(Robot target)
            {
                Console.WriteLine("Using. Health: " + _owner.GetHealth.Remaining + "/" + _owner.GetHealth.Maximum);
                Console.ReadKey(true);
            }

            public Option(Robot owner)
            {
                _owner = owner;
            }
        }

        public class OptionsList : List<Option>
        {
            public TeamRoster FullRoster { get; }

            public OptionsList(TeamRoster fullRoster) : base()
            {
                FullRoster = fullRoster;
            }
        }

        public class Profile
        {
            private string _name, _description;
            private char _call;

            public string Name
            {
                get => _name;
                set => _name = value;
            }

            public string Description => _description;
            public char Call
            {
                get => _call;
                set => _call = value;
            }

            public string Details => ("(" + Call + ") " + Name + " - " + Description);
            
            public Profile(string name, string description, char call)
            {
                _name = name;
                _description = description;
                _call = call;
            }
        }

        public class Weapon : Option
        {
            public static Random randomValue = new Random();
            private int minimumDamage, maximumDamage;
            private int burst = 1;
            protected Range Reload = new Range(1, 1);
            protected Range Ammo { get; }
            protected int baseAccuracy { get; set; }
            public override string Description => (minimumDamage + "-" + maximumDamage + " (" + Ammo.GetComparison() + ")" + " " + baseAccuracy + "%");

            Attack TestAccuracy(Robot target)
            {
                int roll = 0;
                bool AccuracyBeatsTargetEvasion()
                {
                    const int rollSize = 100;
                    roll = randomValue.Next(0, rollSize);
                    roll -= _owner.baseAccuracy;
                    roll += target.baseEvasion;

                    if (roll <= baseAccuracy)
                        return true;

                    return false;
                }

                var damageRoll = randomValue.Next(minimumDamage, maximumDamage + 1);
                var hasHit = AccuracyBeatsTargetEvasion();
                return new Attack(damageRoll, hasHit);
            }

            public void DealDamage(Attack attack, Robot target)
            {
                if (!attack.IsUltimate)
                    _owner.ultimate.AddCharge(attack.Damage);

                target.GetHealth.TakeDamage(attack);
            }

            private void ReloadWeapon()
            {
                CombatTurn.WriteBanner(_owner.Name + " reloads.");

                Reload.Remaining--;
                if (Reload.Remaining == 0)
                {
                    CombatTurn.WriteBanner(_owner.Name + " has finished reloading.");
                    Reload.Remaining = Reload.Maximum;
                    Ammo.Remaining = Ammo.Maximum;
                }
            }

            public override void Use(Robot target)
            {
                if (Ammo.Remaining == 0)
                {
                    ReloadWeapon();
                    Console.ReadKey(true);
                    return;
                }

                for (int burstsRemaining = burst; burstsRemaining > 0 && Ammo.Remaining > 0; burstsRemaining--, Ammo.Remaining--)
                {
                    FireAtTarget(target);
                }

                target.GetHealth.ListHealthBlocks();

                Console.ReadKey(true);
            }

            protected void FireAtTarget(Robot target)
            {
                Attack attack = TestAccuracy(target);
                if (attack.IsHit)
                {
                    Console.WriteLine("Hit! Damage: " + attack.Damage);
                    DealDamage(attack, target);
                }
                else
                {
                    Console.WriteLine("Miss!");
                }
            }

            public Weapon(Robot owner, int minDamage, int maxDamage, int burst, int ammo, int accuracy) : base(owner)
            {
                _type = Type.Weapon;
                minimumDamage = minDamage;
                maximumDamage = maxDamage;
                this.burst = burst;
                Ammo = new Range(ammo);
                baseAccuracy = accuracy;
            }
        }

        public class Ability : Option
        {
            public Action Activate;
            public override string Description => "Restores Shell.";

            public Ability(Robot owner) : base(owner)
            {
                _type = Type.Ability;
                GetTargeting = Targeting.Self;
            }
        }

        public class Primary : Weapon
        {
            public Primary(Robot owner, int minDamage, int maxDamage, int burst, int ammo, int accuracy) : base(owner, minDamage, maxDamage, burst, ammo, accuracy)
            {
            }
        }

        public class Secondary : Weapon
        {
            public Secondary(Robot owner, int minDamage, int maxDamage, int burst, int ammo, int accuracy) : base(owner, minDamage, maxDamage, burst, ammo, accuracy)
            {
            }
        }

        public class Ultimate : Weapon
        {
            private Range charge;
            private bool ChargeComplete => charge.Remaining >= charge.Maximum;
            public override bool IsAvailable => ChargeComplete;

            public override void Use(Robot target)
            {
                EmptyCharge();
                base.Use(target);
            }

            public void AddCharge(int amount = 1)
            {
                charge.Remaining += amount;
            }

            public void EmptyCharge()
            {
                charge.Remaining = 0;
                Ammo.Remaining = Ammo.Maximum;
                Reload.Remaining = Reload.Maximum;
                
            }

            public Ultimate(Robot owner, int minDamage, int maxDamage, int burst, int ammo, int accuracy, int charge) : base(owner, minDamage, maxDamage, burst, ammo, accuracy)
            {
                _type = Type.Ultimate;
                this.charge = new Range(charge);
                //EmptyCharge();
            }
        }

        public class Range
        {
            private const int DEFAULT_TO_VALUE = -1;
            public int Remaining { get; set; }
            public int Maximum { get; set; }

            public string GetComparison()
            {
                return Remaining + "/" + Maximum;
            }

            public Range(int remaining, int maximum = DEFAULT_TO_VALUE)
            {
                Remaining = remaining;
                Maximum = maximum == DEFAULT_TO_VALUE ? remaining : maximum;
            }

            public static Range operator +(Range a, Range b)
            {
                var c = new Range(a.Remaining + b.Remaining, a.Maximum + b.Maximum);
                return c;
            }
        }

        public class Health
        {
            public Range Vital { get; }
            public Range Armor { get; }
            public Range Shield { get; }
            public Range Shell { get; }
            public Range Total => Vital + Armor + Shield + Shell;
            public int Remaining => Total.Remaining;
            public int Maximum => Total.Maximum;
            public bool IsDefeated => Remaining <= 0;
            public int TurnsWithoutDamage { get; set; }

            public void ListHealthBlocks()
            {
                void WriteToConsole(string message)
                {
                    Console.WriteLine(message);
                }

                WriteToConsole("Shell: " + Shell.GetComparison());
                WriteToConsole("Shield: " + Shield.GetComparison());
                WriteToConsole("Armor: " + Armor.GetComparison());
                WriteToConsole("Vital: " + Vital.GetComparison());
            }

            public void ReduceToInstakill()
            {
                Vital.Remaining = 1;
                Armor.Remaining = 0;
                Shield.Remaining = 0;
                Shell.Remaining = 0;

            }

            Attack PassDamageThroughShell(Attack attack)
            {
                const int NONE_REMAINING = 0;
                if (Shell.Remaining > NONE_REMAINING)
                {
                    attack.FullReduce();
                    Shell.Remaining--;
                }

                return attack;
            }

            Attack PassDamageThroughShield(Attack attack)
            {
                for (; Shield.Remaining > 0 && attack.Damage > 0; Shield.Remaining--, attack.Negate())
                {

                }

                return attack;
            }

            Attack PassDamageThroughArmor(Attack attack)
            {
                if (Armor.Remaining < 1)
                    return attack;

                attack.Damage = attack.Damage > 10 ? attack.Damage -= 5 : attack.Damage /= 5;

                if (attack.Damage < 1)
                    attack.Damage = 1;

                for (; Armor.Remaining > 0 && attack.Damage > 0; Armor.Remaining--, attack.Negate())
                {

                }

                return attack;
            }

            void DamageVitality(Attack attack)
            {
                Vital.Remaining -= attack.Damage;
                if (Vital.Remaining < 0)
                    Vital.Remaining = 0;
            }

            public void TakeDamage(Attack attack)
            {
                attack = PassDamageThroughShell(attack);
                attack = PassDamageThroughShield(attack);
                attack = PassDamageThroughArmor(attack);
                DamageVitality(attack);
            }

            public Health()
            {
                Vital = new Range(150);
                Armor = new Range(25);
                Shield = new Range(25);
                Shell = new Range(1);
            }
        }

        public class Attack
        {
            public bool IsHit { get; }
            public bool IsUltimate { get; }
            public int Damage { get; set; }
            public int TotalNegated { get; set; }
            public int TotalToVital { get; set; }

            public void Negate(int amount = 1)
            {
                Damage -= amount;
                TotalNegated += amount;
            }

            public void FullReduce()
            {
                TotalNegated += Damage;
                Damage = 0;
            }

            public Attack(int damage, bool isHit)
            {
                IsHit = isHit;
                Damage = damage;
            }
        }

        protected Profile _profile;
        public Health GetHealth { get; }
        public Profile GetProfile => _profile;
        public string Name => _profile.Name;
        protected OptionsList _options;
        public OptionsList Options => _options;
        protected Team _team;
        public Team GetTeam => _team;
        private int baseAccuracy = 10;
        private int baseEvasion = 0;
        protected Primary primary;
        protected Secondary secondary;
        protected Ability ability;
        protected Ultimate ultimate;
        public bool hasTakenTurn = false;

        public Robot(TeamRoster teamRoster, Team team)
        {
            _profile = new Profile("Robot", "A Robot", 'r');
            _options = new OptionsList(teamRoster);
            _team = team;
            GetHealth = new Health();
        }
    }
}