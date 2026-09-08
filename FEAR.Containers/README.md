# FEAR Application Container Builder

This project builds and configures the FEAR application Docker containers.

## Prerequisites

- Python 3.x
- Docker Desktop
- The required libraries: `pip install -r Requirements.txt`

## Running

From the `FEAR.Containers` directory:

```bash
python FEAR.Containers.py
```

The script will ask a series of questions to configure the deployment (TLD, passwords, CA certificates, LLM settings) and then:

1. Generate certificates and configuration files into the `../deploy/` folder
2. Apply any customisations from `../deploy_overlay/`
3. Build and start the containers via Docker Compose

Use `--build-folder <path>` to override the default build artifact directory (`../build`).

## Folder Structure

```
FEAR.Containers/
├── FEAR.Containers.py       # Main build/config script
├── docker-compose.yaml      # Base Docker Compose configuration
├── Requirements.txt         # Python dependencies
├── src/                     # Supporting Python modules
├── FEAR/                    # FEAR application Dockerfile and config templates
└── Postgres/                # PostgreSQL Dockerfile

../deploy/                   # Generated deployment configuration (do not edit directly)
../deploy_overlay/           # Your local customisations (see below)
../build/                    # Generated build artifacts (certificates, keys)
../secrets/                  # Generated Docker secrets
```

## Customising the Deployment (`deploy_overlay`)

The `deploy_overlay` folder lets you apply site-specific customisations without modifying any source files. It is not committed to source control and is safe to maintain per-environment.

The overlay is applied **after** all configuration files are generated, immediately before the containers are started.

### How it works

Files placed under `deploy_overlay/` are merged into the `deploy/` folder using the same relative path. Three behaviours are supported based on file extension:

| Extension | Behaviour |
|---|---|
| *(any normal extension)* | Copied to the matching path in `deploy/`, creating the file if it doesn't exist or overwriting if it does |
| `.append` | Contents are **appended** to the matching target file in `deploy/` (extension stripped to determine target) |
| `.patch` | A unified diff applied to the matching target file in `deploy/` (extension stripped to determine target) |

Additionally, a `docker-compose.override.yml` placed at the root of `deploy_overlay/` is passed to Docker Compose at startup, allowing service-level overrides without touching the base `docker-compose.yaml`.

---

### Example 1 — Adding a private CA certificate

If your environment uses an internal CA that containers need to trust, place the certificate at:

```
deploy_overlay/
└── ssl/
    └── extra/
        └── my-internal-ca.crt
```

This will be copied to `deploy/ssl/extra/my-internal-ca.crt` before the containers start. The FEAR container is configured to pick up any certificates in that directory as trusted CAs.

> If the directory doesn't exist yet in `deploy/`, it will be created automatically.

---

### Example 2 — Adding entries to the container hosts file

Containers resolve hostnames using their own `/etc/hosts`, which is managed by Docker. To inject custom host entries (e.g. pointing containers at an internal server), add them to `docker-compose.override.yml` using Docker's `extra_hosts` key.

Create `deploy_overlay/docker-compose.override.yml`:

```yaml
services:
    web:
        extra_hosts:
            - "local.git.sys:10.1.1.1"

    jena:
        extra_hosts:
            - "local.git.sys:10.1.1.1"

    postgres:
        extra_hosts:
            - "local.git.sys:10.1.1.1"
```

This adds `10.1.1.1  local.git.sys` to `/etc/hosts` inside each container at startup. Multiple entries can be added by extending the list under each service.

> **Note:** This modifies hosts inside the containers only. To add entries to your **host machine's** hosts file, you will need to do that manually. The build script prints the required entries at the end of configuration.

---

### Example 3 — Combining both

A typical `deploy_overlay/` for a private network deployment might look like:

```
deploy_overlay/
├── docker-compose.override.yml    # extra_hosts for internal DNS
└── ssl/
    └── extra/
        └── internal-root-ca.crt   # Private CA to trust inside containers
```
