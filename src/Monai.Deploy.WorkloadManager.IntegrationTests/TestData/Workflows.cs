//using Monai.Deploy.WorkloadManager.IntegrationTests.Models;

//namespace Monai.Deploy.WorkloadManager.IntegrationTests.TestData
//{
//    public class WorkflowTestData
//    {
//        public string TestName { get; set; }

//        public Workflow Workflow { get; set; }
//    }

//    public static class Workflows
//    {
//        public static List<WorkflowTestData> WorkflowTestData = new List<WorkflowTestData>()
//        {
//            new WorkflowTestData
//            {
//                TestName = "WorkflowEvent_1",
//                Workflow = new Workflow
//                {
//                    Description = "WorkflowEvent_1_Description",
//                    Name = "WorkflowEvent_1_Name",
//                    Version = "WorkflowEvent_1_Version",
//                    InformaticsGateway = new InformaticsGateway
//                    {
//                        AeTitle = "WorkflowEvent_1_AeTitle",
//                        DataOrigins = new string[] {"WorkflowEvent_1_DataOrigins1", "WorkflowEvent_1_DataOrigins2" },
//                        ExportDestinations = new string[] { "WorkflowEvent_1_ExportDestinations1", "WorkflowEvent_1_ExportDestinations2" }
//                    },
//                    Tasks = new TaskObject[]
//                    {
//                        new TaskObject
//                        {
//                            Id = "",
//                            Description = "",
//                            Type = "",
//                            Args = "",
//                            Ref = "",
//                            TaskDestinations = new TaskDestination[]
//                            {
//                                new TaskDestination
//                                {
//                                    Name = "",
//                                    Conditions = new Evaluator[]
//                                    {
//                                        new Evaluator
//                                        {
//                                            CorrelationId = "",
//                                            Dicom = new ExecutionContext
//                                            {

//                                            },
//                                            Executions = "",
//                                            Input = ""
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        //ExportDestinations = new TaskDestination
//                        //{

//                        //},
//                        //Artifacts = new ArtifactMap
//                        //{

//                        //}
//                    }
//                }
//            }
//        };
//    }
//}
