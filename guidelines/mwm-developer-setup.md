<!--
  ~ Copyright 2022 MONAI Consortium
  ~
  ~ Licensed under the Apache License, Version 2.0 (the "License");
  ~ you may not use this file except in compliance with the License.
  ~ You may obtain a copy of the License at
  ~
  ~ http://www.apache.org/licenses/LICENSE-2.0
  ~
  ~ Unless required by applicable law or agreed to in writing, software
  ~ distributed under the License is distributed on an "AS IS" BASIS,
  ~ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  ~ See the License for the specific language governing permissions and
  ~ limitations under the License.
-->

# Developer local setup

## assumptions / prerequisites
- kubernetes running locally either via Docker desktop or Kind/ microk8s (or similar) 
- python 3 installed.
- Helm 3 https://helm.sh/docs/intro/install/
- rabbitmqadmin https://www.rabbitmq.com/management-cli.html
- mc.exe https://github.com/minio/mc install and add its location to the storage_settings_executableLocation setting (appsettings.local.json) including the name itself ! ie `mc.exe` if its in the folder of the running executable (\bin\Debug\net8.0). (for TaskManager)

Note. if you already have docker container for Minio Rabbit etc running Stop these.

### steps
- following the qickstart here https://argoproj.github.io/argo-workflows/quick-start/ but change to namespace to suit. ie 

  - `kubectl create ns argo`
  - `kubectl apply -n argo -f https://raw.githubusercontent.com/argoproj/argo-workflows/master/manifests/quick-start-postgres.yaml`
  - `kubectl config set-context --current --namespace=argo`

To disable argo authentication run

    kubectl patch deployment \
      argo-server \
      --namespace argo \
      --type='json' \
      -p='[{"op": "replace", "path": "/spec/template/spec/containers/0/args", "value": [
      "server",
      "--auth-mode=server"
    ]}]'

Note. below Im using bash as its my preferred option, But if you to are using bash and your on windows (wsl2) you MUST make sure you windows .kube/config is also pointing to the same K8's cluster, this is because the code running in vs will look in there for the context to write k8's secrets too!
you can do this by coping the relevent sections from ~/.kube/config to C:\Users\yourname\.kube\config

now in a bash window (can be cmd or powershell)

new bash window `kubectl -n argo port-forward deployment/minio 9000:9000 9001:9001`

another bash window `kubectl -n argo port-forward deployment/argo-server 2746:2746`

This allows you to access argo [localhost:2746](http://localhost:2746) and minio [localhost:9001](http://localhost:9001)

#### Dns change
Now we have our services running we need to make a DNS change, because minio needs to be accessed from argo (within kubernetes) its addressed something like this `http://minio:9000` but the code running in VisualStudio also needs to access it. To get around this, in notepad (in Aministrator mode) open the file `C:\Windows\System32\drivers\etc\Hosts` add the following line
- `127.0.0.1	minio`

save the file, now [http://minio:9000](http://minio:9000) will route to you local machine

### setup up an input file
- open browser [http://minio:9001/buckets/](http://minio:9001/buckets/)
- log in with `admin` `password`
- using the UI make a bucket called `bucket1` 
- and a folder called `00000000-1000-0000-0000-000000000000/dcm/` be careful not to put spaces before or after this name.
- make an empty local file called `input_dicom` and drag this into the created folder in the browser.

### add rabbit and mongo services
install [Helm 3](https://helm.sh/docs/intro/install/)
from a bash terminal in the root folder of the project
- `helm upgrade -i -n argo -f deploy/helm/mongo-local.yaml mongo deploy/helm`
- `helm upgrade -i -n argo -f deploy/helm/rabbitmq-local.yaml rabbit deploy/helm`

### running in VisualStudio
---
### NEW
------
You must now also run Informaticsgateway (mig), this is a separate project and is required for the workflow to validate and run.

### create an AETitle in the mig
below assumes you are running mig on port 5010.
```
curl -X POST 'http://localhost:5010/config/destination' -H 'Content-Type: application/json' -d '{"name": "MY_SCANNER",  "hostIp": "127.0.0.1",  "port": 10,  "aeTitle": "MY_SCANNER"}'

```


----

Now assuming your launchSettings in workflow manager has the line 
`"ASPNETCORE_ENVIRONMENT": "Local"`
AND in taskManager has the line
`"DOTNET_ENVIRONMENT": "Local"`
and nobody has broken the `appsettings.Local.json` file's

Note. if you dont have a launchsettings file you can make a folder under each project called `Properties` and make a new file called launchSettings.json with the following contents.

WorkflowManager.
```
{
  "profiles": {
    "Monai.Deploy.WorkflowManager": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "local"
      }
    }
  }
}
```
TaskManger.
```
{
  "profiles": {
    "Monai.Deploy.WorkflowManager.TaskManager": {
      "commandName": "Project",
      "environmentVariables": {
        "DOTNET_ENVIRONMENT": "local"
      }
    }
  }
}
```

right click on the solution and choose `Set Startup Projects` from the menu, select ` Multiple Startup Projects` radio button, then select both 
Monai.Deploy.WorkflowManager and  Monai.Deploy.WorkflowManager.TaskManager are set to `start`, then click apply.

![set startup options](static/VS-startup.png)

Select "Start" in VisualStudio and navigate to [http://localhost:5000/swagger](http://localhost:5000/swagger)

Open the post/workflows tab and click `try it out`, paste in the following to the body

```
{
	"name": "noddy workflow",
	"version": "1.0.0",
	"description": "Attempt at making a workflow",
	"informatics_gateway": {
		"ae_title": "MONAISCU",
		"data_origins": [
			"MY_SCANNER"
		],
		"export_destinations": [
			"PROD_PACS"
		]
	},
	"tasks": [
		{
			"id": "argo-task",
			"description": "trigger simple argo workflow",
			"type": "argo",
			"args": {
				"namespace":"argo",
				"workflow_template_name": "simple-workflow",
				"server_url": "https://localhost:2746",
				"allow_insecure": true
			},
			"artifacts": {
				"input": [
					{
						"name": "input_diacom",
						"value": "{{ context.input.dicom }}"
					}
				],
				"output": [
					{
						"name": "report",
						"value": "{{ context.output }}",
						"Mandatory": false
					}
				]
			}
		}
	]
}
```

#### Things to Note,
- `"server_url": "https://localhost:2746"`
this is where the local running code is expecting to talk to Argo.
- `"messaging_endpoint": "rabbit-monai"`
this is where argo is expecting to find rabbitMq, so this is the kubernetes address!
- Also make sure if your running in windows that youe .kube/config is pointing to the correct k8's !

click the `Execute` button, if the code can talk to mongoDb you will see something like this.
```
{
  "workflow_id": "9235f5e8-9ad2-44d2-8b41-2c1e4d2464c6"
}
```

You can use Mongo Compass, with connection string `mongodb://root:rootpassword@localhost:30017` to check the data is there.

### now we need an argo template to run.
navigate to [https://localhost:2746/workflow-templates?namespace=argo](https://localhost:2746/workflow-templates?namespace=argo) proceed passed the warnings about been insecure. (Chrome, click advance and then proceed to destination)
click on `CREATE NEW WORKFLOW TEMPLATE` button top left.
set the name to `name: simple-workflow` and its namespace to `namespace: argo` so it matches the workflow we posted above.
then click the `Create` button.
switch the tab back to [workflows](https://localhost:2746/workflows?limit=50) in the left menu

### rabbitmqAdmin for sending rabbit messages
[https://www.rabbitmq.com/management-cli.html](https://www.rabbitmq.com/management-cli.html)

In the command below replace xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx with the new workflowId from above ie. `9235f5e8-9ad2-44d2-8b41-2c1e4d2464c6`

```rabbitmqadmin -u admin -p admin -P 30672 -V monaideploy publish exchange=monaideploy routing_key=md.workflow.request  properties="{\"app_id\": \"16988a78-87b5-4168-a5c3-2cfc2bab8e54\",\"type\": \"WorkflowRequestMessage\",\"message_id\": \"0277e763-316c-4104-aeda-3620e7a642c7\",\"correlation_id\":\"ab482a7c-4da7-4e76-8d36-d194dd35555e\",\"content_type\": \"application/json\"}" payload="{\"payload_id\":\"00000000-1000-0000-0000-000000000000\",\"workflows\":[\"xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx\"],\"trigger\":{\"source\":\"MY_SCANNER\",\"destination\":\"PROD_PACS\"},\"file_count\":0,\"correlation_id\":\"e4b06f00-5ce3-4477-86cb-4f3bf20680c2\",\"bucket\":\"bucket1\",\"calling_aetitle\":\"MWM\",\"called_aetitle\":\"Basic_AE_3\",\"timestamp\":\"2022-07-13T11:34:34.8428704+01:00\"}"```

paste the above (with the proper workflowId) into bash and press enter.

Debug in VisualStudio (if its not already running) and view the progress
if you see error messages in the debug terminal in vs about mc.exe make sure you've copied it over as mentioned above.
ie copy mc.exe to `\monai-deploy-workflow-manager\src\TaskManager\TaskManager\bin\Debug\net8.0`

in the argo workflows tab [https://localhost:2746/workflows?limit=50](https://localhost:2746/workflows?limit=50)
you should see the activity of the argo task running. once complete the code will process the callback and update messages.

in MongoCompass check the results, by refreshing then selecting the created WorkflowInstance
by drilling down into Tasks -> 0 -> ExecutionStats, you should see the reported stats.

## Congratulations
you have just ran a workflow in Monai Workflow Manager and had that execute a template in Argo, your now free to make changes to the above and run your own docker pods with the Argo template !



## General hints and tips

### Re-running

To re-run the flow again, be sure to delete the workflowInstance object from Mongo, then just redo the `rabbitmqadmin -u admin -p admin -P 30672 -V monai..`

command from above

### Argo

you can use the API to work with Argo, here are some useful commands
exec into a container within the same namespace and install curl

- `kubectl -n monai exec -it mwm-monai-5964656c98-m7k9j -- bash`
- `apt update;apt install curl -y`

get workflow from a namespace
- `curl -k https://argo-server.argo:2746/api/v1/workflows/argo`

post a new template
- `curl -k -v -X POST https://argo-server.argo:2746/api/v1/workflow-templates/argo --header 'content-type: application/json' -d '{"createOptions":{"dryRun":["st.........'`

find and delete old workflows
- `curl -k https://argo-server.argo:2746/api/v1/workflows/argo | grep {\"name\":\"md-simple`
- `curl -k -X DELETE https://argo-server.argo:2746/api/v1/workflows/argo/md-simple-workflow-2tz62`


### Mongo
exaples for mongo (using mongo shell)
exec into the mongo container
- `kubectl -n monai exec -it mongo-monai-6c4dc7d5f9-b8mdd -- bash`

then
- `mongo "mongodb://root:rootpassword@localhost:27017"`
- `use WorkloadManager`
- `db.Workflows.find()`
- `db.WorkflowInstances.find()`
- `db.WorkflowInstances.drop()`


### Minio
examples for Minio (using mc.exe) [https://docs.min.io/docs/minio-client-complete-guide.html](https://docs.min.io/docs/minio-client-complete-guide.html)

exec into the WorkflowManager pod
- `k -n monai exec -it mwm-monai-5964656c98-m7k9j -- bash`

make a bucket
- `mc mb mwm/bucket1`

then
- `mc alias set mwm http://minio.monai:9000 admin password`
- `mc cp input_dicom mwm/bucket1/00000000-1000-0000-0000-000000000000/dcm/input_dicom` copy a file
- `mc ls --recursive mwm/bucket1` list all files in bucket


## Informatics Gateway
Although it has its own repo and is separate from the Workflow Manager, I have included a Helm file for it in here, its called Gateway and to deploy it use.
`helm -n monai upgrade -i -f deploy/helm/Gateway-local.yaml mig helm` Obviously change the namespace, name and path to suit.

Once deployed
- exec into the running container ie `kubectl -n monai exec -it mig-monai-797f584bf9-9tw8j -- bash`
- install curl `apt update;apy install curl -y` 
- add the default AETitle `curl -H 'Content-Type: application/json-patch+json' -d '{"aeTitle": "MonaiSCU","name": "Monai WFM"}' mig-monai:5000/config/ae`
- `exit`

The Informatics Gateway is now set up to receive and pass on data from the PACS or Orthanc servers

if using orthanc something like this `"DicomModalities": {"monai" : [ "MonaiSCU", "mig-monai.monai", 104 ]}` would need to be in its config file.

