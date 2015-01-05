using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using WpfCustomControls;

namespace WpfControlsApp.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

            Title = "test";
          //  Txt = "aaa";
            Suggestion = new ObservableCollection<object>();

            var searchTextChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                ev => PropertyChanged += ev,
                ev => PropertyChanged -= ev
                )
                .Where(ev => ev.EventArgs.PropertyName == "Txt");

            var input = searchTextChanged
                .Where(ev => Txt != null && Txt.Length >= 2)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(40))
                .Select(args => Txt);
                

            var search = Observable.ToAsync<string, List<DataKeyValue>>(DoSearch);


            var results = from searchTerm in input
                          from result in search(searchTerm).TakeUntil(input)
                          select result;

            // Log the search result and add the results to the results collection
            var dispatcherScheduler = new DispatcherScheduler(DispatcherHelper.UIDispatcher);
            results.ObserveOn(dispatcherScheduler)
            .Subscribe(result =>
            {
                Suggestion.Clear();
                result.ToList().ForEach(item => Suggestion.Add(item));
            }
            );

        }

        public List<DataKeyValue> DoSearch(string text)
        {
            DispatcherHelper.UIDispatcher.Invoke(new Action(()=>Busy=true));

            Thread.Sleep(1000);
            var res = new List<DataKeyValue>
            {
                new DataKeyValue("1", "aafirst"),
                new DataKeyValue("2", "aasecond"),
                new DataKeyValue("3", "aathird"),
                new DataKeyValue("4", "aaforth"),
                new DataKeyValue("5", "aafifth"),
                new DataKeyValue("6", "aasixth")
            }.Where(x => ((IResultItem)x).Title.Contains(text)).ToList();

    //        DispatcherHelper.UIDispatcher.Invoke(new Action(()=>Busy = false));

            return res;
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged(()=>Title);
                }
            }
        }

        private string _txt;
        public string Txt
        {
            get { return _txt; }
            set
            {
                if (_txt != value)
                {
                    _txt = value;
                    RaisePropertyChanged(() => Txt);
               //     FillResults(Txt);
                }
            }
        }

        private bool _busy;
        public bool Busy
        {
            get { return _busy; }
            set
            {
                if (_busy != value)
                {
                    _busy = value;
                    RaisePropertyChanged(() => Busy);
                }
            }
        }

        public Task<DataKeyValue[]> GetDataAsync(string text)
        {
            return null;
        }

        private void FillResults(string txt)
        {
            Busy = true;
            
            var res = new List<object>
            {
                new DataKeyValue("1", "aafirst"),
                new DataKeyValue("2", "aasecond"),
                new DataKeyValue("3", "aathird"),
                new DataKeyValue("4", "aaforth"),
                new DataKeyValue("5", "aafifth"),
                new DataKeyValue("6", "aasixth")
            }.Where(x => ((IResultItem) x).Title.Contains(txt));
          
            var result = new ObservableCollection<object>(res);
            Busy = false;
            Suggestion = result;
        }


        private ObservableCollection<object> _suggestion;

        public ObservableCollection<object> Suggestion
        {
            get { return _suggestion; }
            set
            {
                if (_suggestion != value)
                {
                    _suggestion = value;
                    RaisePropertyChanged(()=>Suggestion);
                }
            }
        } 
    }

    public class DataKeyValue:IResultItem
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public DataKeyValue()
        {
            
        }

        public DataKeyValue(string id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}