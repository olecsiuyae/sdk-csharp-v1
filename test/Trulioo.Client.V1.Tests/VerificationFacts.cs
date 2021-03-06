﻿using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Trulioo.Client.V1.Model;
using Xunit;
using Record = Trulioo.Client.V1.Model.Record;

namespace Trulioo.Client.V1.Tests
{
    public class VerificationFacts
    {
        private readonly string _username = ConfigurationManager.AppSettings["username"];
        private readonly string _password = ConfigurationManager.AppSettings["password"];
        private readonly string _hostEndpoint = ConfigurationManager.AppSettings["host"];

        [Theory(Skip = "Calls API")]
        [MemberData(nameof(TransactionRecordVerboseTestData))]
        public async Task GetTransactionRecordVerbose(string transactionRecordID, TransactionRecordResult expectedTransactionRecordResult)
        {
            using (var client = new TruliooApiClient(new Context(_username, _password) { Host = _hostEndpoint }))
            {
                var response = await client.Verification.GetTransactionRecordVerboseAsync(transactionRecordID);

                Assert.Equal(expectedTransactionRecordResult.TransactionID, response.TransactionID);

                Assert.Equal(expectedTransactionRecordResult.Errors.Count(), response.Errors.Count());

                Assert.Equal(expectedTransactionRecordResult.InputFields.Count(), response.InputFields.Count());

                List<string> expectedInputFieldNames = expectedTransactionRecordResult.InputFields.Select(x => x.FieldName).ToList();
                List<string> actualInputFieldNames = response.InputFields.Select(x => x.FieldName).ToList();
                Assert.True(expectedInputFieldNames.All(actualInputFieldNames.Contains));

                List<string> expectedInputFieldValues = expectedTransactionRecordResult.InputFields.Select(x => x.Value).ToList();
                List<string> actualInputFieldValues = response.InputFields.Select(x => x.Value).ToList();
                Assert.True(expectedInputFieldValues.All(actualInputFieldValues.Contains));

                Assert.Equal(expectedTransactionRecordResult.Record.RecordStatus, response.Record.RecordStatus);
                Assert.Equal(expectedTransactionRecordResult.Record.DatasourceResults.Count(), response.Record.DatasourceResults.Count());
            }
        }

        public static IEnumerable<object[]> TransactionRecordVerboseTestData()
        {
            yield return new object[] {"transactionId", new TransactionRecordResult()
            {
                InputFields = new List<DataField>() { new DataField { FieldName = "", Value= "" }},
                TransactionID = "",
                Record = new Record()
                {
                    RecordStatus = "",
                    DatasourceResults = new List<DatasourceResult>()
                    {
                        new DatasourceResult()
                        {
                            DatasourceName = "",
                            DatasourceFields = new List<DatasourceField>() { new DatasourceField() { FieldName = "", Status = "" },
                                                                            new DatasourceField {FieldName = "", Status = "" } }
                        }
                    }
                },
                Errors = new List<Model.Errors.ServiceError> { }
            }
            };
        }

        [Theory(Skip = "Calls API")]
        [MemberData(nameof(TransactionStatusTestData))]
        public async Task GetTransactionStatus(string transactionID, TransactionStatus expectedTransactionStatus)
        {
            using (var client = new TruliooApiClient(new Context(_username, _password) { Host = _hostEndpoint }))
            {
                var response = await client.Verification.GetTransactionStatusAsync(transactionID);
                Assert.Equal(expectedTransactionStatus.Status, response.Status);
                Assert.Equal(expectedTransactionStatus.TransactionId, response.TransactionId);
                Assert.Equal(expectedTransactionStatus.TransactionRecordId, response.TransactionRecordId);
                Assert.Equal(expectedTransactionStatus.IsTimedOut, response.IsTimedOut);
            }
        }

        public static IEnumerable<object[]> TransactionStatusTestData()
        {
            yield return new object[] {"transactionId", new TransactionStatus()
            {
                TransactionId = "",
                TransactionRecordId = "",
                Status = "",
                IsTimedOut = false
            } };
        }

        // Tests if ApiClient throws Exception when parsing WatchListDetails in AppendedField
        // This needs an account which has ComplyAdvantage V3 configured in Germany (or any country)
        [Fact(Skip = "Calls API")]
        public async Task WatchlistDetailsResponseNoExceptionThrownTest_TECH9_103()
        {
            using (var client = new TruliooApiClient(new Context(_username, _password) { Host = _hostEndpoint }))
            {
                var request = new VerifyRequest
                {
                    AcceptTruliooTermsAndConditions = true,
                    VerboseMode = true,
                    Demo = false,
                    ConfigurationName = "Identity Verification",
                    CountryCode = "DE",
                    DataFields = new DataFields
                    {
                        PersonInfo = new PersonInfo
                        {
                            FirstGivenName = "test",
                            FirstSurName = "test",
                            YearOfBirth = 1980
                        }
                    }
                };

                var response = await client.Verification.VerifyAsync(request);
                Assert.NotNull(response);
            }
        }

        [Theory(Skip = "Calls API")]
        [MemberData(nameof(DownloadDocData))]
        public async Task GetDocumentDownload(string transactionRecordId, string fieldName)
        {
            using (var client = new TruliooApiClient(new Context(_username, _password) { Host = _hostEndpoint }))
            {
                var response = await client.Verification.GetDocumentDownload(transactionRecordId, fieldName);
                Assert.NotNull(response);
            }
        }

        public static IEnumerable<object[]> DownloadDocData()
        {
            yield return new object[] { "transactionRecordId", "fieldName" };
        }
    }
}
