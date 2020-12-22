
using System;
using System.Collections.Generic;
using System.Linq;
using AirtableApiClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using GoogleMapsComponents.Maps;
using LittleHelpBook.Server.Services;
using LittleHelpBook.Shared.Data;

namespace LittleHelpBook.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
     
        private readonly ILogger<CityController> logger;
        private readonly AirTableService _airTableService;

        public CityController(ILogger<CityController> logger, AirTableService airTableService)
        {
            this.logger = logger;
            _airTableService = airTableService;
        }

        [HttpGet]
        public async Task<IEnumerable<City>> Get()
        {

            //var data = await _airTableService.GetCitiesPopulatedAsync();
            var data = await _airTableService.GetCitiesAsync();

            return data.OrderBy(o => o.Name).ToArray();

        }

    }
}
