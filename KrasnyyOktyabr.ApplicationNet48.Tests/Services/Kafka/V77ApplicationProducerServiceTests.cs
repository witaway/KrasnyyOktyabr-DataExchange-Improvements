﻿#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using KrasnyyOktyabr.ApplicationNet48.Models.Configuration.Kafka;
using KrasnyyOktyabr.ComV77Application;
using KrasnyyOktyabr.ComV77Application.Contracts.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static KrasnyyOktyabr.ApplicationNet48.Services.IJsonService;
using static KrasnyyOktyabr.ApplicationNet48.Services.IV77ApplicationLogService;
using static KrasnyyOktyabr.ApplicationNet48.Services.Kafka.V77ApplicationHelper;

namespace KrasnyyOktyabr.ApplicationNet48.Services.Kafka.Tests;

[TestClass]
public class V77ApplicationProducerServiceTests
{
    private static readonly Mock<IConfiguration> s_configurationMock = new();

    private static readonly Mock<ILogger<V77ApplicationProducerService>> s_loggerMock = new();

    private static readonly Mock<ILoggerFactory> s_loggerFactoryMock = new();

    private static readonly Mock<IOffsetService> s_offsetServiceMock = new();

    private static readonly Mock<IV77ApplicationLogService> s_logServiceMock = new();

    private static readonly Mock<IWmiService> s_wmiServiceMock = new();

    private static readonly Mock<IComV77ApplicationConnectionFactory> s_connectionFactoryMock = new();

    private static readonly Mock<IJsonService> s_jsonServiceMock = new();

    private static readonly Mock<IKafkaService> s_kafkaServiceMock = new();

    private static readonly V77ApplicationProducerService s_service = new(
        s_configurationMock.Object,
        s_loggerMock.Object,
        s_loggerFactoryMock.Object,
        s_offsetServiceMock.Object,
        s_logServiceMock.Object,
        s_wmiServiceMock.Object,
        s_connectionFactoryMock.Object,
        s_jsonServiceMock.Object,
        s_kafkaServiceMock.Object);

    private static V77ApplicationProducerSettings TestSettings => new()
    {
        InfobasePath = "TestInfobasePath",
        Username = "TestUser",
        Password = "TestPassword",
        ObjectFilters = [
            new V77ApplicationObjectFilter()
            {
                IdPrefix = "Id1",
                JsonDepth = 3
            },
            new V77ApplicationObjectFilter()
            {
                IdPrefix = "Id2",
                JsonDepth = 2
            }],
        TransactionTypeFilters = ["Type1", "Type2"],
        DataTypePropertyName = "TestDatatype",
        ErtRelativePath = "Erts/test.ert",
    };

    private static LogTransaction[] TestLogTransactions => [new LogTransaction(
        objectId: "FakeObjectId",
        objectName: "FakeObjectName",
        type: "FakeTransactionType"
    )];

    [TestMethod]
    public async Task GetLogTransactionsTask_ShouldGetLogTransactions()
    {
        V77ApplicationProducerSettings settings = TestSettings;

        // Setting up offset service mock
        Mock<IOffsetService> offsetServiceMock = new();
        offsetServiceMock
            .Setup(s => s.GetOffset(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("1234&FakeLastReadLine");

        // Setting up log service mock
        Mock<IV77ApplicationLogService> logServiceMock = new();
        long endPosition = 4321;
        string endLine = "NextFakeLastReadLine";
        GetLogTransactionsResult result = new(
            lastReadOffset: new(
                position: endPosition,
                lastReadLine: endLine
            ),
            transactions: [.. TestLogTransactions]
        );
        logServiceMock
            .Setup(s => s.GetLogTransactionsAsync(It.IsAny<string>(), It.IsAny<TransactionFilterWithCommit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Setting up logger mock
        Mock<ILogger> loggerMock = new();

        GetLogTransactionsResult logTransactions = await s_service.GetLogTransactionsTask(
            settings,
            [],
            offsetServiceMock.Object,
            logServiceMock.Object,
            loggerMock.Object,
            cancellationToken: default);

        Assert.AreEqual(1, logTransactions.Transactions.Count);
    }

    [TestMethod]
    public async Task GetObjectJsonsTask_ShouldGetObjectJsons()
    {
        V77ApplicationProducerSettings settings = TestSettings;
        LogTransaction[] logTransactions = TestLogTransactions;

        V77ApplicationObjectFilter objectFilter = new()
        {
            IdPrefix = "Id1",
            JsonDepth = 3
        };
        List<V77ApplicationObjectFilter> objectFilters = [objectFilter];

        // Setting up connection mock
        Mock<IComV77ApplicationConnection> connectionMock = new();
        connectionMock
            .Setup(c => c.RunErtAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}");

        // Setting up connection factory mock
        Mock<IComV77ApplicationConnectionFactory> connectionFactoryMock = new();
        connectionFactoryMock
            .Setup(f => f.GetConnectionAsync(It.IsAny<ConnectionProperties>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionMock.Object);

        // Setting up logger mock
        Mock<ILogger> loggerMock = new();

        List<string> objectJsons = await s_service.GetObjectJsonsTask(
            settings,
            logTransactions,
            objectFilters,
            connectionFactoryMock.Object,
            loggerMock.Object,
            cancellationToken: default);

        Assert.AreEqual(logTransactions.Length, objectJsons.Count);
    }

    [TestMethod]
    public async Task SendObjectJsonsTask_ShouldPrepareAndSendObjectJsons()
    {
        string dataTypeJsonPropertyName = "TestDatatype";
        V77ApplicationProducerSettings settings = new()
        {
            InfobasePath = @"D:\Bases\TestInfobase",
            Username = null!,
            Password = null!,
            ObjectFilters = [],
            TransactionTypeFilters = null!,
            DataTypePropertyName = dataTypeJsonPropertyName,
            ErtRelativePath = null!,
        };
        string objectDate = "01.01.2024";
        string transactionType = "TestTransactionType";
        List<LogTransaction> logTransactions = [new LogTransaction(
            objectId: null!,
            objectName: "FakeObjectName " + objectDate,
            type: transactionType
        )];
        Dictionary<string, object?> expectedPropertiesToAdd = new()
        {
            { TransactionTypePropertyName, transactionType },
            { ObjectDatePropertyName, objectDate },
        };
        string objectJson = "{}";
        List<string> objectJsons = [objectJson];

        // Setting up json service mock
        Mock<IJsonService> jsonServiceMock = new();
        jsonServiceMock
            .Setup(s => s.BuildKafkaProducerMessageData(It.IsAny<string>(), It.IsAny<Dictionary<string, object?>>(), It.IsAny<string>()))
            .Returns(new KafkaProducerMessageData(objectJson: "{\"TestObject\":\"TestValue\"}", dataType: null!));

        // Setting up kafka producer
        Mock<IProducer<string, string>> kafkaProducerMock = new();

        // Setting up kafka service mock
        Mock<IKafkaService> kafkaServiceMock = new();
        kafkaServiceMock
            .Setup(s => s.GetProducer<string, string>())
            .Returns(kafkaProducerMock.Object);
        string topicName = "TestTopicName";
        kafkaServiceMock
            .Setup(s => s.BuildTopicName(It.IsAny<string[]>()))
            .Returns(topicName);

        await s_service.SendObjectJsonsTask(
            settings,
            logTransactions,
            objectJsons,
            jsonServiceMock.Object,
            kafkaServiceMock.Object,
            cancellationToken: default);

        jsonServiceMock.Verify(s => s.BuildKafkaProducerMessageData(
            It.Is<string>(s => s == objectJson),
            It.Is<Dictionary<string, object?>>(d => d.ToString() == expectedPropertiesToAdd.ToString()),
            It.Is<string>(s => s == dataTypeJsonPropertyName)),
            Times.Once);
        kafkaServiceMock.Verify(s => s.BuildTopicName(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        kafkaProducerMock.Verify(p => p.ProduceAsync(
            It.Is<string>(t => t == topicName),
            It.IsAny<Message<string, string>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
