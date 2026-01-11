Biletado backend from Jannik Metz and Devin Schnurr, for Web-Engeneering 2 at DHBW Karlsruhe. 


# Tech Stack
- Backend: .NET 8 (C#)
- Database: PostgreSQL
- Containerization: Podman
- Serilog for logging
- API Documentation: Swagger / Rapidoc

# Setup Instructions

## Prerequisites
- Podman installed
- kind installed
- GitHub access to pull images from GHCR

## Installing biletado

These instructions where taken from the [original repository quickstart repository of biletado](https://gitlab.com/biletado/quickstart).

```bash
kubectl create namespace biletado
kubectl config set-context --current --namespace biletado
kubectl apply -k "https://gitlab.com/biletado/kustomize.git//overlays/kind?ref=main" --prune -l app.kubernetes.io/part-of=biletado -n biletado
kubectl rollout status deployment -n biletado -l app.kubernetes.io/part-of=biletado --timeout=600s
kubectl wait pods -n biletado -l app.kubernetes.io/part-of=biletado --for condition=Ready --timeout=120s
```
See [biletado/kustomize](https://gitlab.com/biletado/kustomize) for more options and more details.

You shoud now be able to open biletado at localhost:9090 and its API-docs

## Forward Ports to Access Services Locally

```bash
kubectl port-forward deployment/postgres  -n biletado 5432:5432
```

## Running the REST API Backend with Podman

- Pull and run latest image with Podman:

    - !Note: Replace `<commit-sha>` with the actual commit SHA of the image you want to run. Find the SHA in the [GitHub Container Registry for the repository](https://github.com/denmasch/web-eng-2-biletado/pkgs/container/web-eng-2-biletado%2Fbiletado-reservations-v3).

```bash
    # Pull the image
    podman pull ghcr.io/denmasch/web-eng-2-biletado/biletado-reservations-v3:<commit-sha>

    # Run the container locally
    podman run -p 5207:5207 ghcr.io/denmasch/web-eng-2-biletado/biletado-reservations-v3:<commit-sha>
```
- The API will be available at http://localhost:5207
- Port 5207 is exposed internally and mapped to your host.



# License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.