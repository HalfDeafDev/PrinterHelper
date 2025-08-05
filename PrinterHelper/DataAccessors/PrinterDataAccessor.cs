using PrinterHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PrinterHelper.DataAccessors
{
    public static class WmiQueryHelper
    {
        public static void Execute(string query, Action<ManagementObjectCollection> action)
        {
            using (ManagementObjectSearcher searcher = new(query))
            using (ManagementObjectCollection results = searcher.Get())
            {
                action(results);
            }
        }
    }
    
    public static class PrinterDataAccessor
    {
        private static string AsString(PropertyData property, string alt="")
        {
            return property.Value?.ToString() ?? alt;
        }

        /// <summary>
        /// Populates an ObservableCollection with Printer Names and Port Names
        /// </summary>
        /// <param name="printers">An empty ObservableCollection ready to be filled.</param>
        /// <returns></returns>
        public static async Task InitializePrinterInformationAsync(ObservableCollection<TCPPrinter> printers)
        {
            if (printers.Count > 0)
            {
                printers.Clear();
            }

            await Task.Run(() =>
            {
                Dictionary<string, TCPPrinter> _printers = new();

                WmiQueryHelper.Execute($"select Name, HostAddress, PortNumber, __SERVER from Win32_TCPIPPrinterPort", (ManagementObjectCollection collection) =>
                {
                    foreach (ManagementObject tcpPrinter in collection)
                    {
                        TCPPrinter printer = new()
                        {
                            PortName = AsString(tcpPrinter.Properties["Name"]),
                            TcpPrinterRelPath = AsString(tcpPrinter.Properties["__PATH"]),
                            HostAddress = AsString(tcpPrinter.Properties["HostAddress"]),
                            Port = Int32.Parse(AsString(tcpPrinter.Properties["PortNumber"])),
                            Server = AsString(tcpPrinter.Properties["__SERVER"])
                        };

                        _printers.Add(printer.PortName, printer);
                    }
                });

                WmiQueryHelper.Execute($"select Name, PortName, __PATH from Win32_Printer", (ManagementObjectCollection collection) =>
                {
                    foreach (ManagementObject localPrinter in collection)
                    {
                        _printers.TryGetValue(AsString(localPrinter.Properties["PortName"]), out TCPPrinter? printer);

                        if (printer == null) continue;

                        printer.Name = AsString(localPrinter.Properties["Name"]);
                        printer.PrinterRelPath = AsString(localPrinter.Properties["__PATH"]);

                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            printers.Add(printer);
                        });
                    }
                });
            });
        }

        private static string AsPrinterRelPath(string name, string server)
        {
            return $@"\\{server}\root\cimv2:Win32_Printer.DeviceID=""{name}""";
        }

        private static string AsTcpPrinterRelPath(string name, string server)
        {
            return $@"\\{server}\root\cimv2:Win32_TCPIPPrinterPort.Name=""{name}""";
        }

        public static void ModifyTCPPrinter(ref TCPPrinter printer, PrinterModifications printerModifications)
        {
            if (printerModifications.Name != "" && printer.Name != printerModifications.Name)
            {
                using (ManagementObject moPrinter = new ManagementObject(printer.PrinterRelPath))
                {
                    moPrinter.Get();

                    uint result = (uint)moPrinter.InvokeMethod("RenamePrinter", new object[] { printerModifications.Name });

                    if (result == 0) // Success
                    {
                        printer.PrinterRelPath = AsPrinterRelPath(printerModifications.Name, printer.Server);
                    }
                }
            }

            using (ManagementObject moTCPPrinter = new ManagementObject(printer.TcpPrinterRelPath))
            {
                moTCPPrinter.Get();

                if (printerModifications.HostAddress != "" && printer.HostAddress != printerModifications.HostAddress)
                {
                    moTCPPrinter.Properties["HostAddress"].Value = printerModifications.HostAddress;

                    string newManagementPath = moTCPPrinter.Put().ToString();

                    if (newManagementPath != null)
                    {
                        if (printer.TcpPrinterRelPath != newManagementPath) printer.TcpPrinterRelPath = newManagementPath;
                        printer.HostAddress = printerModifications.HostAddress;
                    }
                }

                if (printerModifications.Port != 0 && printer.Port != printerModifications.Port)
                {
                    moTCPPrinter.Properties["PortNumber"].Value = printerModifications.Port;

                    string newManagementPath = moTCPPrinter.Put().ToString();

                    if (newManagementPath != null)
                    {
                        if (printer.TcpPrinterRelPath != newManagementPath) printer.TcpPrinterRelPath = newManagementPath;
                        printer.Port = printerModifications.Port;
                    }
                }

                if (printerModifications.PortName != "" && printer.PortName != printerModifications.PortName)
                {
                    string oldPortName = AsString(moTCPPrinter.Properties["Name"]);

                    moTCPPrinter.Properties["Name"].Value = printerModifications.PortName;

                    string newManagementPath = moTCPPrinter.Put().ToString();

                    if (newManagementPath != null)
                    {
                        if (printer.TcpPrinterRelPath != newManagementPath) printer.TcpPrinterRelPath = newManagementPath;
                        printer.PortName = printerModifications.PortName;

                        using (ManagementObject moPrinter = new ManagementObject(printer.PrinterRelPath))
                        {
                            moPrinter.Get();
                            moPrinter.Properties["PortName"].Value = printer.PortName;
                            moPrinter.Put();
                        }

                        using (ManagementObject moPrinterPort = new ManagementObject(AsTcpPrinterRelPath(oldPortName, printer.Server)))
                        {
                            moPrinterPort.Get();
                            moPrinterPort.Delete();
                        }
                    }
                }
            }
        }

        public static Dictionary<TCPPrinter, bool> ModifyTCPPrinters(List<TCPPrinter> printers)
        {
            return new Dictionary<TCPPrinter, bool>();
        }
    }
}
