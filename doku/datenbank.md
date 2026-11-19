# Datenmodell

```mermaid
erDiagram
    users |o--o{ devices : "besitzt"
    users |o--o{ tickets : "erstellt"
    users |o--o{ tickets : "bearbeitet"
    devices |o--o{ tickets : "betrifft"
    devices ||--o{ measurements : "liefert"

    users {
        int id PK
        varchar username UK
        varchar email UK
        varchar password_hash
        enum role
        datetime created_at
    }
    devices {
        int id PK
        varchar name
        varchar type
        varchar hostname
        varchar ip_address
        varchar location
        int owner_id FK
        date warranty_until
        enum status
    } 
    tickets {
        int id PK
        varchar title
        text description
        enum status
        enum priority
        int created_by FK
        int assigned_to FK
        int device_id FK
        enum source
        text solution_note
        datetime created_at
        datetime resolved_at
    }
    measurements {
        bigint id PK
        int device_id FK
        decimal cpu_percent
        decimal ram_percent
        decimal disk_percent
        boolean is_reachable
        datetime measured_at
    }
```

- Passwörter werden nur als BCyrpt-Hash gespeichert.
- `tickets.source` unterscheidet Tickets von Menschen (`manuell`) und vom Agent (`agent`).
- Der Datenbankbennutzer der Anwendung hat nur Rechte auf die Datenbank `servicedesk`.