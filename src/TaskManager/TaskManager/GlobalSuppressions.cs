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

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "this is a test")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "this is a test", Scope = "member", Target = "~P:Monai.Deploy.WorkflowManager.TaskManager.TaskManager.Status")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "this is a test", Scope = "member", Target = "~P:Monai.Deploy.WorkflowManager.TaskManager.TaskManager.ServiceName")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "this is a test", Scope = "member", Target = "~M:Monai.Deploy.WorkflowManager.TaskManager.TaskManager.#ctor(Microsoft.Extensions.Logging.ILogger{Monai.Deploy.WorkflowManager.TaskManager.TaskManager},Microsoft.Extensions.Options.IOptions{Monai.Deploy.WorkflowManager.Configuration.WorkflowManagerOptions},Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "this is a test")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "this is a test", Scope = "type", Target = "~T:Monai.Deploy.WorkflowManager.TaskManager.ApplicationPartsLogger")]
