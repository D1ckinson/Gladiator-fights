﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Гладиаторские_бои
{
    internal class Program
    {
        static void Main()
        {
            Console.CursorVisible = false;

            Arena arena = new Arena();
            Menu menu = new Menu(arena.StartFightAction);
        }
    }

    class Menu
    {
        const ConsoleKey DownArrow = ConsoleKey.DownArrow;
        const ConsoleKey UpArrow = ConsoleKey.UpArrow;
        const ConsoleKey Enter = ConsoleKey.Enter;

        private int _menuIndex = 0;
        private bool _isRunning;

        private Dictionary<string, Action> _mainActions = new Dictionary<string, Action>();

        private Dictionary<string, Func<Warrior>> _subActions = new Dictionary<string, Func<Warrior>>();

        public Menu(Dictionary<string, Action> Actions)
        {
            _mainActions = Actions;
            _mainActions.Add("Выход", Exit);

            MenuWork(_mainActions.Keys.ToArray());
        }

        public Menu(Dictionary<string, Func<Warrior>> warriorsClasses, Warrior[] opponents)
        {
            _subActions = warriorsClasses;

            SubMenuWork(_subActions.Keys.ToArray(), opponents);
        }

        private void MenuWork(string[] menuItems)
        {
            _isRunning = true;

            while (_isRunning)
            {
                DrawMenuItems(menuItems);

                if (IsEnterPress(menuItems.Length - 1))
                    _mainActions[menuItems[_menuIndex]].Invoke();
            }
        }

        private void SubMenuWork(string[] menuItems, Warrior[] opponents)
        {
            int counter = 0;

            while (counter < opponents.Length)
            {
                DrawMenuItems(menuItems);

                if (IsEnterPress(menuItems.Length - 1))
                {
                    opponents[counter] = _subActions[menuItems[_menuIndex]].Invoke();
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
        private Random _random = new Random();
        private Warrior[] _opponents = new Warrior[2];
        private Menu _menu;

        private int _minHealth = 80;
        private int _minArmor = 10;
        private int _minDamage = 20;

        private int _maxHealth = 100;
        private int _maxArmor = 20;
        private int _maxDamage = 30;

        public readonly Dictionary<string, Action> StartFightAction = new Dictionary<string, Action>();
        public readonly Dictionary<string, Func<Warrior>> WarriorsClasses = new Dictionary<string, Func<Warrior>>();

        public Arena()
        {
            StartFightAction.Add("Выбрать бойцов", StartFight);
            WarriorsClasses.Add("Выбрать Рыцаря", CreateKnight);
            WarriorsClasses.Add("Выбрать Мага", CreateMage);
            WarriorsClasses.Add("Выбрать Лучника", CreateArcher);
            WarriorsClasses.Add("Выбрать Паладина", CreatePaladin);
            WarriorsClasses.Add("Выбрать Разбойника", CreateRogue);
        }

        public void StartFight()
        {
            _menu = new Menu(WarriorsClasses, _opponents);

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

        public Warrior CreateKnight() => CreateWarrior(CreateKnight);

        public Warrior CreateMage() => CreateWarrior(CreateMage);

        public Warrior CreateArcher() => CreateWarrior(CreateArcher);

        public Warrior CreatePaladin() => CreateWarrior(CreatePaladin);

        public Warrior CreateRogue() => CreateWarrior(CreateRogue);

        private Knight CreateKnight(int health, int armor, int damage) => new Knight(health, armor, damage);

        private Mage CreateMage(int health, int armor, int damage) => new Mage(health, armor, damage);

        private Archer CreateArcher(int health, int armor, int damage) => new Archer(health, armor, damage);

        private Paladin CreatePaladin(int health, int armor, int damage) => new Paladin(health, armor, damage);

        private Rogue CreateRogue(int health, int armor, int damage) => new Rogue(health, armor, damage);

        private Warrior CreateWarrior(Func<int, int, int, Warrior> createFunc)
        {
            int health = _random.Next(_minHealth, _maxHealth);
            int armor = _random.Next(_minArmor, _maxArmor);
            int damage = _random.Next(_minDamage, _maxDamage);

            return createFunc(health, armor, damage);
        }

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

        public virtual void TakeDamage(int damage) => _health -= damage - (_armor / 2);

        public abstract void Attack(Warrior warrior);

        public bool IsWarriorAlive() => _health > 0;
    }

    abstract class DebuffWarrior : Warrior, ITakeDebuff
    {
        protected int _debuffDuration;
        protected Action _debuff = null;

        public DebuffWarrior(string name, int health, int armor, int damage, int cooldown) : base(name, health, armor, damage, cooldown) { }

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

        public Knight(int health, int armor, int damage) : base("Рыцарь", health + 10, armor + 5, damage, 3) =>
            _damageBonus = _baseDamage / 2;

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

        public Mage(int health, int armor, int damage) : base("Маг", health, armor - 3, 0, 0)
        {
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

        public Archer(int health, int armor, int damage) : base("Лучник", health, armor, damage + 5, 2) { }

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

        public Paladin(int health, int armor, int damage) : base("Паладин", health + 15, armor + 10, damage - 10, 5) =>
            _maxHealth = health;

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

        public Rogue(int health, int armor, int damage) : base("Разбойник", health - 10, armor - 5, damage + 15, 0) { }

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
                Console.WriteLine($"{Name} уворачивается от атаки и увеличивает свой урон");
            }
            else
            {
                _health -= damage - (_armor / 2);
            }
        }
    }
}
