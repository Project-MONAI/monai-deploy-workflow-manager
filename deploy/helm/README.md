# QUICK START GUIDE

> :Disclaimer: This was implemented in the context of a
[SUSE hackweek project](https://hackweek.opensuse.org/projects/hack-on-project-monai-medical-open-network-for-artificial-intelligence) . The goal was to learn the different
bits and pieces to deploy the different Monai Deploy components on a
kubernetes cluster. You can do what you want with this, but there is
NO guarantee of any kind. Note there are credentials stored in the
.yml files that you should change. Plus, there are more things not
mentioned in this file that you should consider if you want to deploy
to production for real. Still, I hope this guide can serve you as
inspiration. Happy hacking!


## Requirements

Before you start, you need to have installed the following tools:

* kubernetes client (also known as kubectl)
* helm client (also known as just helm)
* SUSE Rancher k3s

> You can install rancher with
``` 
curl -sfL https://get.k3s.io | sh -s -
sudo cp /etc/rancher/k3s/k3s.yaml ${HOME}/.k3s-config
sudo chown ${USER}:users ${HOME}/.k3s-config
export KUBECONFIG=${HOME}/.k3s-config
kubectl cluster-info
```

Once you have all the required tools, you need to create a namespace
in your kubernetes cluster. In this guide we are using the namespace
`monai`. We suggest you do the same. If you want to use a different
namespace, you will have to change that in the configuration files and
workflow files.

```
kubectl create ns monai
kubectl config set-context --current --namespace=monai
```

## Installation

We will be mostly using helm to install all the components, thus the
instructions are mostly running `helm install`. Still, we will need
to run some commands with kubectl to create some secrets and alike.

[Monai Deploy] architecture is componed of different components,
some of them implemented as part of the [Monai Project], some are
dependencies.

Specifically, we need these dependencies:

* RabbitMQ
* MongoDB
* MinIO
* Argo Workflows

And we need these components that are part of the Monai Project:

* Monai Workflow Manager
* Monai Task Manager
* Monai Information Gateway

Finally, we are going to also add a component to be able to
test and demonstrate the project:

* Orthanc

### RabbitMQ

1.First we need to create secret for rabbit that we are going to name
rabbit-secret-master

``` kubectl create secret --namespace monai generic rabbit-secret-master --from-literal=username=admin --from-literal=password=admin ```

> :warning: Note we are setting a very insecure password. This is just
for demonstration purposes. This is one of the things you should change
if you want to have a production system.

> :warning: Note the namespace. If you have chosen a different
namespace, here you need to use the one you've chosen.

2. Then, install rabbitmq with helm:

``` helm upgrade -i -n monai -f rabbitmq-local.yaml rabbitmq . ```

> :warning: Note the user and password are hard coded inside the yaml
file.

> :bulb: Note we use the -local.yaml file. This is because this is a
demo and we are not using external storage. On a production environment,
you should consider using volume claims.

Once you have the rabbitmq installed, you can move to the next step and
install mongodb.

### MongoDB

1. Install mongodb with helm

``` helm upgrade -i -n monai -f mongo-local.yaml mongo . ```

> :warning: Note the user and password are hard coded inside the yaml
file.

> :bulb: Note we use the -local.yaml file, so storage is not persisted.
For production, you should consider using volume claims. 

Next, let's install minio, a "like s3" service.

### MinIO

1. Install minio with helm

``` helm upgrade -i -n monai -f minio-local.yaml minio . ```

> :warning: As before, credentials are in the yaml file and you for
production environments you should consider using volume claims for
persistent storage

Once you have minio installed, you can proceed to install the last
dependency, which are argo workflows.

### Argo Workflows

1. Install argo workflows with helm

``` helm upgrade -i -n monai -f argo-workflows.yaml argo-workflows argo/argo-workflows ```

> :warning: Note that we are setting up the authentication method for
demonstration purposes. You might want to review this for production envs.

You need to have some admin permissions so that new deployments can be
created by Argo Workflows. The following will add them:

```kubectl create rolebinding default-admin --clusterrole=admin --serviceaccount=monai:default --namespace=monai```

> :WARNING!: This adds admin permissions to all service accounts in the
monai namespace. This is for demonstration purposes.  Be careful with
this if you want to set up a production environment.

Congrats! You have installed all dependencies. It was not difficult
with helm, right?

Now, let's install the Monai Project components.


### Monai Task Manager

1. Install Monai Task Manager

``` helm upgrade -i -n monai -f MTM.yaml mtm . ```

Easy, right?

> :bulb: If you look at the yaml file, you will notice that we setting
the container image and the tag name to pull. We are using the devel
image from the github registry. You may want to change that and use a
more stable release for production environments and even pull t hem from
a private container registry.

> :bulbt: In the yaml file, you will see that there are the secrets for
mongodb and minio. In case you changed them earlier, you should change
them as well in this yaml file. Not only the secrets, but also the
endpoints. The endpoints are the name of the service in your kubernetes
cluster. You can get this with `kubectl get services`.

> :bulb: You can see in the yaml file that there is a specific syntax
for the configuration environment variables. All configuration variables
can be overwritten with environment variables in this yaml file. To see
the list of variables, see the application.settings file in
src/TaskManager/TaskManager . You will easily guess the syntax ;)

> :waning: You can see that we are setting the environment variables
`ASPNETCORE_ENVIRONMENT` and `DOTNET_ENVIRONMENT` . This is to enable
debug messages and tooling. For production environments you should
change that.

### Monai Workflow Manager

1. Install Monai Workflow Manager

``` helm upgrade -i -n monai -f MWM.yaml mwm . ```

> All previous :bulb: and :warnings: warnings in the Monai Task Manager can be
applied.

With the Worfklow Manager installed, there is only one last piece to
install the Monai Informatics Gateway.

### Monai Informatics Gateway

1. Install Monai Informatics Gateway

``` helm upgrade -i -n monai -f Gateway-local.yaml mig . ```

2. Configure the aeTitle:

```
MIG=$(kubectl get pods --no-headers -o=name --selector=app.kubernetes.io/instance=mig)
kubectl exec -ti ${MIG} -- curl --location --request POST 'http://localhost:5000/config/ae/' --header 'Content-Type: application/json'  --data-raw '{ "aeTitle": "MONAISCU", "name": "MONAISCU" }'
kubectl exec -ti ${MIG} -- curl --location --request POST http://localhost:5000/config/destination --header 'Content-Type: application/json' --data-raw '{"name": "ORTHANC", "hostIp": "orthanc-monai", "port": 4242, "aeTitle": "ORTHANC"}'
```

Note without this extra step, you will not be able to connect to and from
Orthanc.

And now is time to install Orthanc, the last piece.

### Orthanc

1. Install Orthanc

``` helm upgrade -i -n monai -f orthanc.yaml orthanc . ```

> :warning: As before, note credentials are in the yaml file. You should
change that for production environments.

Congrats! You have finish the setup! Now let's test it.

## TEST

We need to access the orthanc web interface. We can do this by using port-forward:

```kubectl port-forward services/orthanc-monai 4242:4242 8042:8042```

> :warning: for production environments, you will need something more
sophisticated, like a load balancer.

Now you can open your browser and point it to `http://127.0.0.1:8042`.
You will be prompted by a user and a password. You can find them in the
orthanc.yaml file.

You need to do four things now:
1- setup an argo workflow
2- setup a clinical workflow
3- upload a dcm image
4- send the dcm to MIG via dicom modality


### Argo Workflow

Argo workflows are yaml files that need to be "pushed" to Argo Workflow.
First you need to forward the port so you can access it:

```kubectl port-forward services/argo-workflows-server 1234:2746```

Then, go to 127.0.0.1:1234/workflow-templates/monai and create a new
template with the contents of:

https://github.com/Project-MONAI/monai-deploy/blob/main/e2e-testing/test-scenarios/Argo_Workflows/argo_workflow_5.yaml

Review the namespace that should be one you have setup. If you followed
the example in this README, it should be "monai".

> :bulb: You could also use curl and use the REST API


### Clinical Workflow

Clinical Workflows are yaml files that need to be "pushed" to the
Monai Workflow Manager. For this, we can use `swagger`. First forward
the port so we can access it with our web browser (i.e. firefox).

```kubectl port-forward services/mwm-monai 5432:5000```

Then, you can connect to http://localhost:5432/swagger.

Open the post/workflows tab and click try it out. You can use the
following workflow:

https://github.com/Project-MONAI/monai-deploy/blob/main/e2e-testing/test-scenarios/Clinical_Workflows/liver_seg.json

Review the namespace that should be one you have setup. If you followed
the example in this README, it should be "monai".

Review the "ae_title" value. If you have followed the example in this
README, it should be "MONAISCU".

> :warning: On a production environment swagger won't be available.
You can still use the REST api.


### Upload a dcm image

Select "upload dcm image"

You can use [0]

### Send the dcm to MIG

Go to "all studies" and select "send to Dicom modality"

You should see the option "MONAISCU", just select that. This should
trigger everything, meaning it will connect to Monai Information Gateway,
then a message to RabbitMQ should be sent from MIG, which will be read
by MWM, which will send a message to MTM through thRabbitMQ, which will
start an argo workflow, which will pull some docker images that will
, finally, run the machine learning algorithm. Then, results will just
go back the same path until they reach Orthanc.


Congrats!! You've made it!


## Uninstall

If you want to uninstall, just run
```
sudo /usr/local/bin/k3s-uninstall.sh
```

> :warning: This will erase everything!


## Troubleshooting

Some times things do not go as expected. For this, here some tips to
help you find out what went wrong:

1- First of all, review the contents of the yaml files and make sure
the credentials and endpoints are correct.

1- use `kubectl describe pods` to find out issues like images not
being pulled or errors on starting the containers. Or just that
the image is being pulled and it takes longer than expected.

1- use `kubectl get services` to know the hostname of the different
components and the ports. These hostnames work inside the namespace,
thus they won't work on your workstation.

1- use `kubectl logs -f service/NAME` to watch the logs for ERRORS.


1- use `kubectl exec -ti PODNAME bash` to "connect" into a pod and
inspect. For example, you can look for the application.settings, or
run some curl commands.

1- see "application.settings" files to know which settings can be
overwritten with environments variables.

1- all Monai components have a health service. Just connect to 
`http://localhost:PORT/health`, where PORT is the one you get with
`kubectl port-forward service/NAME :5000` . Note, if the rabbitmq-admin
is unhealthy, try reloading the page.

1- You can use this medical workflow for testing:


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


This should give you a workflow id. Write this down.

Note, for this to work you will have to login to minio

``` kubectl port-forward services/minio-monai 9000:9000 9001:9001 ```
Connect to http://localhost:9001 and create the bucket and folder.

 and create a bucket called `bucket1` and the following folder in it:

```
00000000-1000-0000-0000-000000000000
```

Then, upload a file called input_diacom. No matters what the file contains.

Then, go to argo and create a template workflow called "simple-workflow".

Once you have done this, you can then send a message to rabbit to trigger
the workflow with:

change XXXX for  workflow id 

```rabbitmqadmin -u admin -p admin -V monaideploy publish exchange=monaideploy routing_key=md.workflow.request  properties="{\"app_id\": \"16988a78-87b5-4168-a5c3-2cfc2bab8e54\",\"type\": \"WorkflowRequestMessage\",\"message_id\": \"0277e763-316c-4104-aeda-3620e7a642c7\",\"correlation_id\":\"ab482a7c-4da7-4e76-8d36-d194dd35555e\",\"content_type\": \"application/json\"}" payload="{\"payload_id\":\"00000000-1000-0000-0000-000000000000\",\"workflows\":[\"xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx\"],\"file_count\":0,\"correlation_id\":\"e4b06f00-5ce3-4477-86cb-4f3bf20680c2\",\"bucket\":\"bucket1\",\"calling_aetitle\":\"MWM\",\"called_aetitle\":\"Basic_AE_3\",\"timestamp\":\"2022-07-13T11:34:34.8428704+01:00\"}" ```

All this is just to test WMW and MTM, you do not need to do this on
a production environment.

1- If you get this error
```ErrorDetails: DicomAssociation - connecting to AET "MONAISCU": DUL Association Rejected```
It is because mig is not configured. You can run this to get the list
of ae:

```kubectl exec -ti mig-monai-58c46879c9-q5zgm curl http://localhost:5000/config/ae```

and then you can set up the aeTitle with:
``` kubectl exec -ti  mig-monai bash ```
``` curl --location --request POST 'http://localhost:5000/config/ae/' --header 'Content-Type: application/json'  --data-raw '{ "aeTitle": "MONAISCU", "name": "MONAISCU" }' ```

More info at:

https://github.com/Project-MONAI/monai-deploy-informatics-gateway/blob/develop/docs/api/rest/config.md

1- If you see an error in the Informatics Gateway Log that could not
connect to the destination AE, it is because the Informatics
Gateway lacks the configuration about ORTHANC as the destination.

More info at:

https://github.com/Project-MONAI/monai-deploy-informatics-gateway/blob/develop/docs/api/rest/config.md


## Deploying in Google Kubernetes Engine

Check you have enough GPU quota in the zone you want to deploy. I have
tested this in europe-west4-a and I had to request to increase the quota
to 1 nvidia-tesla-a100 GPU.

Create a standard cluster and name it monai-deploy-1

> Note you could create an autopilot cluster, so that GPU nodes are
deployed automatically. Be aware that autopilot needs as much quota
as nodes you have multplied by the GPUs you request. With the standard
autopilot cluster configuration, I was getting 11 nodes, and so it was
requesting 11 GPUs, which was way higher than my quota.

Configure your cluster:

```gcloud container clusters get-credentials monai-deploy-1```

Check your cluster:

```kubectl cluster-info```

Edit the argo template and add the node selector:

```
      metadata: {}
      + nodeSelector:
      +  cloud.google.com/gke-accelerator: nvidia-tesla-a100
      container:
```

Add a node pool and inside that pool a node with 1 GPU.

Install drivers with:

```kubectl apply -f https://raw.githubusercontent.com/GoogleCloudPlatform/container-engine-accelerators/master/nvidia-driver-installer/cos/daemonset-preloaded-latest.yaml```

More info at https://cloud.google.com/kubernetes-engine/docs/how-to/gpus#console

Then, you will need a bigger machine for the argo workflows. You need
to create another pool with one node and this time you can select type
e2-standard-2.

Now, you can run all the helm and kubectl commands above.

> :warning: Using GKE with GPUs will cost you money, be careful with
that.



[0] https://drive.google.com/file/d/1d8Scm3q-kHTqr_-KfnXH0rPnCgKld2Iy/view?usp=sharing
a DICOM dataset that was converted to DICOM from Medical Decathlon
training and validation images (see https://github.com/Project-MONAI/monai-deploy/tree/main/deploy/monai-deploy-express#running-a-monai-deploy-workflow)
