using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GoogleMapsComponents.Maps;
using LittleHelpBook.Shared.Data;
using Radzen;
using Place = LittleHelpBook.Shared.Data.Place;

namespace LittleHelpBook.Client.Services
{
    public class ClientCache : BaseCache
    {
        //places data
        private IEnumerable<Place> _allPlaces;
        private IEnumerable<Place> _foundPlaces;
        private IEnumerable<Alert> _alerts;
        private IEnumerable<Info> _infos;

        //lookups
        private IEnumerable<Category> _categories;
        private IEnumerable<Subcategory> _subcategories;

        // filters
        private string[] _selectedCategories;
        private string[] _selectedSubcategories;
        private string _searchWords;

        private NotificationService _notificationService;
 

        public ClientCache(HttpClient http, NotificationService notificationService ) : base(http)
        {
            _notificationService = notificationService;
        }

        public IEnumerable<Place> AllPlaces
        {
            get => _allPlaces;
            set => SetField(ref _allPlaces, value);
        }

        public IEnumerable<Place> FoundPlaces
        {
            get => _foundPlaces;
            set => SetField(ref _foundPlaces, value);
        }

        public IEnumerable<Category> Categories
        {
            get => _categories;
            set => SetField(ref _categories, value);
        }

        public IEnumerable<Alert> Alerts
        {
            get => _alerts;
            set => SetField(ref _alerts, value);
        }

        public IEnumerable<Subcategory> Subcategories
        {
            get => _subcategories;
            set => SetField(ref _subcategories, value);
        }

        public IEnumerable<Info> Infos
        {
            get => _infos;
            set => SetField(ref _infos, value);
        }

        public async Task<IEnumerable<Place>> GetAllPlaces()
        {
            return _allPlaces ??= await Http.GetFromJsonAsync<IEnumerable<Place>>("Place");
        }

        public async Task<IEnumerable<Alert>> GetAlerts()
        {
            return _alerts ??= await Http.GetFromJsonAsync<IEnumerable<Alert>>("Alert");
        }

        public async Task<IEnumerable<Info>> GetInfos()
        {
            return _infos ??= await Http.GetFromJsonAsync<IEnumerable<Info>>("Info");
        }

        public async Task<IEnumerable<Place>> GetFoundPlaces()
        {
            return _foundPlaces ??= await GetAllPlaces();
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            return _categories ??= await Http.GetFromJsonAsync<IEnumerable<Category>>("Category");
        }

        public async Task<IEnumerable<Subcategory>> GetSubcategories()
        {
            return _subcategories ??= await Http.GetFromJsonAsync<IEnumerable<Subcategory>>("Subcategory");
        }

        public async Task Clear()
        {
            _categories = null;
            _subcategories = null;
            _searchWords = null;
            _infos = null;
            _alerts = null;
            _allPlaces = null;
            _foundPlaces = null;
        }
        public async Task Reset()
        {
            await Clear();
            await GetCategories();
            await GetSubcategories();
            await GetAlerts();
            await GetInfos();
        }

        public async Task<Place> GetPlace(string Id)
        {
            await GetAllPlaces();
            var place = AllPlaces.SingleOrDefault(p => p.Id == Id); ;
            return place;
        }

        public async Task Search(string words)
        {
            _searchWords = words;
            await ApplyFilters();
        }

        public async Task FilterCategory(string[] selectedCategories)
        {
            _selectedCategories = selectedCategories;
            await ApplyFilters();
        }

        public async Task FilterSubcategory(string[] selectedSubcategories)
        {
            _selectedSubcategories = selectedSubcategories;
            await ApplyFilters();
        }

        private async Task ApplyFilters()
        {
            await GetAllPlaces();
            await GetSubcategories();
            await GetCategories();

            var query = from p in AllPlaces select p;

            // add cat and sub-cat to search
            if (_searchWords != null)
            {
                _selectedCategories = Categories.Where(c => c.Name != null &&  c.Name.Contains(_searchWords)).Select(c => c.Id).ToArray();

                _selectedSubcategories = Subcategories.Where(c => c.Name!=null && c.Name.Contains(_searchWords)).Select(c => c.Id).ToArray();
            }


            if (_selectedCategories?.Length > 0)
            {
                query = from p in query
                    where p.CategoryList.Any(c => _selectedCategories.Contains(c.Id))
                    select p;
            }

            if (_selectedSubcategories?.Length > 0)
            {
                query = from p in query
                        where p.SubcategoryList.Any(c => _selectedSubcategories.Contains(c.Id))
                        select p;
            }

            if (_searchWords != null)
            {
                query = query.Where(p => p.Name.Contains(_searchWords, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var list = query.ToList();  // deferred execution
            if (list.Count == 0)
            {
                var msg = new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info, 
                    Summary = "No matches", 
                    Detail = "Nothing found with those filters, showing everything.",
                    Duration = 4000
                };
                _notificationService.Notify(msg);
                FoundPlaces = AllPlaces;
            }
            else
            {
                var msg = new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Matches!",
                    Detail = $"Found {list.Count} matching places.",
                    Duration = 4000
                };
                _notificationService.Notify(msg);
                FoundPlaces = list;
            }

            OnCacheUpdated();
        }

        public async Task<List<Place>> GetPlacesBySub(string id)
        {
            await GetAllPlaces();
            var list = AllPlaces.Where(p => p.SubcategoryList.Any(sc=>sc.Id==id)).ToList();
            return list;
        }
        public async Task<List<Place>> GetPlacesByCat(string id)
        {
            await GetAllPlaces();
            var list = AllPlaces.Where(p => p.CategoryList.Any(sc => sc.Id == id)).ToList();
            return list;
        }

        public async Task<List<Info>> GetInfosByCat(string id)
        {
            await GetInfos();
            var list = Infos.Where(p => p.CategoryList.Any(c => c.Id == id)).ToList();
            return list;
        }

      
    }
}
