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

using System;
using Mongo.Migration.Documents;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Common.Contracts.Migrations
{
    public class DocumentVersionConvert : JsonConverter
    {
        public override bool CanConvert(Type objectType) => GetType() == objectType;
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var major = 1;
            var minor = 0;
            var revision = 0;

            var res = (reader.Value as string)?.Split(new char[] { ',', '.' });

            if (res?.Length == 3)
            {
                major = Convert.ToInt32(res[0]);
                minor = Convert.ToInt32(res[1]);
                revision = Convert.ToInt32(res[2]);
            }
            return new DocumentVersion(major, minor, revision);

        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.ToString());
        }
    }

    public class DocumentVersionConverBson : IBsonSerializer
    {
        public Type ValueType => typeof(DocumentVersion);

        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var versionString = context.Reader.ReadString();
            return new DocumentVersion(versionString);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var versionObj = (DocumentVersion)value;
            var versionString = $"{versionObj.Major}.{versionObj.Minor}.{versionObj.Revision}";
            context.Writer.WriteString(versionString);
        }
    }
}
