# SercviceDesk Lite

Kleine IT-Betriebsplattform: Tickets, Geräteinventar und ein Monitoring-Agent,
der Probleme automatisch als Tickets meldet. Abschlussprojekt über alle
Teilqualifikationen (TQ1 bis TQ6).

## Funktionen
- Tickets anlegen, filtern und bearbeiten (Status, Priorität, Berarbeiter, Lösungsvermerk)
- Rollen: Nutzer, Techniker, Admin
- Geräteinventar und Dashboard mit Messwerten
- Python-Agent: meldet CPU, RAM, Festplatte und legt bei Problemen Tickets an, ohne Duplikate

## Architektur 
Diagramme: [Architektur](doku/architektur.md), [Datenmodell](doku/datenbank.md), [Netzwerk](doku/netzwerk.md)

Browser → Nginx (HTTPS) → Weböberfläsche (Razor Pages) → API (ASP.NET Core, EF Core) → MySQL Agent (Python) → Nginx (HTTPS, nur POST /api/measurements) → API

## Technik und Teilqualifikationen
| TQ | Umsetzung |
| --- | --- |
| TQ1 | Ubuntu Server und Windows-Client in VirtualBox, Installation und Wartung dokumentiert |
| TQ2 | Eigenes Host-only-Netz, feste IP, Netplan in `doku/` |
| TQ3 | SSH-Schlüssel, Firewall, Nginx mit HTTPS, Dienstbenutzer, Backup |
| TQ4 | MySQL-Datenmodell, Benutzer mit minimalen Rechten, ER-Diagramm in `doku/` |
| TQ5 | ASP.NET Core Web API mit Entity Framework, Python-Agent |
| TQ6 | Weboberfläche mit Razor Pages, Login Rollen |

## Sicherheit
- Passwörter nur als BCrypt-Hash, Anmeldung per JWT, Zugriff nach Rollen
- Geheimnisse in einer Datei nur für root lesbar, nie in Git
- SSH nur mit Schlüssel, Firewall erlaubt nur 22, 80 und 443
- Programme laufen unter einem eigenen Benutzer ohne Login
- Von der API ist nach außen nur der Messwert-Weg freigegeben

## Betrieb
- Dienste mit systemd, Start nach Neustart automatisch
- Backup jede Nacht, 14 Tage aufbewahrt, Wiederherstellung getestet
- Update-Ablauf: bauen, hochladen, Dienst stoppe, austauschen, starten
- Reihenfolge fürs lokale Entwickeln: VM, SSH-Tunnel, API, Weböberfläche

## Bekannte Grenzen
- Selbst signiertes Zertifikat (kein öffentlicher Domainname im internen Netz)
- Backups liegen auf demselben Server, eine Kopie auf einem zweiten System fehlt
- Das Dashboard fragt dir Messwerte pro Gerät einzel ab, bei vielen Gräten müsste man das anders lösen

## Was habe ich daraus gelernt
Mit diesem Projekt habe ich zum ersten Mal alle Teilqualifikationen zu einem durchgehenden System verbunden.
Server und Netzwerk in VirtualBox, abgesicherte Dienste mit Nginx und HTTPS, eine MySQL-Datenbank, eine ASP.NET-Core-API
mit Entity Framework, eine Razer-Pages-Oberfläche und ein Python-Agent. Im Unterricht habe ich mit SQLite, MySQL und mit einzelnen 
Anwendungen gearbeitet, hier habe ich eine echte Datenbank mit eigenen Benutzer, Beziehungen und Backup
eingesetzt udn die Wiederherstellung getestet. Am meisten habe ich beim Eingrenzen von Fehlern gelernt,
zum Beispiel bei einem Versionskonflikt zwischen Pomelo und EF-Core, bei einer falschen Route,
die ich über OpenAPI-Liste gefunden habe, und bei Rechtefehlern und Namensfehlern  in systemd und den Umgebungsvariablen.
Außerdem habe ich gelernt, Passwörter und Schlüssel aus dem Code herauszuhalten und Dokumentation wie Datenmodell 
und Architektur als Text durch Mermaid im Repository zu pflegen. 

## Ablauf
- Tag1 bis Tag3: Server, Netzwerk, Datenbank
- Tag4 bis Tag11: API, Login, Rollen, Agent, Weboberfläche, Dashboard
- Tag12 bis Tag14: Deployment, HTTPS, Backup