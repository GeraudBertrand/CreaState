# 🖨️ Créalab Monitor

Interface de supervision et de gestion pour le Fablab de l'ESILV.

Ce projet est une application web Blazor (ASP.NET Core 8) conçue pour monitorer en temps réel le parc d'imprimantes 3D via le protocole MQTT, gérer l'historique des impressions et l'annuaire des membres.

## 🚀 Fonctionnalités Clés

- Dashboard Temps Réel : Visualisation de l'état des imprimantes (Températures, % avancement, Caméra).
- Architecture Réactive : Utilisation de SignalR pour des mises à jour instantanées sans rechargement de page.
- Mode Mock : Possibilité de tester l'interface avec des données simulées (parfait pour le développement hors Fablab).
- Administration : Gestion des IP machines et des annonces (Fermetures, Événements).

## 🛠️ Prérequis Techniques

Pour lancer le projet sur votre machine, vous avez besoin de :
.NET 8.0 SDK (Indispensable).

## ⚡ Démarrage Rapide (Pour tester le visuel)

Le projet est configuré par défaut pour utiliser des données de test (Mocks). Vous n'avez pas besoin d'être connecté au réseau du Fablab pour voir l'interface.

### Option A : Via Visual Studio 2022

Ouvrez le fichier Crealab.Monitor.sln.
Assurez-vous que le projet de démarrage est bien Crealab.Web.
Appuyez sur F5 (ou le bouton vert "Play").

### Option B : Via le Terminal / VS Code

#### 1. Cloner le dépôt
git clone [https://github.com/votre-repo/crealab-monitor.git](https://github.com/votre-repo/crealab-monitor.git)

#### 2. Entrer dans le dossier du projet web
cd Crealab.Web

#### 3. Lancer l'application
dotnet watch run

L'application sera accessible sur https://localhost:7000 (ou le port indiqué dans la console).

## 🎮 Navigation
- Aller sur Imprimantes pour voir l'état des imprimantes.
- Aller sur Membres pour voir l'annuaire et tester la recherche instantanée.
- Aller sur Historique pour voir le tableau des logs.
- Aller sur Admin pour tester l'ajout d'une imprimante ou la publication d'une annonce.

## 🏗️ Architecture du Code

Voici comment le projet est organisé :

- Components/ : Contient toutes les vues (Pages) et les briques UI (Shared).
- Layout/MainLayout.razor : Structure globale (Menu latéral + Header).
- Pages/Dashboard.razor : La vue principale avec la grille des machines.
- Services/ : La logique métier.
  - PrinterService.cs : Singleton qui stocke l'état des machines en mémoire RAM.
  - BambuMqttWorker.cs : BackgroundService qui tourne en tâche de fond pour écouter les imprimantes via MQTT.
- Models/ : Les définitions d'objets (Printer, Member, PrintJob).

Note sur le MQTT (Bambu Lab):
Le projet utilise la librairie MQTTnet pour se connecter au port 8883 des imprimantes.
En mode développement (local), le service MQTT tente de se connecter mais ne plantera pas l'application s'il ne trouve pas les imprimantes.
