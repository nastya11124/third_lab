using App3.CoreSpace.Interfaces;

namespace App3.CoreSpace
{
    public class TrackServices: IServices
    {
        private readonly IRepository _trackRepository;


        public TrackServices(IRepository trackRepository)
        {
            _trackRepository = trackRepository;

        }

        public async Task<bool> DeleteTrack(string artistName, string TrackName)
        {
            return await _trackRepository.DeleteTrack(artistName, TrackName);
        }
        public async Task<bool> AddTrack(string artistName, string TrackName)
        {

            if (await _trackRepository.CheckTrackExists(artistName, TrackName)) { return false; }
            return await _trackRepository.AddTrack(artistName, TrackName);
        }
        public async Task<Dictionary<string, List<string>>> SearchTrack(bool byAuthor, string criterion, int page, int pageSize)
        {
            return await _trackRepository.SearchTracks(byAuthor, criterion, page, pageSize);
        }
        public async Task<Dictionary<string, List<string>>> ShowTracks(int page, int pageSize)
        {
            return await _trackRepository.Search(page, pageSize);
        }


    }
}