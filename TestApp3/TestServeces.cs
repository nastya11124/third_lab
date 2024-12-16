using Microsoft.AspNetCore.Mvc;
using Moq;
using App3.CoreSpace;
using App3.CoreSpace.Interfaces;
using App3.Modals;

namespace TestApp3
{
    public class TestServeces
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly TrackServices _trackService;
        private readonly TracksController _controller;

        public TestServeces()
        {
            _mockRepository = new Mock<IRepository>();
            _trackService = new TrackServices(_mockRepository.Object);
            _controller = new TracksController(_trackService);
        }

        [Fact]
        public async Task DeleteTrack_ReturnsOk_WhenTrackIsDeleted()
        {
            // Arrange
            var request = new TrackRequest { ArtistName = "Artist", TrackName = "Track" };
            _mockRepository.Setup(repo => repo.CheckTrackExists(request.ArtistName, request.TrackName))
                .ReturnsAsync(true);
            _mockRepository.Setup(repo => repo.DeleteTrack(request.ArtistName, request.TrackName))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTrack(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Track deleted successfully", okResult.Value.GetType().GetProperty("message").GetValue(okResult.Value));
        }

        [Fact]
        public async Task DeleteTrack_ReturnsNotFound_WhenTrackIsNotFound()
        {
            // Arrange
            var request = new TrackRequest { ArtistName = "Artist", TrackName = "Track" };
            _mockRepository.Setup(repo => repo.CheckTrackExists(request.ArtistName, request.TrackName))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteTrack(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Track not found", notFoundResult.Value.GetType().GetProperty("error").GetValue(notFoundResult.Value));
        }

        [Fact]
        public async Task AddTrack_ReturnsOk_WhenTrackIsAdded()
        {
            // Arrange
            var request = new TrackRequest { ArtistName = "Artist", TrackName = "Track" };
            _mockRepository.Setup(repo => repo.CheckTrackExists(request.ArtistName, request.TrackName))
                .ReturnsAsync(false);
            _mockRepository.Setup(repo => repo.AddTrack(request.ArtistName, request.TrackName))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddTrack(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Track added successfully", okResult.Value.GetType().GetProperty("message").GetValue(okResult.Value));
        }

        [Fact]
        public async Task AddTrack_ReturnsConflict_WhenTrackAlreadyExists()
        {
            // Arrange
            var request = new TrackRequest { ArtistName = "Artist", TrackName = "Track" };
            _mockRepository.Setup(repo => repo.CheckTrackExists(request.ArtistName, request.TrackName))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddTrack(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Track already exists", conflictResult.Value.GetType().GetProperty("error").GetValue(conflictResult.Value));
        }

        [Fact]
        public async Task Search_ReturnsOk_WithTracks()
        {
            // Arrange
            var tracks = new Dictionary<string, List<string>>
            {
                { "Artist", new List<string> { "Track1", "Track2" } }
            };
            _mockRepository.Setup(repo => repo.Search(1, 10))
                .ReturnsAsync(tracks);

            // Act
            var result = await _controller.Search();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(tracks, okResult.Value);
        }

        [Fact]
        public async Task Search_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Search(1, 10))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Search();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Test exception", statusCodeResult.Value.GetType().GetProperty("error").GetValue(statusCodeResult.Value));
        }

        [Fact]
        public async Task SearchTrack_ReturnsOk_WithTracks()
        {
            // Arrange
            var tracks = new Dictionary<string, List<string>>
            {
                { "Artist", new List<string> { "Track1", "Track2" } }
            };
            _mockRepository.Setup(repo => repo.SearchTracks(true, "Artist", 1, 10))
                .ReturnsAsync(tracks);

            // Act
            var result = await _controller.searchTrack(Criterion.artist, "Artist");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(tracks, okResult.Value);
        }

        [Fact]
        public async Task SearchTrack_ReturnsNotFound_WhenNoMatchesFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.SearchTracks(true, "Artist", 1, 10))
                .ReturnsAsync(new Dictionary<string, List<string>>());

            // Act
            var result = await _controller.searchTrack(Criterion.artist, "Artist");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No matches found", notFoundResult.Value.GetType().GetProperty("error").GetValue(notFoundResult.Value));
        }

        [Fact]
        public async Task SearchTrack_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.SearchTracks(true, "Artist", 1, 10))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.searchTrack(Criterion.artist, "Artist");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Test exception", statusCodeResult.Value.GetType().GetProperty("error").GetValue(statusCodeResult.Value));
        }
    }
}
