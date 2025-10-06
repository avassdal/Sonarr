using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class AiEpisodeMatchingServiceFixture : TestBase<AiEpisodeMatchingService>
    {
        private Series _series;
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Id = 1)
                .With(s => s.Title = "Test Series")
                .Build();

            _episodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(e => e.SeriesId = _series.Id)
                .With(e => e.SeasonNumber = 1)
                .TheFirst(1).With(e => e.EpisodeNumber = 1).With(e => e.Title = "Pilot")
                .TheNext(1).With(e => e.EpisodeNumber = 2).With(e => e.Title = "Second Episode")
                .TheNext(1).With(e => e.EpisodeNumber = 3).With(e => e.Title = "Third Episode")
                .TheNext(1).With(e => e.EpisodeNumber = 4).With(e => e.Title = "Fourth Episode")
                .TheNext(1).With(e => e.EpisodeNumber = 5).With(e => e.Title = "Fifth Episode")
                .Build()
                .ToList();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodesBySeason(_series.Id, 1))
                .Returns(_episodes);

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodeBySeries(_series.Id))
                .Returns(_episodes);
        }

        [Test]
        public void should_return_empty_list_when_disabled()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.AiEpisodeMatchingEnabled)
                .Returns(false);

            var result = Subject.MatchEpisodes("Test.Series.S01E01.720p", _series, 1);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_return_empty_list_when_no_api_key()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.AiEpisodeMatchingEnabled)
                .Returns(true);

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.AiEpisodeMatchingApiKey)
                .Returns(string.Empty);

            var result = Subject.MatchEpisodes("Test.Series.S01E01.720p", _series, 1);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_return_empty_list_when_release_title_is_empty()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.AiEpisodeMatchingEnabled)
                .Returns(true);

            var result = Subject.MatchEpisodes(string.Empty, _series, 1);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_return_empty_list_when_series_is_null()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.AiEpisodeMatchingEnabled)
                .Returns(true);

            var result = Subject.MatchEpisodes("Test.Series.S01E01.720p", null, 1);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_check_is_enabled_property()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.AiEpisodeMatchingEnabled)
                .Returns(true);

            Subject.IsEnabled.Should().BeTrue();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.AiEpisodeMatchingEnabled)
                .Returns(false);

            Subject.IsEnabled.Should().BeFalse();
        }
    }
}
