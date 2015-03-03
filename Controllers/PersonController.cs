using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models;
using Models.Interfaces;


namespace Controllers
{
    public class PersonController : IController
    {
        private IView m_View;

        public PersonController(IView view)
        {
            this.m_View = view;

            this.m_View.SetController(this);
        }

        public IView View
        {
            get
            {
                return this.m_View;
            }
        }

        public Person Model{get; private set;}

        public void InitModel()
        {
            this.Model = new Person();
        }

        public void LoadPerson(int id)
        {
            this.Model.Load(id);
        }

        public void SavePerson()
        {
            this.Model.Save();
        }

        public void DeletePerson()
        {
            this.Model.Delete();
        }

        public void UpdatePerson()
        {
            this.Model.Update();
        }
    }
}
