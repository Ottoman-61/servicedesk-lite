# Netzwerk

| Gerät | Adapter | Adresse | Art |
| --- | --- | --- | --- |
| Ubuntu Server | enp0s3 (NAT) | 10.0.2.15 | automatisch (DHCP), Zugang ins Internet |
| Ubuntu Server | enp0s8 (Host-only) | 192.168.56.10/24 | fest (Netplan) |
| Windows-PC | Host-only-Adapter | 192.168.56.1 | fest (VirturalBox) |
| DHCP-Bereich Host-only |  | 192.168.56.101 bis .254 | feste IP bewusst außerhalb |

```mermaid
flowchart TB
    Internet((Internet))
    NAT["VirtualBox NAT 10.0.2.0/24"]
    HO["Host-only-Netz 192.168.56.0/24"]
    PC["Windows-PC 192.168.56.1"]
    subgraph Srv["Ubuntu Server"]
        A["enp0s3 10.0.2.15"]
        B["enp0s8 192.168.56.10 (fest)"]
    end
    Internet --- NAT --- A
    PC --- HO --- B
```


Freigegeben in der Firewall (ufw): 22 (SSH), 80 und 443 (Nginx).