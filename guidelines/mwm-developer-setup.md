# Developer local setup

## assumptions
you have kubernetes running locally either via Docker desktop or microk8s (or similar)


### steps
- following the qickstart here https://argoproj.github.io/argo-workflows/quick-start/ but change to namespace to suit. ie 

- - `kubectl create ns minio`
- - `kubectl apply -n minio -f https://raw.githubusercontent.com/argoproj/argo-workflows/master/manifests/quick-start-postgres.yaml`

