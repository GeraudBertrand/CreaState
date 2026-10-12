# Déploiement

| Environnement | Où | Image | `ASPNETCORE_ENVIRONMENT` |
|---|---|---|---|
| Local (dev) | PC, repo complet | buildée localement | `Development` (forcé par `docker-compose.override.yml`) |
| Staging | Raspberry Pi | `ghcr.io/geraudbertrand/creastate` | `Staging` (dans `.env`) |
| Production | Raspberry Pi | `ghcr.io/geraudbertrand/creastate` | `Production` (dans `.env`) |

Staging et prod utilisent le même `docker-compose.yml` ; seul le `.env` change.

## Local

```bash
cp .env.example .env
docker compose up --build
```

`docker-compose.override.yml` est chargé automatiquement : il ajoute le `build:` et force `Development`.
Appli sur http://localhost:8080.

## Staging / Production (Raspberry Pi)

Fichiers à copier sur le Pi, dans un même dossier :

- `docker-compose.yml`
- `.env` (créé à partir de `.env.example`, **jamais** `docker-compose.override.yml`)

Dans `.env` : `ASPNETCORE_ENVIRONMENT=Staging` ou `Production`, mots de passe MySQL (`openssl rand -base64 24`), SMTP. En prod, figer `APP_TAG` (ex. `v1.0.0`).

> Les mots de passe MySQL ne sont lus qu'à la première création du volume `creastate-mysql`.

### Login ghcr (seulement si le package est privé)

Créer un token GitHub (classic) avec le scope `read:packages`, puis :

```bash
docker login ghcr.io -u geraudbertrand
```

### Premier lancement et mises à jour

```bash
docker compose pull
docker compose up -d --no-build
```

L'image est publiée par la CI à chaque push sur `main` (tag `latest`) ou tag `v*`.
Les migrations EF sont appliquées au démarrage de l'appli.

Logs : `docker compose logs -f app`
