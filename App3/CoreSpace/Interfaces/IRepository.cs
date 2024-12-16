namespace App3.CoreSpace.Interfaces
{
    public interface IRepository
    {
        public Task<bool> CheckTrackExists(string title, string artistName);
        public Task<bool> DeleteTrack(string title, string artistName);
        public Task<bool> AddTrack(string title, string artistName);
        public Task<Dictionary<string, List<string>>> SearchTracks(bool byAuthor, string criterion, int page, int pageSize);
        public Task<bool> HasMoreResults(bool byAuthor, string criterion, int page, int pageSize);
        public Task<Dictionary<string, List<string>>> Search(int page, int pageSize);
    }

}