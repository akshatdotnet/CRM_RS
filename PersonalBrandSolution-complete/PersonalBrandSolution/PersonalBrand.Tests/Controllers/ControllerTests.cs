using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PersonalBrand.API.Controllers;
using PersonalBrand.API.Services.Interfaces;
using PersonalBrand.Shared.DTOs;
using PersonalBrand.Shared.Models;
using Xunit;

namespace PersonalBrand.Tests.Controllers;

// ══════════════════════════════════════════════════════
//  PERSONA CONTROLLER TESTS
// ══════════════════════════════════════════════════════
public class PersonaControllerTests
{
    private static PersonaController MakeController(PersonaDto? returnValue)
    {
        var mock = new Mock<IPersonaService>();
        mock.Setup(s => s.GetPersonaAsync()).ReturnsAsync(returnValue);
        return new PersonaController(mock.Object);
    }

    [Fact]
    public async Task Get_Returns200_WithPersona()
    {
        // Arrange
        var persona = new PersonaDto { Name = "Arjun Sharma", Experience = 15 };
        var ctrl = MakeController(persona);

        // Act
        var result = await ctrl.Get();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        body.Success.Should().BeTrue();
        body.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_Returns404_WhenPersonaNotFound()
    {
        // Arrange
        var ctrl = MakeController(null);

        // Act
        var result = await ctrl.Get();

        // Assert
        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var body = notFound.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        body.Success.Should().BeFalse();
        body.StatusCode.Should().Be(404);
    }
}

// ══════════════════════════════════════════════════════
//  LEADS CONTROLLER TESTS
// ══════════════════════════════════════════════════════
public class LeadsControllerTests
{
    private static (Mock<ILeadService> mock, LeadsController ctrl) Setup()
    {
        var mock = new Mock<ILeadService>();
        return (mock, new LeadsController(mock.Object));
    }

    [Fact]
    public async Task GetPipeline_Returns200_WithSummary()
    {
        // Arrange
        var (mock, ctrl) = Setup();
        mock.Setup(s => s.GetPipelineSummaryAsync())
            .ReturnsAsync(new PipelineSummaryDto { TotalLeads = 5 });

        // Act
        var result = await ctrl.GetPipeline();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        body.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Create_Returns201_WithValidPayload()
    {
        // Arrange
        var (mock, ctrl) = Setup();
        var dto = TestData.MakeContactForm();
        mock.Setup(s => s.CreateLeadAsync(dto))
            .ReturnsAsync(new LeadResponseDto { Id = 1, Name = dto.Name, Status = "new" });

        // Act
        var result = await ctrl.Create(dto);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_Returns400_WhenNameEmpty()
    {
        // Arrange
        var (_, ctrl) = Setup();
        var dto = TestData.MakeContactForm(name: "");

        // Act
        var result = await ctrl.Create(dto);

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var body = bad.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        body.Success.Should().BeFalse();
        body.Errors.Should().Contain(e => e.Contains("required") || e.Contains("Name"));
    }

    [Fact]
    public async Task Create_Returns400_WithInvalidEmail()
    {
        // Arrange
        var (_, ctrl) = Setup();
        var dto = TestData.MakeContactForm(email: "not-an-email");

        // Act
        var result = await ctrl.Create(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddNote_Returns200_WithValidNote()
    {
        // Arrange
        var (mock, ctrl) = Setup();
        mock.Setup(s => s.AddNoteAsync(1, "Test note")).Returns(Task.CompletedTask);

        // Act
        var result = await ctrl.AddNote(1, new AddNoteDto { Note = "Test note" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        mock.Verify(s => s.AddNoteAsync(1, "Test note"), Times.Once);
    }

    [Fact]
    public async Task AddNote_Returns400_WhenNoteEmpty()
    {
        // Arrange
        var (_, ctrl) = Setup();

        // Act
        var result = await ctrl.AddNote(1, new AddNoteDto { Note = "" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("new")]
    [InlineData("contacted")]
    [InlineData("proposal")]
    [InlineData("negotiation")]
    [InlineData("closed")]
    [InlineData("lost")]
    public async Task UpdateStatus_Returns200_ForAllValidStatuses(string status)
    {
        // Arrange
        var (mock, ctrl) = Setup();
        mock.Setup(s => s.UpdateStatusAsync(1, status)).Returns(Task.CompletedTask);

        // Act
        var result = await ctrl.UpdateStatus(1, new UpdateLeadStatusDto { Status = status });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateStatus_Returns400_ForInvalidStatus()
    {
        // Arrange
        var (_, ctrl) = Setup();

        // Act
        var result = await ctrl.UpdateStatus(1, new UpdateLeadStatusDto { Status = "invalid_status" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_Returns404_WhenLeadNotFound()
    {
        // Arrange
        var (mock, ctrl) = Setup();
        mock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((LeadResponseDto?)null);

        // Act
        var result = await ctrl.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}

// ══════════════════════════════════════════════════════
//  PROJECTS CONTROLLER TESTS
// ══════════════════════════════════════════════════════
public class ProjectsControllerTests
{
    [Fact]
    public async Task GetAll_Returns200_WithAllProjects()
    {
        // Arrange
        var mock = new Mock<IProjectService>();
        mock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<ProjectDto> {
                new() { Id = 1, Title = "Banking Platform" },
                new() { Id = 2, Title = "Healthcare Portal" }
            });
        var ctrl = new ProjectsController(mock.Object);

        // Act
        var result = await ctrl.GetAll(featured: null);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsFeaturedOnly_WhenFlagSet()
    {
        // Arrange
        var mock = new Mock<IProjectService>();
        mock.Setup(s => s.GetFeaturedAsync())
            .ReturnsAsync(new List<ProjectDto> { new() { Id = 1, IsFeatured = true } });
        var ctrl = new ProjectsController(mock.Object);

        // Act
        await ctrl.GetAll(featured: true);

        // Assert
        mock.Verify(s => s.GetFeaturedAsync(), Times.Once);
        mock.Verify(s => s.GetAllAsync(), Times.Never);
    }
}

// ══════════════════════════════════════════════════════
//  QA CONTROLLER TESTS
// ══════════════════════════════════════════════════════
public class QAControllerTests
{
    [Fact]
    public async Task GetAll_UsesSearch_WhenSearchParamProvided()
    {
        // Arrange
        var mock = new Mock<IQAService>();
        mock.Setup(s => s.SearchAsync("CQRS"))
            .ReturnsAsync(new List<QAItemDto> { new() { Id = 1, Question = "What is CQRS?" } });
        var ctrl = new QAController(mock.Object);

        // Act
        await ctrl.GetAll(level: null, search: "CQRS");

        // Assert
        mock.Verify(s => s.SearchAsync("CQRS"), Times.Once);
        mock.Verify(s => s.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAll_FiltersByLevel_WhenLevelProvided()
    {
        // Arrange
        var mock = new Mock<IQAService>();
        mock.Setup(s => s.GetByLevelAsync("advanced"))
            .ReturnsAsync(new List<QAItemDto>());
        var ctrl = new QAController(mock.Object);

        // Act
        await ctrl.GetAll(level: "advanced", search: null);

        // Assert
        mock.Verify(s => s.GetByLevelAsync("advanced"), Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsAll_WhenNoFilters()
    {
        // Arrange
        var mock = new Mock<IQAService>();
        mock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<QAItemDto>());
        var ctrl = new QAController(mock.Object);

        // Act
        await ctrl.GetAll(level: null, search: null);

        // Assert
        mock.Verify(s => s.GetAllAsync(), Times.Once);
    }
}

// ══════════════════════════════════════════════════════
//  BLOG CONTROLLER TESTS
// ══════════════════════════════════════════════════════
public class BlogControllerTests
{
    [Fact]
    public async Task GetAll_Returns400_WhenPageSizeExceeds50()
    {
        // Arrange
        var mock = new Mock<IBlogService>();
        var ctrl = new BlogController(mock.Object);

        // Act
        var result = await ctrl.GetAll(page: 1, pageSize: 51);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAll_Returns400_WhenPageIsZero()
    {
        // Arrange
        var mock = new Mock<IBlogService>();
        var ctrl = new BlogController(mock.Object);

        // Act
        var result = await ctrl.GetAll(page: 0, pageSize: 10);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetBySlug_Returns404_WhenSlugNotFound()
    {
        // Arrange
        var mock = new Mock<IBlogService>();
        mock.Setup(s => s.GetBySlugAsync("nonexistent-slug"))
            .ReturnsAsync((BlogPostDto?)null);
        var ctrl = new BlogController(mock.Object);

        // Act
        var result = await ctrl.GetBySlug("nonexistent-slug");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}

// ══════════════════════════════════════════════════════
//  NEWSLETTER CONTROLLER TESTS
// ══════════════════════════════════════════════════════
public class NewsletterControllerTests
{
    [Fact]
    public async Task Subscribe_Returns200_ForValidEmail()
    {
        // Arrange
        var mock = new Mock<INewsletterService>();
        mock.Setup(s => s.SubscribeAsync("test@email.com")).ReturnsAsync(true);
        var ctrl = new NewsletterController(mock.Object);

        // Act
        var result = await ctrl.Subscribe(new NewsletterDto { Email = "test@email.com" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Subscribe_Returns400_WhenEmailEmpty()
    {
        // Arrange
        var mock = new Mock<INewsletterService>();
        var ctrl = new NewsletterController(mock.Object);

        // Act
        var result = await ctrl.Subscribe(new NewsletterDto { Email = "" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        mock.Verify(s => s.SubscribeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Subscribe_Returns200_WithAlreadySubscribedMessage()
    {
        // Arrange
        var mock = new Mock<INewsletterService>();
        mock.Setup(s => s.SubscribeAsync("existing@email.com")).ReturnsAsync(false);
        var ctrl = new NewsletterController(mock.Object);

        // Act
        var result = await ctrl.Subscribe(new NewsletterDto { Email = "existing@email.com" });

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        body.Message.Should().Contain("Already");
    }
}

// ══════════════════════════════════════════════════════
//  HEALTH CONTROLLER TESTS
// ══════════════════════════════════════════════════════
public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsHealthyStatus()
    {
        // Arrange
        var ctrl = new HealthController();

        // Act
        var result = ctrl.Get();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
        ok.Value!.ToString().Should().Contain("healthy");
    }
}
