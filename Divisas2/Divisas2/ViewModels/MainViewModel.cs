namespace Divisas2.ViewModels
{
    using Models;
    using Services;
    using GalaSoft.MvvmLight.Command;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Net.Http;
    using System.Reflection;
    using System.Windows.Input;
    using System.Linq;
    using System.Threading.Tasks;
    using Helpers;

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Attributes
        private ExchangeRates exchangeRates;

        private ApiService apiService;

        private DialogService dialogService;

        private DataService dataService;

        private decimal amount;

        private double sourceRate;

        private double targetRate;

        private int sourceIndex;

        private int targetIndex;

        private bool isRunning;

        private bool isEnabled;

        private string message;

        private string connectionStatus;

        private string statusDescription;

        private List<Rate> FullRates;

        private Rate Filler1;

        private Rate Filler2;
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public ObservableCollection<Rate> Rates { get; set; }
        
        public Dictionary<string, string> MoneyDescription;
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
        
        public string ConnectionStatus
        {
            set
            {
                if (connectionStatus != value)
                {
                    connectionStatus = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectionStatus"));
                }
            }
            get
            {
                return connectionStatus;
            }
        }

        public string StatusDescription
        {
            set
            {
                if (statusDescription != value)
                {
                    statusDescription = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusDescription"));
                }
            }
            get
            {
                return statusDescription;
            }
        }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            Rates = new ObservableCollection<Rate>();
            MoneyDescription = new Dictionary<string, string>();
            apiService = new ApiService();
            dataService = new DataService();
            dialogService = new DialogService();
            FullRates = new List<Rate>();

            IsEnabled = true;
            Message = "Ingrese la cantidad a convertir, la moneda orgien, la monda destino y presione el botón de 'Convertir'";

            Filler1 = new Rate
            {
                Code = "",
                Descripcion = "",
                TaxRate = 0,
                Selected = -1,
            };
            Filler2 = Filler1;

            AsyncHelpers.RunSync(() => CheckConnection());
        }
        #endregion

        #region Methods
        private void LoadRates()
        {
            Rates.Clear();
            var MoneyName = string.Empty;
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();

            dataService.DeleteAllAndInsert<Rate>(Filler1);

            foreach (var property in properties)
            {
                
                var code = property.Name.Substring(1, 3);
                MoneyDescription.TryGetValue(code, out MoneyName);
                var modelRate = new Rate
                {
                    Code = code,
                    Descripcion = MoneyName,
                    TaxRate = (double)property.GetValue(exchangeRates.Rates),
                    Selected = -1,
                };
                Rates.Add(modelRate);
                dataService.Insert<Rate>(modelRate);
            }

            LoadIndex();
        }
        private async Task GetRates()
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
        private async Task GetRatesDescription()
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

        private async Task CheckConnection()
        {
            IsRunning = true;
            IsEnabled = false;

            FullRates = dataService.Get<Rate>(false);
            if (FullRates.Count > 0)
            {
                FullRates.RemoveAt(0);
            }

            var checkConnetion = await apiService.CheckConnection();
            if (!checkConnetion.IsSuccess)
            {
                ConnectionStatus = "ic_disconnected.png";
                IsRunning = false;
                IsEnabled = true;
                StatusDescription = "Sin acceso a internet"; //checkConnetion.Message;
                LoadDataBase();
                LoadIndex();
                return;
            }
            ConnectionStatus = "ic_connected.png";
            StatusDescription = "Con acceso a internet";
            await GetRates();
            await GetRatesDescription();
        }

        public void LoadDataBase()
        {
            foreach (var item in FullRates)
            {
                var modelRate = new Rate
                {
                    Code = item.Code,
                    Descripcion = item.Descripcion,
                    TaxRate = item.TaxRate,
                };
                Rates.Add(modelRate);
            }
        }

        public void LoadIndex()
        {
            var Index = FullRates.FindAll(r => r.Selected != -1);
            if((Index != null) && (Index.Count == 2))
            {
                SourceIndex = Index.ElementAt<Rate>(0).Selected;
                TargetIndex = Index.ElementAt<Rate>(1).Selected;
            }
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

            if ((Filler1.Selected != -1) && (Filler2.Selected != -1))
            {
                Filler1.Selected = -1;
                Filler2.Selected = -1;
                dataService.InsertOrUpdate<Rate>(Filler1);
                dataService.InsertOrUpdate<Rate>(Filler2);
            }

            var indexSource = Rates.ElementAt(SourceIndex);
            var targetSource = Rates.ElementAt(TargetIndex);

            indexSource.Selected = SourceIndex;
            targetSource.Selected = TargetIndex;
            Filler1 = indexSource;
            Filler2 = targetSource;

            dataService.InsertOrUpdate<Rate>(indexSource);
            dataService.InsertOrUpdate<Rate>(targetSource);
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
