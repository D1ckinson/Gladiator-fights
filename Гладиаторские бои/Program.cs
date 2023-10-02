using NPOI.Util;
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
            Dictionary<string, Action> actions = new Dictionary<string, Action> { { "Начать бой", arena.Fight } };
            Menu menu = new Menu(actions);

            menu.Work();
        }
    }

    class Menu
    {
        const ConsoleKey MoveSelectionUp = ConsoleKey.UpArrow;
        const ConsoleKey MoveSelectionDown = ConsoleKey.DownArrow;
        const ConsoleKey ConfirmSelection = ConsoleKey.Enter;

        private int _index = 0;
        private bool _isRunning;
        private string[] _items;

        private Dictionary<string, Action> _actions = new Dictionary<string, Action>();
        private IReadOnlyDictionary<string, Func<Warrior>> _arenaActions = new Dictionary<string, Func<Warrior>>();

        public Menu(Dictionary<string, Action> actions)
        {
            _actions = actions;
            _actions.Add("Выход", Exit);
            _items = _actions.Keys.ToArray();
        }

        public Menu(IReadOnlyDictionary<string, Func<Warrior>> warriorsClasses)
        {
            _arenaActions = warriorsClasses;
            _items = _arenaActions.Keys.ToArray();
        }

        public void Work()
        {
            _isRunning = true;

            while (_isRunning)
            {
                DrawItems();

                if (IsConfirmButtonPress())
                    _actions[_items[_index]].Invoke();
            }
        }

        public void WorkArena(Warrior[] opponents)
        {
            int counter = 0;

            while (counter < opponents.Length)
            {
                DrawItems();

                if (IsConfirmButtonPress())
                {
                    opponents[counter] = _arenaActions[_items[_index]].Invoke();
                    Console.WriteLine($"{counter + 1} противник - {opponents[counter].Name}          ");
                    counter++;
                }
            }
        }

        private bool IsConfirmButtonPress()
        {
            int lastIndex = _items.Length - 1;

            switch (Console.ReadKey().Key)
            {
                case MoveSelectionDown:
                    _index++;
                    break;

                case MoveSelectionUp:
                    _index--;
                    break;

                case ConfirmSelection:
                    return true;
            }

            _index = _index > lastIndex ? _index = lastIndex : _index < 0 ? _index = 0 : _index;

            return false;
        }

        private void DrawItems()
        {
            Console.SetCursorPosition(0, 0);

            for (int i = 0; i < _items.Length; i++)
            {
                if (i == _index)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine(_items[i]);
                Console.ResetColor();
            }
        }

        private void Exit() => _isRunning = false;
    }

    class Arena
    {
        private Warrior[] _opponents = new Warrior[2];
        private Menu _menu;
        private IReadOnlyDictionary<string, Func<Warrior>> _warriorsClasses;

        public Arena()
        {
            _warriorsClasses = new Dictionary<string, Func<Warrior>>
            {
                { "Выбрать Рыцаря", CreateKnight },
                { "Выбрать Мага", CreateMage },
                { "Выбрать Лучника", CreateArcher },
                { "Выбрать Паладина", CreatePaladin },
                { "Выбрать Разбойника", CreateRogue }
            };

            _menu = new Menu(_warriorsClasses);
        }

        public void Fight()
        {
            _menu.WorkArena(_opponents);

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
        private int _armorDivider = 2;

        protected Warrior(string name, int cooldown)
        {
            Name = name;
            _health = _random.Next(_healthStats[0], _healthStats[1]);
            _armor = _random.Next(_armorStats[0], _armorStats[1]);
            _damage = _random.Next(_damageStats[0], _damageStats[1]);
            _cooldown = cooldown;
            _counter = cooldown;
        }

        public virtual void TakeDamage(int damage) => _health -= damage - (_armor / _armorDivider);

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
        private int _damageAbilityBonus;
        private int _healthBonus = 10;
        private int _armorBonus = 5;
        private int _divider = 2;

        public Knight() : base("Рыцарь", 3)
        {
            _health += _healthBonus;
            _armor += _armorBonus;
            _damageAbilityBonus = _baseDamage / _divider;
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
                _damage += _damageAbilityBonus;
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
        private int _armorAbilityBonus = 3;
        private int _armorBonus = -3;
        private float _debuffDamageMultiplier = 0.7f;

        public Mage() : base("Маг", 0)
        {
            _armor += _armorBonus;
            _spells.Add(CastDebuffOnEnemy);
            _spells.Add(CastFireball);
            _spells.Add(IncreaseArmor);
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
                {
                    CastFireball(warrior);
                }
            }
            else
            {
                _spells.Remove(CastDebuffOnEnemy);
            }
        }

        private void ApplyDebuff() => _damage = (int)(_damage * _debuffDamageMultiplier);

        private void CastFireball(Warrior warrior)
        {
            warrior.TakeDamage(_fireballDamage);

            _fireballDamage += _fireballDamageBonus;
            Console.WriteLine($"{Name} кидает огненный шар.");
        }

        private void IncreaseArmor(Warrior warrior)
        {
            _armor += _armorAbilityBonus;
            Console.WriteLine($"{Name} увеличивает свою защиту");
        }
    }

    class Archer : DebuffWarrior
    {
        private int _powerShotDamage = 10;
        private int _damageBonus = 5;

        public Archer() : base("Лучник", 2) =>
            _damage += _damageBonus;

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
        private int _healthBonus = 15;
        private int _armorBonus = 10;
        private int _damageBonus = 10;

        public Paladin() : base("Паладин", 5)
        {
            _health += _healthBonus;
            _armor += _armorBonus;
            _damage -= _damageBonus;
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
                base.TakeDamage(damage);
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
        private int _healthBonus = -10;
        private int _armorBonus = -5;
        private int _damageBonus = 15;

        public Rogue() : base("Разбойник", 0)
        {
            _health += _healthBonus;
            _armor += _armorBonus;
            _damage += _damageBonus;
        }

        public override void Attack(Warrior warrior)
        {
            warrior.TakeDamage(_damage);
            Console.WriteLine($"{Name} наносит {_damage} урона");
        }

        public override void TakeDamage(int damage)
        {
            int percent = _random.Next(_maxPercent + 1);

            if (percent <= _dodgePercent)
            {
                _damage++;
                Console.WriteLine($"{Name} уклоняется от атаки и увеличивает свой урон");
            }
            else
            {
                base.TakeDamage(damage);
            }
        }
    }
}
