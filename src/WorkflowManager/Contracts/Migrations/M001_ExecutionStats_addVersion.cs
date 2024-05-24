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

using Monai.Deploy.WorkflowManager.Common.Contracts.Models;
using Mongo.Migration.Migrations.Document;
using MongoDB.Bson;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Migrations
{
    public class M001_ExecutionStats_addVersion : DocumentMigration<ExecutionStats>
    {
        public M001_ExecutionStats_addVersion() : base("1.0.1") { }

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
            catch
            {  // can ignore we dont want failures stopping startup !
            }
        }
    }
}
