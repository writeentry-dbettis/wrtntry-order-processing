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
    --allowed-audiences="https://github.com/[GITHUB ACCOUNT or ORG]" \
    --attribute-mapping="attribute.actor=assertion.actor,google.subject=assertion.sub,attribute.repository=assertion.repository" \
    --attribute-condition="assertion.repository_owner=='[GITHUB ACCOUNT or ORG]'"
    ```
1. Create the Service Account in the *Service Accounts* menu
1. Map the new Service Account to the GitHub user (note: the SUBJECT is in the format repo:writeentry-dbettis/wrtntry-order-processing:environment:production)
```
gcloud iam service-accounts add-iam-policy-binding github-actions@[PROJECT ID].iam.gserviceaccount.com \
    --role=roles/iam.workloadIdentityUser \
    --member="principal://iam.googleapis.com/projects/[PROJECT_NUMBER]/locations/global/workloadIdentityPools/[POOL_ID]/subject/[SUBJECT]"
```
1. Add the secrets to the GitHub environment
    1. WIF_PROVIDER: projects/PROJECT_NUMBER/locations/global/workloadIdentityPools/POOL_ID/providers/PROVIDER_ID
    1. WIF_SERVICE_ACCOUNT: service account email
