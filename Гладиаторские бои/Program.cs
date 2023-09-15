using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace Гладиаторские_бои
{
    internal class Program
    {
        static void Main()
        {
            const char Command1 = '1';//
            const char Command2 = '2';//

            string playerInput;
            char playerChar = ' ';
            bool isRunning = true;

            Arena arena = new Arena();

            while (isRunning)
            {
                playerInput = Console.ReadLine();

                playerChar = playerInput == "" ? ' ' : playerInput[0];

                switch (playerChar)
                {
                    case Command1:
                        //
                        break;

                    case Command2:
                        //
                        break;
                }
            }
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

    interface ICastDamageBuff
    {
        void CastDamageBuff();
    }

    interface ITakeDebuff
    {
        void TakeDebuff();

        void ApplyDebuff();
    }

    abstract class Warrior
    {
        public readonly string Name;

        protected int _health;
        protected int _armor;
        protected int _damage;
        protected int _baseDamage;

        protected int _cooldown;
        protected int _counter;
        protected int _debuffDuration;
        protected bool _isIUnderDebuff = false;
        protected Action _debuff = null;

        protected Warrior(string name, int health, int armor, int damage, int cooldown)
        {
            Name = name;
            _health = health;
            _armor = armor;
            _damage = damage;
            _baseDamage = damage;
            _cooldown = cooldown;
        }

        public void TakeDamage(int damage) => _health -= damage - (_armor / 2);

        public abstract int Attack();

        public bool IsWarriorAlive() => _health > 0;

        public void GetDebuff(Action debuff, int debuffDuration)
        {
            _debuff = debuff;
            _debuffDuration = debuffDuration;
            _isIUnderDebuff = true;
        }
    }

    class Knight : Warrior, ITakeDebuff
    {
        private int _damageBonus;

        public Knight(int health, int armor, int damage) : base("Рыцарь", health + 10, armor + 5, damage, 3)
        {
            _damageBonus = _baseDamage / 2;
        }

        public override int Attack()
        {
            if (_isIUnderDebuff)
                ApplyDebuff();

            CastDamageBuff();

            return _damage;
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

            if (_counter == 0 && _damage > _baseDamage)
                _damage = _baseDamage;
        }

        public void TakeDebuff()
        {
            throw new NotImplementedException();
        }

        public void ApplyDebuff()
        {
            _debuff.Invoke();

            if (_debuffDuration > 0)
                _debuffDuration--;

            if (_debuffDuration == 0 && _damage < _baseDamage)
                _damage = _baseDamage;

            if (_debuffDuration == 0)
                _isIUnderDebuff = false;
        }
    }

    class Mage : Warrior
    {
        private Random random = new Random();

        private int spellsQuantity = 3;

        public Mage(int health, int armor, int damage) : base("Маг", health, armor - 3, damage + 7, 0) { }

        public override int Attack()
        {
            throw new NotImplementedException();
        }

        public void CastSpellOnEnemy(Warrior warrior)
        {
            bool i = warrior is ITakeDebuff;
        }


    }

    class Archer : Warrior
    {
        public Archer(int health, int armor, int damage) : base("Лучник", health, armor, damage + 5, 2) { }

        public override int Attack()
        {
            throw new NotImplementedException();
        }
    }

    class Paladin : Warrior
    {
        public Paladin(int health, int armor, int damage) : base("Паладин", health + 15, armor + 10, damage - 10, 5) { }

        public override int Attack()
        {
            throw new NotImplementedException();
        }
    }

    class Rogue : Warrior
    {
        public Rogue(int health, int armor, int damage) : base("Разбойник", health - 10, armor - 5, damage + 15, 3) { }

        public override int Attack()
        {
            throw new NotImplementedException();
        }
    }
}
