using System;
using System.Collections.Generic;
using System.Linq;

namespace Гладиаторские_бои
{
    internal class Program
    {
        static void Main()
        {
            Console.CursorVisible = false;

            Arena arena = new Arena();
            Menu menu = new Menu(arena.StartFightAction);

            menu.MainMenuWork();
        }
    }

    class Menu
    {
        const ConsoleKey DownArrow = ConsoleKey.DownArrow;
        const ConsoleKey UpArrow = ConsoleKey.UpArrow;
        const ConsoleKey Enter = ConsoleKey.Enter;

        private int _menuIndex = 0;
        private bool _isRunning;
        private string[] _menuItems;

        private Dictionary<string, Action> _mainActions = new Dictionary<string, Action>();
        private Dictionary<string, Func<Warrior>> _subActions = new Dictionary<string, Func<Warrior>>();

        public Menu(Dictionary<string, Action> Actions)
        {
            _mainActions = Actions;
            _mainActions.Add("Выход", Exit);
            _menuItems = _mainActions.Keys.ToArray();
        }

        public Menu(Dictionary<string, Func<Warrior>> warriorsClasses)
        {
            _subActions = warriorsClasses;
            _menuItems = _subActions.Keys.ToArray();
        }

        public void MainMenuWork()
        {
            _isRunning = true;

            while (_isRunning)
            {
                DrawMenuItems(_menuItems);

                if (IsEnterPress(_menuItems.Length - 1))
                    _mainActions[_menuItems[_menuIndex]].Invoke();
            }
        }

        public void ArenaMenuWork(Warrior[] opponents)
        {
            int counter = 0;

            while (counter < opponents.Length)
            {
                DrawMenuItems(_menuItems);

                if (IsEnterPress(_menuItems.Length - 1))
                {
                    opponents[counter] = _subActions[_menuItems[_menuIndex]].Invoke();
                    Console.WriteLine($"{counter + 1} противник - {opponents[counter].Name}          ");
                    counter++;
                }
            }
        }

        private bool IsEnterPress(int menuLength)
        {
            switch (Console.ReadKey().Key)
            {
                case DownArrow:
                    _menuIndex++;
                    break;

                case UpArrow:
                    _menuIndex--;
                    break;

                case Enter:
                    return true;
            }

            _menuIndex = _menuIndex > menuLength ? _menuIndex = menuLength : _menuIndex < 0 ? _menuIndex = 0 : _menuIndex;

            return false;
        }

        private void DrawMenuItems(string[] menuItems)
        {
            Console.SetCursorPosition(0, 0);

            for (int i = 0; i < menuItems.Length; i++)
            {
                if (i == _menuIndex)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine(menuItems[i]);
                Console.ResetColor();
            }
        }

        private void Exit() => _isRunning = false;
    }

    class Arena
    {
        public readonly Dictionary<string, Action> StartFightAction = new Dictionary<string, Action>();
        public readonly Dictionary<string, Func<Warrior>> WarriorsClasses = new Dictionary<string, Func<Warrior>>();

        private Warrior[] _opponents = new Warrior[2];
        private Menu _menu;

        public Arena()
        {
            StartFightAction.Add("Выбрать бойцов", StartFight);
            WarriorsClasses.Add("Выбрать Рыцаря", CreateKnight);
            WarriorsClasses.Add("Выбрать Мага", CreateMage);
            WarriorsClasses.Add("Выбрать Лучника", CreateArcher);
            WarriorsClasses.Add("Выбрать Паладина", CreatePaladin);
            WarriorsClasses.Add("Выбрать Разбойника", CreateRogue);
            _menu = new Menu(WarriorsClasses);
        }

        public void StartFight()
        {
            _menu.ArenaMenuWork(_opponents);

            while (_opponents[0].IsWarriorAlive() && _opponents[1].IsWarriorAlive())
            {
                AttackOpponent(_opponents[0], _opponents[1]);

                AttackOpponent(_opponents[1], _opponents[0]);
            }

            if (_opponents[0].IsWarriorAlive() == false && _opponents[1].IsWarriorAlive() == false)
                Console.WriteLine("Войны не смогли решить кто сильнее... Оба пали в бою друг с другом.");
            else if (_opponents[0].IsWarriorAlive())
                Console.WriteLine($"Победил {_opponents[0].Name}!");
            else
                Console.WriteLine($"Победил {_opponents[1].Name}!");

            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
            Console.Clear();
        }

        private Knight CreateKnight() => new Knight();

        private Mage CreateMage() => new Mage();

        private Archer CreateArcher() => new Archer();

        private Paladin CreatePaladin() => new Paladin();

        private Rogue CreateRogue() => new Rogue();

        private void AttackOpponent(Warrior warrior1, Warrior warrior2)
        {
            warrior1.Attack(warrior2);
            System.Threading.Thread.Sleep(1000);
        }
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
        protected int _cooldown;
        protected int _counter = 0;

        private Random _random = new Random();
        private int[] _healthStats = { 80, 100 };
        private int[] _armorStats = { 10, 20 };
        private int[] _damageStats = { 20, 30 };

        protected Warrior(string name, int cooldown)
        {
            Name = name;
            _health = _random.Next(_healthStats[0], _healthStats[1]);
            _armor = _random.Next(_armorStats[0], _armorStats[1]);
            _damage = _random.Next(_damageStats[0], _damageStats[1]);
            _cooldown = cooldown;
            _counter = cooldown;
        }

        public virtual void TakeDamage(int damage) => _health -= damage - (_armor / 2);

        public abstract void Attack(Warrior warrior);

        public bool IsWarriorAlive() => _health > 0;
    }

    abstract class DebuffWarrior : Warrior, ITakeDebuff
    {
        protected int _baseDamage;
        protected int _debuffDuration;
        protected Action _debuff = null;

        public DebuffWarrior(string name, int cooldown) : base(name, cooldown) =>
            _baseDamage = _damage;

        public bool IsIUnderDebuff { get; private set; }

        public override void Attack(Warrior warrior)
        {
            if (IsIUnderDebuff)
                UseDebuff();
        }

        public void TakeDebuff(Action debuff, int debuffDuration)
        {
            _debuff = debuff;
            _debuffDuration = debuffDuration;
            IsIUnderDebuff = true;
        }

        public void UseDebuff()
        {
            _debuff?.Invoke();

            _debuff = null;

            if (_debuffDuration > 0)
                _debuffDuration--;

            if (_debuffDuration == 0 && _damage < _baseDamage)
                _damage = _baseDamage;

            if (_debuffDuration == 0)
                IsIUnderDebuff = false;
        }
    }

    class Knight : DebuffWarrior
    {
        private int _damageBonus;

        public Knight() : base("Рыцарь", 3)
        {
            _health += 10;
            _armor += 5;
            _damageBonus = _baseDamage / 2;
        }

        public override void Attack(Warrior warrior)
        {
            base.Attack(warrior);

            CastDamageBuff();

            warrior.TakeDamage(_damage);

            Console.WriteLine($"{Name} наносит {_damage} урона");
        }

        public void CastDamageBuff()
        {
            if (_counter == _cooldown)
            {
                _damage += _damageBonus;
                _counter = 0;

                Console.WriteLine("Рыцарь использует усиление урона.");
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

        public Mage() : base("Маг", 0)
        {
            _armor -= 3;
            _spells.Add(CastDebuffOnEnemy);
            _spells.Add(FireballAttack);
            _spells.Add(UpArmor);
        }

        public override void Attack(Warrior warrior)
        {
            Action<Warrior> spell = _spells[_random.Next(_spells.Count)];

            spell.Invoke(warrior);
        }

        private void CastDebuffOnEnemy(Warrior warrior)
        {
            if (warrior is DebuffWarrior debuffWarrior)
            {
                if (debuffWarrior.IsIUnderDebuff == false)
                {
                    debuffWarrior.TakeDebuff(ApplyDebuff, _spellDebuffDuration);
                    Console.WriteLine($"{Name} накладывает проклятье на противника.");
                }
                else
                    FireballAttack(warrior);
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
            Console.WriteLine($"{Name} кидает огненный шар.");
        }

        private void UpArmor(Warrior warrior)
        {
            _armor += _armorBonus;
            Console.WriteLine($"{Name} увеличивает свою защиту");
        }
    }

    class Archer : DebuffWarrior
    {
        private int _powerShotDamage = 10;

        public Archer() : base("Лучник", 2) =>
            _damage += 5;

        public override void Attack(Warrior warrior)
        {
            base.Attack(warrior);

            CastPowerShot(warrior);

            warrior.TakeDamage(_damage);
            Console.WriteLine($"{Name} наносит {_damage} урона");
        }

        private void CastPowerShot(Warrior warrior)
        {
            if (_counter == _cooldown)
            {
                warrior.TakeDamage(_powerShotDamage);
                _counter = 0;
                Console.WriteLine($"{Name} использует усиленный выстрел.");
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

        public Paladin() : base("Паладин", 5)
        {
            _health += 15;
            _armor += 10;
            _damage -= 10;
            _maxHealth = _health;
        }

        public override void Attack(Warrior warrior)
        {
            warrior.TakeDamage(_damage);
            Console.WriteLine($"{Name} наносит {_damage} урона");
        }

        public override void TakeDamage(int damage)
        {
            if (_counter == _cooldown)
            {
                RecoverHealth();

                _counter = 0;
                Console.WriteLine($"{Name} использует святой щит и восстанавливает здоровье.");
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

        public Rogue() : base("Разбойник", 0)
        {
            _health -= 10;
            _armor -= 5;
            _damage += 15;
        }

        public override void Attack(Warrior warrior)
        {
            warrior.TakeDamage(_damage);
            Console.WriteLine($"{Name} наносит {_damage} урона");
        }

        public override void TakeDamage(int damage)
        {
            int Percent = _random.Next(0, _maxPercent + 1);

            if (Percent <= _dodgePercent)
            {
                _damage++;
                Console.WriteLine($"{Name} уклоняется от атаки и увеличивает свой урон");
            }
            else
            {
                _health -= damage - (_armor / 2);
            }
        }
    }
}
