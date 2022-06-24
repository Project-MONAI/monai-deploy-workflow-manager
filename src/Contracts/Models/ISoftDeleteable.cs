using System;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public interface ISoftDeleteable
    {
        DateTime? Deleted { get; set; }

        bool IsDeleted { get => Deleted is not null; }
    }
}
