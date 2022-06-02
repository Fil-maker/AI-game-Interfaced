using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_game
{
    public enum TypesOfBuilding
    {
        Living,
        Entertaining,
        Working
    }

    abstract class Building
    {
        public List<Person> people = new List<Person>();
        public int Capacity;
        public int Quality;
        public TypesOfBuilding BuildingType;
        public double Happiness;
        public Needs needs = new Needs();
        public List<Building> entartainings = new List<Building>();

        abstract public void ProgressStatement();

        abstract public int GetMoney();

        abstract public override string ToString();

        abstract public string GetDescription();

        abstract public Building GetClone();

    }

    class LivingBuilding : Building { 

        public List<Building> neighbors = new List<Building>();

        public LivingBuilding(int capacity, int quality)
        {
            this.Quality = quality;
            this.Capacity = capacity;
            BuildingType = TypesOfBuilding.Living;
            for (int i = 0; i < capacity; i++)
                AddPerson();
        }

        public LivingBuilding(int capacity, int quality, Person[] residents)
        {
            this.Quality = quality;
            this.Capacity = capacity;
            BuildingType = TypesOfBuilding.Living;
            foreach (Person person in residents)
                AddPerson(person);
        }

        public void AddPerson(Person person)
        {
            if (people.Count < Capacity)
            {
                person.Home = this;
                people.Add(person);
            }
        }

        public void AddPerson()
        {
            if (people.Count < Capacity)
            {
                var citizen = new Person
                {
                    Home = this
                };
                people.Add(citizen);
            }
        }

        override public void ProgressStatement()
        {
            Happiness = 0;

            for (int i = 0; i < people.Count; i++)
                if (new Random().NextDouble() < 0.7)
                {
                    var num = i;
                    while (num == i)
                    {
                        i = new Random().Next(people.Count);
                    }
                    people[i].Talk(people[num]);
                }
                else if (neighbors.Count() != 0) {
                    Random random = new Random();
                    var meet = random.Next(neighbors.Count());
                    people[i].Talk(neighbors[meet].people[random.Next(neighbors[meet].people.Count())]);
                }
            foreach (Person person in people)
            {
                person.ProgressStatement();
                Happiness += person.Happiness / people.Count();

                needs.Physiological += person.Needs.Physiological / people.Count();
                needs.Security += person.Needs.Security / people.Count();
                needs.Social += person.Needs.Social / people.Count();
                needs.Prestigious += person.Needs.Prestigious / people.Count();
                needs.Spiritual += person.Needs.Spiritual / people.Count();
                needs.Normalize();
            }
        }

        public override string ToString()
        {
            if (Happiness != 0)
            return $"Счастье = {Math.Round(Happiness * 100)}% Кол-во людей = {Capacity}\nСредние потребности:\n" +
                $"Физ={Math.Round(needs.Physiological,2)}, Без={Math.Round(needs.Security, 2)},\n" +
                $"Соц={Math.Round(needs.Social, 2)}, Прес={Math.Round(needs.Prestigious, 2)}, Дух={Math.Round(needs.Spiritual, 2)}\n" +
                $"Качество = {Quality}";
            return "Здание только что поставлено";
        }

        override public string GetDescription()
        {
            return $"Вместительность = {Capacity}; Качество = {Quality}";
        }

        override public Building GetClone()
        {
            return new LivingBuilding(Capacity, Quality);
        }

        public override int GetMoney()
        {
            return (int)(people.Count() * 0.8);
        }
    }

    class EntartainingBuliding: Building
    {
        public Tolerance Buildingtolerance;

        public EntartainingBuliding(int quality, Tolerance tolerance)
        {
            BuildingType = TypesOfBuilding.Entertaining;
            Quality = quality;
            Buildingtolerance = tolerance;
        }

        public EntartainingBuliding(int quality)
        {
            Quality = quality;
            Buildingtolerance = new Tolerance();
        }

        override public string GetDescription()
        {
            return $"Изменение потребностей:\n" +
                $"Физ={Math.Round(Buildingtolerance.Physiological, 2)}, Без={Math.Round(Buildingtolerance.Security, 2)}, " +
                $"Соц={Math.Round(Buildingtolerance.Social, 2)},\n" +
                $"Прес={Math.Round(Buildingtolerance.Prestigious, 2)}, Дух={Math.Round(Buildingtolerance.Spiritual, 2)}\n" +
                $"Качество={Quality}";
        }

        override public string ToString()
        {
            return $"Изменение потребностей:\n" +
                $"Физ={Math.Round(Buildingtolerance.Physiological, 2)}, Без={Math.Round(Buildingtolerance.Security, 2)}," +
                $" Соц={Math.Round(Buildingtolerance.Social, 2)}," + $" Прес={Math.Round(Buildingtolerance.Prestigious, 2)}," +
                $" Дух={Math.Round(Buildingtolerance.Spiritual, 2)}\n" +
                $" Качество={Quality}";
        }

        public override Building GetClone()
        {
            return new EntartainingBuliding(Quality, Buildingtolerance);
        }

        public override void ProgressStatement()
        {
        }

        public override int GetMoney()
        {
            throw new NotImplementedException();
        }
    }
    
    class WorkingBuilding: Building {
        private readonly int Income;
        public List<Person> People = new List<Person>();

        public WorkingBuilding(int income, params Person[] workers)
        {
            Income = income;
            People = workers.ToList();
            BuildingType = TypesOfBuilding.Working;
        }

        override public int GetMoney()
        {
            var ret = 0;
            foreach (Person person in People)
                ret += (int)(person.Happiness * Income);
            return ret;
        }

        override public string GetDescription() {
            return $"Денег за каждого 100%\nсчастливого рабочего в округе = {Income}";
        }

        override public string ToString()
        {
            return $"Денег за каждого 100%\nсчастливого рабочего в округе = {Income}";
        }

        public override Building GetClone()
        {
            return new WorkingBuilding(Income);
        }

        public override void ProgressStatement() { }
    }
}
