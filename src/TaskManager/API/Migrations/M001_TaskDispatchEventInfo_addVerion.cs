/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using Mongo.Migration.Migrations.Document;
using MongoDB.Bson;

namespace Monai.Deploy.WorkflowManager.TaskManager.Migrations
{
    public class M001_TaskDispatchEventInfo_addVerion : DocumentMigration<TaskDispatchEventInfo>
    {
        public M001_TaskDispatchEventInfo_addVerion() : base("1.0.0") { }

        public override void Up(BsonDocument document)
        {
            // empty, but this will make all objects re-saved with a version
        }
        public override void Down(BsonDocument document)
        {
            try
            {
                document.Remove("Version");
            }
            catch (Exception)
            {
            }
        }
    }
}
