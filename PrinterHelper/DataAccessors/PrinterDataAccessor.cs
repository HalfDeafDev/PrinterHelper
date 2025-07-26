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
