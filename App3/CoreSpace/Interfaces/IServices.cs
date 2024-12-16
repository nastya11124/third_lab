namespace App3.CoreSpace.Interfaces
{
    public interface IServices
    {
        public Task<bool> DeleteTrack(string artistName, string TrackName);
        public Task<bool> AddTrack(string artistName, string TrackName);
        public Task<Dictionary<string, List<string>>> SearchTrack(bool byAuthor, string criterion, int page, int pageSize);
        public Task<Dictionary<string, List<string>>> ShowTracks(int page, int pageSize);

    }
}