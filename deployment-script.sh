#!/bin/bash

# Define variables
# NOTE: GitHub Actions provides these automatically, but for manual running you need to set them
# YOUR_GITHUB_USERNAME and YOUR_REPO_NAME must be lowercase
GITHUB_USERNAME="YOUR_GITHUB_USERNAME"
REPO_NAME="YOUR_REPO_NAME"
IMAGE_NAME="part-time-jobs-api"
FULL_IMAGE="ghcr.io/$GITHUB_USERNAME/$REPO_NAME/$IMAGE_NAME:latest"

# 1. Login to GitHub Container Registry
# You need a Personal Access Token (PAT) with 'read:packages' scope
echo "Please enter your GitHub PAT:"
read -s CR_PAT
echo $CR_PAT | docker login ghcr.io -u $GITHUB_USERNAME --password-stdin

# 2. Pull the latest image
docker pull $FULL_IMAGE

# 3. Stop and Remove existing container
docker stop $IMAGE_NAME || true
docker rm $IMAGE_NAME || true

# 4. Run the new container
# Replace ConnectionString with your actual production DB string
docker run -d \
  --name $IMAGE_NAME \
  --restart unless-stopped \
  -p 5000:8080 \
  -e ConnectionStrings__Default="Server=...;Database=...;User Id=...;Password=..." \
  $FULL_IMAGE

# 5. Cleanup
docker image prune -f

echo "Deployment complete!"
