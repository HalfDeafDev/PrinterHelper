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
        public TrackableProperty<string> Name { get; } = new("");
        public TrackableProperty<string> PortName { get; } = new("");
        public TrackableProperty<string> HostAddress { get; } = new("");
        public TrackableProperty<int> Port { get; } = new(0);
        public TrackableProperty<string> Server { get; } = new("");
        public TrackableProperty<string> PrinterRelPath { get; } = new("");
        public TrackableProperty<string> TCPPrinterRelPath { get; } = new("");

        public TCPPrinter() { }

        public TCPPrinter(
            string name, string portName, string hostAddress, int port,
            string server, string printerRelPath, string tcpPrinterRelPath
        )
        {
            Name.Solidify(name);
            PortName.Solidify(portName);
            HostAddress.Solidify(hostAddress);
            Port.Solidify(port);
            Server.Solidify(server);
            PrinterRelPath.Solidify(printerRelPath);
            TCPPrinterRelPath.Solidify(tcpPrinterRelPath);

        }
    }
}
