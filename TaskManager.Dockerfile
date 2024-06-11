# Copyright 2022 MONAI Consortium
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#     http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy as build

# Install the tools
RUN dotnet tool install --tool-path /tools dotnet-trace
RUN dotnet tool install --tool-path /tools dotnet-dump
RUN dotnet tool install --tool-path /tools dotnet-counters
RUN dotnet tool install --tool-path /tools dotnet-stack
WORKDIR /app
COPY . ./

RUN echo "Building MONAI Workflow Manager..."
RUN dotnet publish -c Release -o out --nologo src/TaskManager/TaskManager/Monai.Deploy.WorkflowManager.TaskManager.csproj

RUN echo "Fetching mc executable for minio..."
RUN wget -O mc https://dl.min.io/client/mc/release/linux-amd64/archive/mc.RELEASE.2024-06-10T16-44-15Z
RUN chmod +x mc

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get clean \
 && apt-get update \
 && apt-get install -y --no-install-recommends \
    curl \
   && rm -rf /var/lib/apt/lists

WORKDIR /opt/monai/wm

COPY --from=build /app/out .
COPY --from=build /tools /opt/dotnetcore-tools
COPY --from=build /app/mc /usr/local/bin/mc
COPY docs/compliance/third-party-licenses.md .

EXPOSE 5000

RUN ls -lR /opt/monai/wm
ENV PATH="/opt/dotnetcore-tools:${PATH}"

ENTRYPOINT ["/opt/monai/wm/Monai.Deploy.WorkflowManager.TaskManager"]
