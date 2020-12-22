using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleMapsComponents.Maps;
using LittleHelpBook.Services;
using LittleHelpBook.Shared.Data;
using Microsoft.Extensions.Configuration;
using Place = LittleHelpBook.Shared.Data.Place;

namespace LittleHelpBook.Server.Services
{
    /// <summary>
    /// a bruit force airtable data caching service
    /// </summary>
    public class AirTableService : AirTableBase
    {
        private IEnumerable<Place> _placesPop;  // the highest level populated places list.
        private IEnumerable<Place> _places;  
        private IEnumerable<Category> _categories;
        private IEnumerable<Subcategory> _subcategories;
        private IEnumerable<City> _cities;
        private IEnumerable<Alert> _alerts;
        private IEnumerable<Info> _infos;

        public AirTableService(IConfiguration configuration) : base(configuration) {}

        public async Task<IEnumerable<Place>> GetPlacesAsync()
        {
            return _places ??= await GetTableAsync<Place>("Help Services");
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return _categories ??= await GetTableAsync<Category>("Categories");
        }

        public async Task<IEnumerable<Category>> GetCategoriesPopulatedAsync()
        {
            if (_categories != null)
            {
                return _categories;
            }

            _categories = await GetCategoriesAsync();
            foreach (var cat in _categories)
            {
                if (cat.Subcategories != null)
                {
                    foreach (var id in cat.Subcategories)
                    {
                        cat.SubcategoryList.Add(await GetSubcategoryAsync(id));
                    }

                    cat.SubcategoryList.OrderBy(sc => sc.Name);
                }
            }

            return _categories;
        }

        public async Task<Category> GetCategoryAsync(string id)
        {
            _categories ??= await GetCategoriesAsync();

            return _categories.FirstOrDefault(c => c.Id == id);
        }

        public async Task<IEnumerable<Subcategory>> GetSubcategoriesAsync()
        {
            return _subcategories ??= await GetTableAsync<Subcategory>("Subcategories");
        }

        public async Task<Subcategory> GetSubcategoryAsync(string id)
        {
            _subcategories ??= await GetSubcategoriesAsync();

            return _subcategories.FirstOrDefault(c => c.Id == id);
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        //public async Task<City> GetCitiesAsync()
        {
            return _cities ??= await GetTableAsync<City>("Cities");
        }

        public async Task<City> GetCityAsync(string id)
        {
            _cities ??= await GetCitiesAsync();

            return _cities.FirstOrDefault(c => c.Id == id);
        }

        public async Task<IEnumerable<Place>> GetPlacesPopulatedAsync()
        {
            if (_placesPop != null)
            {
                return _placesPop;
            }
            var places = await GetPlacesAsync();

            // populate lookups
            foreach (var place in places)
            {
                if (place.Categories != null)
                {
                    foreach (var id in place.Categories)
                    {
                        place.CategoryList.Add(await GetCategoryAsync(id));
                    }

                    place.Categories = null; // remove from payload after hydration.
                }
                if (place.Subcategories != null)
                {
                    foreach (var id in place.Subcategories)
                    {
                        place.SubcategoryList.Add(await GetSubcategoryAsync(id));
                    }

                    place.Subcategories = null;
                }
                if (place.Cities != null)
                {
                    foreach (var id in place.Cities)
                    {
                        place.CityList.Add(await GetCityAsync(id));
                    }

                    place.Cities = null; // remove from payload after hydration.
                }
            }
            _placesPop = places.ToArray();
            return _placesPop;

        }

        public async Task<IEnumerable<Alert>> GetAlertsAsync()
        {
            return _alerts ??= await GetTableAsync<Alert>("Alerts");
        }

        public async Task<IEnumerable<Info>> GetInfosAsync()
        {
            return _infos ??= await GetTableAsync<Info>("Infos");
        }

        public async Task<IEnumerable<Info>> GetInfosPopulatedAsync()
        {
            if (_infos != null)
            {
                return _infos;
            }

            _infos = await GetInfosAsync();
            foreach (var info in _infos)
            {
                if (info.Categories != null)
                {
                    foreach (var id in info.Categories)
                    {
                        info.CategoryList.Add(await GetCategoryAsync(id));
                    }

                    info.CategoryList.OrderBy(sc => sc.Name);
                }
            }

            return _infos;
        }

        public async Task Clear()
        {
         _placesPop = null;  
         _places = null;  
         _categories = null;
         _subcategories = null;
         _alerts = null;
         _infos = null;
         _cities = null;
        }
    }
}
