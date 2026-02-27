using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IndicoToolkit.Tests.Results;


public class DocumentTests
{
    [Fact]
    public void TestEmptySections()
    {
        var result = Result.FromJson(JObject.Parse(@"
            {
                ""file_version"": 3,
                ""submission_id"": 0,
                ""modelgroup_metadata"": {
                    ""123"": {
                        ""id"": 123,
                        ""task_type"": ""annotation"",
                        ""name"": ""Empty Model Section"",
                        ""selected_model"": {
                            ""id"": 123,
                            ""model_type"": ""finetune""
                        }
                    }
                },
                ""component_metadata"": {
                    ""456"": {
                        ""id"": 456,
                        ""component_type"": ""static_model"",
                        ""task_type"": ""annotation"",
                        ""name"": ""Empty Model Section""
                    }
                },
                ""submission_results"": [
                    {
                        ""submissionfile_id"": 0,
                        ""etl_output"": """",
                        ""input_filename"": """",
                        ""model_results"": {
                            ""ORIGINAL"": {
                                ""123"": []
                            }
                        },
                        ""component_results"": {
                            ""ORIGINAL"": {
                                ""456"": []
                            }
                        }
                    }
                ],
                ""reviews"": {
                },
                ""errored_files"": {
                }
            }
        "));

        var expected = JArray.Parse(@"
            [
                {
                    ""submissionfile_id"": 0,
                    ""model_results"": {
                        ""123"": [],
                    },
                    ""component_results"": {
                        ""456"": [],
                    },
                }
            ]
        ");

        Assert.True(JToken.DeepEquals(expected, result.Predictions.ToChanges(result)));
    }
}
