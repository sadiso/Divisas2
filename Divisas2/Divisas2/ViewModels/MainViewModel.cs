using Divisas2.Controls;
using Divisas2.Models;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Divisas2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Attributes
        private ExchangeRates exchangeRates;

        private decimal amount;

        private double sourceRate;

        private double targetRate;

        private int sourceIndex;

        private int targetIndex;

        private bool isRunning;

        private bool isEnabled;

        private string message;
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public ObservableCollection<Rate> Rates { get; set; }

        Dictionary<string, string> MoneyDescription;
        public decimal Amount
        {
            set
            {
                if (amount != value)
                {
                    amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
                }
            }
            get
            {
                return amount;
            }
        }
        public double SourceRate
        {
            set
            {
                if (sourceRate != value)
                {
                    sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceRate"));
                }
            }
            get
            {
                return sourceRate;
            }
        }
        public double TargetRate
        {
            set
            {
                if (targetRate != value)
                {
                    targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetRate"));
                }
            }
            get
            {
                return targetRate;
            }
        }
        public int SourceIndex
        {
            set
            {
                if (sourceIndex != value)
                {
                    sourceIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceIndex"));
                }
            }
            get
            {
                return sourceIndex;
            }
        }
        public int TargetIndex
        {
            set
            {
                if (targetIndex != value)
                {
                    targetIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetIndex"));
                }
            }
            get
            {
                return targetIndex;
            }
        }
        public bool IsRunning
        {
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get
            {
                return isRunning;
            }
        }
        public bool IsEnabled
        {
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get
            {
                return isEnabled;
            }
        }
        public string Message
        {
            set
            {
                if (message != value)
                {
                    message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
                }
            }
            get
            {
                return message;
            }
        }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            Rates = new ObservableCollection<Rate>();
            MoneyDescription = new Dictionary<string, string>();
            IsEnabled = false;
            Message = "Ingrese la cantidad a convertir, la moneda orgien, la monda destino y presione el botón de 'Convertir'";
            GetRates();
            GetRatesDescription();
        }
        #endregion

        #region Methods
        private void LoadRates()
        {
            Rates.Clear();
            var MoneyName = string.Empty;
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();

            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);
                MoneyDescription.TryGetValue(code, out MoneyName);
                Rates.Add(new Rate
                {
                    Code = code,
                    Descripcion = MoneyName,
                    TaxRate = (double)property.GetValue(exchangeRates.Rates),
                });
            }
        }
        private async void GetRates()
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://openexchangerates.org");
                var url = "/api/latest.json?app_id=f490efbcd52d48ee98fd62cf33c47b9e";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Error", response.StatusCode.ToString(), "Aceptar");
                    IsRunning = false;
                    IsEnabled = false;
                    return;
                }

                var result = await response.Content.ReadAsStringAsync();
                exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(result);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "Aceptar");
                IsRunning = false;
                IsEnabled = false;
                return;
            }
            
            IsRunning = false;
            IsEnabled = true;
        }
        private async void GetRatesDescription()
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://gist.githubusercontent.com");
                var url = "/picodotdev/88512f73b61bc11a2da4/raw/9407514be22a2f1d569e75d6b5a58bd5f0ebbad8";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Error", response.StatusCode.ToString(), "Aceptar");
                    IsRunning = false;
                    IsEnabled = false;
                    return;
                }
                var r= string.Empty;
                var result = await response.Content.ReadAsStringAsync();
                MoneyDescription = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "Aceptar");
                IsRunning = false;
                IsEnabled = false;
                return;
            }

            LoadRates();
            IsRunning = false;
            IsEnabled = true;
        }
        #endregion

        #region Commands
        public ICommand ConvertMoneyCommand
        {
            get { return new RelayCommand(ConvertMoney); }
        }
        private async void ConvertMoney()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Debes ingresar un valor a convertir", "Aceptar");
                return;
            }

            if (SourceRate == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Debes seleccionar la moneda origen", "Aceptar");
                return;
            }

            if (TargetRate == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Debes seleccionar la moneda destino", "Aceptar");
                return;
            }

            decimal amountConverted = amount / (decimal)sourceRate * (decimal)targetRate;

            Message = string.Format("{0:N2} = {1:N2}", amount, amountConverted);
        }
        public ICommand InvertMoneyCommand
        {
            get { return new RelayCommand(InvertMoney); }
        }
        private void InvertMoney()
        {
            var aux = SourceIndex;
            SourceIndex = TargetIndex;
            TargetIndex = aux;
        }
        #endregion

    }

}
