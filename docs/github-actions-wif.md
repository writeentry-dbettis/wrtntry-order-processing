# Configure GCP Project with WIF
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
1. Add the secrets to the GitHub environment
    1. WIF_PROVIDER: projects/[PROJECT NUMBER]/locations/global/workloadIdentityPools/[POOL ID]/providers/[PROVIDER ID]
    1. WIF_SERVICE_ACCOUNT: service account email
