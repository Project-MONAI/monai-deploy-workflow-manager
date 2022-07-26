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

using Monai.Deploy.Messaging.Events;

namespace Monai.Deploy.WorkflowManager.TaskManager.API
{
    public abstract class MetadataRepositoryBase : IMetadataRepository
    {
        protected bool DisposedValue { get; private set; }

        protected TaskDispatchEvent DispatchEvent { get; }
        protected TaskCallbackEvent CallbackEvent { get; }

        protected MetadataRepositoryBase(TaskDispatchEvent taskDispatchEvent, TaskCallbackEvent taskCallbackEvent)
        {
            DispatchEvent = taskDispatchEvent ?? throw new ArgumentNullException(nameof(taskDispatchEvent));
            CallbackEvent = taskCallbackEvent ?? throw new ArgumentNullException(nameof(taskCallbackEvent));
        }

        public abstract Task<Dictionary<string, object>> RetrieveMetadata(CancellationToken cancellationToken = default);

        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                DisposedValue = true;
            }
        }

        ~MetadataRepositoryBase() => Dispose(disposing: false);

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
