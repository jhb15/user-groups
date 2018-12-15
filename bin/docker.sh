echo "$ENCRYPTED_DOCKER_PASSWORD" | docker login -u "$ENCRYPTED_DOCKER_USERNAME" --password-stdin
cd UserGroups
docker build -t sem56402018/user-groups:$1 -t sem56402018/user-groups:$TRAVIS_COMMIT .
docker push sem56402018/user-groups:$TRAVIS_COMMIT
docker push sem56402018/user-groups:$1
