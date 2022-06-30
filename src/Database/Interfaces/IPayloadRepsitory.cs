using System.Collections.Generic;
using System.Threading.Tasks;
using Monai.Deploy.WorkflowManager.Contracts.Models;

namespace Monai.Deploy.WorkflowManager.Database.Interfaces
{
    public interface IPayloadRepsitory
    {
        /// <summary>
        /// Creates a payload in the database.
        /// </summary>
        /// <param name="payload">A payload to create.</param>
        Task<bool> CreateAsync(Payload payload);

        /// <summary>
        /// Retrieves a list of payloads in the database.
        /// </summary>
        Task<IList<Payload>> GetAllAsync();

        /// <summary>
        /// Retrieves a payload by id in the database.
        /// </summary>
        /// <param name="payloadId">A payloadId to retrieve.</param>
        Task<Payload> GetByIdAsync(string payloadId);
    }
}
