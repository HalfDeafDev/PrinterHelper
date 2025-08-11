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
        private string printerNameFilter = "";
        public string PrinterNameFilter { get => printerNameFilter; set => SetProperty(ref printerNameFilter, value); }
        private string ipFilter = "";
        public string IPFilter { get => ipFilter; set => SetProperty(ref ipFilter, value); }
        private string portFilter = "";
        public string PortFilter { get => portFilter; set => SetProperty(ref portFilter, value); }
        private bool useRegex = false;
        public bool UseRegex { get => useRegex; set => SetProperty(ref useRegex, value); }

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
            }
        }

        private void updatePrinter()
        {
            if (_selectedPrinter != null) PrinterDataAccessor.ModifyTCPPrinter(_selectedPrinter);
        }
        public ICommand UpdatePrinter { get; }

        public TestVM()
        {
            _ = LoadPrintersAsync();
            UpdatePrinter = new RelayCommand(_ => updatePrinter(), _ => SelectedPrinter != null); 
        }

        private async Task LoadPrintersAsync()
        {
            await PrinterDataAccessor.InitializePrinterInformationAsync(Printers);
        }
    }
}
