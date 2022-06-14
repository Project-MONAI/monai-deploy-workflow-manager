// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
