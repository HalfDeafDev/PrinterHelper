using PrinterHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PrinterHelper.Models
{
    public class TCPPrinter : ObservableObject
    {
        private string _name = "";
        private string _hostAddress = "";
        private int _port = 0;
        private string _server = "";
        private string _portName = "";
        private string _printerRelPath = "";
        private string _tcpPrinterRelPath = "";

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string HostAddress { get => _hostAddress; set => SetProperty(ref _hostAddress, value); }
        public int Port { get => _port; set => SetProperty(ref _port, value); }
        public string Server { get => _server; set => SetProperty(ref _server, value); }
        public string PortName { get => _portName; set => SetProperty(ref _portName, value); }
        public string PrinterRelPath { get => _printerRelPath; set => SetProperty(ref _printerRelPath, value); }
        public string TcpPrinterRelPath { get => _tcpPrinterRelPath; set => SetProperty(ref _tcpPrinterRelPath, value); }
    }
}
