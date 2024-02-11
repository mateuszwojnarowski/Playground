using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using WebApiFundamentals.Models;
using WebApiFundamentals.Services;

namespace WebApiFundamentals.Controllers;

[Route("api/cities/{cityId:int}/pointsofinterest")]
[ApiController]
public class PointsOfInterestController : ControllerBase
{
    private readonly ILogger<PointsOfInterestController> _logger;
    private readonly IMailService _mailService;
    private readonly CitiesDataStore _citiesDataStore;

    public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, CitiesDataStore citiesDataStore)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
    }

    [HttpGet]
    public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
    {
        var city = _citiesDataStore.Cities.SingleOrDefault(c => c.Id == cityId);

        if(city == null)
        {
            return NotFound();
        }

        return Ok(city.PointsOfInterest);
    }

    [HttpGet("{pointofinterestid}", Name ="GetPointOfInterest")]
    public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        try
        {
            //throw new Exception("sample");
            var city = _citiesDataStore.Cities.SingleOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                _logger.LogInformation("City with id {cityId} wasn't found when accessing points of interest.", cityId);
                return NotFound();
            }

            var poi = city.PointsOfInterest.SingleOrDefault(p => p.Id == pointOfInterestId);

            if(poi == null)
            {
                return NotFound();
            }

            return Ok(poi);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Exception while getting point of interest for city with id {cityId}", ex);
            return StatusCode(500, "A problem happened while handling your request");
        }
    }

    [HttpPost]
    public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId,
        PointOfInterestForCreationDto pointOfInterest)
    {
        var city = _citiesDataStore.Cities.Find(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }

        // demo purposes - to be improved
        var maxPointOfInterestId =
            _citiesDataStore.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

        var poi = new PointOfInterestDto
        {
            Id = ++maxPointOfInterestId,
            Name = pointOfInterest.Name,
            Description = pointOfInterest.Description
        };

        city.PointsOfInterest.Add(poi);

        return CreatedAtRoute("GetPointOfInterest", new { cityId = cityId, pointOfInterestId = poi.Id }, poi);
    }

    [HttpPut("{pointofinterestid}")]
    public ActionResult UpdatePointOfInterest(int cityId, int pointofinterestid,
        PointOfInterestForUpdateDto pointOfInterest)
    {
        var city = _citiesDataStore.Cities.Find(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }
            
        // find point of interest
        var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointofinterestid);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }

        pointOfInterestFromStore.Name = pointOfInterest.Name;
        pointOfInterestFromStore.Description = pointOfInterest.Description;

        return NoContent();
    }

    [HttpPatch("{pointofinterestid}")]
    public ActionResult PartiallyUpdatePointOfInterest(int cityId, int pointofinterestid,
        JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
    {
        var city = _citiesDataStore.Cities.Find(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }
            
        var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointofinterestid);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }

        var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
        {
            Name = pointOfInterestFromStore.Name,
            Description = pointOfInterestFromStore.Description
        };

        patchDocument.ApplyTo(pointOfInterestToPatch);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryValidateModel(pointOfInterestToPatch))
        {
            return BadRequest(ModelState);
        }

        pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
        pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;
            
        return NoContent();
    }

    [HttpDelete("{pointofinterestid}")]
    public ActionResult DeletePointOfInterest(int cityId, int pointofinterestid)
    {
        var city = _citiesDataStore.Cities.Find(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }
            
        var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointofinterestid);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }

        city.PointsOfInterest.Remove(pointOfInterestFromStore);
        _mailService.Send("Point of interest deleted.",
            $"Point oif interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted");
        return NoContent();
    }
}