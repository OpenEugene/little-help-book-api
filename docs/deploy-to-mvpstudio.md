## Deploying to MVP Studio

MVP studio is a K8s cluster hosted at continu in Eugene, OR, USA.  The following are some high level steps to deploy.

- Make sure you have [VPN access](https://github.com/MVPStudio/k8/blob/master/docs/VPN_README.md) to MVP's cluster.
- (add more stuff here)
- build your docker container  
    ``` docker build -t mvpstudio/little-help-book:June25 ``` .
- gain access to the MVP [studio ORG](https://hub.docker.com/orgs/mvpstudio)
- push you image to dockerhub
    ``` docker push mvpstudio/little-help-book:Dec22 ```
- issue k8 controls to set context 
    ``` kubectl config use-context mvp-studio
    kubectl config set-context --current --namespace=little-help-book-prod ```
- edit your version to match what you built
    ``` image: mvpstudio/little-help-book:Dec22 ```
- deploy to the cluser
    ``` kubectl apply -f prod-deploy.yml ```

