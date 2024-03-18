

```
docker run -itd --name marten-test-db --restart=unless-stopped -p:7432:5432 -e POSTGRES_PASSWORD=password postgres:14

```