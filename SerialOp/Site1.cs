using System;
using System.ComponentModel;

namespace SerialOp
{
    /// <summary>
    /// This class is defined just for the Name field.
    /// The aim is to know which timer (name) elapsed
    /// </summary>
    class Site1 : ISite
    {
        public Site1(string n)
        {
            this.Name = n;
        }
        public IComponent Component => throw new NotImplementedException();

        public IContainer Container => throw new NotImplementedException();

        public bool DesignMode { get; set; }

        public string Name { get; set; }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
