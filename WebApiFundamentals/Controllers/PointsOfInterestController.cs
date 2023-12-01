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

        [HttpGet("{pointofinterestid}", Name ="GetPointOfInterest")]
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

        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInerest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {
            var city = CitiesDataStore.Current.Cities.Find(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            // demo purposes - to be improved
            var maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

            var poi = new PointOfInterestDto
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterest.Add(poi);

            return CreatedAtRoute("GetPointOfInterest", new { cityId = cityId, pointOfInterestId = poi.Id }, poi);
        }
    }
}
