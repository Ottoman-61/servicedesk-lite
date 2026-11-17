import os
import sys
import time

import psutil
import requests

API_URL = os.environ.get("SD_API_URL", "http://localhost:5071")
API_KEY = os.environ["SD_API_KEY"]
DEVICE_ID = int(os.environ.get("SD_DEVICE_ID", "2"))
INTERVAL = int(os.environ.get("SD_INTERVAL", "30"))
VERIFY = os.environ.get("SD_CA_CERT") or True


def collect():
    disk = psutil.disk_usage(os.path.abspath(os.sep)).percent
    if os.environ.get("SD_FAKE_DISK"):  # nur zum Testen
        disk = float(os.environ["SD_FAKE_DISK"])
    return {
        "deviceId": DEVICE_ID,
        "cpuPercent": psutil.cpu_percent(interval=1),
        "ramPercent": psutil.virtual_memory().percent,
        "diskPercent": disk,
        "isReachable": True,
    }


def send(measurement):
    response = requests.post(
        f"{API_URL}/measurements",
        json=measurement,
        headers={"X-Api-Key": API_KEY},
        timeout=10,
        verify=VERIFY,
    )
    response.raise_for_status()
    return response.json()


def main():
    once = "--once" in sys.argv
    while True:
        m = collect()
        try:
            result = send(m)
            print(f"CPU {m['cpuPercent']} %, RAM {m['ramPercent']} %, "
                  f"Disk {m['diskPercent']} % -> neue Tickets: {result['newTickets']}")
        except requests.RequestException as error:
            print(f"Senden fehlgeschlagen: {error}")
        if once:
            break
        time.sleep(INTERVAL)


if __name__ == "__main__":
    main()