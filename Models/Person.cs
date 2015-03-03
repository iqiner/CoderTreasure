using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models.Enums;
using System.ComponentModel;

namespace Models
{
    public static class PersonRepository
    {
        private static List<Person> Persons = new List<Person>
        {
            new Person{ID = 1,Name = "Sure",Age=28, Sex = HumanSex.Male,Salary = 1000m},
            new Person{ID = 2,Name = "Angel",Age=25,Sex = HumanSex.Female,Salary = 2000m},
            new Person{ID = 3,Name = "Rollin",Age=47,Sex = HumanSex.Male,Salary = 3000m}
        };

        public static void Add(Person person)
        {
            int newID = Persons.Select(p => p.ID).Max() + 1;
            
            Persons.Add(new Person
            {
                ID = newID,
                Name = person.Name,
                Age = person.Age,
                Sex = person.Sex,
                Salary = person.Salary
            });
        }

        public static void Delete(Person person)
        {
            Persons.RemoveAll(p => p.ID == person.ID);
        }

        public static void Update(Person person)
        {
            var _person = Load(person.ID);
            _person.ID = person.ID;
            _person.Name = person.Name;
            _person.Age = person.Age;
            _person.Sex = person.Sex;
            _person.Salary = person.Salary;
        }

        public static Person Load(int id)
        {
            return Persons.FirstOrDefault(person => person.ID == id);
        }
    }


    public class Person : INotifyPropertyChanged
    {
        private int m_ID;
        private string m_Name;
        private int? m_Age;
        private HumanSex m_Sex;
        private decimal? m_Salary;

        public int ID
        {
            get
            {
                return this.m_ID;
            }
            set
            {
                this.m_ID = value;
                this.OnPropertyChanged("ID");
            }
        }

        public string Name
        {
            get
            {
                return this.m_Name;
            }
            set
            {
                this.m_Name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public int? Age
        {
            get
            {
                return this.m_Age;
            }
            set
            {
                this.m_Age = value;
                this.OnPropertyChanged("Age");
            }
        }

        public HumanSex Sex
        {
            get
            {
                return this.m_Sex;
            }
            set
            {
                this.m_Sex = value;
                this.OnPropertyChanged("Sex");
            }
        }

        public decimal? Salary
        {
            get
            {
                return this.m_Salary;
            }
            set
            {
                this.m_Salary = value;
                this.OnPropertyChanged("Salary");
            }
        }

        public void Delete()
        {
            PersonRepository.Delete(this);
        }

        public void Save()
        {
            PersonRepository.Add(this);
        }

        public void Update()
        {
            PersonRepository.Update(this);
        }

        public void Load(int id)
        {
            //return PersonRepository.Load(id);
            Person person = PersonRepository.Load(id);
            if (person == null)
            {
                throw new ApplicationException("Can not find person with ID : " + id);
            }
            this.ID = person.ID;
            this.Name = person.Name;
            this.Age = person.Age;
            this.Sex = person.Sex;
            this.Salary = person.Salary;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
        {
            return String.Format("ID:{0};Name:{1};Sex:{2};Salary:{3}", this.ID, this.Name, this.Sex.ToString(), this.Salary.ToString());
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null)
        //    {
        //        return false;
        //    }

        //    Person target = obj as Person;
        //    if (target == null)
        //    {
        //        return false;
        //    }

        //    return this.ID == target.ID;
        //}

        ///// <summary>
        ///// Read all property and do bitwise xor(^) operation for them
        ///// </summary>
        //public override int GetHashCode()
        //{
        //    int hashCode = this.ID.GetHashCode()
        //                ^ this.Name.GetHashCode()
        //                ^ this.Sex.GetHashCode()
        //                ^ this.Salary.GetHashCode();
        //    return hashCode;
        //}
    }
}
