// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.WorkloadManager.Contracts.Rest
{
    /// <summary>
    /// Defines the state of a running service.
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// Unknown - default, during start up.
        /// </summary>
        Unknown,

        /// <summary>
        /// Service is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// Service is running.
        /// </summary>
        Running,

        /// <summary>
        /// Service has been cancelled by a cancellation token.
        /// </summary>
        Cancelled,

        /// <summary>
        /// Service has been disposed by the host container.
        /// </summary>
        Disposed,
    }
}
