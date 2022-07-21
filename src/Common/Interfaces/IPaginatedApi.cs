// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkflowManager.Common.Interfaces
{
    public interface IPaginatedApi<T>
    {
        Task<long> CountAsync();

        Task<IList<T>> GetAllAsync(int? skip = null, int? limit = null);
    }
}
