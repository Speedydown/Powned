using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using XamlControlLibrary;

namespace PownedLogic.ViewModels
{
    public sealed class SearchViewModel : ViewModel
    {
        public static readonly SearchViewModel instance = new SearchViewModel();

        public ObservableCollection<SearchResult> SearchResults { get; private set; }
        public string SearchQuery { get; set; }

        private SearchViewModel()
        {
            SearchResults = new ObservableCollection<SearchResult>();
            SearchQuery = string.Empty;
        }

        public void SetLoadingControl(LoadingControl loadingControl)
        {
            if (this.loadingControl == null)
            {
                this.loadingControl = loadingControl;
            }

            this.IsLoading = false;
        }

        public async Task Search()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SearchResults.Clear();
                this.IsLoading = true;
            });

            try
            {
                var Result = await Datahandler.instance.Search(SearchQuery);

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (SearchResult sr in Result)
                        {
                            SearchResults.Add(sr);
                        }

                        this.IsLoading = false;
                    });
            }
            catch
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        this.DisplayError = true;
                    });
            }
        }
    }
}
