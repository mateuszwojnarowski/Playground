using Microsoft.AspNetCore.Mvc;
using WebApiFundamentals.Models;

namespace WebApiFundamentals.Controllers;

[ApiController]
[Route("api/cities")]
public class CitiesController : ControllerBase
{
    private readonly CitiesDataStore _citiesDataStore;

    public CitiesController(CitiesDataStore citiesDataStore)
    {
        _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
    }

    [HttpGet]
    public ActionResult<IEnumerable<CityDto>> GetCities()
    {
        return Ok(_citiesDataStore.Cities);
    }

    [HttpGet("{id:int}")]
    public ActionResult<CityDto> GetCity(int id)
    {
        var city = _citiesDataStore.Cities.SingleOrDefault(c => c.Id == id);

        if(city == null)
        {
            return NotFound();
        }

        return Ok(city);
    }
}