using Microsoft.AspNetCore.Mvc;
using WebApiFundamentals.Models;

namespace WebApiFundamentals.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<CityDto>> GetCities()
        {
            return Ok(CitiesDataStore.Current.Cities);
        }

        [HttpGet("{id:int}")]
        public ActionResult<CityDto> GetCity(int id)
        {
            var city = CitiesDataStore.Current.Cities.SingleOrDefault(c => c.Id == id);

            if(city == null)
            {
                return NotFound();
            }

            return Ok(city);
        }
    }
}
