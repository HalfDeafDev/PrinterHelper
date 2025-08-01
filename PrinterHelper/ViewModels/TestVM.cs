using PrinterHelper.DataAccessors;
using PrinterHelper.Helpers;
using PrinterHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace PrinterHelper.ViewModels
{
    public class TestVM : ObservableObject
    {
        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }
        private bool isLoaded = false;
        public bool IsLoaded
        {
            get => isLoaded;
            set => SetProperty(ref isLoaded, value);
        }

        private ObservableCollection<TCPPrinter> _printers = new();
        public ObservableCollection<TCPPrinter> Printers
        {
            get => _printers;
            set
            {
                SetProperty(ref _printers, value);
            }
        }

        private TCPPrinter? _selectedPrinter;

        public TCPPrinter? SelectedPrinter
        {
            get => _selectedPrinter;
            set
            {
                SetProperty(ref _selectedPrinter, value);

                if (value != null) SetModificationPropertiesFromPrinter(PrinterModifications, value);
                else ClearModificationPropertiesFromPrinter();
            }
        }

        private void SetModificationPropertiesFromPrinter(PrinterModifications printerModifications, TCPPrinter printer)
        {
            printerModifications.Name = printer.Name;
            printerModifications.HostAddress = printer.HostAddress;
            printerModifications.Port = printer.Port;
            printerModifications.PortName = printer.PortName;
        }

        private void ClearModificationPropertiesFromPrinter()
        {
            printerModifications.Name = "";
            printerModifications.HostAddress = "";
            printerModifications.Port = 0;
            printerModifications.PortName = "";
        }

        private void updatePrinter()
        {
            if (_selectedPrinter != null) PrinterDataAccessor.ModifyTCPPrinter(ref _selectedPrinter, PrinterModifications);
        }
        public ICommand UpdatePrinter { get; }

        private PrinterModifications printerModifications = new();
        public PrinterModifications PrinterModifications
        {
            get => printerModifications;
            set => SetProperty(ref printerModifications, value);
        }

        public TestVM()
        {
            _ = LoadPrintersAsync();
            UpdatePrinter = new RelayCommand(_ => updatePrinter(), _ => SelectedPrinter != null);
        }

        private async Task LoadPrintersAsync()
        {
            await PrinterDataAccessor.InitializeMinimalPrinterInformation(Printers);
        }
    }
}
