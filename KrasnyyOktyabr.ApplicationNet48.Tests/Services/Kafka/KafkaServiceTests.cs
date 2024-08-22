using System.Collections.Generic;
using KrasnyyOktyabr.ApplicationNet48.Modules.Kafka.Services;
using KrasnyyOktyabr.ApplicationNet48.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace KrasnyyOktyabr.ApplicationNet48.Tests.Services.Kafka;

[TestClass]
public class KafkaServiceTests
{
    private static Mock<ITransliterationService>? s_transliterationServiceMock;

    private static Mock<ILogger<KafkaService>>? s_loggerMock;

    private static KafkaService? s_kafkaService;

    private static Dictionary<string, string?> TestKafkaSettings => new()
    {
        { "Kafka:Socket", "TestSocketValue" }
    };

    [TestInitialize]
    public void TestInitialize()
    {
        s_transliterationServiceMock = new();

        s_loggerMock = new();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(TestKafkaSettings)
            .Build();

        s_kafkaService = new(configuration, s_transliterationServiceMock.Object, s_loggerMock.Object);
    }

    [TestMethod]
    public void BuildTopicName_ShouldBuildTopicName()
    {
        s_transliterationServiceMock
            !.Setup(s => s.TransliterateToLatin(It.IsAny<string>()))
            .Returns<string>(source => source);

        Assert.AreEqual("name1_name2", s_kafkaService!.BuildTopicName("name1", "name2"));
    }
}
