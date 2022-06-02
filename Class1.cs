using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_game
{

class PersonType
    {
        public static List<PersonType> m_PersonTypes = new List<PersonType> {
            new PersonType("Standart", new Tolerance(1.0)),
            new PersonType("Hungry", new Tolerance(10.0, 1.0, 1.0, 1.0, 1.0)),
            new PersonType("Monk", new Tolerance(0.5, 0.1, 0.05, 0.01, 1.0)),
            new PersonType("Demon", new Tolerance(0.0, 0.0, 0.00, 0.00, 1.0)),
        };
        public Tolerance TypeTolerance;
        public string Name;

        public PersonType(string name, Tolerance tolerance)
        {
            TypeTolerance = tolerance;
            Name = name;
        }
    }

    class Tolerance
    {
        public double Physiological;
        public double Security;
        public double Social;
        public double Prestigious;
        public double Spiritual;
        public Tolerance()
        {
            Random random = new Random();
            Physiological = random.NextDouble();
            Security = random.NextDouble();
            Social = random.NextDouble();
            Prestigious = random.NextDouble();
            Spiritual = random.NextDouble();
            Normalize();
        }

        public Tolerance(double value)
        {
            Random random = new Random();
            Physiological = value;
            Security = value;
            Social = value;
            Prestigious = value;
            Spiritual = value;
            Normalize();
        }

        public Tolerance(double physiological, double security, double social, double prestigious, double sprirtual)
        {
            Physiological = physiological;
            Security = security;
            Social = social;
            Prestigious = prestigious;
            Spiritual = sprirtual;
            Normalize();
        }

        public void Normalize()
        {
            double normolizedTolerance = Math.Pow(Math.Pow(Physiological, 2) + Math.Pow(Security, 2) +
                Math.Pow(Social, 2) + Math.Pow(Prestigious, 2) + Math.Pow(Spiritual, 2), 1.0 / 2);
            Physiological /= normolizedTolerance;
            Security /= normolizedTolerance;
            Social /= normolizedTolerance;
            Prestigious /= normolizedTolerance;
            Spiritual /= normolizedTolerance;
        }

        public Tolerance Affect(Tolerance tolerance)
        {
            return new Tolerance(Physiological * tolerance.Physiological, Security * tolerance.Security,
                Social * tolerance.Social, Prestigious * tolerance.Prestigious, Spiritual * tolerance.Spiritual);
        }

        public override string ToString()
        {
            return string.Format("Ph={0}, Sec={1}, Soc={2}, Pres={3}, Spir={4}",
                Math.Round(Physiological, 2), Math.Round(Security, 2), Math.Round(Social, 2),
                Math.Round(Prestigious, 2), Math.Round(Spiritual, 2));
        }
    }
    class Needs
    {
        public double Physiological = 0;
        public double Security = 0;
        public double Social = 0;
        public double Prestigious = 0;
        public double Spiritual = 0;

        public void ProgressNeeds(Tolerance tolerance)
        {
            Random random = new Random();
            Physiological += tolerance.Physiological * random.NextDouble();
            Security += tolerance.Security * random.NextDouble();
            Social += tolerance.Social * random.NextDouble();
            Prestigious += tolerance.Prestigious * random.NextDouble();
            Spiritual += tolerance.Spiritual * random.NextDouble();
            Normalize();
        }

        private void Normalize()
        {
            double normolizedNeeds = Math.Pow(Math.Pow(Physiological, 2) + Math.Pow(Security, 2) + Math.Pow(Social, 2) + Math.Pow(Prestigious, 2) + Math.Pow(Spiritual, 2), 1.0 / 2);
            Physiological /= normolizedNeeds;
            Security /= normolizedNeeds;
            Social /= normolizedNeeds;
            Prestigious /= normolizedNeeds;
            Spiritual /= normolizedNeeds;
        }

        public override string ToString()
        {
            return string.Format("Ph={0}, Sec={1}, Soc={2}, Pres={3}, Spir={4}",
                Math.Round(Physiological, 2), Math.Round(Security, 2), Math.Round(Social, 2),
                Math.Round(Prestigious, 2), Math.Round(Spiritual, 2));
        }
    }
    class Person
    {
        public Needs Needs;
        public Tolerance InintTolerances;
        public Tolerance CurrentTolerances;
        public PersonType PersonsType;
        public double Happiness;

        public Person()
        {
            Random random = new Random();
            PersonsType = PersonType.m_PersonTypes[random.Next(PersonType.m_PersonTypes.Count)];
            InintTolerances = new Tolerance();
            CurrentTolerances = InintTolerances.Affect(PersonsType.TypeTolerance);
            Needs = new Needs();
            Happiness = 1 - (Needs.Physiological + Needs.Security + Needs.Social + Needs.Prestigious + Needs.Spiritual) / Math.Pow(5, 1.0 / 2);
        }

        public void ProgressStatement()
        {
            Needs.ProgressNeeds(CurrentTolerances);
            Happiness = 1 - (Needs.Physiological + Needs.Security + Needs.Social + Needs.Prestigious + Needs.Spiritual) / Math.Pow(5, 1.0 / 2);
        }        
    }
}
