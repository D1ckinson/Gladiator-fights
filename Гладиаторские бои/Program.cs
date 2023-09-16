using System;
using System.Collections.Generic;

namespace Гладиаторские_бои
{
    internal class Program
    {
        static void Main()
        {
            const char ChoeseWarriors = '1';//
            const char ExitCommand = '2';//

            string playerInput;
            char playerChar;
            bool isRunning = true;

            Dictionary<char, string> commandsDescriptions = new Dictionary<char, string>
            {
                { ChoeseWarriors, "Выбрать бойцов" },
                { ExitCommand, "Выход" }
            };

            Arena arena = new Arena();

            Warrior[] warriors = { arena.CreateKnight(), arena.CreateMage(), arena.CreateArcher(), arena.CreatePaladin(), arena.CreateRogue() };

            while (isRunning)
            {
                Console.Clear();

                foreach (var item in commandsDescriptions)
                    Console.WriteLine($"{item.Key} - {item.Value}");

                Console.Write("Введите команду:");

                //playerInput = Console.ReadLine();

                // playerChar = playerInput == "" ? ' ' : playerInput[0];

                Warrior warrior;

                playerChar = '1';//

                switch (playerChar)
                {
                    case ChoeseWarriors:
                        warrior = ChoiceWarrior(warriors, "asd");
                        break;

                    case ExitCommand:
                        isRunning = false;
                        break;
                }
            }
        }

        static Warrior ChoiceWarrior(Warrior[] warriors, string info)
        {
            int warriorIndex;

            do
            {
                Console.Clear();
                Console.WriteLine(info);

                for (int i = 0; i < warriors.Length; i++)
                    Console.WriteLine($"{i} - {warriors[i].Name}");

                int.TryParse(Console.ReadLine(), out warriorIndex);
            }
            while (warriorIndex < 0 || warriorIndex > warriors.Length);

            Warrior warrior = warriors[warriorIndex];

            warriors[warriorIndex] = null;

            return warriors[warriorIndex];
        }
    }

    class Arena
    {
        private readonly Random _random = new Random();

        private readonly int _minHealth = 80;
        private readonly int _minArmor = 10;
        private readonly int _minDamage = 20;

        private readonly int _maxHealth = 100;
        private readonly int _maxArmor = 20;
        private readonly int _maxDamage = 30;

        public Knight CreateKnight() => (Knight)CreateWarrior(CreateKnight);

        public Mage CreateMage() => (Mage)CreateWarrior(CreateMage);

        public Archer CreateArcher() => (Archer)CreateWarrior(CreateArcher);

        public Paladin CreatePaladin() => (Paladin)CreateWarrior(CreatePaladin);

        public Rogue CreateRogue() => (Rogue)CreateWarrior(CreateRogue);

        private Warrior CreateWarrior(Func<int, int, int, Warrior> createFunc)
        {
            int health = _random.Next(_minHealth, _maxHealth);
            int armor = _random.Next(_minArmor, _maxArmor);
            int damage = _random.Next(_minDamage, _maxDamage);

            return createFunc(health, armor, damage);
        }

        private Knight CreateKnight(int health, int armor, int damage) => new Knight(health, armor, damage);

        private Mage CreateMage(int health, int armor, int damage) => new Mage(health, armor, damage);

        private Archer CreateArcher(int health, int armor, int damage) => new Archer(health, armor, damage);

        private Paladin CreatePaladin(int health, int armor, int damage) => new Paladin(health, armor, damage);

        private Rogue CreateRogue(int health, int armor, int damage) => new Rogue(health, armor, damage);
    }

    interface ITakeDebuff
    {
        void TakeDebuff(Action debuff, int debuffDuration);

        void UseDebuff();
    }

    abstract class Warrior
    {
        public readonly string Name;

        protected int _health;
        protected int _armor;
        protected int _damage;
        protected int _baseDamage;

        protected int _cooldown;
        protected int _counter = 0;

        protected Warrior(string name, int health, int armor, int damage, int cooldown)
        {
            Name = name;
            _health = health;
            _armor = armor;
            _damage = damage;
            _baseDamage = damage;
            _cooldown = cooldown;
            _counter = cooldown;
        }

        public void TakeDamage(int damage) => _health -= damage - (_armor / 2);

        public abstract void Attack(Warrior warrior);

        public bool IsWarriorAlive() => _health > 0;
    }

    abstract class DebuffWarrior : Warrior, ITakeDebuff
    {
        protected int _debuffDuration;
        protected bool _isIUnderDebuff = false;
        protected Action _debuff = null;

        public DebuffWarrior(string name, int health, int armor, int damage, int cooldown) : base(name, health, armor, damage, cooldown) { }

        public override void Attack(Warrior warrior)
        {
            if (_isIUnderDebuff)
                UseDebuff();
        }

        public void TakeDebuff(Action debuff, int debuffDuration)
        {
            _debuff = debuff;
            _debuffDuration = debuffDuration;
            _isIUnderDebuff = true;
        }

        public void UseDebuff()
        {
            _debuff.Invoke();

            _debuff = null;

            if (_debuffDuration > 0)
                _debuffDuration--;

            if (_debuffDuration == 0 && _damage < _baseDamage)
                _damage = _baseDamage;

            if (_debuffDuration == 0)
                _isIUnderDebuff = false;
        }
    }

    class Knight : DebuffWarrior
    {
        private int _damageBonus;

        public Knight(int health, int armor, int damage) : base("Рыцарь", health + 10, armor + 5, damage, 3)
        {
            _damageBonus = _baseDamage / 2;
        }

        public override void Attack(Warrior warrior)
        {
            base.Attack(warrior);

            CastDamageBuff();

            warrior.TakeDamage(_damage);
        }

        public void CastDamageBuff()
        {
            if (_counter == _cooldown)
            {
                _damage += _damageBonus;
                _counter = 0;
            }
            else
            {
                _counter++;
            }

            if (_counter == 1 && _damage > _baseDamage)
                _damage = _baseDamage;
        }
    }

    class Mage : DebuffWarrior
    {
        private Random _random = new Random();

        private List<Action<Warrior>> _spells = new List<Action<Warrior>>();
        private int _spellDebuffDuration = 3;
        private int _fireballDamage = 10;
        private int _fireballDamageBonus = 4;
        private int _armorBonus = 3;

        public Mage(int health, int armor, int damage) : base("Маг", health, armor - 3, 0, 0)
        {
            _spells.Add(CastDebuffOnEnemy);
            _spells.Add(FireballAttack);
            _spells.Add(UpArmor);
        }

        public override void Attack(Warrior warrior)
        {
            Action<Warrior> spell = _spells[_random.Next(_spells.Count)];

            spell = CastDebuffOnEnemy;

            spell.Invoke(warrior);
        }

        private void CastDebuffOnEnemy(Warrior warrior)
        {
            if (warrior is ITakeDebuff)
            {
                ITakeDebuff debuffWarrior = warrior as ITakeDebuff;

                debuffWarrior.TakeDebuff(ApplyDebuff, _spellDebuffDuration);
            }
            else
            {
                _spells.Remove(CastDebuffOnEnemy);
            }
        }

        private void ApplyDebuff()
        {
            double damage = _damage;
            damage *= 0.8;
            _damage = (int)damage;
        }

        private void FireballAttack(Warrior warrior)
        {
            warrior.TakeDamage(_fireballDamage);

            _fireballDamage += _fireballDamageBonus;
        }

        private void UpArmor(Warrior warrior) => _armor += _armorBonus;
    }

    class Archer : DebuffWarrior
    {
        private int _powerShotDamage = 10;

        public Archer(int health, int armor, int damage) : base("Лучник", health, armor, damage + 5, 2) { }

        public override void Attack(Warrior warrior)
        {
            base.Attack(warrior);

            CastPowerShot(warrior);

            warrior.TakeDamage(_damage);
        }

        private void CastPowerShot(Warrior warrior)
        {
            if (_counter == _cooldown)
            {
                warrior.TakeDamage(_powerShotDamage);
                _counter = 0;
            }
            else
            {
                _counter++;
            }
        }
    }

    class Paladin : Warrior
    {
        private int _maxHealth;
        private int recoveryHealthQuantity = 8;

        public Paladin(int health, int armor, int damage) : base("Паладин", health + 15, armor + 10, damage - 10, 5)
        {
            _maxHealth = health;
        }

        public override void Attack(Warrior warrior) => warrior.TakeDamage(_damage);

        public new void TakeDamage(int damage)
        {
            if (_counter == _cooldown)
            {
                RecoverHealth();

                _counter = 0;
            }
            else
            {
                _health -= damage - (_armor / 2);
                _counter++;
            }
        }

        private void RecoverHealth()
        {
            if (_health < _maxHealth)
                _health += recoveryHealthQuantity;
        }
    }

    class Rogue : DebuffWarrior
    {
        private Random _random = new Random();
        private int _maxPercent = 100;
        private int _dodgePercent = 30;

        public Rogue(int health, int armor, int damage) : base("Разбойник", health - 10, armor - 5, damage + 15, 0) { }

        public override void Attack(Warrior warrior) => warrior.TakeDamage(_damage);

        public new void TakeDamage(int damage)
        {
            int Percent = _random.Next(0, _maxPercent + 1);

            if (Percent <= _dodgePercent)
                _damage++;
            else
                _health -= damage - (_armor / 2);
        }
    }
}
