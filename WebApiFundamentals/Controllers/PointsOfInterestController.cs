using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiFundamentals.Models;

namespace WebApiFundamentals.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            var city = CitiesDataStore.Current.Cities.SingleOrDefault(c => c.Id == cityId);

            if(city == null)
            {
                return NotFound();
            }

            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{pointofinterestid}")]
        public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            var city = CitiesDataStore.Current.Cities.SingleOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var poi = city.PointsOfInterest.SingleOrDefault(p => p.Id == pointOfInterestId);

            if(poi == null)
            {
                return NotFound();
            }

            return Ok(poi);
        }
    }
}
