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
        public static async Task InitializeMinimalPrinterInformation(ObservableCollection<TCPPrinter> printers)
        {
            if (printers.Count > 0)
            {
                printers.Clear();
            }

            await Task.Run(() =>
            {
                ManagementScope scope = new(ManagementPath.DefaultPath);
                SelectQuery tcpPrinterQuery = new($"select Name, __SERVER from Win32_TCPIPPrinterPort");

                ManagementObjectSearcher tcpPrinterSearcher = new(scope, tcpPrinterQuery);
                ManagementObjectCollection tcpPrinters = tcpPrinterSearcher.Get();

                SelectQuery localPrinterQuery = new($"select Name, PortName, __PATH from Win32_Printer");

                ManagementObjectSearcher localPrinterSearcher = new(scope, localPrinterQuery);
                ManagementObjectCollection localPrinters = localPrinterSearcher.Get();

                Dictionary<string, TCPPrinter> _printers = new();

                foreach (ManagementObject tcpPrinter in tcpPrinters)
                {
                    TCPPrinter printer = new()
                    {
                        PortName = AsString(tcpPrinter.Properties["Name"]),
                        TcpPrinterRelPath = AsString(tcpPrinter.Properties["__PATH"]),
                        Server = AsString(tcpPrinter.Properties["__SERVER"])
                    };

                    _printers.Add(printer.PortName, printer);
                }

                foreach (ManagementObject localPrinter in localPrinters)
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

                tcpPrinterSearcher.Dispose();
                tcpPrinters.Dispose();
                localPrinterSearcher.Dispose();
                localPrinters.Dispose();
            });
        }

        public static async Task<ObservableCollection<TCPPrinter>> GetConnectedPrintersAsync()
        {
            ObservableCollection<TCPPrinter> printers = await Task.Run(() =>
            {
                List<TCPPrinter> listOfPrinters = new();

                ManagementScope scope = new ManagementScope(ManagementPath.DefaultPath);
                SelectQuery wmiTCPIPPrinterQuery = new SelectQuery($"select * from Win32_TCPIPPrinterPort");

                ManagementObjectSearcher wmiTCPIPPrinterSearcher = new ManagementObjectSearcher(scope, wmiTCPIPPrinterQuery);
                ManagementObjectCollection wmiTCPIPPrinters = wmiTCPIPPrinterSearcher.Get();

                foreach (ManagementObject wmiTCPIPPrinter in wmiTCPIPPrinters)
                {
                    try
                    {
                        string printerPortName = AsString(wmiTCPIPPrinter.Properties["Name"]);
                        SelectQuery printerNameQuery = new SelectQuery($"select * from Win32_Printer Where PortName=\"{printerPortName}\"");

                        using (ManagementObjectSearcher printerSearcher = new ManagementObjectSearcher(scope, printerNameQuery))
                        using (ManagementObject? printer = printerSearcher.Get().OfType<ManagementObject>().FirstOrDefault())
                        {
                            if (printer is not null)
                            {
                                string _server = AsString(wmiTCPIPPrinter.Properties["__SERVER"]);
                                string printerName = AsString(printer.Properties["Name"]);

                                TCPPrinter tcpPrinter = new()
                                {
                                    Name = printerName,
                                    HostAddress = AsString(wmiTCPIPPrinter.Properties["HostAddress"]),
                                    PortName = printerPortName,
                                    Port = int.Parse(AsString(wmiTCPIPPrinter.Properties["PortNumber"])),
                                    Server = _server,
                                    TcpPrinterRelPath = AsTcpPrinterRelPath(printerPortName, _server),
                                    PrinterRelPath = AsPrinterRelPath(printerName, _server)
                                };

                                listOfPrinters.Add(tcpPrinter);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Something went wrong when getting a printer");
                        Trace.WriteLine(ex);
                    }
                }

                wmiTCPIPPrinters.Dispose();
                wmiTCPIPPrinterSearcher.Dispose();

                return new ObservableCollection<TCPPrinter>(listOfPrinters);
            });

            return printers;
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
