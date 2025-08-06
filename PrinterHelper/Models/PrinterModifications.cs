using PrinterHelper.Helpers;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace PrinterHelper.Models
{
    public class Trackable<T> : ObservableObject
    {
        private T _original;
        protected T _value;

        public T Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public Trackable(T value)
        {
            _value = value;
            _original = value;
        }

        public bool HasChanged()
        {
            if (_value is not null && !_value.Equals(_original))
            {
                return true;
            } else
            {
                return false;
            }
        }
    }

    public class PrinterModifications : ObservableObject
    {
        //private PrinterModifications original;
        public Trackable<string> Name { get; }

        public Trackable<string> HostAddress { get; }

        public Trackable<string> PortName { get; }

        public Trackable<int> Port { get; }

        public PrinterModifications(string name = "", string hostAddress = "", string portName = "", int port = 0)
        {
            Name = new(name);
            HostAddress = new(hostAddress);
            PortName = new(portName);
            Port = new(port);
        }
    }
}
