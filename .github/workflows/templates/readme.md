# CI/CD Pipeline Templates
The provided .yml files provide templates for CI/CD pipelines in GitHub.  The templates can be used as described in the following document.

## Setup
The `templates` folder can be copied to the repository's `.github/workflows` as reference because GitHub only uses the workflow files in the top-level `workflows` directory.  To create a new pipeline, copy the corresponding template from the `templates` folder to the parent `workflows` folder and rename the template file accordingly.  For example, a CI build template for a .NET API would use the **_dotnet.api.ci.yml** template file and would be renamed to **_exampleProject.api.ci.yml** (note the `_` prefix which should be used to denote a reusable workflow).  In most workflow templates, the **\<\<PROJECT PATH\>\>** and/or the **\<\<PROJECT DEPS\>\>** will need to be replaced by actual paths in the source directory.

## Environments
The workflows depend on variables and secrets configured in associated environments.  The environments map to the following branches in the repository:
* **Production**: `main`
* **Demo**: `main`
* **Test**: any branch in the `releases/*` folder
* **Development**: `development`

The environment-to-branch mapping can be managed in the **setup-env** jobs where the triggering branch determines the `environment` environment variable.

Each environment should contain the following variables and secrets in order to build and deploy to GCP projects:

#### Variables
* **ARTIFACT_REPOSITORY**: the name of the Artifact Repository in GCP in which to store the container images (this is not the full URL, just the name of the repository),
* **CLOUD_RUN_SA**: the service account email used to run the Cloud Run services.  This can be the same account used to deploy the service or another account configured specifically for running Cloud Run apps.
* **GCP_PROJECT_ID**: the GCP project ID in which to deploy the application service.  This is NOT the project number, but rather the ID as listed on the main project dashboard.
* **REGION**: the region in which to deploy the artifacts and services.

#### Secrets
* **WIF_PROVIDER**: the WIF provider identifier to authenticate with GCP.  Refer to the section [Configure GCP Project with WIF](#github-secrets) below for format details.
* **WIF_SERVICE_ACCOUNT**: the service account email address used to authenticate GitHub pipelines with the GCP project.

## Top-Level Workflows
Top-level workflows are typically the full CI/CD pipeline workflows that reference the individual CI and CD workflows.  If additional steps are necessary (ie, unit testing, output packaging, and/or status notifications/alerts), these can be added to the top-level workflows so that the reusable workflows a limited to a single responsibility be it building, testing, or deploying the project.  The following sections discuss each top-level workflow in more detail.

### .NET API & Serverless CI/CD
Build a .NET project (either API or serverless application), create the build artifact, and deploy a containerized image to Cloud Run service/job/function.
* Template File: `dotnet.api.yml` & `dotnet.serverless.yml`
* Triggers: 
    * any commit to **main**, **development**, or any branch in the **releases/** folder, or
    * manual workflow dispatch in GitHub UI
* Environment: (no environment variables needed)

### Python CI/CD
Copy a Python serverless project to an artifact output and deploy the artifact as a containerized image to Cloud Run service/job/function.
* Template File: `python.serverless.yml`
* Triggers: 
    * any commit to **main**, **development**, or any branch in the **releases/** folder, or
    * manual workflow dispatch in GitHub UI
* Environment: (no environment variables needed)

### Angular CI/CD
Build an Angular project, create the build artifact, and deploy a containerized image to Cloud Run service.
* Template File: `angular.web.yml`
* Triggers: 
    * any commit to **main**, **development**, or any branch in the **releases/** folder, or
    * manual workflow dispatch in GitHub UI
* Environment: (no environment variables needed)

## Specific Workflows
Specific CI workflows should be built for each project associated with a pipeline.  The builds can be configured as necessary for each individual project and the CI pipelines can be reused for CI/CD or just CI in the case of PR builds/testing.
### .NET API & Serverless CI
Build an API or Serverless application that targets the .NET framework.  A file named `Dockerfile.deploy` should exist in the project root folder which is copied to the build artifact to be used in a subsequent deployment step.
* Template File: `_dotnet.api.ci.yml` or `_dotnet.serverless.ci.yml`
* Triggers: reusable workflow triggered on `workflow_call`
* Inputs:
    * **configuration**: (string, required) the build configuration to use when building the application, typically "Debug" or "Release"
    * **artifactName**: (string, required) the name of the GitHub artifact produced from the build output
* Environment:
    * **DOTNET_VERSION**: the .NET version to use when building the project. **NOTE**: this should correspond to the same version used in the Docker deployment file for the project.

### Python Serverless CI
Build serverless application that uses the Python GCP cloud framework.  A file named `Dockerfile.deploy` should exist in the project root folder which is copied to the build artifact to be used in a subsequent deployment step.
* Template File: `_python.serverless.ci.yml`
* Triggers: reusable workflow triggered on `workflow_call`
* Inputs:
    * **artifactName**: (string, required) the name of the GitHub artifact produced from the build output

### Angular Web Application CI
Build a web application that targets an Angular framework.  A file named `Dockerfile.deploy` should exist in the project root folder which is copied to the build artifact to be used in a subsequent deployment step.  A file name `default.conf` should also exist in the project root folder to use as the NGINX configuration in the cloud service instance.
* Template File: `_angular.web.ci.yml`
* Triggers: reusable workflow triggered on `workflow_call`
* Inputs:
    * **webui-configuration**: (string, required) the build configuration to use when building the application, typically "development" or "production".  This value is passed in to the `--configuration` argument of the `ng build` command.
    * **artifactName**: (string, required) the name of the GitHub artifact produced from the build output

## Generic Workflows
The outputs of the CI workflows have been configured to produce a package that can be easily converted into a containerized image and pushed to GCP for deployment.  Therefore, the same CD pipeline can be used to deploy any of the application builds created by the CI pipelines in this project.
### GCP Cloud Run CD
Deploy any container-ready build artifact to GCP.
* Template File: `_cloudrun.gcp.cd.yml`
* Triggers: reusable workflow triggered on `workflow_call`
* Inputs:
    * **environment**: (string, required) the name of the environment to use for deployment variables and secrets, ie GCP_PROJECT_ID, REGION, etc.
    * **ref**: (string, required) the image tag for this build, typically the commit hash
    * **artifactName**: (string, required) the base name of the artifact created from the workflow
    * **serviceName**: (string, required) the target Cloud Run service/job/function in which to deploy the newly created image
    * **cloudRunEnvVars**: (string, optional) a list of environment variables to apply in the target Cloud Run environment, ie '["ASPNETCORE_ENVIRONMENT=Development", "DEBUG=true"]'.  This will overwrite any existing values or add new values if the key does not already exist.  **NOTE**: Do NOT put secrets in this list.  Secrets should be created manually and linked to secrets in GCP Secret Manager for secure storage.

## GCP Project Requirements
### Configure GCP Project with WIF (Workload Identity Federation)
Using the GCP console in the browser, run the following:

1. Create a workload identity pool
    ```
    gcloud iam workload-identity-pools create github-wif-pool \
        --location="global" \
        --description="CI/CD Pipelines in GitHub" \
        --display-name="GitHub Pipelines"
    ```
1. Create a workload identity pool with OIDC connection for GitHub
    ```
    gcloud iam workload-identity-pools providers create-oidc github-actions-wif \
    --location="global" \
    --workload-identity-pool="github-wif-pool" \
    --issuer-uri="https://token.actions.githubusercontent.com/" \
    --attribute-mapping="google.subject=assertion.sub,attribute.actor=assertion.actor,attribute.repository=assertion.repository,attribute.repository_owner=assertion.repository_owner" \
    --attribute-condition="assertion.repository_owner=='[GITHUB ACCOUNT or ORG]'"
    ```
1. Create the Service Account in the *Service Accounts* menu
1. Map the new Service Account to the GitHub user (**NOTE**: the [REPO] is in the format of [ORG]/[REPO_NAME])
    ```
    gcloud iam service-accounts add-iam-policy-binding "github-actions@$[PROJECT_ID].iam.gserviceaccount.com" \
        --project="${PROJECT_ID}" \
        --role="roles/iam.workloadIdentityUser" \
        --member="principalSet://iam.googleapis.com/projects/[PROJECT NUMBER]/locations/global/workloadIdentityPools/github-wif-pool/attribute.repository/[REPO]"

    ```
1. Add the "Service Account User" role to the new principalSet created in previous step.
1. Add project roles to the Service Account via the IAM menu -> Grant Access.
1. <a name="github-secrets"></a>Add the secrets to the GitHub environment
    1. WIF_PROVIDER: projects/[PROJECT NUMBER]/locations/global/workloadIdentityPools/[POOL ID]/providers/[PROVIDER ID]
    1. WIF_SERVICE_ACCOUNT: service account email
