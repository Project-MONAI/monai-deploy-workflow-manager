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

using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Mongo.Migration.Migrations.Document;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Migrations
{
    public class M003_Payload_addDataTrigger : DocumentMigration<Payload>
    {
        public M003_Payload_addDataTrigger() : base("1.0.3") { }

        public override void Up(BsonDocument document)
        {
            var calling = document.GetValue("CallingAeTitle").AsString;
            var called = document.GetValue("CalledAeTitle").AsString;
            document.Remove("CallingAeTitle");
            document.Remove("CalledAeTitle");
            var trigger = new DataOrigin { DataService = DataService.DIMSE, Source = calling, Destination = called };
            document.Add("DataTrigger", trigger.ToJson(), true);
        }

        public override void Down(BsonDocument document)
        {
            try
            {
                var trigger = BsonSerializer.Deserialize<DataOrigin>(document.GetValue("DataTrigger").ToBson());
                document.Remove("DataTrigger");
                document.Add("CallingAeTitle", trigger.Source);
                document.Add("CalledAeTitle", trigger.Destination);
            }
            catch
            {  // can ignore we dont want failures stopping startup !
            }
        }
    }
}
