using Microsoft.AspNetCore.Mvc;
using App3.Modals;
using App3.CoreSpace.Interfaces;

namespace App3.CoreSpace
{
    [ApiController]
    [Route("api/tracks/[action]")]
    public class TracksController : ControllerBase
    {
        private readonly IServices _trackService;

        public TracksController(IServices trackService)
        {
            _trackService = trackService;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTrack(TrackRequest request)
        {
            if  (await _trackService.DeleteTrack(request.ArtistName, request.TrackName))
            {
                return Ok(new { message = "Track deleted successfully" });
            }
            else
            {
                return NotFound(new { error = "Track not found" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTrack(TrackRequest request)
        {
            if (await _trackService.AddTrack(request.ArtistName, request.TrackName))
            {
                return Ok(new { message = "Track added successfully" });
            }
            else
            {
                return Conflict(new { error = "Track already exists" });
            }
        }

        [HttpGet()]
        public async Task<IActionResult> Search([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _trackService.ShowTracks(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet()]
        public async Task<IActionResult> SearchTrack([FromQuery] Criterion criterion, [FromQuery] string value, 
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {

            bool byAuthor = criterion == Criterion.track ? false : true;
            
            try
            {
                var result = await _trackService.SearchTrack(byAuthor, value, page, pageSize);
                if (result.Count == 0)
                {
                    return NotFound(new { error = "No matches found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}