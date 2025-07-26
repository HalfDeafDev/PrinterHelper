using PrinterHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrinterHelper.Models
{
    public class PrinterModifications : ObservableObject
    {
        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        
        private string _hostAddress = "";
        public string HostAddress { get => _hostAddress; set => SetProperty(ref _hostAddress, value); }

        private string _portName = "";
        public string PortName { get => _portName; set => SetProperty(ref _portName, value); }
        
        private int _port = 0;
        public int Port { get => _port; set => SetProperty(ref _port, value); }
    }
}
