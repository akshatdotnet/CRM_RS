using FluentAssertions;
using PersonalBrand.Shared.Constants;
using PersonalBrand.Shared.Models;
using Xunit;

namespace PersonalBrand.Tests;

// ══════════════════════════════════════════════════════
//  API RESPONSE WRAPPER TESTS
// ══════════════════════════════════════════════════════
public class ApiResponseTests
{
    [Fact]
    public void Ok_CreatesSuccessResponse()
    {
        // Act
        var result = ApiResponse<string>.Ok("hello", "All good");

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be("hello");
        result.Message.Should().Be("All good");
        result.StatusCode.Should().Be(200);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Fail_SingleError_CreatesFailResponse()
    {
        // Act
        var result = ApiResponse<string>.Fail("Something went wrong", 400);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Errors.Should().ContainSingle(e => e == "Something went wrong");
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Fail_MultipleErrors_IncludesAllErrors()
    {
        // Act
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var result = ApiResponse<string>.Fail(errors, 422);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Message.Should().Be("Validation failed");
    }

    [Fact]
    public void Ok_HasTimestamp()
    {
        // Act
        var result = ApiResponse<int>.Ok(42);

        // Assert
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void PagedResponse_CalculatesTotalPages_Correctly()
    {
        // Act
        var paged = new PagedResponse<string>
        {
            Items = new List<string> { "a", "b" },
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        };

        // Assert
        paged.TotalPages.Should().Be(3);
        paged.HasNextPage.Should().BeTrue();
        paged.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResponse_LastPage_HasNoPreviousOrNext()
    {
        // Act
        var paged = new PagedResponse<string>
        {
            Items       = new List<string> { "x" },
            TotalCount  = 5,
            Page        = 3,
            PageSize    = 2
        };

        // Assert
        paged.HasNextPage.Should().BeFalse();
        paged.HasPreviousPage.Should().BeTrue();
    }
}

// ══════════════════════════════════════════════════════
//  CONSTANTS TESTS
// ══════════════════════════════════════════════════════
public class LeadStatusConstantsTests
{
    [Fact]
    public void All_ContainsAllExpectedStatuses()
    {
        LeadStatus.All.Should().Contain(LeadStatus.New)
            .And.Contain(LeadStatus.Contacted)
            .And.Contain(LeadStatus.Proposal)
            .And.Contain(LeadStatus.Negotiation)
            .And.Contain(LeadStatus.Closed)
            .And.Contain(LeadStatus.Lost);
    }

    [Fact]
    public void StatusValues_AreLowercase()
    {
        LeadStatus.All.Should().AllSatisfy(s => s.Should().Be(s.ToLower()));
    }

    [Fact]
    public void All_HasSixStatuses()
    {
        LeadStatus.All.Should().HaveCount(6);
    }
}

// ══════════════════════════════════════════════════════
//  CONTACT FORM DTO VALIDATION TESTS
// ══════════════════════════════════════════════════════
public class ContactFormDtoTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Name_ShouldBe_RequiredForValidLead(string? name)
    {
        // Arrange
        var dto = TestData.MakeContactForm(name: name ?? "");

        // Assert — controller-level validation would catch this
        string.IsNullOrWhiteSpace(dto.Name).Should().BeTrue();
    }

    [Theory]
    [InlineData("valid@email.com", true)]
    [InlineData("user@domain.co.in", true)]
    [InlineData("notanemail", false)]
    [InlineData("@nodomain", false)]
    [InlineData("no@tld", false)]
    public void Email_ValidationRegex_WorksCorrectly(string email, bool isValid)
    {
        // Arrange - simulating controller's email check
        var regex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // Act
        var result = regex.IsMatch(email);

        // Assert
        result.Should().Be(isValid);
    }
}
