// 
// Copyright 2023 Guy’s and St Thomas’ NHS Foundation Trust
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Mongo.Migration.Migrations.Document;
using MongoDB.Bson;


namespace Monai.Deploy.WorkflowManager.Common.Contracts.Migrations
{
    public class M004_WorkflowRevision_addConditions : DocumentMigration<WorkflowRevision>
    {
        public M004_WorkflowRevision_addConditions() : base("1.0.4") { }

        public override void Up(BsonDocument document)
        {
            try
            {
                var workflow = document["Workflow"].AsBsonDocument;
                workflow.Add("Predicate", new BsonArray { });
            }
            catch
            { // can ignore we dont want failures stopping startup !
            }

        }

        public override void Down(BsonDocument document)
        {
            try
            {
                var workflow = document["Workflow"].AsBsonDocument;
                workflow.Remove("Predicate");
            }
            catch
            {  // can ignore we dont want failures stopping startup !
            }
        }
    }
}
