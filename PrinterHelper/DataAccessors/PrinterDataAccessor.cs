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
                        TCPPrinter printer = new(
                            "",
                            AsString(tcpPrinter.Properties["Name"]),
                            AsString(tcpPrinter.Properties["HostAddress"]),
                            Int32.Parse(AsString(tcpPrinter.Properties["PortNumber"])),
                            AsString(tcpPrinter.Properties["__SERVER"]),
                            "",
                            AsString(tcpPrinter.Properties["__PATH"])
                        );

                        _printers.Add(printer.PortName.Value, printer);
                    }
                });

                WmiQueryHelper.Execute($"select Name, PortName, __PATH from Win32_Printer", (ManagementObjectCollection collection) =>
                {
                    foreach (ManagementObject localPrinter in collection)
                    {
                        _printers.TryGetValue(AsString(localPrinter.Properties["PortName"]), out TCPPrinter? printer);

                        if (printer == null) continue;

                        printer.Name.Solidify(AsString(localPrinter.Properties["Name"]));
                        printer.PrinterRelPath.Solidify(AsString(localPrinter.Properties["__PATH"]));

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

        public static void ModifyTCPPrinter(TCPPrinter printer)
        {
            if (printer.Name.HasChanged())
            {
                using (ManagementObject moPrinter = new ManagementObject(printer.PrinterRelPath.Value))
                {
                    moPrinter.Get();

                    uint result = (uint)moPrinter.InvokeMethod("RenamePrinter", new object[] { printer.Name.Value });

                    if (result == 0) // Success
                    {
                        printer.PrinterRelPath.Solidify(AsPrinterRelPath(printer.Name.Value, printer.Server.Value));
                        printer.Name.Solidify();
                    }
                }
            }

            using (ManagementObject moTCPPrinter = new ManagementObject(printer.TCPPrinterRelPath.Value))
            {
                moTCPPrinter.Get();

                if (printer.HostAddress.HasChanged())
                {
                    moTCPPrinter.Properties["HostAddress"].Value = printer.HostAddress.Value;

                    string newManagementPath = moTCPPrinter.Put().ToString();

                    if (newManagementPath != null)
                    {
                        if (printer.TCPPrinterRelPath.Value != newManagementPath) printer.TCPPrinterRelPath.Solidify(newManagementPath);
                        printer.HostAddress.Solidify();
                    }
                }

                if (printer.Port.HasChanged())
                {
                    moTCPPrinter.Properties["PortNumber"].Value = printer.Port.Value;

                    string newManagementPath = moTCPPrinter.Put().ToString();

                    if (newManagementPath != null)
                    {
                        if (printer.TCPPrinterRelPath.Value != newManagementPath) printer.TCPPrinterRelPath.Solidify(newManagementPath);
                        printer.Port.Solidify();
                    }
                }

                if (printer.PortName.HasChanged())
                {
                    string oldPortName = AsString(moTCPPrinter.Properties["Name"]);

                    moTCPPrinter.Properties["Name"].Value = printer.PortName.Value;

                    string newManagementPath = moTCPPrinter.Put().ToString();

                    if (newManagementPath != null)
                    {
                        if (printer.TCPPrinterRelPath.Value != newManagementPath) printer.TCPPrinterRelPath.Solidify(newManagementPath);
                        printer.PortName.Solidify();

                        using (ManagementObject moPrinter = new ManagementObject(printer.PrinterRelPath.Value))
                        {
                            moPrinter.Get();
                            moPrinter.Properties["PortName"].Value = printer.PortName.Value;
                            moPrinter.Put();
                        }

                        using (ManagementObject moPrinterPort = new ManagementObject(AsTcpPrinterRelPath(oldPortName, printer.Server.Value)))
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
