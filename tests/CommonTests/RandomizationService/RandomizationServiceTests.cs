using Castle.Core.Logging;
using DiscordImagePoster.Common.IndexService;
using DiscordImagePoster.Common.RandomizationService;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Common;

public class RandomizationServiceTests
{
    private RandomizationService _randomizationService;

    [SetUp]
    public void Setup()
    {
        ILogger<RandomizationService> mockLogger = Substitute.For<ILogger<RandomizationService>>();

        _randomizationService = new RandomizationService(mockLogger);

    }

    [Test]
    public void GetRandomImage_ForEmptyCollection_ReturnsNull()
    {
        var imageIndex = new ImageIndex()
        {
            RefreshedAt = DateTimeOffset.UtcNow,
            Images = new List<ImageIndexMetadata>()
        };

        var result = _randomizationService.GetRandomImage(imageIndex);

        result.Should().BeNull();
    }


    [Test]
    public void GetRandomImage_ForIgnoredImages_ReturnsNull()
    {
        var imageIndex = new ImageIndex()
        {
            RefreshedAt = DateTimeOffset.UtcNow,
            Images = new List<ImageIndexMetadata>
            {
                new ImageIndexMetadata
                {
                    Ignore = true,
                    AddedAt = DateTime.UtcNow,
                    Description = null,
                    LastPostedAt = null,
                    TimesPosted = 0,
                    Name = "Test Image"
                }
            }
        };

        var result = _randomizationService.GetRandomImage(imageIndex);

        result.Should().BeNull();
    }

    [Test]
    public void GetRandomImage_ForSomeImage_ReturnsImage()
    {
        var imageIndex = new ImageIndex()
        {
            RefreshedAt = DateTimeOffset.UtcNow,
            Images = new List<ImageIndexMetadata>
            {
                new ImageIndexMetadata
                {
                    Ignore = false,
                    AddedAt = DateTime.UtcNow,
                    Description = null,
                    LastPostedAt = null,
                    TimesPosted = 27,
                    Name = "Test Image"
                }
            }
        };

        var result = _randomizationService.GetRandomImage(imageIndex);

        result.Should().BeEquivalentTo(imageIndex.Images.First());
    }

    [Test]
    public void GetRandomImage_TwoImages_ReturnsLessPostedImage()
    {
        var imageIndex = new ImageIndex()
        {
            RefreshedAt = DateTimeOffset.UtcNow,
            Images = new List<ImageIndexMetadata>
            {
                new ImageIndexMetadata
                {
                    Ignore = false,
                    AddedAt = DateTime.UtcNow,
                    Description = null,
                    LastPostedAt = null,
                    TimesPosted = 27,
                    Name = "Test Image 1"
                },
                new ImageIndexMetadata
                {
                    Ignore = false,
                    AddedAt = DateTime.UtcNow,
                    Description = null,
                    LastPostedAt = null,
                    TimesPosted = 25,
                    Name = "Test Image 2"
                }
            }
        };

        for (var i = 0; i < 10; i++)
        {
            var result = _randomizationService.GetRandomImage(imageIndex);
            result.Should().BeEquivalentTo(imageIndex.Images.Last());
        }
    }
}