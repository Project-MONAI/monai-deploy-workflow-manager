# Copyright 2021 MONAI Consortium
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#     http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

SCRIPT_DIR=$(dirname "$(readlink -f "$0")")

VERSION=$(cat $SCRIPT_DIR/VERSION)
FILEVERSION=$VERSION

# pass in pre-releae tags as argument
if [ ! -z "$1" ]; then
    VERSION=$VERSION-$1
fi 


echo "Building Workload Manager Docker Image. VERSION=$VERSION, FILEVERSION=$FILEVERSION"
pushd $SCRIPT_DIR
docker build --tag monai/workload-manager:$VERSION --build-arg Version=$VERSION --build-arg FileVersion=$FILEVERSION . 
popd

pushd $SCRIPT_DIR/src/CLI
echo "Building Workload Manager CLI: linux-x64"
dotnet publish -r linux-x64 -c Release -o $SCRIPT_DIR/cli/linux-x64
popd
