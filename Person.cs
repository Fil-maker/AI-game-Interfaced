using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_game
{
    class Person
    {
        public Needs Needs = new Needs();
        public Tolerance InintTolerances;
        public Tolerance CurrentTolerances;
        public PersonType PersonsType;
        public double Happiness;
        public LivingBuilding Home;
        public double Confidence;

        public Person()
        {
            Random random = new Random();
            PersonsType = PersonType.m_PersonTypes[random.Next(PersonType.m_PersonTypes.Count)];
            InintTolerances = new Tolerance();
            CurrentTolerances = InintTolerances.Affect(PersonsType.TypeTolerance);
            Happiness = Math.Pow(5, 0.5) / Math.Pow(5, 0.5);
            Confidence = random.NextDouble();
        }

        public void ProgressStatement()
        {
            var runnigTolerances = CurrentTolerances;
            foreach (EntartainingBuliding entartaining in Home.entartainings)
                for (var i = 0; i < entartaining.Quality; i++)
                    runnigTolerances = runnigTolerances.Affect(entartaining.Buildingtolerance);
            Needs.ProgressNeeds(runnigTolerances);
            Happiness = (Needs.GetSum() - 1 + Home.Quality / 5) / Math.Pow(5, 0.5);
        }

        public void Talk(Person person)
        {
            Random rand = new Random();
            var persuadePoints = (Happiness - person.Happiness) + (Confidence - person.Confidence);
            var beingPersuadedPoints = (person.Happiness - Happiness) + (person.Confidence - Confidence);
            if (persuadePoints < beingPersuadedPoints)
            {
                if (beingPersuadedPoints < rand.NextDouble())
                    CurrentTolerances = CurrentTolerances.Affect(person.CurrentTolerances);
            }
            else if (persuadePoints > beingPersuadedPoints)
            if (persuadePoints < rand.NextDouble())
                person.CurrentTolerances = person.CurrentTolerances.Affect(CurrentTolerances);
        }
    }

    class PersonType
    {
        public static List<PersonType> m_PersonTypes = new List<PersonType> {
            new PersonType("Стандарт", new Tolerance(1.0)),
            new PersonType("Эгоист", new Tolerance(3, 2, 1, 1, 1)),
            new PersonType("Наставник", new Tolerance(1.5, 1, 3, 1, 3.5)),
            new PersonType("Душа компании", new Tolerance(1, 1, 2, 4, 3)),
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
            Random random = new Random();
            return new Tolerance(
                (Physiological + tolerance.Physiological) * (Physiological * tolerance.Physiological + random.NextDouble() / 100),
                (Security + tolerance.Security) * (Security * tolerance.Security + random.NextDouble() / 100),
                (Social + tolerance.Social) * (Social * tolerance.Social + random.NextDouble() / 100),
                (Prestigious + tolerance.Prestigious) * (Prestigious * tolerance.Prestigious + random.NextDouble() / 100),
                (Spiritual + tolerance.Spiritual) * (Spiritual * tolerance.Spiritual + random.NextDouble() / 100));
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

        public void Normalize()
        {
            double normolizedNeeds = Math.Pow(Math.Pow(Physiological, 2) + Math.Pow(Security, 2) + Math.Pow(Social, 2) + Math.Pow(Prestigious, 2) + Math.Pow(Spiritual, 2), 1.0 / 2);
            Physiological /= normolizedNeeds;
            Security /= normolizedNeeds;
            Social /= normolizedNeeds;
            Prestigious /= normolizedNeeds;
            Spiritual /= normolizedNeeds;
        }

        public double GetSum()
        {
            return Physiological + Security + Social + Prestigious + Spiritual;
        }

        public override string ToString()
        {
            return string.Format("Ph={0}, Sec={1}, Soc={2}, Pres={3}, Spir={4}",
                Math.Round(Physiological, 2), Math.Round(Security, 2), Math.Round(Social, 2),
                Math.Round(Prestigious, 2), Math.Round(Spiritual, 2));
        }
    }

}
